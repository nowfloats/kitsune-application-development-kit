using AmazonAWSHelpers.SQSQueueHandler;
using Kitsune.API.Model.ApiRequestModels;
using Kitsune.Compiler.BuildService.Helpers;
using Kitsune.Compiler.BuildService.Models;
using Kitsune.Language.Models;
using Kitsune.Models;
using Kitsune.Models.BuildAndRunModels;
using Kitsune.Models.Project;
using Kitsune.Models.PublishModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Serilog;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Reflection;
using ProtoBuf;
using Amazon;

namespace Kitsune.Compiler.BuildService
{
    public class APIHelpers
    {
        static HttpClient client = new HttpClient();
#if DEBUG
        //static string host = "http://localhost:41342/api/";
        static string host = "https://api2.kitsunedev.com/api/";
        //static string host = "https://api2.kitsune.tools/api/";

#else
        static string host = "https://api2.kitsune.tools/api/";
#endif
        static string buildUrl = host + "project/v1/build?user={0}";
        static string projectUpdateUrl = host + "project/v1/project";
        public static bool UpdateBuildStatus(string user, string projectId, int buildVersion)
        {

            var amazonBuildSqsQueueHandler = new AmazonSQSQueueHandlers<CompilerServiceSQSModel>(Program.ServiceConfiguration.GetSection("KitsuneBuildSQSUrl").Value);
            try
            {
                var queueResult = amazonBuildSqsQueueHandler.PushMessageToQueue(new CompilerServiceSQSModel
                {
                    ProjectId = projectId,
                    BuildVersion = buildVersion
                }, Program.ServiceConfiguration.GetSection("AWSAccessKey").Value, Program.ServiceConfiguration.GetSection("AWSSecretKey").Value, RegionEndpoint.GetBySystemName(Program.ServiceConfiguration.GetSection("AWSRegion").Value));

                if (!string.IsNullOrEmpty(queueResult))
                {
                    var jsonData = JsonConvert.SerializeObject(new { ProjectId = projectId, BuildVersion = buildVersion, Stage = BuildStatus.QueuedBuild });

                    var result = client.PostAsync(string.Format(buildUrl, user), new StringContent(jsonData, Encoding.UTF8, "application/json")).Result;
                    //Add to BuildSQS
                    if (result != null && result.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        return true;
                    }
                    else
                    {
                        Log.Error(String.Format("Unable to update the project status : ProjectId {0}", projectId));
                        return false;
                    }
                }
                else
                {
                    Log.Error(String.Format("Push to build queue failed : ProjectId {0}", projectId));
                    return false;
                }
            }
            catch (Exception ex)
            {
                Log.Error(String.Format("Push to build queue failed, Error : {0}", ex.Message), projectId, buildVersion.ToString());
                Console.WriteLine(String.Format("Push to build queue failed, Error : {0}", ex.Message), projectId, buildVersion.ToString());
                return false;
            }
        }
        public static bool UpdateProjectStatus(string user, string projectId, string buildId)
        {
            try
            {
                var jsonData = JsonConvert.SerializeObject(new { ProjectId = projectId, UserEmail = user, ProjectStatus = ProjectStatus.BUILDING });
                var result = client.PostAsync(projectUpdateUrl, new StringContent(jsonData, Encoding.UTF8, "application/json")).Result;
                if (result != null && result.StatusCode == HttpStatusCode.OK)
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                Log.Error(String.Format("UpdateProjectStatus failed for project '{0}': Error : {1}", projectId, ex.Message));
                Console.WriteLine(String.Format("UpdateProjectStatus failed for project '{0}': Error : {1}", projectId, ex.Message));
            }
            return false;
        }
        public static bool UpdateProjectCompilerVersion(string user, string projectId, int compilerVersion)
        {
            try
            {
                var jsonData = JsonConvert.SerializeObject(new { ProjectId = projectId, UserEmail = user, CompilerVersion = compilerVersion });
                var result = client.PostAsync(projectUpdateUrl, new StringContent(jsonData, Encoding.UTF8, "application/json")).Result;
                if (result != null && result.StatusCode == HttpStatusCode.OK)
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                Log.Error(String.Format("UpdateProjectCompilerVersion failed for project '{0}': Error : {1}", projectId, ex.Message));
                Console.WriteLine(String.Format("UpdateProjectCompilerVersion failed for project '{0}': Error : {1}", projectId, ex.Message));
            }
            return false;
        }
        public static bool UpdateProjectErrorStatus(string user, string projectId, int buildVersion, List<BuildError> errors)
        {
            try
            {
                var jsonDataBuild = JsonConvert.SerializeObject(new { ProjectId = projectId, BuildVersion = buildVersion, Stage = BuildStatus.Error, Error = errors });

                var jsonDataProject = JsonConvert.SerializeObject(new { ProjectId = projectId, UserEmail = user, ProjectStatus = ProjectStatus.BUILDINGERROR });

                var resultBuild = client.PostAsync(buildUrl, new StringContent(jsonDataBuild, Encoding.UTF8, "application/json")).Result;
                var resultProject = client.PostAsync(projectUpdateUrl, new StringContent(jsonDataProject, Encoding.UTF8, "application/json")).Result;

                if (resultBuild != null && resultProject != null && resultBuild.StatusCode == HttpStatusCode.OK && resultProject.StatusCode == HttpStatusCode.OK)
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                Log.Error(String.Format("UpdateProjectStatus failed for project '{0}': Error : {1}", projectId, ex.Message));
                Console.WriteLine(String.Format("UpdateProjectStatus failed for project '{0}': Error : {1}", projectId, ex.Message));
            }
            return false;
        }



    }
    public class CompilerAPI
    {
        internal string GetFileFromS3(GetFileFromS3RequestModel fileRequest)
        {
            var response = HttpHelper.PostSync<string>(BuildServiceConstants.GetFileFromS3, fileRequest);
            return response;
        }

        internal GetKitsuneBuildStatusResponseModel GetLastCompletedBuild(string userEmail, string projectId)
        {
            var response = HttpHelper.Get<GetKitsuneBuildStatusResponseModel>(string.Format(BuildServiceConstants.GetLastCompletedBuild, userEmail, projectId));
            return response;
        }

        internal GetProjectDetailsResponseModel GetProjectDetailsApi(string userEmail, string projectId = "null")
        {
            var response = HttpHelper.Get<GetProjectDetailsResponseModel>(string.Format(BuildServiceConstants.GetProjectDetailsQuery, projectId ?? "null", userEmail));
            if (response != null)
                return response;
            return null;
        }

        internal KitsuneProject GetProjectDetailsByClientIdApi(string clientId, string projectId)
        {
            var response = HttpHelper.Get<KitsuneProject>(string.Format(BuildServiceConstants.GetProjectDetailsByClientIdQuery, projectId, clientId));
            if (response != null)
                return response;
            return null;
        }

        internal GetResourceDetailsResponseModel GetProjectResourceDetailsApi(string userEmail, string projectId, string sourcePath)
        {
            var response = HttpHelper.Get<GetResourceDetailsResponseModel>(string.Format(BuildServiceConstants.GetProjectResourceDetailsQuery, projectId, sourcePath, userEmail));
            return response;
        }

        internal bool UpdateBuildStatus(string userEmail, CreateOrUpdateKitsuneStatusRequestModel createOrUpdateKitsuneStatusRequestModel)
        {
            var response = HttpHelper.PostSync<bool>(string.Format(BuildServiceConstants.UpdateBuildStatus, userEmail), createOrUpdateKitsuneStatusRequestModel);
            return response;
        }

        internal string SaveFileContentToS3(SaveFileContentToS3RequestModel resourceContent)
        {
            var response = HttpHelper.PostSync<string>(BuildServiceConstants.SaveFileContentToS3, resourceContent);
            return response;
        }
        internal bool CreateOrUpdateResourceApi(CreateOrUpdateResourceRequestModel resourceModel)
        {
            var regex = Kitsune.Helper.Constants.DynamicFileExtensionRegularExpression;
            var isHTML = regex.IsMatch(resourceModel.SourcePath.ToLower());
            if (resourceModel != null && !string.IsNullOrEmpty(resourceModel.FileContent) && isHTML)
                resourceModel.FileContent = Convert.ToBase64String(Encoding.UTF8.GetBytes(resourceModel.FileContent));
            var response = HttpHelper.PostSync<bool>(BuildServiceConstants.CreateOrUpdateResourceCommand, resourceModel);
            return response;
        }
        internal GetAuditProjectAndResourcesDetailsResponseModel GetAuditProjectAndResourcesDetails(string userEmail, string projectId, int latestProjectVersion)
        {
            var response = HttpHelper.Get<GetAuditProjectAndResourcesDetailsResponseModel>(string.Format(BuildServiceConstants.GetAuditProjectAndResourcesDetails, projectId, latestProjectVersion, userEmail));
            return response;
        }
        public GetPartialPagesDetailsResponseModel GetPartialPagesDetailsApi(string userEmail, string projectId, List<string> sourcePath)
        {
            var response = HttpHelper.Get<GetPartialPagesDetailsResponseModel>(string.Format(BuildServiceConstants.GetPartialPagesDetailsQuery, projectId, sourcePath != null ? string.Join(",", sourcePath) : "", userEmail));
            return response;
        }
    }

    public class UserAPI
    {
        internal string GetUserId(string userEmail)
        {
            var response = HttpHelper.Get<GetUserIdResult>(string.Format(BuildServiceConstants.GetUserIdQuery, userEmail));
            return response?.Id;
        }
    }
    public class LanguageAPI
    {
        public KEntity GetLanguage(string languageid, string userid)
        {
            var response = HttpHelper.Get<KEntity>(string.Format(BuildServiceConstants.GetLanguage, languageid), userid);
            return response;
        }
        public KEntity GetLanguageByClientId(string languageid, string clientid)
        {
            var response = HttpHelper.Get<KEntity>(string.Format(BuildServiceConstants.GetLanguage, languageid, clientid));
            return response;
        }
    }
    public class MetaHelper
    {
        public static T Deserialize<T>(string serializedContent)
        {
            //FileStream fs = new FileStream("C:\\Users\\User\\Downloads\\Practo\\index.html.dat", FileMode.Open);
            try
            {
                //BinaryFormatter formatter = new BinaryFormatter();
                //var parentNode = (T)formatter.Deserialize(fs);

                byte[] bytes = Convert.FromBase64String(serializedContent);
                BinaryFormatter formatter = new BinaryFormatter();

                using (MemoryStream stream = new MemoryStream(bytes))
                {
                    var result = (T)formatter.Deserialize(stream);
                    return result;
                }
            }
            catch (SerializationException e)
            {
                return default(T);
            }
        }
        public static string SerializeMetaClass(object InputClass)
        {
            //FileStream fs = new FileStream("E:\\DataFile.dat", FileMode.Create);
            //BinaryFormatter formatter = new BinaryFormatter();
            try
            {
                //formatter.Serialize(fs, metaClass);

                if (!InputClass.GetType().IsSerializable)
                {
                    return null;
                }

                using (MemoryStream stream = new MemoryStream())
                {
                    new BinaryFormatter().Serialize(stream, InputClass);
                    var serializedString = Convert.ToBase64String(stream.ToArray());
                    return serializedString;
                }
            }
            catch (SerializationException e)
            {
                return null;
            }
        }

        public static string ProtoSerializer(object InputClass)
        {
            try
            {
                if (!InputClass.GetType().IsSerializable)
                {
                    return null;
                }

                using (MemoryStream stream = new MemoryStream())
                {
                    Serializer.Serialize(stream, InputClass);
                    var serializedString = Convert.ToBase64String(stream.ToArray());
                    return serializedString;
                }
            }
            catch (SerializationException e)
            {
                return null;
            }
        }
        public static T ProtoDeserialize<T>(string serializedContent)
        {
            try
            {
                byte[] bytes = Convert.FromBase64String(serializedContent);

                using (MemoryStream stream = new MemoryStream(bytes))
                {
                    var result = Serializer.Deserialize<T>(stream);
                    return result;
                }
            }
            catch (SerializationException e)
            {
                return default(T);
            }
        }
    }
}
