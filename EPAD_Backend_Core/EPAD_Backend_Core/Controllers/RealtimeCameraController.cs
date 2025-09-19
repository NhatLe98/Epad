using EPAD_Backend_Core.Base;
using EPAD_Backend_Core.WebUtilitys;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using EPAD_Services.Impl;
using EPAD_Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EPAD_Backend_Core.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class RealtimeCameraController : ApiControllerBase
    {
        private IIC_CameraService _IIC_CameraService;

        private IMemoryCache cache;
        private EPAD_Context context;
        static WebSocket webSocket;
        byte[] SOI = new byte[2] { 0xFF, 0xD8 };

        // List cameras is streaming
        private static List<IC_Camera> _usingCameras = new List<IC_Camera>();
        // List websockets for transfer stream result to front-end
        // with a key is using combine of id received from front-end and camera's index
        private static ConcurrentDictionary<string, WebsocketManager> _clients = new ConcurrentDictionary<string, WebsocketManager>();
        // List websocket keys per camera with a key is using combine of camera's ip and camera's port
        private static ConcurrentDictionary<string, List<string>> _streams = new ConcurrentDictionary<string, List<string>>();
        // List tasks process camera streaming with a key is using combine of camera's ip and camera's port
        private static ConcurrentDictionary<string, Task> _tasks = new ConcurrentDictionary<string, Task>();

        public RealtimeCameraController(IServiceProvider pProvider) : base(pProvider)
        {
            context = TryResolve<EPAD_Context>();
            cache = TryResolve<IMemoryCache>();

            _IIC_CameraService = TryResolve<IIC_CameraService>();
        }

        public void RemoveClientAsync(string clientId)
        {
            try
            {
                if (_clients.TryRemove(clientId, out var removedClient))
                {
                    if (removedClient != null)
                    {
                        removedClient.Websocket.Dispose();
                    }
                    //Console.WriteLine($"Client removed. ClientId: {clientId}");
                }

                if (_streams != null && _streams.Count > 0)
                {
                    foreach (var _stream in _streams)
                    {
                        _stream.Value.RemoveAll(x => x == clientId);
                    }
                    if (_streams.Any(x => x.Value == null || (x.Value != null && x.Value.Count == 0)))
                    {
                        foreach (var _stream in _streams)
                        {
                            _usingCameras.RemoveAll(x => (x.IpAddress + x.Port).ToString() == _stream.Key);
                            _tasks.TryRemove(_stream.Key, out _);
                            _streams.TryRemove(_stream);
                        }
                    }
                }

                //Task removedTask;
                //if (_tasks.TryRemove(clientId, out removedTask))
                //{

                //}
            }
            catch (Exception ex)
            {
                //_Logger.LogError(ex.ToString());
            }
        }

        private async Task HandleClientAsync(string ip, int port, string username, string password)
        {
            var key = ip + port.ToString();
            try
            {

                HttpClientHandler handler = new HttpClientHandler();
                handler.Credentials = new NetworkCredential("admin", "Vstarschool");

                const string imgDelimiter = "\r\n--boundarySample\r\n";
                const string jpegStarter = "/9j/";
                using (var client = new HttpClient(handler))
                {
                    var link = $"http://{username}:{password}@{ip}:{port}/ISAPI/Streaming/channels/101/httpPreview";
                    client.BaseAddress = new Uri(link);
                    client.Timeout = TimeSpan.FromSeconds(5);

                    var request = new HttpRequestMessage(HttpMethod.Get, link);
                    var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

                    var stream = await response.Content.ReadAsStreamAsync();

                    const int limit = 100 * 1024 * 1024;
                    byte[] buffer = new byte[limit];
                    var base64ASCII = string.Empty;
                    while (true)
                    {
                        var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                        base64ASCII += Encoding.ASCII.GetString(buffer, 0, bytesRead);
                        if (bytesRead == 0)
                            break;

                        string base64 = Convert.ToBase64String(buffer, 0, bytesRead);
                        if (_clients != null && _clients.Count > 0 && _streams != null && _streams.Count > 0)
                        {
                            _streams.TryGetValue(key, out var _stream);
                            if (_stream != null && _stream.Count > 0 && base64.StartsWith(jpegStarter)
                                && base64ASCII.Substring(base64ASCII.Length - imgDelimiter.Length) == imgDelimiter)
                            {
                                foreach (var item in _clients)
                                {
                                    if (_stream.Contains(item.Key))
                                    {
                                        var id = item.Key;
                                        var socket = item.Value;

                                        if (socket != null && socket.Websocket != null && socket.Websocket.State == WebSocketState.Open)
                                        {
                                            var bytes = Encoding.ASCII.GetBytes(base64);
                                            var arraySegment = new ArraySegment<byte>(bytes);
                                            socket.Websocket.SendAsync(arraySegment, WebSocketMessageType.Text, true, CancellationToken.None);
                                        }
                                    }
                                }
                                buffer = new byte[limit];
                                if (base64ASCII.Length > 10000000)
                                {
                                    base64ASCII = string.Empty;
                                }
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                }               
            }
            catch (Exception ex)
            {
                //_Logger.LogError(ex.ToString());
                if (_clients != null && _clients.Count > 0 && _streams != null && _streams.Count > 0)
                {
                    _streams.TryGetValue(key, out var _stream);
                    if (_stream != null && _stream.Count > 0)
                    {
                        List<Task> tasks = new List<Task>();
                        foreach (var item in _clients)
                        {
                            if (_stream.Contains(item.Key))
                            {
                                var id = item.Key;
                                var socket = item.Value;

                                if (socket != null && socket.Websocket != null && socket.Websocket.State == WebSocketState.Open)
                                {
                                    var bytes = Encoding.ASCII.GetBytes("ErrorConnect");
                                    var arraySegment = new ArraySegment<byte>(bytes);
                                    tasks.Add(socket.Websocket.SendAsync(arraySegment, WebSocketMessageType.Text, true, CancellationToken.None));
                                }
                            }
                        }
                        await Task.WhenAll(tasks).ContinueWith(task => 
                        { 
                            RemoveWebsocketByCameraIndex(key); 
                        });
                    }
                }
            }
        }

        [HttpGet]
        public async Task InitializeWebSocket()
        {
            var context = ControllerContext.HttpContext;
            var isSocketRequest = context.WebSockets.IsWebSocketRequest;

            #region MULTIPLE THREAD STREAM CAMERA USING WEBSOCKET TEST
            //try
            //{
            //    // Retrieve clientId from query parameter
            //    var clientId = context.Request.Query["id"];

            //    var ws = await context.WebSockets.AcceptWebSocketAsync();
            //    _clients.TryAdd(clientId, ws);
            //    Console.WriteLine($"Client connected. ClientId: {clientId}");
            //    var task = Task.Run(() => HandleClientAsync(clientId, ws));
            //    _tasks.TryAdd(clientId, task);
            //    while (_clients.ContainsKey(clientId)) 
            //    {
            //        Thread.Sleep(1000);
            //    }
            //}
            //catch (Exception ex)
            //{

            //}
            #endregion

            try
            {
                ConfigObject config = ConfigObject.GetConfig(cache);
                if (config == null)
                {
                    return;
                }

                if (isSocketRequest)
                {
                    var listCamera = await _IIC_CameraService.GetAllCamera(config.CompanyIndex);

                    var clientId = context.Request.Query["id"];
                    var action = context.Request.Query["action"];
                    var cameraIndex = int.Parse(context.Request.Query["cameraIndex"]);
                    var camera = listCamera.FirstOrDefault(x => x.Index == cameraIndex);
                    var cameraKey = camera.IpAddress + camera.Port.ToString();

                    if (camera != null)
                    {
                        if (!_usingCameras.Any(x => x.IpAddress == camera.IpAddress && x.Port == camera.Port))
                        {
                            var newCamera = new IC_Camera();
                            newCamera.Index = cameraIndex;
                            newCamera.IpAddress = camera.IpAddress;
                            newCamera.Port = camera.Port;
                            newCamera.UserName = camera.UserName;
                            newCamera.Password = camera.Password;
                            newCamera.Description = "wait";
                            _usingCameras.Add(newCamera);
                        }
                        else
                        {
                            var usingCamera = _usingCameras.FirstOrDefault(x => x.IpAddress == camera.IpAddress && x.Port == camera.Port
                                && (x.UserName != camera.UserName || x.Password != camera.Password));
                            if (usingCamera != null)
                            {
                                usingCamera.UserName = camera.UserName;
                                usingCamera.Password = camera.Password;
                                usingCamera.Description = "update";
                            }
                        }
                    }

                    if (action == "add")
                    {
                        var ws = await context.WebSockets.AcceptWebSocketAsync();
                        var wsm = new WebsocketManager();
                        wsm.Websocket = ws;
                        wsm.LastCheckAlive = DateTime.Now;
                        _clients.TryAdd(clientId + cameraIndex.ToString(), wsm);

                        if (_streams.ContainsKey(cameraKey))
                        {
                            _streams.TryGetValue(cameraKey, out var client);
                            client.Add(clientId + cameraIndex.ToString());
                        }
                        else
                        {
                            _streams.TryAdd(cameraKey, new List<string> { clientId + cameraIndex.ToString() });
                        }
                    }

                    if (_streams != null && _streams.Count > 0)
                    {
                        if (_usingCameras.Any(x => !string.IsNullOrWhiteSpace(x.Description)))
                        {
                            var usingCameras = _usingCameras.Where(x => !string.IsNullOrWhiteSpace(x.Description)).ToList();
                            if (usingCameras != null && usingCameras.Count > 0)
                            {
                                foreach (var usingCamera in usingCameras)
                                {
                                    var key = usingCamera.IpAddress + usingCamera.Port.ToString();
                                    if (usingCamera.Description == "wait")
                                    {
                                        _tasks.TryAdd(key, Task.Run(async () =>
                                        {
                                            await HandleClientAsync(usingCamera.IpAddress, usingCamera.Port, usingCamera.UserName, usingCamera.Password);
                                        }));
                                    }
                                    else if (usingCamera.Description == "update")
                                    {
                                        _tasks.TryRemove(key, out var removedTask);
                                        _tasks.TryAdd(key, Task.Run(async () =>
                                        {
                                            await HandleClientAsync(usingCamera.IpAddress, usingCamera.Port, usingCamera.UserName, usingCamera.Password);
                                        }));
                                    }
                                    usingCamera.Description = string.Empty;
                                }
                            }
                        }                        
                    }

                    while (_clients != null && _clients.Count > 0)
                    {
                        if (_clients.Any(x => x.Value.LastCheckAlive.AddMinutes(3) < DateTime.Now))
                        {
                            var expiredClients = _clients.Where(x => x.Value.LastCheckAlive.AddMinutes(3) < DateTime.Now).ToList();
                            if (expiredClients != null && expiredClients.Count > 0)
                            {
                                foreach (var item in expiredClients)
                                {
                                    RemoveClientAsync(item.Key);
                                }
                            }
                        }
                        if (_streams != null && _streams.Count > 0)
                        {
                            if (_usingCameras.Any(x => !string.IsNullOrWhiteSpace(x.Description)))
                            {
                                var usingCameras = _usingCameras.Where(x => !string.IsNullOrWhiteSpace(x.Description)).ToList();
                                if (usingCameras != null && usingCameras.Count > 0)
                                {
                                    foreach (var usingCamera in usingCameras)
                                    {
                                        var key = usingCamera.IpAddress + usingCamera.Port.ToString();
                                        if (usingCamera.Description == "wait")
                                        {
                                            _tasks.TryAdd(key, Task.Run(async () =>
                                            {
                                                await HandleClientAsync(usingCamera.IpAddress, usingCamera.Port, usingCamera.UserName, usingCamera.Password);
                                            }));
                                        }
                                        else if (usingCamera.Description == "update")
                                        {
                                            _tasks.TryRemove(key, out var removedTask);
                                            _tasks.TryAdd(key, Task.Run(async () =>
                                            {
                                                await HandleClientAsync(usingCamera.IpAddress, usingCamera.Port, usingCamera.UserName, usingCamera.Password);
                                            }));
                                        }
                                        usingCamera.Description = string.Empty;
                                    }
                                }
                            }
                        }
                        Thread.Sleep(1000);
                    }
                }
            }
            catch (Exception ex)
            {
                //_Logger.LogError(ex.ToString());
            }
            finally
            {
                
            }

            #region SINGLE THREAD STREAM CAMERA USING WEBSOCKET TEST
            //try
            //{
            //    if (isSocketRequest && webSocket == null)
            //    //if (webSocket == null)
            //    {
            //        webSocket = await context.WebSockets.AcceptWebSocketAsync();

            //        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            //        HttpClientHandler handler = new HttpClientHandler();
            //        handler.Credentials = new NetworkCredential("admin", "Vstarschool");

            //        HttpClient client = new HttpClient(handler);
            //        Task.Run(async () =>
            //        {
            //            if (webSocket != null && webSocket.State == WebSocketState.Open && !cancellationTokenSource.Token.IsCancellationRequested)
            //            {

            //                #region TRY TO EXTRACT IMAGE FROM STREAM RESULT
            //                //HttpResponseMessage result = await client.GetAsync("http://171.244.236.56:5678/ISAPI/Streaming/channels/101/picture");
            //                //result.EnsureSuccessStatusCode();

            //                //HttpContent content = result.Content;
            //                //var stream = await result.Content.ReadAsStreamAsync();
            //                //using (MemoryStream memoryStream = new MemoryStream())
            //                //{
            //                //    stream.CopyTo(memoryStream);
            //                //    var base64 = Convert.ToBase64String(memoryStream.ToArray());

            //                //    if (webSocket.State == WebSocketState.Open)
            //                //    {
            //                //        var bytes = Encoding.ASCII.GetBytes(base64);
            //                //        var arraySegment = new ArraySegment<byte>(bytes);
            //                //         webSocket.SendAsync(arraySegment, WebSocketMessageType.Text, true, CancellationToken.None);
            //                //        //Thread.Sleep(500); //sleeping so that we can see several messages are sent
            //                //    }
            //                //}
            //                #endregion

            //                const string imgDelimiter = "\r\n--boundarySample\r\n";
            //                const string jpegStarter = "/9j/";
            //                using (var client = new HttpClient(handler))
            //                {
            //                    client.BaseAddress = new Uri("http://admin:Vstarschool@171.244.236.56:5678/ISAPI/Streaming/channels/101/httpPreview");

            //                    var request = new HttpRequestMessage(HttpMethod.Get, "http://admin:Vstarschool@171.244.236.56:5678/ISAPI/Streaming/channels/101/httpPreview");
            //                    var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

            //                    var stream = await response.Content.ReadAsStreamAsync();

            //                    const int limit = 100 * 1024 * 1024;
            //                    byte[] buffer = new byte[limit];
            //                    var base64ASCII = string.Empty;
            //                    while (true)
            //                    {
            //                        var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            //                        base64ASCII += Encoding.ASCII.GetString(buffer, 0, bytesRead);
            //                        if (bytesRead == 0)
            //                            break;

            //                        string base64 = Convert.ToBase64String(buffer, 0, bytesRead);
            //                        if (webSocket != null)
            //                        {
            //                            if (webSocket.State == WebSocketState.Open && base64.StartsWith(jpegStarter)
            //                                && base64ASCII.Substring(base64ASCII.Length - imgDelimiter.Length) == imgDelimiter)
            //                            {
            //                                var bytes = Encoding.ASCII.GetBytes(base64);
            //                                var arraySegment = new ArraySegment<byte>(bytes);
            //                                await webSocket.SendAsync(arraySegment, WebSocketMessageType.Text, true, CancellationToken.None);
            //                                buffer = new byte[limit];
            //                                if (base64ASCII.Length > 10000000)
            //                                {
            //                                    base64ASCII = string.Empty;
            //                                }
            //                            }
            //                        }
            //                        else
            //                        {
            //                            break;
            //                        }
            //                    }
            //                }

            //                #region TRY TO EXTRACT IMAGE FROM STREAM RESULT VER 2
            //                //using (var client = new HttpClient(handler))
            //                //{
            //                //    client.BaseAddress = new Uri("http://admin:Vstarschool@171.244.236.56:5678/ISAPI/Streaming/channels/101/httpPreview");

            //                //    var request = new HttpRequestMessage(HttpMethod.Get, "http://admin:Vstarschool@171.244.236.56:5678/ISAPI/Streaming/channels/101/httpPreview");
            //                //    var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

            //                //    var stream = await response.Content.ReadAsStreamAsync();

            //                //    var headers = string.Empty;
            //                //    int contentLength = 0;
            //                //    byte[] imageBuffer = new byte[contentLength];
            //                //    int bytesRead = 0;

            //                //    while (true)
            //                //    {
            //                //        byte[] buffer = new byte[500 * 1024 * 1024];
            //                //        int read = await stream.ReadAsync(buffer, 0, buffer.Length);

            //                //        if (read == 0)
            //                //            break;

            //                //        for (int i = 0; i < read; i++)
            //                //        {
            //                //            if (MatchSOI(buffer, i))
            //                //            {
            //                //                contentLength = GetContentLength(headers);
            //                //                imageBuffer = new byte[contentLength];
            //                //            }

            //                //            if (contentLength > 0)
            //                //            {
            //                //                imageBuffer[bytesRead++] = buffer[i];

            //                //                if (bytesRead == contentLength)
            //                //                {
            //                //                    // Image fully read
            //                //                    string base64 = Convert.ToBase64String(buffer, 0, bytesRead);
            //                //                    if (webSocket.State == WebSocketState.Open)
            //                //                    {
            //                //                        var bytes = Encoding.ASCII.GetBytes(base64);
            //                //                        var arraySegment = new ArraySegment<byte>(bytes);
            //                //                        webSocket.SendAsync(arraySegment, WebSocketMessageType.Text, true, CancellationToken.None);
            //                //                        //Thread.Sleep(500); //sleeping so that we can see several messages are sent
            //                //                    }

            //                //                    //headers.Clear();
            //                //                    headers = string.Empty;
            //                //                    contentLength = 0;
            //                //                    bytesRead = 0;
            //                //                }
            //                //            }
            //                //            else
            //                //            {
            //                //                headers += buffer[i];
            //                //            }
            //                //        }
            //                //    }
            //                //}
            //                #endregion
            //            }
            //        }, cancellationTokenSource.Token);
            //        var receiveBuffer = new byte[1024 * 1024];
            //        while (webSocket != null && webSocket.State == WebSocketState.Open)
            //        {
            //            await webSocket.ReceiveAsync(new ArraySegment<byte>(receiveBuffer), CancellationToken.None);
            //            var text = Encoding.Default.GetString(receiveBuffer);
            //            if (text.StartsWith("close"))
            //            {
            //                var a = true;
            //                cancellationTokenSource.Cancel();
            //                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
            //                webSocket.Dispose();
            //                webSocket = null;
            //            }
            //        }
            //    }
            //    else
            //    {
            //        //context.Response.StatusCode = 400;

            //        var receiveBuffer = new byte[1024 * 1024];
            //        while (webSocket != null && webSocket.State == WebSocketState.Open) 
            //        {
            //            await webSocket.ReceiveAsync(new ArraySegment<byte>(receiveBuffer), CancellationToken.None);
            //            var text = Encoding.Default.GetString(receiveBuffer);
            //            if (text.StartsWith("close"))
            //            {
            //                var a = true;
            //                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
            //                webSocket.Dispose();
            //                webSocket = null;
            //            }
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    webSocket = null;
            //}
            #endregion
        }

        [HttpGet]
        public async Task CheckStatusWebsocket()
        {
            var context = ControllerContext.HttpContext;
            var isSocketRequest = context.WebSockets.IsWebSocketRequest;

            try
            {
                if (isSocketRequest)
                {
                    var clientId = context.Request.Query["id"];
                    var action = context.Request.Query["action"];
                    var cameraIndex = int.Parse(context.Request.Query["cameraIndex"]);

                    if (action == "check" && _clients.ContainsKey(clientId + cameraIndex.ToString()))
                    {
                        _clients.TryGetValue(clientId + cameraIndex.ToString(), out var client);
                        client.LastCheckAlive = DateTime.Now;
                    }
                }
            }
            catch (Exception ex)
            {
                //_Logger.LogError(ex.ToString());
            }
        }

        [HttpGet]
        public async Task RemoveWebsocketByPageId()
        {
            var context = ControllerContext.HttpContext;
            var isSocketRequest = context.WebSockets.IsWebSocketRequest;

            try
            {
                if (isSocketRequest)
                {
                    var clientId = context.Request.Query["id"];

                    var keysToRemove = new List<string>();
                    foreach (var key in _clients.Keys)
                    {
                        if (key.StartsWith(clientId))
                        {
                            keysToRemove.Add(key);
                        }
                    }
                    foreach (var key in keysToRemove)
                    {
                        _clients.TryRemove(key, out _);
                    }

                    if (_streams != null && _streams.Count > 0)
                    {
                        foreach (var _stream in _streams)
                        {
                            _stream.Value.RemoveAll(x => x.StartsWith(clientId));
                        }
                        if (_streams.Any(x => x.Value == null || (x.Value != null && x.Value.Count == 0)))
                        {
                            foreach (var _stream in _streams)
                            {
                                _usingCameras.RemoveAll(x => (x.IpAddress + x.Port).ToString() == _stream.Key);
                                _tasks.TryRemove(_stream.Key, out _);
                                _streams.TryRemove(_stream);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //_Logger.LogError(ex.ToString());
            }
        }

        public void RemoveWebsocketByCameraIndex(string key)
        {
            var context = ControllerContext.HttpContext;
            var isSocketRequest = context.WebSockets.IsWebSocketRequest;

            try
            {
                if (_streams != null && _streams.Count > 0)
                {
                    foreach (var _stream in _streams)
                    {
                        if (_stream.Key == key)
                        {
                            _usingCameras.RemoveAll(x => (x.IpAddress + x.Port).ToString() == _stream.Key);
                            _tasks.TryRemove(_stream.Key, out _);
                            if (_stream.Value != null && _stream.Value.Count > 0)
                            {
                                foreach (var stream in _stream.Value)
                                {
                                    _clients.TryRemove(stream, out _);
                                }
                            }
                            _streams.TryRemove(_stream);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //_Logger.LogError(ex.ToString());
            }
        }

        bool MatchSOI(byte[] buffer, int i)
        {
            return buffer[i] == SOI[0] && buffer[i + 1] == SOI[1];
        }

        int GetContentLength(string headers)
        {
            int contentLength = -1;

            string[] headerLines = headers.Split('\n');
            foreach (string header in headerLines)
            {
                string[] parts = header.Split(':');
                if (parts[0] == "Content-length")
                {
                    contentLength = int.Parse(parts[1]);
                }
            }

            return contentLength;
        }
    }

    public class WebsocketManager
    {
        public WebSocket Websocket { get; set; }
        public DateTime LastCheckAlive { get; set; }
    }
}
