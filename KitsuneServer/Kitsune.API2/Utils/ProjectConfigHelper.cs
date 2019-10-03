using Kitsune.API.Model.ApiRequestModels;
using Kitsune.API2.DataHandlers.Mongo;
using Kitsune.Models.ProjectConfigModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kitsune.API2.Utils
{
    public class ProjectConfigHelper
    {
        private string ProjectId { get; set; }
        private FileLevel FileLevel { get; set; }
        public string ProjectConfigString { get; set; }

        public ProjectConfigHelper(string projectId, FileLevel fileLevel = FileLevel.SOURCE)
        {
            if (String.IsNullOrEmpty(projectId))
                throw new Exception(nameof(projectId));

            this.ProjectId = projectId;
            this.FileLevel = fileLevel;
            InitialiseProjectConfiguration();
        }

        private void InitialiseProjectConfiguration()
        {
            var requestModel = new GetProjectConfigRequestModel
            {
                ProjectId = this.ProjectId,
                Level = this.FileLevel
            };
            var result = MongoConnector.GetProjectConfig(requestModel);
            string fileConfig = result.File.Content;
            this.ProjectConfigString = fileConfig;
        }

        public string GetSourceSyncExcludeRegex()
        {
            try
            {
                var configString = this.ProjectConfigString;
                var configObject = JsonConvert.DeserializeObject<dynamic>(configString);
                var customSourceSyncSettings = configObject["custom_source_sync"];
                if(customSourceSyncSettings!=null)
                {
                    CustomSourceSynModel customSourceSync = JsonConvert.DeserializeObject<CustomSourceSynModel>(customSourceSyncSettings.ToString());
                    if (customSourceSync != null)
                        return customSourceSync.resource_removal_exclude;
                    else
                        return String.Empty;
                }
                else
                {
                    return String.Empty;
                }
            }
            catch(Exception ex)
            {
                return String.Empty;
            }
        }
        

        public bool IsAutoPublishEnabled()
        {
            try
            {
                var configString = this.ProjectConfigString;
                var configObject = JsonConvert.DeserializeObject<dynamic>(configString);
                var customSourceSyncSettings = configObject["custom_source_sync"];
                if (customSourceSyncSettings != null)
                {
                    CustomSourceSynModel customSourceSync = JsonConvert.DeserializeObject<CustomSourceSynModel>(customSourceSyncSettings.ToString());
                    if (customSourceSync != null)
                        return customSourceSync.auto_publish;
                    else
                        return false;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        #region HELPERS

        public static string GetExcludeUrlsRegex(List<string> list)
        {
            try
            {
                List<string> listOfRegex = new List<string>();
                foreach (var str in list)
                {
                    var regex = ConvertKitsuneUrlPatternToRegex(str);
                    listOfRegex.Add(regex);
                }
                string regexString = String.Join("|", listOfRegex.ToArray());
                return regexString;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static string ConvertKitsuneUrlPatternToRegex(string str)
        {
            str = str.Replace(".", "\\.");
            str = str.Replace("*", ".*");
            str = str.Replace("/", "\\/");
            str = $"^{str}$";
            return str;
        }

        #endregion
    }
}
