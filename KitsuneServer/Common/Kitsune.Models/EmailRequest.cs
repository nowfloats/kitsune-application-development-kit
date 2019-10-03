using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kitsune.Models
{
    public class KitsuneEnquiry : MongoEntity
    {
        public string EmailBody { get; set; }
        public string Name { get; set; }
        public string Subject { get; set; }
        public string From { get; set; }
        public int Type { get; set; }
    }

    #region Email Helper

    public class EmailRequestWithAttachments
    {
        public string EmailBody { get; set; }
        public string Name { get; set; }
        public string Subject { get; set; }
        public string From { get; set; }
        public int Type { get; set; }
        public List<string> To { get; set; }
        public List<string> Attachments { get; set; }
        public CustomSMTPConfiguration CustomSMTPConfig { get; set; }
    }

    public enum EmailUserConfigType
    {
        WEBSITE_USER = 0,
        WEBSITE_DEVELOPER = 1
    }

    public class CustomSMTPConfiguration
    {
        public string ServerHost, ServerPort;
        public int TimeOut;
        public bool EnableSsl = true;
        public string SMTPUserName;

        //Username field is deprecated. Please do not use it
        public string UserName;

        //This is not required because the fromUserPassword would be used
        //  as the SMTP Password.
        public string SMTPUserPassword;
    }

    #endregion

    public class KitsuneConvertMailRequest
    {
        public string ClientId { get; set; }
        public string EmailBody { get; set; }
        public string Subject { get; set; }
        public List<string> To { get; set; }
        public string From { get; set; }
        public int Type { get { return 4; } }
        public List<string> Attachments { get; set; }
        public WithFloatsExternalSMTPConfiguration CustomSMTPConfig { get; set; }
    }

    public class WithFloatsExternalSMTPConfiguration
    {
        public string ServerHost, ServerPort;
        public int TimeOut;
        public bool EnableSsl = true;
        public string SMTPUserName;

        //Username field is deprecated. Please do not use it
        public string UserName;

        //This is not required because the fromUserPassword would be used
        //  as the SMTP Password.
        public string SMTPUserPassword;
    }
}
