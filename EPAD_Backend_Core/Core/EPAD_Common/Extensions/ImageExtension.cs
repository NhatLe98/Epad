using EPAD_Data.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;

namespace EPAD_Common.Extensions
{
    public static class ImageExtension
    {
        public static string GetImageFromCamera(string pIp, string pPort, string pUser, string pPass, string pChannel, int pCameraIndex,
            ref string error, int pTimeOut, UserInfo user = null)
        {
            HttpClientHandler handler = new HttpClientHandler();
            handler.Credentials = new NetworkCredential(pUser, pPass);

            HttpClient client = new HttpClient(handler);

            if (pTimeOut > 0)
            {
                client.Timeout = TimeSpan.FromSeconds(pTimeOut);
            }
            string filePath = "";
            error = "";
            DateTime now = DateTime.Now;
            try
            {
                HttpResponseMessage result = client.GetAsync($"http://{pIp}:{pPort}/ISAPI/Streaming/channels/{pChannel}/picture").Result;
                result.EnsureSuccessStatusCode();

                HttpContent content = result.Content;
                var stream = result.Content.ReadAsStreamAsync().Result;
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    stream.CopyTo(memoryStream);

                    Convert.ToBase64String(memoryStream.ToArray());
                }


                Image image = Bitmap.FromStream(stream);
                string path = AppDomain.CurrentDomain.BaseDirectory;
                if (user == null)
                {
                    filePath = $"Files/ImageFromCamera/{pCameraIndex }/{pChannel}/{now.ToString("yyyy-MM-dd")}/{now.ToString("HH")}";
                }
                else
                {
                    filePath = $"Files/ImageFromCamera/CheckImage/{user.UserName}";
                    Directory.Delete(path + filePath);
                }

                if (Directory.Exists(path + filePath) == false)
                {
                    Directory.CreateDirectory(path + filePath);
                }
                filePath += "/" + DateTime.Now.ToString("mmss_ffff") + ".jpg";
                try
                {
                    image.Save(path + filePath);
                }
                catch (Exception ex)
                {
                    filePath = "";
                    error = ex.Message;
                }
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return filePath;
            }

            return filePath;
        }
    }
}
