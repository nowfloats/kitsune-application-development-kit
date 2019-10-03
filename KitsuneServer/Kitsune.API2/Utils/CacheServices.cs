using Kitsune.API.Model.ApiRequestModels;
using Kitsune.API2.DataHandlers.Mongo;
using MongoDB.Bson;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Kitsune.API2.Utils
{
    public class CacheServices
    {
        public bool InvalidateDataCachePerDomain(string domain)
        {
            try
            {
                if (!string.IsNullOrEmpty(domain))
                {
                    HttpClient client = new HttpClient();
                    client.BaseAddress = new Uri(@"https://api2.kitsune.tools/website/v1/");
                    var jsondata = JsonConvert.SerializeObject(new
                    {
                        key = domain.ToUpper()
                    });
                    var result = client.PostAsync(string.Format("RemoveCache?clientId=4C627432590419E9CF79252291B6A3AE25D7E2FF13347C6ACD1587C47C6ACDD"), new StringContent(jsondata, Encoding.UTF8, "application/json"));
                    //  if (result.StatusCode == HttpStatusCode.OK)
                    return true;
                }
            }
            catch (Exception ex)
            {

            }
            return false;
        }

        public bool InvalidateDataCache(string customerId)
        {
            var objectid = new ObjectId();
            if (ObjectId.TryParse(customerId, out objectid))
            {

                var customer = ((WebsiteDetailsResponseModel)MongoConnector.GetKitsuneWebsiteDetails(customerId).Response);
                if (customer != null)
                {
                    var cloudProviderDetails = MongoConnector.GetCloudProviderDetails(customer.ProjectId);
                    if (cloudProviderDetails != null && cloudProviderDetails.provider == Kitsune.Models.Cloud.CloudProvider.AliCloud)
                    {
                        return InvalidateAliCloudCDNCache(customerId, customer.WebsiteUrl, customer.KitsuneProjectVersion.ToString());
                    }
                    else
                    {
                        if (customer.IsSSLEnabled)
                            InvalidateDataCachePerDomain($"https://{customer.WebsiteUrl}");
                        else
                            InvalidateDataCachePerDomain($"http://{customer.WebsiteUrl}");

                        InvalidateDataCachePerDomain($"{customer.WebsiteUrl}");
                    }
                }
            }
            return false;
        }
        
        public bool InvalidateKitsuneCache(string websiteid, string websiteurl, string projectVersion, string projectid)
        {
            try
            {
                MongoConnector.AddCDNCacheInvalidationTask(websiteid);
            }
            catch (Exception ex)
            {

            }
            return false;
        }

        public bool InvalidateProjectCache(string projectid)
        {
            try
            {
                MongoConnector.AddCDNCacheInvalidationTask(projectid);

            }
            catch (Exception ex)
            {

            }
            return false;
        }

        public bool InvalidateAliCloudCDNCache(string websiteid, string websiteurl, string projectVersion)
        {
            try
            {
                string host = websiteurl.ToLower().Replace("http://", "").Replace("https://", "").Split('/')[0];
                string html = string.Empty;
                string url = @"http://" + host + "/k-api/clear-cache";

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.AutomaticDecompression = DecompressionMethods.GZip;

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode != HttpStatusCode.OK)
                    return false;
            }
            catch { }
            //try
            //{
            //    HttpClient client = new HttpClient();
            //    client.BaseAddress = new Uri(@"http://973a605a42324d2b8f9918278262caa6-ap-southeast-1.alicloudapi.com/");
            //    var jsondata = JsonConvert.SerializeObject(new
            //    {
            //        WebsiteId = websiteid,
            //        WebsiteUrl = websiteurl,
            //        ProjectVersion = projectVersion,
            //    });
            //    var result = client.PostAsync("invalidatecache", new StringContent(jsondata, Encoding.UTF8, "application/json")).Result;
            //    if (result.StatusCode == HttpStatusCode.OK)
            //        return true;
            //}
            //catch (Exception ex)
            //{

            //}
            return false;
        }
    }
}
