using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading;
using Microsoft.Extensions.Caching.Memory;
using System.Net.Mime;
using EPAD_Data.Models;

namespace EPAD_Logic.SendMail
{
    public class EmailUltility
    {
        public static bool SendEmailToMulti(string pMailType, string SubjectMessage, string BodyMessage)
        {
            return SendEmailToMulti(pMailType, SubjectMessage, BodyMessage, "");
        }
        public static bool SendEmailToMulti(string pMailType, string SubjectMessage, string BodyMessage, string pMailTo)
        {
            return SendEmailToMulti(pMailType, SubjectMessage, BodyMessage, pMailTo, null);
        }
        public static bool SendEmailToMulti(string pMailType, string SubjectMessage, string BodyMessage, string pMailTo, string[] pAttachedFilePaths)
        {
            return SendEmailToMulti(pMailType, SubjectMessage, BodyMessage, pMailTo, pAttachedFilePaths, "");
        }

        public static bool SendEmailToMulti(string pMailType, string SubjectMessage, string BodyMessage, string pMailTo, string[] pAttachedFilePaths, string pMailCC)
        {
            return SendEmailToMulti(pMailType, SubjectMessage, BodyMessage, pMailTo, pAttachedFilePaths, pMailCC, null);
        }

        public static bool SendEmailToMulti(string pMailType, string SubjectMessage, string BodyMessage, string pMailTo, string[] pAttachedFilePaths, string pMailCC, IMemoryCache cache)
        {
            BodyMessage = RemoveMultipleNewLineInBodyEmail(BodyMessage);
            System.Net.Mail.MailMessage msg = new MailMessage();
            msg.Subject = SubjectMessage;
            msg.Body = BodyMessage.Replace("\n", "<br/>");
            msg.BodyEncoding = System.Text.Encoding.UTF8;
            msg.Priority = MailPriority.High;
            msg.IsBodyHtml = true;
            
            try
            {
                if (pAttachedFilePaths != null)
                {
                    foreach (string file in pAttachedFilePaths) msg.Attachments.Add(new Attachment(file));
                }
            }
            catch { }

            string[] receipt = new string[0];
            string[] cc = new string[0];
            char[] splitChar = new char[2] { ',', ';' };
            receipt = pMailTo.Trim().Split(splitChar);
            cc = pMailCC.Trim().Split(splitChar);
            foreach (string r in receipt)
            {
                if (r.Trim() == "") continue;
                msg.To.Add(r);
            }
            foreach (string r in cc)
            {
                if (r.Trim() == "") continue;
                msg.CC.Add(r);
            }

            object[] pars = new object[3];
            pars[0] = msg;
            pars[1] = pMailType;
            pars[2] = cache;

            //ThreadSendMail(msg);
            Thread thr = new Thread(new ParameterizedThreadStart(ThreadSendMail));
            thr.Start(pars);

            return true;
        }
        public static bool SendEmailWithHtmlBody(string pMailType, string SubjectMessage, string BodyMessage, string pMailTo,  string pMailCC, IMemoryCache cache,
            Dictionary<string,string> pDicImage)
        {
            BodyMessage = RemoveMultipleNewLineInBodyEmail(BodyMessage);
            System.Net.Mail.MailMessage msg = new MailMessage();
            msg.Subject = SubjectMessage;
            msg.Priority = MailPriority.High;
            msg.IsBodyHtml = true;
           

            string[] receipt = new string[0];
            string[] cc = new string[0];
            char[] splitChar = new char[2] { ',', ';' };
            receipt = pMailTo.Trim().Split(splitChar);
            cc = pMailCC.Trim().Split(splitChar);
            foreach (string r in receipt)
            {
                if (r.Trim() == "") continue;
                msg.To.Add(r);
            }
            foreach (string r in cc)
            {
                if (r.Trim() == "") continue;
                msg.CC.Add(r);
            }


            AlternateView htmlView = AlternateView.CreateAlternateViewFromString(BodyMessage, null, "text/html");
            for (int i = 0; i < pDicImage.Count; i++)
            {
                string key = pDicImage.ElementAt(i).Key;
                string filePath = AppDomain.CurrentDomain.BaseDirectory + @"\Files" + @"\Images\" + pDicImage[key];
                //path of image or stream
                LinkedResource imagelink = new LinkedResource(filePath, "image/png");
                imagelink.ContentId = key;
                htmlView.LinkedResources.Add(imagelink);

                
            }

            msg.AlternateViews.Add(htmlView);
            msg.Body = BodyMessage;
            object[] pars = new object[3];
            pars[0] = msg;
            pars[1] = pMailType;
            pars[2] = cache;

            //ThreadSendMail(msg);
            Thread thr = new Thread(new ParameterizedThreadStart(ThreadSendMail));
            thr.Start(pars);

            return true;
        }

        private static AlternateView GetEmbeddedImage(String filePath)
        {
            LinkedResource res = new LinkedResource(filePath);
            res.ContentId = Guid.NewGuid().ToString();
            string htmlBody = @"<img src='cid:" + res.ContentId + @"'/>";
            AlternateView alternateView = AlternateView.CreateAlternateViewFromString(htmlBody, null, MediaTypeNames.Text.Html);
            alternateView.LinkedResources.Add(res);
            return alternateView;
        }


        public static string RemoveMultipleNewLineInBodyEmail(string pBody)
        {
            string value = "";
            value = pBody.Replace("<strong></strong>", "");
            for (int i = 0; i < 4; i++)
            {
                value = value.Replace("<br/><br/><br/>", "<br/><br/>");
            }
            return value;
        }
        static private void ThreadSendMail(object pParam)
        {
            try
            {
                object[] pars = (object[])pParam;
                string SMTP_SERVER = "", SMTP_USERNAME = "", SMTP_PASSWORD = "", SMTP_PORT = "", SMTP_ENABLE_SSL = "", SMTP_SENDER_NAME = "";
                string MAIL_TO_AUTOSYNCHUSER = "", MAIL_WHEN_SHUTDOWN_PROGRAM = "";
                GetMailServerConfig(ref SMTP_SERVER, ref SMTP_USERNAME, ref SMTP_PASSWORD, ref SMTP_PORT, ref SMTP_ENABLE_SSL, ref SMTP_SENDER_NAME, ref MAIL_TO_AUTOSYNCHUSER, ref MAIL_WHEN_SHUTDOWN_PROGRAM, (IMemoryCache)pars[2]);
                System.Net.Mail.MailMessage msg = (System.Net.Mail.MailMessage)pars[0];
                string mailType = (string)pars[1];
                msg.From = new MailAddress(SMTP_USERNAME, SMTP_SENDER_NAME, System.Text.Encoding.UTF8);
                string[] receipt = new string[0];
                char[] splitChar = new char[2] { ',', ';' };

                foreach (string r in receipt)
                {
                    if (r.Trim() == "") continue;
                    msg.To.Add(r);
                }
                if (msg.To.Count == 0) return;
                SmtpClient client = new SmtpClient();
                client.UseDefaultCredentials = false;
                client.Port = int.Parse(SMTP_PORT);
                client.Credentials = new System.Net.NetworkCredential(SMTP_USERNAME, SMTP_PASSWORD);
                client.Host = SMTP_SERVER;
                client.EnableSsl = bool.Parse(SMTP_ENABLE_SSL);
                client.DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network;

                try
                {
                    client.Send(msg);
                    msg.Attachments.Dispose();
                }
                catch (Exception ex)
                {
                    //LogManagement.LogObject.WriteLog("SmartUtility.CommonFunctions", "EmailUltility", "ThreadSendMail", "Error in send mail", ex.ToString());
                }
                //return true;
            }
            catch { }
        }

        private static void GetMailServerConfig(ref string SMTP_SERVER, ref string SMTP_USERNAME, ref string SMTP_PASSWORD, ref string SMTP_PORT, ref string SMTP_ENABLE_SSL, ref string SMTP_SENDER_NAME, ref string MAIL_TO_AUTOSYNCHUSER, ref string MAIL_WHEN_SHUTDOWN_PROGRAM, IMemoryCache cache)
        {
            ConfigObject configObject = ConfigObject.GetConfig(cache);
            SMTP_SERVER = configObject.SMTP_SERVER;
            SMTP_USERNAME = configObject.SMTP_USERNAME;
            SMTP_PASSWORD = configObject.SMTP_PASSWORD;
            SMTP_PORT = configObject.SMTP_PORT.ToString();
            SMTP_ENABLE_SSL = configObject.SMTP_ENABLE_SSL.ToString();
            SMTP_SENDER_NAME = configObject.SMTP_SENDER_NAME;
        }
    }
}
