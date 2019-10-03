using System;
using System.Collections.Generic;
using System.Text;

namespace Kitsune.BasePlugin.Models
{
    public class SMTPConfigurationModel
    {
        public string FromEmailAddress;

        public string ServerHost, ServerPort;
        public int TimeOut;
        public bool EnableSsl = true;

        //Username field is deprecated. Please do not use it
        public string SMTPUserName;

        //This is not required because the fromUserPassword would be used as the SMTP Password.
        public string SMTPUserPassword;

        public Dictionary<string, string> Headers = new Dictionary<string, string>();

        public SMTPConfigurationModel(string serverHost, string serverPort, int timeout, bool enableSsl = true, Dictionary<string, string> headers = null)
        {
            this.ServerHost = serverHost;
            this.ServerPort = serverPort;
            this.TimeOut = timeout;
            this.EnableSsl = enableSsl;

            if (headers != null)
                this.Headers = headers;
        }
    }
    
}
