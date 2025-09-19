using EPAD_Data;
using EPAD_Data.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading;

namespace EPAD_Common.EmailProvider
{
    class SmtpConfiguration
    {
        public string SMTP_SERVER { get; set; }
        public string SMTP_USERNAME { get; set; }
        public string SMTP_PASSWORD { get; set; }
        public string SMTP_SENDER_NAME { get; set; }
        public int SMTP_PORT { get; set; } = 587;
        public bool SMTP_ENABLE_SSL { get; set; } = true;
    }
    public class SmtpEmailSender : IEmailSender
    {
        private readonly ILogger Logger;
        private readonly ILoggerFactory LoggerFactory;

        IMemoryCache _Cache;
        ConfigObject _Config;
        private string _configClientName;
        private IConfiguration _Configuration;

        private readonly SmtpConfiguration _SmtpConfig;
        public SmtpEmailSender(IConfiguration configuration, ILoggerFactory loggerFactory, IServiceProvider provider)
        {
            _Configuration = configuration;
            LoggerFactory = loggerFactory;
            Logger = LoggerFactory.CreateLogger<SmtpEmailSender>();
            _Cache = provider.GetService<IMemoryCache>();
            _Config = ConfigObject.GetConfig(_Cache);
            _SmtpConfig = new SmtpConfiguration();
            _SmtpConfig.SMTP_SERVER = _Config.SMTP_SERVER;
            _SmtpConfig.SMTP_USERNAME = _Config.SMTP_USERNAME;
            _SmtpConfig.SMTP_PASSWORD = _Config.SMTP_PASSWORD;
            _SmtpConfig.SMTP_PORT = _Config.SMTP_PORT;
            _SmtpConfig.SMTP_SENDER_NAME = _Config.SMTP_SENDER_NAME;
            _SmtpConfig.SMTP_ENABLE_SSL = _Config.SMTP_ENABLE_SSL;
            _configClientName = _Configuration.GetValue<string>("ClientName").ToUpper();
        }

        private void ThreadSendMail(object pParam)
        {
            try
            {
                object[] pars = (object[])pParam;
                System.Net.Mail.MailMessage msg = (System.Net.Mail.MailMessage)pars[0];
                string mailType = (string)pars[1];
                msg.From = new MailAddress(_SmtpConfig.SMTP_USERNAME, _SmtpConfig.SMTP_SENDER_NAME, System.Text.Encoding.UTF8);
                string[] receipt = new string[0];
                char[] splitChar = new char[2] { ',', ';' };

                foreach (string r in receipt)
                {
                    if (r.Trim() == "") continue;
                    msg.To.Add(r);
                }
                if (msg.To.Count == 0) return;

                try
                {
                    if(_configClientName == ClientName.PSV.ToString())
                    {
                        using (SmtpClient client = new SmtpClient
                        {
                            Host = _SmtpConfig.SMTP_SERVER,
                            Port = _SmtpConfig.SMTP_PORT,
                            DeliveryMethod = SmtpDeliveryMethod.Network,
                            //EnableSsl = _SmtpConfig.SMTP_ENABLE_SSL,
                            //UseDefaultCredentials = false,
                            //Credentials = new System.Net.NetworkCredential(_SmtpConfig.SMTP_USERNAME, _SmtpConfig.SMTP_PASSWORD)
                        })
                        {
                            client.Send(msg);
                        }
                        msg.Attachments.Dispose();
                    }
                    else
                    {
                        using (SmtpClient client = new SmtpClient
                        {
                            Host = _SmtpConfig.SMTP_SERVER,
                            Port = _SmtpConfig.SMTP_PORT,
                            DeliveryMethod = SmtpDeliveryMethod.Network,
                            EnableSsl = _SmtpConfig.SMTP_ENABLE_SSL,
                            UseDefaultCredentials = false,
                            Credentials = new System.Net.NetworkCredential(_SmtpConfig.SMTP_USERNAME, _SmtpConfig.SMTP_PASSWORD)
                        })
                        {
                            client.Send(msg);
                        }
                        msg.Attachments.Dispose();
                    }
                  
                }
                catch (Exception ex)
                {
                    msg.Attachments.Dispose();
                    Logger.LogError("Error when send mail: " + ex.Message + " \n" + ex.StackTrace);
                }
            }
            catch (Exception e)
            {
                Logger.LogError("Send Mail Error: " + e.Message + " \n" + e.StackTrace);
            }
        }

        public bool SendEmailToMulti(string pMailType, string SubjectMessage, string BodyMessage, string pMailTo, string[] pAttachedFilePaths, string pMailCC)
        {
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
            object[] pars = new object[2];
            pars[0] = msg;
            pars[1] = pMailType;

            Thread thr = new Thread(new ParameterizedThreadStart(ThreadSendMail));
            thr.Start(pars);

            return true;
        }

        public bool SendEmailToMulti(string pMailType, string SubjectMessage, string BodyMessage, string pMailTo, string[] pAttachedFilePaths)
        {
            return SendEmailToMulti(pMailType, SubjectMessage, BodyMessage, pMailTo, pAttachedFilePaths, "");
        }

        public bool SendEmailToMulti(string pMailType, string SubjectMessage, string BodyMessage, string pMailTo)
        {
            return SendEmailToMulti(pMailType, SubjectMessage, BodyMessage, pMailTo, null);
        }

        public bool SendEmailToMulti(string pMailType, string SubjectMessage, string BodyMessage)
        {
            return SendEmailToMulti(pMailType, SubjectMessage, BodyMessage, "");
        }

        public bool SendEmailWithHtmlBody(string pMailType, string SubjectMessage, string BodyMessage, string pMailTo, string pMailCC, Dictionary<string, string> pDicImage)
        {

            System.Net.Mail.MailMessage msg = new MailMessage();
            msg.Subject = SubjectMessage;
            msg.Priority = MailPriority.High;
            msg.BodyEncoding = System.Text.Encoding.UTF8;
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
                string filePath = AppDomain.CurrentDomain.BaseDirectory + @"StaticFiles/Images/" + pDicImage[key];
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

            //ThreadSendMail(msg);
            Thread thr = new Thread(new ParameterizedThreadStart(ThreadSendMail));
            thr.Start(pars);

            return true;
        }
    }
}
