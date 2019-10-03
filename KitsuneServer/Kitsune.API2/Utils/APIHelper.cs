using Kitsune.API2.EnvConstants;
using Kitsune.API2.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Kitsune.API2.Utils
{
    public class APIHelper
    {
        private const string GetWebActionAPI ="https://webactions.kitsune.tools/api/v1/List";

        public static GetWebActionsCommandResult GetWebAction(string userId)
        {
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(String.Format(GetWebActionAPI, userId));
                request.Headers.Add("Authorization", userId);
                request.Method = "GET";
                string data = null;
                var response = request.GetResponse();

                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    data = reader.ReadToEnd();
                }

                GetWebActionsCommandResult domainDetails = JsonConvert.DeserializeObject<GetWebActionsCommandResult>(data);
                return domainDetails;
            }
            catch {
                return null;
            }
        }

        public static void CreateNewtWordPressInstance(string projectId)
        {
            if (String.IsNullOrEmpty(projectId))
                throw new ArgumentNullException(nameof(projectId));

            try
            {
                var api = String.Format(EnvironmentConstants.ApplicationConfiguration.WordPressConfiguration.CreatingNewWordPressInstanceAPI,projectId);
                var request = (HttpWebRequest)WebRequest.Create(api);
                request.Method = "POST";
                var response = request.GetResponseAsync();
            }
            catch (Exception ex)
            {

            }
        }

        private class ScreenShotResultModel
        {
            [JsonProperty("message")]
            public string Message { get; set; }
            [JsonProperty("future_link")]
            public string ScreenShotUrl { get; set; }
            [JsonProperty("code")]
            public int StatusCode { get; set; }
        }

        public static string TakeScreenShotForDemoWebsite(string projectId,Uri uri)
        {
            if (String.IsNullOrEmpty(projectId))
                throw new ArgumentNullException(nameof(projectId));
            if (uri == null)
                throw new ArgumentNullException(nameof(uri));
            try
            {
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders
                  .Accept
                  .Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var requestObject = new { project_id = projectId, url = uri.AbsoluteUri };
                var jsonData = JsonConvert.SerializeObject(requestObject);
                var response = client.PostAsync(EnvironmentConstants.ApplicationConfiguration.ScreenShotAPIUrl,
                    new StringContent(jsonData, Encoding.UTF8, "application/json")).Result;
                
                if(response.StatusCode.Equals(HttpStatusCode.OK))
                {
                    string responseString = String.Empty;
                    var rawResult = response.Content.ReadAsStringAsync().Result;
                    var screenshotDetails = JsonConvert.DeserializeObject<ScreenShotResultModel>(rawResult);
                    
                    if(screenshotDetails.StatusCode.Equals(200))
                    {
                        return screenshotDetails.ScreenShotUrl;
                    }
                    else
                    {
                        throw new Exception($"Error: ScreenShot Service failed to take screenshot for Url:{uri.AbsoluteUri} with " +
                            $"response status code :{screenshotDetails.StatusCode}, Message :{screenshotDetails.Message}");
                    }
                }
                else
                {
                    throw new Exception($"Error: Unable to call screenshot API for Url:{uri.AbsoluteUri} with " +
                            $"response status code :{response.StatusCode}");
                }
            }
            catch(Exception ex)
            {
                //LOG: Error
                throw ex;
            }
        }

        public static string TakeScreenShotForLiveWebsites(LiveWebsiteScreenShotRequestModel websites)
        {
            if (websites == null)
                throw new ArgumentNullException(nameof(websites));
            try
            {
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders
                  .Accept
                  .Add(new MediaTypeWithQualityHeaderValue("application/json"));
                
                var jsonData = JsonConvert.SerializeObject(websites);
                var response = client.PostAsync(EnvironmentConstants.ApplicationConfiguration.ScreenShotAPIUrl,
                    new StringContent(jsonData, Encoding.UTF8, "application/json")).Result;

                if (response.StatusCode.Equals(HttpStatusCode.OK))
                {
                    string responseString = String.Empty;
                    var rawResult = response.Content.ReadAsStringAsync().Result;
                    var screenshotDetails = JsonConvert.DeserializeObject<ScreenShotResultModel>(rawResult);

                    if (screenshotDetails.StatusCode.Equals(200))
                    {
                        return screenshotDetails.ScreenShotUrl;
                    }
                    else
                    {
                        throw new Exception($"Error: ScreenShot Service failed to take screenshot with " +
                            $"response status code :{screenshotDetails.StatusCode}, Message :{screenshotDetails.Message}");
                    }
                }
                else
                {
                    throw new Exception($"Error: Unable to call screenshot API with " +
                            $"response status code :{response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                //LOG: Error
                throw ex;
            }
        }

        public static void CreateProjectRoute(string projectId,bool isDemo=false)
        {
            if (String.IsNullOrEmpty(projectId))
                throw new ArgumentNullException(nameof(projectId));

            try
            {
                var requestType = isDemo ? 0 : 1;

                var temp = String.Empty;
                string routingJsonObject = String.Empty;

                if(isDemo)
                {
                    temp = "{\"ProjectId\":\"" + projectId + "\" , \"IsArchived\" : false  }";
                    
                    routingJsonObject = String.Format("{0}\n{1}\n{2}\n{3}", requestType, projectId, "new_KitsuneResources", temp);
                }   
                else
                {
                    temp = "{\"ProjectId\":\"" + projectId + "\"}";
                    routingJsonObject = String.Format("{0}\n{1}\n{2}\n{3}", requestType, projectId, "new_KitsuneResourcesProduction", temp);
                }

                var url = EnvironmentConstants.ApplicationConfiguration.RoutingAPIDomain + "/update";
                var request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "POST";

                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    streamWriter.Write(routingJsonObject);
                    streamWriter.Flush();
                    streamWriter.Close();
                }

                var response = request.GetResponse();
            }
            catch (Exception ex)
            {
                //LOG: Error
                throw ex;
            }
        }
    }
}
