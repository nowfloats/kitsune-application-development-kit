using Kitsune.BasePlugin.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Kitsune.Models.Cloud;

namespace Kitsune.BasePlugin.ClientConfigurations
{
    public class NowFloatsClientConfig : BasePlugin
    {
        public NowFloatsClientConfig()
        {
            this.PostWebactionUpdateProcessor = ConfigureWebactionUpdateProcessor;

            SetClientName("NowFloats");

            SetClientId("<<sample_custom_client_id>>");

            SetDefaultSMTPGateway(new SMTPConfigurationModel("email-smtp.xxx.com", "587", 10000, true)
            {
                FromEmailAddress = "xxx@xxxx.com",
                SMTPUserName = "XXXXXXXXXXXXXXXXXXXX",
                SMTPUserPassword = "XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX"
            });

            SetDefaultWebsiteUserSMTPGateway(null);

            SetDefaultWebsiteAccessEmail(false);

            SetDefaultCloudConfiguration(new List<BasePluginCloudConfigurationDetails>()
            {
                new BasePluginCloudConfigurationDetails()
                {
                    CloudProviderType = CloudProvider.AWS,
                    DefaultPreConfiguredDomain = ".NOWFLOATS.COM",
                    IsDefaultCloudProvider = true,
                    AccountId = "xxxxxxxxx"
                },
                new BasePluginCloudConfigurationDetails()
                {
                    CloudProviderType = CloudProvider.AWS,
                    DefaultPreConfiguredDomain = ".NOWFLOATS.COM",
                    AccountId = "xxxxxxxxxx"
                },
                new BasePluginCloudConfigurationDetails()
                {
                    CloudProviderType = CloudProvider.AliCloud,
                    DefaultPreConfiguredDomain = ".NOWFLOATS.COM",
                    AccountId = "xxxxxxxxxx"
                }
            });
        }

        protected void ConfigureWebactionUpdateProcessor(dynamic webactionDetails, string actionType, string websiteId, Func<dynamic, bool> callbackFunction)
        {
            try
            {
                HttpClient client = new HttpClient();
                Uri uriResult;
                client.BaseAddress = new Uri(@"https://api2.kitsune.tools/website/");
                if (!Uri.TryCreate(websiteId, UriKind.Absolute, out uriResult))
                {
                    var website = GetDomainFromWebsiteId(websiteId);
                    var newwebsiteUrl = website?.WebsiteUrl;
                    var providerDetails = GetCloudProviderDetails(website?.ProjectId);
                    //1 = alicloud
                    if(providerDetails == 1)
                    {
                        InvalidateAliCloudCDNCache(websiteId, newwebsiteUrl, ((int)(website?.KitsuneProjectVersion ?? 0)).ToString());
                    }
                    else if (!string.IsNullOrEmpty(newwebsiteUrl))
                    {
                        var jsondata = JsonConvert.SerializeObject(new
                        {
                            key = newwebsiteUrl.ToUpper()
                        });

                        var urlResult = client.PostAsync(string.Format("v1/RemoveCache?clientId=xxxxxxxxxxxxxxxxxxxxxxxx"), new StringContent(jsondata, Encoding.UTF8, "application/json")).Result;
                        return;
                    }
                }

            }
            catch (Exception ex)
            {

            }
        }
        public int GetCloudProviderDetails(string projectId)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri("https://api2.kitsune.tools/api/project/");
                var result = client.GetAsync($"v1/GetCloudProviderDetails/?projectId={projectId}?clientid={this.GetClientId()}").Result;
                if (result != null && result.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var cloudProvider = JsonConvert.DeserializeObject<dynamic>(result.Content.ReadAsStringAsync().Result);
                    return (int)cloudProvider.provider;
                }
            }
            catch
            {
            }
            return 0;
        }
        private dynamic GetDomainFromWebsiteId(string websiteId)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri("https://api2.kitsune.tools/api/website/");
                var result = client.GetAsync($"v1/{websiteId}?clientid={this.GetClientId()}").Result;
                if (result != null && result.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var website = JsonConvert.DeserializeObject<dynamic>(result.Content.ReadAsStringAsync().Result);
                    return website;
                }
            }
            catch
            {
            }
            return null;

        }
        public bool InvalidateAliCloudCDNCache(string websiteid, string websiteurl, string projectVersion)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(@"http://973a605a42324d2b8f9918278262caa6-ap-southeast-1.alicloudapi.com/");
                var jsondata = JsonConvert.SerializeObject(new
                {
                    WebsiteId = websiteid,
                    WebsiteUrl = websiteurl,
                    ProjectVersion = projectVersion,
                });
                var result = client.PostAsync("invalidatecache", new StringContent(jsondata, Encoding.UTF8, "application/json")).Result;
                if (result.StatusCode == System.Net.HttpStatusCode.OK)
                    return true;
            }
            catch (Exception ex)
            {

            }
            return false;
        }
    }
}