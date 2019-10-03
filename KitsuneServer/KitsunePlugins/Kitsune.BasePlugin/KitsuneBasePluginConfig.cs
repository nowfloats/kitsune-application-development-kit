using Kitsune.BasePlugin.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Kitsune.Models.Cloud;

namespace Kitsune.BasePlugin
{
    public delegate void PreWebsiteCreationProcessor(dynamic websiteDetails, Func<dynamic, bool> callbackFunction);

    public delegate void PostWebactionUpdateProcessor(dynamic webactionDetails, string websiteurl, string websiteId, Func<dynamic, bool> callbackFunction);

    public class BasePlugin
    {
        private string ClientId = "4C627432590419E9CF79252291B6A3AE25D7E2FF13347C6ACD1587C47C6ACDD";
        private string ClientName = "Kitsune";
        private SMTPConfigurationModel _defaultSMTPGateway { get; set; }
        private SMSConfigurationModel _defaultSMSGateway { get; set; }
        private SMTPConfigurationModel _defaultKAdminSMTPGateway { get; set; }
        public PreWebsiteCreationProcessor PreWebsiteCreateProcessor = null;
        public PostWebactionUpdateProcessor PostWebactionUpdateProcessor = null;

        public List<BasePluginCloudConfigurationDetails> CloudConfigurations = new List<BasePluginCloudConfigurationDetails>();

        private bool? _sendEmailNotificationsToKAdminCustomers { get; set; }

        public BasePlugin()
        {
            this.PostWebactionUpdateProcessor = ConfigureWebactionUpdateProcessor;

            SetDefaultSMTPGateway(new SMTPConfigurationModel("xxxx.xxxx.xxx.xxx", "xxx", 10000, true)
            {
                FromEmailAddress = "xxxxxx@xxxx.xxxx",
                SMTPUserName = "xxxxxxxxxxxxxxxxx",
                SMTPUserPassword = "xxxxxxxxxxxxxxxxx"
            });
            SetDefaultSMSGateway(null);
            SetDefaultWebsiteUserSMTPGateway(new SMTPConfigurationModel("xxxx.xxxx.xxx.xxx", "xxx", 10000, true)
            {
                FromEmailAddress = "xxxxxx@xxxx.xxxx",
                SMTPUserName = "xxxxxxxxxxxxxxxxx",
                SMTPUserPassword = "xxxxxxxxxxxxxxxxx"
            });

            SetDefaultCloudConfiguration(new List<BasePluginCloudConfigurationDetails>()
            {
                new BasePluginCloudConfigurationDetails()
                {
                    CloudProviderType = CloudProvider.AWS,
                    DefaultPreConfiguredDomain = ".GETKITSUNE.COM",
                    IsDefaultCloudProvider = true,
                    AccountId = "xxxxxxxxxxxxxxxxx"
                },
                new BasePluginCloudConfigurationDetails()
                {
                    CloudProviderType = CloudProvider.AliCloud,
                    DefaultPreConfiguredDomain = ".GETKITSUNE-ALICLOUD.COM",
                    AccountId = "xxxxxxxxxxxxxxxxx"
                }
            });
        }

        #region Protected Methods (for Implementation Class to set default values)

        protected void SetDefaultCloudConfiguration(List<BasePluginCloudConfigurationDetails> configurationDetails)
        {
            this.CloudConfigurations = configurationDetails;
        }

        protected void SetClientId(string clientId)
        {
            if (!string.IsNullOrEmpty(clientId))
            {
                this.ClientId = clientId;
            }
        }

        protected void SetClientName(string clientName)
        {
            if (!string.IsNullOrEmpty(clientName))
            {
                this.ClientName = clientName;
            }
        }

        protected void SetDefaultWebsiteAccessEmail(bool sendEmail)
        {
            this._sendEmailNotificationsToKAdminCustomers = sendEmail;
        }

        /// <summary>
        /// all communications to developer will be made by this email configuration
        /// developer email communication - smtp gateway
        /// </summary>
        /// <param name="smtpConfiguration"></param>
        protected void SetDefaultSMTPGateway(SMTPConfigurationModel smtpConfiguration)
        {
            this._defaultSMTPGateway = smtpConfiguration;
        }

        /// <summary>
        /// all communications to k-admin user will be made by this email configuration
        /// k-admin user email communication - smtp gateway
        /// </summary>
        /// <param name="smtpConfiguration"></param>
        protected void SetDefaultWebsiteUserSMTPGateway(SMTPConfigurationModel smtpConfiguration)
        {
            this._defaultKAdminSMTPGateway = smtpConfiguration;
        }

        /// <summary>
        /// all communications to merchant for will be made by this sms configuration
        /// merchant sms communication - sms gateway
        /// </summary>
        /// <param name="smtpConfiguration"></param>
        protected void SetDefaultSMSGateway(SMSConfigurationModel smsConfiguration)
        {
            this._defaultSMSGateway = smsConfiguration;
        }
        #endregion

        #region Public virtual methods (which can be overriden for specific implementation)

        public virtual SMTPConfigurationModel GetEmailConfiguration()
        {
            return this._defaultSMTPGateway;
        }

        public virtual SMTPConfigurationModel GetKAdminEmailConfiguration()
        {
            return this._defaultKAdminSMTPGateway;
        }
        public virtual SMSConfigurationModel GetSMSConfiguration()
        {
            return this._defaultSMSGateway;
        }

        #endregion

        #region Public methods

        public string GetClientId()
        {
            return this.ClientId;
        }

        public string GetClientName()
        {
            return this.ClientName;
        }

        public string GetSubDomain(CloudProvider? cloudProviderType = null)
        {
            if (cloudProviderType.HasValue)
            {
               return this.CloudConfigurations?.Find(x => x.CloudProviderType == cloudProviderType)?.DefaultPreConfiguredDomain;
            }

            return this.CloudConfigurations?.Find(x => x.IsDefaultCloudProvider)?.DefaultPreConfiguredDomain;
        }

        public bool GetIsDefaultWebsiteAccessEmailEnabled()
        {
            return _sendEmailNotificationsToKAdminCustomers ?? true;
        }
        #endregion

        protected dynamic ConfigureWebsiteCreateProcessor(dynamic websiteDetails, Func<string, string, bool> callbackFunction)
        {
            throw new NotImplementedException();
        }

        protected void ConfigureWebactionUpdateProcessor(dynamic webactionDetails, string actionType, string websiteUrl, Func<dynamic, bool> callbackFunction)
        {
            try
            {
                HttpClient client = new HttpClient();
                Uri uriResult;
                client.BaseAddress = new Uri(@"https://api2.kitsune.tools/Website/");
                if (!Uri.TryCreate(websiteUrl, UriKind.Absolute, out uriResult))
                {
                    var newwebsiteUrl = GetDomainFromWebsiteId(websiteUrl);
                    if (!string.IsNullOrEmpty(newwebsiteUrl))
                    {
                        var jsondata = JsonConvert.SerializeObject(new
                        {
                            key = newwebsiteUrl.ToUpper()
                        });

                        var urlResult = client.PostAsync(string.Format("v1/RemoveCache?clientId=4C627432590419E9CF79252291B6A3AE25D7E2FF13347C6ACD1587C47C6ACDD"), new StringContent(jsondata, Encoding.UTF8, "application/json")).Result;
                        return;
                    }
                }
                var result = client.GetAsync($"v2/InvalidateFLMCache?clientId=4C627432590419E9CF79252291B6A3AE25D7E2FF13347C6ACD1587C47C6ACDD&key={websiteUrl.ToUpper()}").Result;
            }
            catch (Exception ex)
            {

            }
        }
        private string GetDomainFromWebsiteId(string websiteId)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri("https://api2.kitsune.tools/api/website/");
                var result = client.GetAsync($"v1/{websiteId}?clientid={this.GetClientId()}").Result;
                if (result != null && result.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var website = JsonConvert.DeserializeObject<dynamic>(result.Content.ReadAsStringAsync().Result);
                    return website.WebsiteUrl;
                }
            }
            catch
            {
            }
            return null;

        }
    }
}
