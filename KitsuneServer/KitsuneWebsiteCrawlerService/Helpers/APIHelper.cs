using Kitsune.Models.Project;
using KitsuneWebsiteCrawlerService.Constants;
using KitsuneWebsiteCrawlerService.Models;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace KitsuneWebsiteCrawlerService.Helpers
{
    public class APIHelper
    {
        private const string updateFaviconUrl = "{0}/api/krawler/v1/UpdateWebsiteDetails";
        private const string UpdateKitsuneProjectStatus = "{0}/api/krawler/v1/updateprojectstatus?projectId={1}&status={2}";
        private const string KrawlingCompletedUpdateKitsuneProjectsAPI = "{0}/api/krawler/v1/updatekrawlcomplete?projectId={1}";
        private const string RegisterEventAPI = "{0}/api/Event/v1/Register";
        private const string ProjectConfigAPI = "{0}/api/Project/v1/ProjectConfig?level=SOURCE&projectId={1}";

        public static bool UpdateWebsiteFaviconUrl(UpdateWebsiteDetails details)
        {
            if (details==null)
                return false;
            
            try
            {
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders
                  .Accept
                  .Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var jsonData = JsonConvert.SerializeObject(details);
                var response = client.PostAsync(new Uri(String.Format(updateFaviconUrl, EnvironmentConstants.ApplicationConfiguration.APIDomain)), new StringContent(jsonData, Encoding.UTF8, "application/json")).Result;
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    throw new Exception("UnAuthorized");
                else if (!((int)response.StatusCode >= 200 && (int)response.StatusCode <= 300))//If status code is not 20X, error.
                {
                    throw new Exception(String.Format("API call unsuccessful : {0}", updateFaviconUrl));
                }
                return true;
            }
            catch(Exception ex)
            {
                Log.Error(ex, String.Format("Error updating the project, ProjectId:{0}", details.ProjectId));
            }
            return false;
        }

        public static bool UpdateKitsuneProjectsStatus(string projectId, ProjectStatus status)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders
                  .Accept
                  .Add(new MediaTypeWithQualityHeaderValue("application/json"));
                
                var response = client.PostAsync(new Uri(String.Format(UpdateKitsuneProjectStatus, EnvironmentConstants.ApplicationConfiguration.APIDomain, projectId,status)),null).Result;
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    throw new Exception("UnAuthorized");
                else if (!((int)response.StatusCode >= 200 && (int)response.StatusCode <= 300))//If status code is not 20X, error.
                {
                    throw new Exception(String.Format("API call unsuccessful : {0}", UpdateKitsuneProjectStatus));
                }
                return true;
            }
            catch(Exception ex)
            {
                Log.Error(ex, String.Format("Error updating the project status, ProjectId:{0}", projectId));
            }
            return false;
        }

        public static bool KrawlingCompletedUpdateKitsuneProjects(string projectId)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders
                  .Accept
                  .Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = client.PostAsync(String.Format(KrawlingCompletedUpdateKitsuneProjectsAPI, EnvironmentConstants.ApplicationConfiguration.APIDomain, projectId),null).Result;
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    throw new Exception("UnAuthorized");
                else if (!((int)response.StatusCode >= 200 && (int)response.StatusCode <= 300))//If status code is not 20X, error.
                {
                    throw new Exception(String.Format("API call unsuccessful : {0}", KrawlingCompletedUpdateKitsuneProjectsAPI));
                }
                return true;
            }
            catch(Exception ex)
            {
                Log.Error(ex, String.Format("Error updating kitsuneproject after crawling completed, ProjectId:{0}", projectId));
            }
            return false;
        }

        public static void RegisterAnalyseCompleteEvent(string projectId)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders
                  .Accept
                  .Add(new MediaTypeWithQualityHeaderValue("application/json"));
                
                CreateEventRequest requestObject = new CreateEventRequest()
                {
                    Event = EventTypeWebEngage.CrawlerAnalysed,
                    ProjectId=projectId
                };
                var jsonData = JsonConvert.SerializeObject(requestObject);
                var response = client.PostAsync(String.Format(RegisterEventAPI, EnvironmentConstants.ApplicationConfiguration.APIDomain, projectId), new StringContent(jsonData, Encoding.UTF8, "application/json")).Result;
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    throw new Exception("UnAuthorized");
                else if (!((int)response.StatusCode >= 200 && (int)response.StatusCode <= 300))//If status code is not 20X, error.
                {
                    throw new Exception(String.Format("API call unsuccessful : {0}", RegisterEventAPI));
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, String.Format("Error updating kitsuneproject after Analyse completed, ProjectId:{0}", projectId));
            }
        }

        public static dynamic GetProjectConfig(string projectId)
        {
            try
            {
                string url = String.Format(ProjectConfigAPI, EnvironmentConstants.ApplicationConfiguration.APIDomain, projectId);

                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("clientId", "394D599C21104007901DE762E9E290B9");
                    HttpResponseMessage response = client.GetAsync(url).Result;
                    if (response.StatusCode.Equals(HttpStatusCode.OK))
                    {
                        using (HttpContent content = response.Content)
                        {
                            // ... Read the string.
                            string result = content.ReadAsStringAsync().Result;
                            if (!String.IsNullOrEmpty(result))
                            { 
                                dynamic resultString = JsonConvert.DeserializeObject(result);
                                var file = resultString["File"];
                                var settingString = file["Content"].ToString();

                                var settingObject = JsonConvert.DeserializeObject(settingString);
                                return settingObject;
                            }
                            else
                                throw new Exception($"Response Content was empty for Url : {url}");
                        }
                    }
                    else
                    {
                        throw new Exception($"Error get the config file with status code : {response.StatusCode}, Url : {url}");
                    }
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public static IncludeStaticAssetAPIModel GetStaticAssetFromAPI(ExternalAPIRequestModel request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (request.EndPoint == null)
                throw new ArgumentNullException(nameof(request.EndPoint));
            
            using (HttpClient client = new HttpClient())
            {
                if (request.Headers != null)
                {
                    foreach (var header in request.Headers)
                    {
                        client.DefaultRequestHeaders.Add(header.Key, header.Value);
                    }
                }

                HttpResponseMessage response = null;
                if(request.Method.Equals(REQUESTMETHOD.GET))
                    response = client.GetAsync(request.EndPoint).Result;
                else if(request.Method.Equals(REQUESTMETHOD.POST))
                    response = client.PostAsync(request.EndPoint,null).Result;
                else
                    response = client.GetAsync(request.EndPoint).Result;

                if (response.StatusCode.Equals(HttpStatusCode.OK))
                {
                    using (HttpContent content = response.Content)
                    {
                        // ... Read the string.
                        string result = content.ReadAsStringAsync().Result;
                        if (!String.IsNullOrEmpty(result))
                        {
                            IncludeStaticAssetAPIModel resultString = JsonConvert.DeserializeObject<IncludeStaticAssetAPIModel>(result);
                            return resultString;
                        }
                        else
                            throw new Exception($"Response Content was empty for Uri : {request.EndPoint}");
                    }
                }
                else
                {
                    throw new Exception($"Error getting Static assets with status code : {response.StatusCode}, Url : {request.EndPoint}");
                }
                
            }
        }

    }
}
