
using Kitsune.API2.EnvConstants;
using Kitsune.Models;
using Kitsune.Models.Krawler;
using Kitsune.Models.Project;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using ProtoBuf;
using AmazonAWSHelpers.SQSQueueHandler;
using Kitsune.API2.DataHandlers.Mongo;
using Kitsune.API.Model.ApiRequestModels;
using Kitsune.Language.Models;

namespace Kitsune.API2.Utils
{
    public class Helpers
    {
        public static List<KitsuneResource> CreateKitsuneResource(List<AssetDetails> resources, string projectId, ResourceType type)
        {
            try
            {
                List<KitsuneResource> kitsuneResources = new List<KitsuneResource>();
                DateTime dateTime;
                foreach (var resource in resources)
                {
                    if (resource.NewUrl != null && resource.ResponseStatusCode.Equals(HttpStatusCode.OK))
                    {
                        dateTime = DateTime.UtcNow;
                        KitsuneResource newResource = new KitsuneResource
                        {
                            CreatedOn = dateTime,
                            UpdatedOn = dateTime,
                            SourcePath = resource.NewUrl,
                            UrlPattern = resource.NewUrl,
                            ResourceType = type,
                            Version = 1,
                            ProjectId = projectId,
                            IsArchived = false,
                            IsStatic = type.Equals(ResourceType.LINK) ? false : true
                        };
                        if (resource.NewUrl.Equals("/index.html"))
                            newResource.IsDefault = true;
                        kitsuneResources.Add(newResource);
                    }
                }
                return kitsuneResources;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static string GenerateUrl(string url)
        {
            try
            {
                Uri uri = null;
                string userAgent = "KitsuneBot/1.0; +https://getkitsune.com";
                if (Uri.TryCreate(url, UriKind.Absolute, out uri))
                {
                    try
                    {
                        var request = (HttpWebRequest)WebRequest.Create(uri);
                        request.Method = "GET";
                        request.Accept = "*/*";
                        request.UserAgent = userAgent;
                        request.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
                        var response = (HttpWebResponse)request.GetResponse();

                        if (response.StatusCode.Equals(HttpStatusCode.OK))
                        {
                            var responseDomain = response.ResponseUri.Host;
                            if (responseDomain.Equals(uri.Host, StringComparison.InvariantCultureIgnoreCase) ||
                                responseDomain.Equals("www." + uri.Host, StringComparison.InvariantCultureIgnoreCase))
                            {
                                return uri.AbsoluteUri;
                            }
                            else
                                throw new Exception("please enter a valid url");
                        }
                        else
                        {
                            throw new Exception("please enter a valid url");
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("error while hitting the url");
                    }
                }
                else
                {
                    Uri newUri = null;
                    try
                    {
                        #region Check with https

                        newUri = new Uri("http://" + url);
                        var request = (HttpWebRequest)WebRequest.Create(newUri);
                        request.Method = "GET";
                        request.Accept = "*/*";
                        request.UserAgent = userAgent;
                        HttpWebResponse response = null;
                        try
                        {
                            response = (HttpWebResponse)request.GetResponse();
                        }
                        catch { }

                        #endregion

                        if (response != null && response.StatusCode.Equals(HttpStatusCode.OK))
                        {
                            var responseDomain = response.ResponseUri.Host;
                            if (responseDomain.Equals(newUri.Host, StringComparison.InvariantCultureIgnoreCase) ||
                                responseDomain.Equals("www." + newUri.Host, StringComparison.InvariantCultureIgnoreCase))
                            {
                                return newUri.AbsoluteUri;
                            }
                            else
                                throw new Exception("please enter a valid url");
                        }
                        else
                        {
                            #region Check with http

                            newUri = new Uri("https://" + url);
                            request = (HttpWebRequest)WebRequest.Create(newUri);
                            request.Method = "GET";
                            request.Accept = "*/*";
                            request.UserAgent = userAgent;
                            request.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
                            try
                            {
                                response = (HttpWebResponse)request.GetResponse();
                            }
                            catch(Exception ex)
                            {
                                throw new Exception("please enter a valid url");
                            }

                            #endregion

                            if (response != null && response.StatusCode == HttpStatusCode.OK)
                            {
                                var responseDomain = response.ResponseUri.Host;
                                if (responseDomain.Equals(newUri.Host, StringComparison.InvariantCultureIgnoreCase) ||
                                    responseDomain.Equals("www." + newUri.Host, StringComparison.InvariantCultureIgnoreCase))
                                {
                                    return newUri.AbsoluteUri;
                                }
                                else
                                    throw new Exception("please enter a valid url");
                            }
                            else
                            {
                                throw new Exception();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("please enter a valid url");
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static Uri GetDemoUri(string websiteId)
        {
            if (String.IsNullOrEmpty(websiteId))
                throw new ArgumentNullException(nameof(websiteId));

            if (Uri.TryCreate($"https://{websiteId}{EnvConstants.Constants.KitsuneIdentifierKitsuneDemoDomain}?hideHeader",UriKind.Absolute, out Uri uri))
            {
                return uri;
            }
            else
            {
                throw new Exception($"Error: Creating Uri for websiteId:{websiteId}");
            }
        }

        public static T Deserialize<T>(string serializedContent)
        {
            try
            {
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

        #region Wallet Related
        public static double GetNetWalletBalance(Wallet wallet)
        {
            double netBalance = wallet.Balance - wallet.UnbilledUsage;
            return netBalance;
        }

        internal static void PopulateDefaultAliCloudCreds(CloudProviderModel providerModel)
        {
            providerModel.accountId = "5314231824462312";
            providerModel.key = "LTAIH7MQsCO6USMu";
            providerModel.secret = "1DgY7IQgTtkLC6QqV6K6VG0AsVoOiq";
            providerModel.region = "ap-southeast-1";
        }
        #endregion

        public static List<PropertyPathSegment> ExtractPropertiesFromPath(string propertyPath, KEntity entity)
        {
            List<PropertyPathSegment> kProperties = new List<PropertyPathSegment>();

            var objectPathArray = propertyPath.ToLower().Split('.');
            var obClass = new KClass();
            var obProperty = new KProperty();
            var dataTypeClasses = new string[] { "str", "date", "number", "boolean", "kstring" };
            var currentProperty = string.Empty;
            var arrayRegex = new System.Text.RegularExpressions.Regex(@".*\[(\d+)\]", System.Text.RegularExpressions.RegexOptions.Compiled);
            var functionRegex = new System.Text.RegularExpressions.Regex(@"(\w+)\((.*)\)", System.Text.RegularExpressions.RegexOptions.Compiled);
            int? arrayIndex = 0;
            int tempIndex = 0;

            System.Text.RegularExpressions.Match arrayMatch = null;
            System.Text.RegularExpressions.Match functionMatch = null;
            for (var i = 0; i < objectPathArray.Length; i++)
            {
                currentProperty = objectPathArray[i];
                arrayMatch = arrayRegex.Match(currentProperty);
                arrayIndex = null;
                if (arrayMatch != null && arrayMatch.Success)
                {
                    if (int.TryParse(arrayMatch.Groups[1].Value, out tempIndex))
                    {
                        arrayIndex = tempIndex;
                    }
                    currentProperty = currentProperty.Substring(0, currentProperty.IndexOf('['));
                }

                if (i == 0)
                {
                    obClass = entity.Classes.FirstOrDefault(x => x.ClassType == KClassType.BaseClass && x.Name.ToLower() == currentProperty);
                    if (obClass != null)
                        kProperties.Add(new PropertyPathSegment { PropertyDataType = obClass.Name.ToLower(), PropertyName = currentProperty, Type = PropertyType.obj });
                }
                else
                {
                    obProperty = obClass.PropertyList.FirstOrDefault(x => x.Name.ToLower() == currentProperty);
                    if (obProperty != null)
                    {
                        if ((obProperty.Type == PropertyType.array && !dataTypeClasses.Contains(obProperty.DataType?.Name?.ToLower())) || obProperty.Type == PropertyType.obj || obProperty.Type == PropertyType.kstring || obProperty.Type == PropertyType.phonenumber)
                        {
                            kProperties.Add(new PropertyPathSegment
                            {
                                PropertyName = obProperty.Name.ToLower(),
                                PropertyDataType = obProperty.DataType.Name.ToLower(),
                                Index = arrayIndex,
                                Type = obProperty.Type
                            });

                            obClass = entity.Classes.FirstOrDefault(x => x.Name?.ToLower() == obProperty.DataType?.Name?.ToLower());
                        }
                        else
                        {
                            kProperties.Add(new PropertyPathSegment
                            {
                                PropertyName = obProperty.Name.ToLower(),
                                PropertyDataType = obProperty.DataType.Name.ToLower(),
                                Index = arrayIndex,
                                Type = obProperty.Type
                            });
                        }
                    }
                    else
                    {
                        functionMatch = functionRegex.Match(currentProperty);
                        if (functionMatch.Success)
                        {
                            kProperties.Add(new PropertyPathSegment
                            {
                                PropertyName = functionMatch.Groups[1].Value,
                                PropertyDataType = "function",
                                Type = PropertyType.function
                            });
                        }
                    }

                }
            }
            return kProperties;
        }
    }

    public class CommonHelpers
    {
        internal static string PushSitemapGenerationTaskToSQS(SitemapGenerationTaskModel sitemapModel)
        {
            try
            {
                if (sitemapModel == null || String.IsNullOrEmpty(sitemapModel.WebsiteId))
                    return null;

                if (String.IsNullOrEmpty(sitemapModel.ProjectId))
                {
                    sitemapModel.ProjectId = MongoConnector.GetProjectIdFromWebsiteId(sitemapModel.WebsiteId);
                    if (String.IsNullOrEmpty(sitemapModel.ProjectId))
                        return null;
                }

                if (String.IsNullOrEmpty(sitemapModel.WebsiteUrl))
                {
                    //exclude paths
                    //Handle exception for config
                   // var websiteConfig = MongoConnector.GetProjectConfig(new GetProjectConfigRequestModel() { ProjectId = sitemapModel.ProjectId });
                    sitemapModel.WebsiteUrl = MongoConnector.GetWebsiteUrlFromWebsiteId(sitemapModel.WebsiteId)?.ToLower();
                }

                //To-Do fetch the entire config from db or baseplugin
                var requestModel = new SitemapGenerationTaskQueueModel()
                {
                    param = new SitemapGenerationTaskParameterModel()
                    {
                        access_key = EnvironmentConstants.ApplicationConfiguration.AWSSQSConfiguration.AWS_AccessKey,
                        secret = EnvironmentConstants.ApplicationConfiguration.AWSSQSConfiguration.AWS_SecretKey,
                        region = "ap-south-1",
                        acl = "public-read",
                        bucket = "kitsune-resource-production",
                        key = $"{sitemapModel.ProjectId}/websiteresources/{sitemapModel.WebsiteId}/sitemap.xml",
                        url = sitemapModel.WebsiteUrl
                    },
                    type = 2
                };

                var sqsHanlder = new AmazonSQSQueueHandlers<SitemapGenerationTaskQueueModel>(AmazonAWSConstants.SitemapServiceSQSUrl);
                return sqsHanlder.PushMessageToQueue(requestModel, EnvironmentConstants.ApplicationConfiguration.AWSSQSConfiguration.AWS_AccessKey,
                    EnvironmentConstants.ApplicationConfiguration.AWSSQSConfiguration.AWS_SecretKey, Amazon.RegionEndpoint.APSouth1);
            }
            catch { }
            return null;
        }
    }
}
