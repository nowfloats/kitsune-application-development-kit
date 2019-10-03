using System;
using System.Collections.Generic;
using System.Text;

namespace Kitsune.BasePlugin.Models
{
    public class SMSConfigurationModel
    {
        public string UserName;
        public string Password;
        public string FromSMS;
        public string SMSProvider;

        public string ServerUrl;
        
        public SMSConfigurationModel(string userName, 
            string password,
            string fromSMS, 
            string smsProvider, 
            string serverUrl)
        {
            this.UserName = userName;
            this.Password = password;
            this.FromSMS = fromSMS;
            this.SMSProvider = smsProvider;
            this.ServerUrl = serverUrl;
        }
    }

}
