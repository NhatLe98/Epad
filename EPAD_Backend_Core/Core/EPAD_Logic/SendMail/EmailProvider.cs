using EPAD_Common.Types;
using EPAD_Common.Utility;
using EPAD_Data;
using EPAD_Data.Entities;
using EPAD_Data.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Threading;

namespace EPAD_Logic.SendMail
{
    public class EmailProvider : IEmailProvider
    {
        private readonly IMemoryCache cache;
        private readonly EPAD_Context context;
        private readonly ILogger _logger;

        public EmailProvider(IMemoryCache pCache, EPAD_Context pContext, ILoggerFactory loggerFactory)
        {
            cache = pCache;
            context = pContext;
            _logger = loggerFactory.CreateLogger<EmailProvider>();
        }

        #region SendMailUtility
        public bool SendEmailToMulti(string pMailType, string SubjectMessage, string BodyMessage)
        {
            return SendEmailToMulti(pMailType, SubjectMessage, BodyMessage, "");
        }
        public bool SendEmailToMulti(string pMailType, string SubjectMessage, string BodyMessage, string pMailTo)
        {
            return SendEmailToMulti(pMailType, SubjectMessage, BodyMessage, pMailTo, null);
        }
        public bool SendEmailToMulti(string pMailType, string SubjectMessage, string BodyMessage, string pMailTo, string[] pAttachedFilePaths)
        {
            return SendEmailToMulti(pMailType, SubjectMessage, BodyMessage, pMailTo, pAttachedFilePaths, "");
        }
        public bool SendEmailToMulti(string pMailType, string SubjectMessage, string BodyMessage, string pMailTo, string[] pAttachedFilePaths, string pMailCC)
        {
            try
            {
                var msg = new MailMessage();
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
                catch (Exception ex)
                {
                    _logger.LogError($"pAttachedFilePaths: {ex}");
                }

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
            catch (Exception ex)
            {
                _logger.LogError($"SendEmailToMulti: {ex}");
                return false;
            }
        }

        private void ThreadSendMail(object pParam)
        {
            try
            {
                string SMTP_SERVER = "", SMTP_USERNAME = "", SMTP_PASSWORD = "", SMTP_PORT = "", SMTP_ENABLE_SSL = "", SMTP_SENDER_NAME = "";
                string MAIL_TO_AUTOSYNCHUSER = "", MAIL_WHEN_SHUTDOWN_PROGRAM = "";
                GetMailServerConfig(ref SMTP_SERVER, ref SMTP_USERNAME, ref SMTP_PASSWORD, ref SMTP_PORT, ref SMTP_ENABLE_SSL, ref SMTP_SENDER_NAME, ref MAIL_TO_AUTOSYNCHUSER, ref MAIL_WHEN_SHUTDOWN_PROGRAM);
                object[] pars = (object[])pParam;
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

                try
                {
                    using (SmtpClient client = new SmtpClient
                    {
                        Host = SMTP_SERVER,
                        Port = int.Parse(SMTP_PORT),
                        DeliveryMethod = SmtpDeliveryMethod.Network,
                        //EnableSsl = bool.Parse(SMTP_ENABLE_SSL),
                        //UseDefaultCredentials = false,
                        //Credentials = new System.Net.NetworkCredential(SMTP_USERNAME, SMTP_PASSWORD),
                    })
                    {
                        client.Send(msg);
                    }
                    msg.Attachments.Dispose();
                }
                catch (Exception ex)
                {
                    _logger.LogError($"client.Send: PORT:{SMTP_PORT} USER:{SMTP_USERNAME} PASS:{SMTP_PASSWORD} SERVER:{SMTP_SERVER} SSL:{SMTP_ENABLE_SSL} {ex}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"ThreadSendMail: {ex}");
            }
        }

        public void GetMailServerConfig(ref string SMTP_SERVER, ref string SMTP_USERNAME, ref string SMTP_PASSWORD, ref string SMTP_PORT, ref string SMTP_ENABLE_SSL, ref string SMTP_SENDER_NAME,
            ref string MAIL_TO_AUTOSYNCHUSER, ref string MAIL_WHEN_SHUTDOWN_PROGRAM)
        {
            try
            {
                ConfigObject config = ConfigObject.GetConfig(cache);

                SMTP_SERVER = config.SMTP_SERVER;
                SMTP_USERNAME = config.SMTP_USERNAME;
                SMTP_PASSWORD = config.SMTP_PASSWORD;
                SMTP_PORT = config.SMTP_PORT.ToString();
                SMTP_ENABLE_SSL = config.SMTP_ENABLE_SSL.ToString();
                SMTP_SENDER_NAME = config.SMTP_SENDER_NAME;
            }
            catch (Exception ex)
            {
                return;
            }
        }

        #endregion

        #region SendMailFunction
        private string GetEmailTemplate()
        {
            string template = "";
            string filePath = AppDomain.CurrentDomain.BaseDirectory + "/EmailTemplate/DownloadLog_Email.html";
            if (File.Exists(filePath))
            {
                template = File.ReadAllText(filePath);
            }
            return template;
        }

        private string GetConfigName(string pEventType)
        {
            switch (pEventType)
            {
                case "DOWNLOAD_LOG": return "Tự động tải log";
                case "DELETE_LOG": return "Tự động xóa log";
                case "START_MACHINE": return "Khởi động máy chấm công";
                case "DOWNLOAD_USER": return "Tự động tải thông tin người dùng";
                case "ADD_OR_DELETE_USER": return "Tự động thêm / xóa người dùng";
                case "EMPLOYEE_INTEGRATE": return "Tích hợp nhân sự";
                default: return "Hệ thống xử lý tự động";
            }
        }

        public void SendMailConfigProcessDone(CommandGroup pCmdGroup)
        {
            IC_Config cfg = context.IC_Config.FirstOrDefault(x => x.EventType == pCmdGroup.EventType);
            if (cfg == null) return;
            if (!cfg.SendMailWhenError && !cfg.AlwaysSend) return;
            if (cfg.Email.Trim() == "") return;

            string title = "";
            string body = "";

            if (cfg.SendMailWhenError)
            {
                title = cfg.TitleEmailError;
                body = cfg.BodyEmailError;
            }

            if (cfg.AlwaysSend)
            {
                title = cfg.TitleEmailSuccess;
                body = cfg.BodyEmailSuccess;
            }

            // cau hinh: Gui mail khi xay ra loi = true
            // Luon luon gui mail = false
            // khong xay ra loi => khong gui mail
            if (cfg.SendMailWhenError && !cfg.AlwaysSend && pCmdGroup.Errors.Count == 0) return;

            if (body == "")
            {
                body = GetEmailTemplate();
            }
            body.Replace("{{TITLE}}", title);
            body.Replace("{{DATETIME}}", DateTime.Now.ToString("yyyy-MM-dd"));
            body.Replace("{{CONFIG}}", GetConfigName(pCmdGroup.EventType));
            body.Replace("{{SUCCESS}}", (pCmdGroup.ListCommand.Count - pCmdGroup.ListCommand.Count).ToString());
            body.Replace("{{FAIL}}", pCmdGroup.Errors.Count.ToString());
            body.Replace("{{EXCUTEDTIME}}", pCmdGroup.FinishedTime.ToString("yyyy-MM-dd HH:mm:ss"));

            if (pCmdGroup.Errors.Count > 0)
            {
                body.Replace("id=\"detail\" style=\"display: none;\"", "id=\"detail\" style=\"display: block;\"");
                string tblBody = "";
                foreach (var item in pCmdGroup.Errors)
                {
                    tblBody += "<tr><td>" + item + "</td></tr>";
                }

                body.Replace("{{BODY_ERROR}}", tblBody);
            }



            string mailTo = cfg.Email;

            SendEmailToMulti("", title, body, mailTo);
        }

        public void SendMailConfigProcessDoneGroup(CommandGroup pCmdGroup, string groupName)
        {
            IC_CommandSystemGroup commandSystemGroup = context.IC_CommandSystemGroup.Where(t => t.Index.Equals(Convert.ToInt32(pCmdGroup.ID))).FirstOrDefault();
            var nameGroup = commandSystemGroup.UpdatedUser.Split('_')[2];
            IC_GroupDevice groupDevice = context.IC_GroupDevice.Where(t => t.Name.Equals(nameGroup)).FirstOrDefault();
            IC_ConfigByGroupMachine cfg = context.IC_ConfigByGroupMachine.FirstOrDefault(x => x.EventType == pCmdGroup.EventType && x.GroupDeviceIndex.Equals(groupDevice.Index));

            if (cfg == null) return;
            if (!cfg.SendMailWhenError && !cfg.AlwaysSend) return;
            if (cfg.Email.Trim() == "") return;

            string title = "";
            string body = "";

            if (cfg.SendMailWhenError)
            {
                title = cfg.TitleEmailError;
                body = groupName + ": " + cfg.BodyEmailError;
            }

            if (cfg.AlwaysSend)
            {
                title = cfg.TitleEmailSuccess;
                body = groupName + ": " + cfg.BodyEmailSuccess;
            }

            // cau hinh: Gui mail khi xay ra loi = true
            // Luon luon gui mail = false
            // khong xay ra loi => khong gui mail
            if (cfg.SendMailWhenError && !cfg.AlwaysSend && pCmdGroup.Errors.Count == 0) return;

            if (body == "")
            {
                body = GetEmailTemplate();
            }
            body.Replace("{{TITLE}}", title);
            body.Replace("{{DATETIME}}", DateTime.Now.ToString("yyyy-MM-dd"));
            body.Replace("{{CONFIG}}", GetConfigName(pCmdGroup.EventType));
            body.Replace("{{SUCCESS}}", (pCmdGroup.ListCommand.Count - pCmdGroup.ListCommand.Count).ToString());
            body.Replace("{{FAIL}}", pCmdGroup.Errors.Count.ToString());
            body.Replace("{{EXCUTEDTIME}}", pCmdGroup.FinishedTime.ToString("yyyy-MM-dd HH:mm:ss"));

            if (pCmdGroup.Errors.Count > 0)
            {
                body.Replace("id=\"detail\" style=\"display: none;\"", "id=\"detail\" style=\"display: block;\"");
                string tblBody = "";
                foreach (var item in pCmdGroup.Errors)
                {
                    tblBody += "<tr><td>" + item + "</td></tr>";
                }

                body.Replace("{{BODY_ERROR}}", tblBody);
            }



            string mailTo = cfg.Email;

            SendEmailToMulti("", title, body, mailTo);
        }

        public void SendMailIntegrateEmployeeDone(int pSuccess, int pFail, DateTime pExcutedTime)
        {
            IC_Config cfg = context.IC_Config.FirstOrDefault(x => x.EventType == ConfigAuto.EMPLOYEE_INTEGRATE.ToString());
            if (cfg == null) return;
            if (!cfg.SendMailWhenError && !cfg.AlwaysSend) return;
            if (cfg.Email.Trim() == "") return;

            string title = "";
            string body = "";

            if (cfg.SendMailWhenError)
            {
                title = cfg.TitleEmailError;
                body = cfg.BodyEmailError;
            }

            if (cfg.AlwaysSend)
            {
                title = cfg.TitleEmailSuccess;
                body = cfg.BodyEmailSuccess;
            }

            if (body == "")
            {
                body = GetEmailTemplate();
            }
            body.Replace("{{TITLE}}", title);
            body.Replace("{{DATETIME}}", DateTime.Now.ToString("yyyy-MM-dd"));
            body.Replace("{{CONFIG}}", GetConfigName(ConfigAuto.EMPLOYEE_INTEGRATE.ToString()));
            body.Replace("{{SUCCESS}}", pSuccess.ToString());
            body.Replace("{{FAIL}}", pFail.ToString());
            body.Replace("{{EXCUTEDTIME}}", pExcutedTime.ToString("yyyy-MM-dd HH:mm:ss"));
            string mailTo = cfg.Email;

            SendEmailToMulti("", title, body, mailTo);
        }
        public void SendMailIntegrateLog(IC_Config cfg, int successDB, List<string> errorsDB, int successFile, List<string> errorsFile)
        {
            if (cfg == null) return;
            if (!cfg.SendMailWhenError && !cfg.AlwaysSend) return;
            if (cfg.Email.Trim() == "") return;
            if (errorsDB.Count == 0 && errorsFile.Count == 0 && cfg.SendMailWhenError == true) return;

            string title = "";
            string body = "";

            if (cfg.SendMailWhenError)
            {
                title = cfg.TitleEmailError;
                body = cfg.BodyEmailError;
            }

            if (cfg.AlwaysSend)
            {
                title = cfg.TitleEmailSuccess;
                body = cfg.BodyEmailSuccess;
            }

            if (body == "")
            {
                body = GetEmailTemplate();
            }
            string strError = "";
            string pathWriteText = "";
            for (int i = 0; i < errorsFile.Count; i++)
            {
                strError += errorsFile[i] + "\n";
                if (errorsFile[i].Contains("Path log"))
                {
                    pathWriteText = errorsFile[i].Split('-')[1];
                }
            }

            body = body.Replace("{{TITLE}}", title);
            body = body.Replace("{{DATETIME}}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            body = body.Replace("{{CONFIG}}", GetConfigName(cfg.EventType));
            body = body.Replace("{{SUCCESS}}", successFile.ToString());
            body = body.Replace("{{FAIL}}", errorsFile.Count.ToString());
            if (strError.Contains("Path error"))
            {
                body = "Tích hợp log thành công nhưng không thể ghi vào file vì đường dẫn sai";
            }
            else
            {
                if (errorsFile.Count > 0)
                {
                    body = body.Replace("{{ERRORLIST}}", strError);
                }
            }
            string mailTo = cfg.Email;
            if (string.IsNullOrEmpty(pathWriteText) == false)
            {
                body += "\nĐường dẫn ghi ra file:" + pathWriteText;
            }
            SendEmailToMulti("", title, body, mailTo);
        }
        #endregion
    }

    public interface IEmailProvider
    {
        void SendMailConfigProcessDone(CommandGroup pCmdGroup);
        void SendMailConfigProcessDoneGroup(CommandGroup pCmdGroup, string groupName);
        void SendMailIntegrateEmployeeDone(int pSuccess, int pFail, DateTime pExcutedTime);
        void SendMailIntegrateLog(IC_Config cfg, int successDB,List<string> errorsDB,int successFile, List<string> errorsFile);
        void GetMailServerConfig(ref string SMTP_SERVER, ref string SMTP_USERNAME, ref string SMTP_PASSWORD, ref string SMTP_PORT, ref string SMTP_ENABLE_SSL, ref string SMTP_SENDER_NAME,
            ref string MAIL_TO_AUTOSYNCHUSER, ref string MAIL_WHEN_SHUTDOWN_PROGRAM);


        bool SendEmailToMulti(string pMailType, string SubjectMessage, string BodyMessage, string pMailTo, string[] pAttachedFilePaths, string pMailCC);
        bool SendEmailToMulti(string pMailType, string SubjectMessage, string BodyMessage, string pMailTo, string[] pAttachedFilePaths);
        bool SendEmailToMulti(string pMailType, string SubjectMessage, string BodyMessage, string pMailTo);
        bool SendEmailToMulti(string pMailType, string SubjectMessage, string BodyMessage);
    }

    public static class EmailProviderExtension
    {
        /// <summary>
        /// Add Email provider service to the ICollectionSerrvice
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddEmailProvider(this IServiceCollection services)
        {
            services.AddScoped<IEmailProvider, EmailProvider>();
            return services;
        }
    }
}
