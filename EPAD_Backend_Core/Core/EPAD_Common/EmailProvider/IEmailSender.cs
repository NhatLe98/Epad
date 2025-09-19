using System;
using System.Collections.Generic;
using System.Text;

namespace EPAD_Common.EmailProvider
{
    public interface IEmailSender
    {
        bool SendEmailToMulti(string pMailType, string SubjectMessage, string BodyMessage, string pMailTo, string[] pAttachedFilePaths, string pMailCC);
        bool SendEmailToMulti(string pMailType, string SubjectMessage, string BodyMessage, string pMailTo, string[] pAttachedFilePaths);
        bool SendEmailToMulti(string pMailType, string SubjectMessage, string BodyMessage, string pMailTo);
        bool SendEmailToMulti(string pMailType, string SubjectMessage, string BodyMessage);
        bool SendEmailWithHtmlBody(string pMailType, string SubjectMessage, string BodyMessage, string pMailTo, string pMailCC, Dictionary<string, string> pDicImage);
    }
}
