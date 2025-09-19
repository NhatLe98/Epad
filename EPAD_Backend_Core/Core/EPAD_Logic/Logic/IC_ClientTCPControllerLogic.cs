using EPAD_Data.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace EPAD_Logic
{

    public class IC_ClientTCPControllerLogic : IIC_ClientTCPControllerLogic
    {
        //public class ControllerParam
        //{
        //    public string IpAddress { get; set; }
        //    public int Port { get; set; }
        //    public List<int> ListChannel { get; set; }
        //    public bool SetOn { get; set; }
        //    public int SecondsNumberOff { get; set; }
        //}

        public async Task<string> SendCommandToDevice(TcpClient tcpClient, List<int> pListChannel, bool pOn)
        {
            if (tcpClient.Connected == false) return "Relay Controller Not Connected";

            List<int> listCheck = new List<int>();
            string onOff = pOn == true ? "on" : "off";
            string sendData = "{";
            for (int i = 0; i < pListChannel.Count; i++)
            {
                if (listCheck.Contains(pListChannel[i]))
                    continue;

                if (pListChannel[i] > 0 && pListChannel[i] <= 8)
                {
                    sendData += $"\"relay{pListChannel[i]}\":\"{onOff}\",";
                    listCheck.Add(pListChannel[i]);
                }
            }

            if (sendData.EndsWith(","))
                sendData = sendData.Substring(0, sendData.Length - 1);
            
            sendData += "}";

            NetworkStream stream = tcpClient.GetStream();
            string returnData = "";
            string error = "";
            //send request
            try
            {
                byte[] buffer = Encoding.ASCII.GetBytes(sendData);
                stream.Write(buffer, 0, buffer.Length);
                stream.Flush();

                // receive 
                buffer = new byte[2048];
                stream.Read(buffer, 0, buffer.Length);

                returnData = Encoding.ASCII.GetString(buffer);


                if (returnData.Contains("ok") == false)
                    error = "Relay Controller return error";
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }

            return error;
        }

        public string SendCommandOnOrOffToDevice(TcpClient tcpClient, List<ChannelParam> pListChannel)
        {
            if (tcpClient.Connected == false) return "Relay Controller Not Connected";

            List<int> listCheck = new List<int>();
            //string onOff = pOn == true ? "on" : "off";
            string sendData = "{";
            for (int i = 0; i < pListChannel.Count; i++)
            {
                var onOff = pListChannel[i].ChannelStatus == true ? "on" : "off";
                if (listCheck.Contains(pListChannel[i].Index))
                {
                    continue;
                }

                if (pListChannel[i].Index > 0 && pListChannel[i].Index <= 8)
                {
                    sendData += $"\"relay{pListChannel[i].Index}\":\"{onOff}\",";
                    listCheck.Add(pListChannel[i].Index);
                }
            }

            if (sendData.EndsWith(","))
            {
                sendData = sendData.Substring(0, sendData.Length - 1);
            }
            sendData += "}";

            NetworkStream stream = tcpClient.GetStream();
            string returnData = "";
            string error = "";
            //send request
            try
            {
                byte[] buffer = Encoding.ASCII.GetBytes(sendData);
                stream.Write(buffer, 0, buffer.Length);
                stream.Flush();

                // receive 
                buffer = new byte[2048];
                stream.Read(buffer, 0, buffer.Length);

                returnData = Encoding.ASCII.GetString(buffer);


                if (returnData.Contains("ok") == false)
                {
                    error = "Relay Controller return error";
                }
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }

            return error;
        }

        public ControllerParam SendCommandGetChannelStatus(TcpClient tcpClient, ControllerParam controllerParam)
        {
            if (tcpClient.Connected == false)
                return null;

            List<int> listCheck = new List<int>();
            string sendData = "{";
            sendData += $"\"get\":\"relayStatus\"";
            sendData += "}";

            NetworkStream stream = tcpClient.GetStream();
            string returnData = "";
            string error = "";
            //send request
            try
            {
                byte[] buffer = Encoding.ASCII.GetBytes(sendData);
                stream.Write(buffer, 0, buffer.Length);
                stream.Flush();

                // receive 
                buffer = new byte[2048];
                stream.Read(buffer, 0, buffer.Length);

                returnData = Encoding.ASCII.GetString(buffer);

                var listStatus = new List<ChannelParam>();
                var listRelay = JsonConvert.DeserializeObject<RelayDTO>(returnData);

                if (listRelay != null)
                {
                    controllerParam.ListChannel[0].ChannelStatus = listRelay.relay1 == "on" ? true : false;
                    controllerParam.ListChannel[1].ChannelStatus = listRelay.relay2 == "on" ? true : false;
                    controllerParam.ListChannel[2].ChannelStatus = listRelay.relay3 == "on" ? true : false;
                    controllerParam.ListChannel[3].ChannelStatus = listRelay.relay4 == "on" ? true : false;
                    controllerParam.ListChannel[4].ChannelStatus = listRelay.relay5 == "on" ? true : false;
                    controllerParam.ListChannel[5].ChannelStatus = listRelay.relay6 == "on" ? true : false;
                    controllerParam.ListChannel[6].ChannelStatus = listRelay.relay7 == "on" ? true : false;
                    controllerParam.ListChannel[7].ChannelStatus = listRelay.relay8 == "on" ? true : false;
                }
                return controllerParam;
            }
            catch (Exception ex)
            {
               
                error = ex.Message;
                return null;
            }

        }

        public async Task<string> SetOnAndAutoOffController(string pIpAddress, int pPort, List<int> pListChannel, double pSecondsNumberOff)
        {
            string error = "";
            int retry = 1;
            do
            {
                if (retry > 1) await Task.Delay(1000);
                using (TcpClient tcpClient = new TcpClient())
                {
                    try
                    {
                        var result =  tcpClient.BeginConnect(pIpAddress, pPort, null, null);
                        var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(1));

                        if (!success)
                            throw new Exception("Failed to connect Relay Controller.");

                        tcpClient.EndConnect(result);

                        // set on
                        error = await SendCommandToDevice(tcpClient, pListChannel, true);
                        if (error != "") continue;

                        // Close after waiting time
                         SetAutoOffController(tcpClient, pListChannel, pSecondsNumberOff);
                    }
                    catch (Exception ex)
                    {
                        error = ex.Message;
                    }
                    finally
                    {
                        tcpClient.Close();
                    }
                }
                retry++;
            } while (error != "" && retry < 5);

            return error;
        }

        // 1 lần gửi tín hiệu SetOnOff thì đèn sáng rồi lại tắt
        public async Task<string> SetOnOffController(string pIpAddress, int pPort, List<int> pListChannel, bool pSetOn)
        {
            string error = "";
            int retry = 1;
            do
            {
                using (var tcpClient = new TcpClient())
                {
                    try
                    {
                        error += "Step 1";
                        var result = tcpClient.BeginConnect(pIpAddress, pPort, null, null);
                        var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(1));

                        if (!success)
                            return "Failed to connect Relay Controller.";
                        
                        tcpClient.EndConnect(result);

                        //error += "Step 2";
                        // set on
                        error = await SendCommandToDevice(tcpClient, pListChannel, true);

                        //error += "Step 3";

                        if (error != "") continue;

                        // Close the door after 1s
                        error = await SetAutoOffController(tcpClient, pListChannel, 1);
                        //error += "Step 4";
                    }
                    catch (Exception ex)
                    {
                        error = ex.Message;
                    }
                    finally
                    {
                        tcpClient.Close();
                    }
                }
                retry++;
            } while (error != "" && retry < 5);

            return error;
        }

        private async Task<string> SetAutoOffController(TcpClient tcpClient, List<int> listChannel, double numberOfSeconds)
        {
            //var delay = Task.Delay(numberOfSeconds * 1000);
            numberOfSeconds = numberOfSeconds * 1000;
            Thread.Sleep((int)numberOfSeconds);
            return await SendCommandToDevice(tcpClient, listChannel, false);
        }

        public string SetOnOrOffController(ControllerParam controllerParam)
        {
            string error = "";
            int retry = 1;
            do
            {
                using (TcpClient tcpClient = new TcpClient())
                {
                    try
                    {
                        var result = tcpClient.BeginConnect(controllerParam.IPAddress, controllerParam.Port, null, null);
                        var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(1));

                        if (!success)
                        {
                            throw new Exception("Failed to connect Relay Controller.");
                        }
                        tcpClient.EndConnect(result);

                        // set on
                        error = SendCommandOnOrOffToDevice(tcpClient, controllerParam.ListChannel);
                        if (error != "") continue;

                    }
                    catch (Exception ex)
                    {
                        error = ex.Message;
                    }
                    finally
                    {
                        tcpClient.Close();
                    }
                }
                retry++;
            } while (error != "" && retry < 5);
            return error;
        }

        public ControllerParam GetChannelStatus(ControllerParam controllerParam)
        {

            string error = "";

            using (TcpClient tcpClient = new TcpClient())
            {
                try
                {
                    var result = tcpClient.BeginConnect(controllerParam.IPAddress, controllerParam.Port, null, null);
                    var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(1));

                    if (!success)
                    {
                        throw new Exception("Failed to connect Relay Controller.");
                    }
                    tcpClient.EndConnect(result);

                    // get status
                    controllerParam = SendCommandGetChannelStatus(tcpClient, controllerParam);

                }
                catch (Exception ex)
                {
                    error = ex.Message;
                }
                finally
                {
                    tcpClient.Close();
                }
            }


            return controllerParam;
        }

    }
    public interface IIC_ClientTCPControllerLogic
    {

        Task<string> SendCommandToDevice(TcpClient tcpClient, List<int> pListChannel, bool pOn);
        string SetOnOrOffController(ControllerParam controllerParam);
        ControllerParam GetChannelStatus(ControllerParam controllerParam);
        Task<string> SetOnOffController(string pIpAddress, int pPort, List<int> pListChannel, bool pSetOn);
        Task<string> SetOnAndAutoOffController(string pIpAddress, int pPort, List<int> pListChannel, double pSecondsNumberOff);
    }
}
