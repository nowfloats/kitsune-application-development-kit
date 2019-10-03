using Kitsune.API.Model.ApiRequestModels;
using Kitsune.API2.EnvConstants;
using Kitsune.Models.WebsiteModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using Kitsune.Models;
using Kitsune.Models.Project;
using Kitsune.API2.Utils;
using Kitsune.BasePlugin.Utils;
using MongoDB.Bson;
using Kitsune.Helper;
using Newtonsoft.Json;
using System.Web;
using System.Reflection;
using Kitsune.API.Model.ApiRequestModels.Application;
using AmazonAWSHelpers.SQSQueueHandler;
using Kitsune.Models.AWSModels;
using System.Security.Cryptography;
using System.Text;
using Kitsune.Models.Cloud;
using Kitsune.Models.ProjectModels;

namespace Kitsune.API2.DataHandlers.Mongo
{
    /// <summary>
    /// Partial class for all the database queries (New customer collection)
    /// </summary>
    public static partial class MongoConnector
    {
        /// <summary>
        /// Create new website 
        /// Creates a record in kitsunewebsite collection
        /// Creates default website user 
        /// Creates a websiteDNS record with getkitsune domain
        /// </summary>
        /// <param name="requestModel"></param>
        /// <returns></returns>
        internal static CommonAPIResponse CreateNewWebsite(CreateNewWebsiteRequestModel requestModel)
        {
            if (requestModel == null)
            {
                return CommonAPIResponse.InternalServerError(new ArgumentNullException(nameof(requestModel)));
            }

            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();
                var websiteCollection = _kitsuneDatabase.GetCollection<KitsuneWebsiteCollection>(KitsuneWebsiteCollectionName);

                #region CREATE WEBSITETAG

                var websiteTag = requestModel.WebsiteTag.ToLower().Replace("http://", "").Replace("https://", "");

                char[] trimChars = new char[] { ' ', '/', ':', '-' };

                websiteTag = websiteTag.Trim(trimChars).ToUpper();

                #endregion

                //Create websiteuser and assign the website id to that user with owner access
                var basePlugin = BasePluginConfigGenerator.GetBasePlugin(requestModel.ClientId);
                string defaultSubDomain = basePlugin.GetSubDomain(requestModel.CloudProviderType);

                //update clientId if the id is not present
                requestModel.ClientId = basePlugin.GetClientId();

                #region Custom code for nowfloats sync service
                //Get DeveloperId from project id if the request if from the NowFloats sync service
                if (requestModel.DeveloperId.ToLower() == "syncservice" && requestModel.ClientId == Constants.ClientIdConstants.NowFloatsClientId)
                {
                    var developerId = GetUserIdFromProjectId(requestModel.ProjectId);
                    if (string.IsNullOrEmpty(developerId))
                        return CommonAPIResponse.UnAuthorized(new System.ComponentModel.DataAnnotations.ValidationResult("Could not find the user for the given project id"));

                    requestModel.DeveloperId = developerId;
                }
                #endregion

                var count = websiteCollection.Count(x => x.WebsiteTag.Equals(websiteTag));
                if (count > 0)
                    return CommonAPIResponse.BadRequest(new System.ComponentModel.DataAnnotations.ValidationResult($"Website tag '{websiteTag}' already exist"));

                var kitsuneUrl = String.Format("{0}{1}", websiteTag, defaultSubDomain);


                var websiteDocument = CreateNewWebsite(requestModel.ProjectId, websiteTag, requestModel.Email, requestModel.PhoneNumber,
                                        requestModel.UserEmail, kitsuneUrl, requestModel.DeveloperId, requestModel.ClientId, requestModel.FullName, domain: requestModel.Domain, customWebsiteId: requestModel.WebsiteId);

                if (requestModel.CopyDemoData)
                {
                    try
                    {
                        #region Get Data for demo website
                        //Copy data if the demo website is active
                        requestModel.ActivateWebsite = CopyWebsiteData(null, websiteDocument._id, requestModel.ProjectId, true);

                        #endregion


                    }
                    catch (Exception ex)
                    {
                        //TODO : log website data could not be copied
                    }


                }

                #region Activate website
                //Activate the new website 
                if (requestModel.ActivateWebsite)
                    MongoConnector.ActivateWebsite(new ActivateWebsiteRequestModel { WebsiteId = websiteDocument._id }, websiteDocument);
                #endregion

                return CommonAPIResponse.Created(websiteDocument._id);
            }
            catch (Exception ex)
            {
                return CommonAPIResponse.InternalServerError(ex);
            }
        }
        internal static string GetUserIdFromProjectId(string projectId)
        {
            var project = GetProjectDetails(projectId);

            if (project != null)
            {
                var user = GetUserIdFromUserEmail(new GetUserIdRequestModel { UserEmail = project.UserEmail });
                if (user != null)
                {
                    return user.Id;
                }
            }
            return null;
        }
        internal static bool CopyWebsiteData(string oldWebsiteId, string newWebsiteId, string projectId, bool copyFromDemo = false)
        {
            if (_kitsuneServer == null)
                InitializeConnection();
            var websiteCollection = _kitsuneDatabase.GetCollection<KitsuneWebsiteCollection>(KitsuneWebsiteCollectionName);
            KitsuneWebsiteCollection demoWebsite = null;
            if (copyFromDemo)
            {
                if (!string.IsNullOrEmpty(newWebsiteId) && !string.IsNullOrEmpty(projectId))
                {
                    demoWebsite = websiteCollection.Find(x => x.ProjectId == projectId && !x.IsArchived).SortBy(x => x.CreatedOn).FirstOrDefault();
                    if (demoWebsite != null)
                        oldWebsiteId = demoWebsite._id;
                    else
                        return false;
                }
                else
                {
                    return false;
                }
            }

            #region Get Data for demo website
            if (demoWebsite == null)
                demoWebsite = websiteCollection.Find(x => x._id == oldWebsiteId && x.IsActive && !x.IsArchived).SortBy(x => x.CreatedOn).Limit(1).FirstOrDefault();



            //Copy data if the demo website is active
            if (demoWebsite != null)
            {
                var projectCollection = _kitsuneDatabase.GetCollection<KitsuneProject>(KitsuneProjectsCollectionName);
                var project = projectCollection.Find(x => x.ProjectId == demoWebsite.ProjectId).FirstOrDefault();


                #region Archive Existing Data  
                var LanguageCollection = _kitsuneSchemaDatabase.GetCollection<KLanguageModel>(KitsuneLanguageCollectionName);
                var language = LanguageCollection.Find(x => x._id == project.SchemaId).Limit(1).FirstOrDefault();
                if (language != null)
                {
                    var baseCollectionName = EnvConstants.Constants.GenerateSchemaName(language.Entity.EntityName);
                    var baseCollection = _kitsuneSchemaDatabase.GetCollection<BsonDocument>(baseCollectionName);
                    baseCollection.UpdateMany(new BsonDocument("websiteid", newWebsiteId), new UpdateDefinitionBuilder<BsonDocument>().Set("isarchived", true).Set("updatedon", DateTime.UtcNow));
                }
                #endregion


                if (project != null && !string.IsNullOrWhiteSpace(project.SchemaId))
                {
                    var demoData = MongoConnector.GetWebsiteData(new GetWebsiteDataRequestModel
                    {
                        SchemaId = project.SchemaId,
                        WebsiteId = demoWebsite._id,
                        UserId = demoWebsite.DeveloperId
                    });

                    try
                    {
                        if (demoData != null && demoData.Data != null && demoData.Data.Count > 0)
                        {
                            IDictionary<string, object> dataObject = new Dictionary<string, object>();
                            var schemaInfo = MongoConnector.GetLanguageEntity(new GetLanguageEntityRequestModel { EntityId = project.SchemaId });
                            if (schemaInfo != null)
                            {
                                //foreach (PropertyInfo propertyInfo in demoData.Data[0].GetType().GetProperties().Where(x => x.CanRead))
                                //{
                                //    dataObject.Add(propertyInfo.Name, propertyInfo.GetValue(demoData.Data[0]));
                                //}
                                dataObject = (IDictionary<string, object>)(demoData.Data[0]);
                                var response = MongoConnector.AddDataForWebsite(new AddOrUpdateWebsiteRequestModel
                                {
                                    Data = new Dictionary<string, object>(dataObject),
                                    SchemaName = schemaInfo.EntityName,
                                    WebsiteId = newWebsiteId,
                                    UserId = demoWebsite.DeveloperId
                                });

                                #region Update Image Collection with the url to new website id
                                var imageCollection = _kitsuneSchemaDatabase.GetCollection<BsonDocument>($"{EnvConstants.Constants.GenerateSchemaName(schemaInfo.EntityName)}_image");
                                if (imageCollection.Count(new FilterDefinitionBuilder<BsonDocument>().Eq("websiteid", newWebsiteId)) > 0)
                                {
                                    var images = imageCollection.Find(new FilterDefinitionBuilder<BsonDocument>().Eq("websiteid", newWebsiteId));
                                    if (images != null)
                                    {
                                        foreach (var image in images.ToList())
                                        {
                                            if (image.Contains("url"))
                                            {
                                                imageCollection.UpdateOne(new FilterDefinitionBuilder<BsonDocument>().Eq("_kid", image["_kid"].AsString),
                                                    new UpdateDefinitionBuilder<BsonDocument>().Set("url", image["url"].AsString.Replace(demoWebsite._id, newWebsiteId)));
                                            }

                                        }
                                    }
                                }
                                #endregion

                                #region Copy the S3 images

                                AmazonSQSQueueHandlers<S3FolderCopyQueueModdel> sqsHanlder = new AmazonSQSQueueHandlers<S3FolderCopyQueueModdel>(AmazonAWSConstants.S3FolderCopySQSUrl);
                                S3FolderCopyQueueModdel model = new S3FolderCopyQueueModdel()
                                {
                                    DestinationBucket = AmazonAWSConstants.WebsiteFilesBucketName,
                                    SourceBucket = AmazonAWSConstants.WebsiteFilesBucketName,
                                    DestinationFolder = $"v1/{newWebsiteId}",
                                    SourceFolder = $"v1/{demoWebsite._id}"
                                };
                                sqsHanlder.PushMessageToQueue(model,
                                                            EnvironmentConstants.ApplicationConfiguration.AWSSQSConfiguration.AWS_AccessKey,
                                                            EnvironmentConstants.ApplicationConfiguration.AWSSQSConfiguration.AWS_SecretKey);
                                return true;

                                #endregion
                            }

                        }
                    }
                    catch (Exception ex)
                    {
                        return false;
                    }
                }
            }
            return false;
            #endregion
        }


        /// <summary>
        /// Update website details 
        /// Website Tag, Website Url, Kitsune Version, Active status 
        /// </summary>
        /// <param name="requestModel"></param>
        /// <returns>WebsiteId</returns>
        internal static CommonAPIResponse UpdateWebsiteDetails(UpdateWebsiteRequestModel requestModel)
        {
            if (requestModel == null)
            {
                return CommonAPIResponse.InternalServerError(new ArgumentNullException(nameof(requestModel)));
            }

            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();

                var websiteCollection = _kitsuneDatabase.GetCollection<KitsuneWebsiteCollection>(KitsuneWebsiteCollectionName);
                var kitsuneUserCollection = _kitsuneDatabase.GetCollection<UserModel>(KitsuneUserCollectionName);


                #region Custom code for nowfloats sync service
                //Get DeveloperId from project id if the request if from the NowFloats sync service
                if (!string.IsNullOrEmpty(requestModel.ProjectId) && requestModel.DeveloperId.ToLower() == "syncservice" && requestModel.ClientId == Constants.ClientIdConstants.NowFloatsClientId)
                {
                    var developerId = GetUserIdFromProjectId(requestModel.ProjectId);
                    if (string.IsNullOrEmpty(developerId))
                        return CommonAPIResponse.UnAuthorized(new System.ComponentModel.DataAnnotations.ValidationResult("Could not find the user for the given project id"));

                    requestModel.DeveloperId = developerId;
                }
                #endregion


                var isAuthorized = kitsuneUserCollection.Find(x => x._id == requestModel.DeveloperId).FirstOrDefault() != null;
                if (!isAuthorized)
                    return CommonAPIResponse.UnAuthorized(new System.ComponentModel.DataAnnotations.ValidationResult("Not Authorized"));


                var filterDefinitionBuilder = Builders<KitsuneWebsiteCollection>.Filter;
                var updateDefinitionBuilder = Builders<KitsuneWebsiteCollection>.Update;
                UpdateDefinition<KitsuneWebsiteCollection> updateDefinition = updateDefinitionBuilder.Set(x => x.UpdatedOn, DateTime.UtcNow);

                if (requestModel.IsActive != null)
                    updateDefinition = updateDefinitionBuilder.Set(x => x.IsActive, requestModel.IsActive);
                if (requestModel.Version != null && requestModel.Version != 0)
                    updateDefinition = updateDefinitionBuilder.Set(x => x.KitsuneProjectVersion, requestModel.Version);
                if (requestModel.WebsiteTag != null)
                {
                    var websiteTag = requestModel.WebsiteTag.ToLower().Replace("http://", "").Replace("https://", "");
                    char[] trimChars = new char[] { ' ', '/', ':', '-' };
                    websiteTag = websiteTag.Trim(trimChars).ToUpper();
                    updateDefinition = updateDefinitionBuilder.Set(x => x.WebsiteTag, websiteTag);
                }
                if (requestModel.WebsiteUrl != null)
                {
                    #region Add to DNS Collection 

                    var websiteDNSCollection = _kitsuneDatabase.GetCollection<WebsiteDNSInfo>(KitsuneWebsiteDNSInfoCollectionName);
                    var websiteDNSObj = new WebsiteDNSInfo
                    {
                        CreatedOn = DateTime.UtcNow,
                        DNSStatus = DNSStatus.Pending,
                        DomainName = requestModel.WebsiteUrl,
                        IsSSLEnabled = false,
                        RootPath = requestModel.WebsiteUrl.ToUpper(),
                        WebsiteId = requestModel.WebsiteId
                    };
                    websiteDNSCollection.InsertOne(websiteDNSObj);
                    return new CommonAPIResponse() { Response = websiteDNSObj._id };
                    #endregion
                }

                var updateResult = websiteCollection.UpdateOne(filterDefinitionBuilder.Eq(x => x._id, requestModel.WebsiteId), updateDefinition);
                if (updateResult.IsAcknowledged && updateResult.MatchedCount > 0 && updateResult.ModifiedCount == 1)
                    return new CommonAPIResponse() { Response = requestModel.WebsiteId };

                return CommonAPIResponse.BadRequest(new System.ComponentModel.DataAnnotations.ValidationResult($"No website found to update with website id '{requestModel.WebsiteId}'."));
            }
            catch (Exception ex)
            {
                return CommonAPIResponse.InternalServerError(ex);
            }
        }

        /// <summary>
        /// Update the user of website 
        /// Update UserName, WebsiteAccess, ContactDetails
        /// </summary>
        /// <param name="requestModel"></param>
        /// <returns></returns>
        internal static CommonAPIResponse UpdateWebsiteUserDetails(CreateOrUpdateWebsiteUserRequestModel requestModel)
        {
            if (requestModel == null)
            {
                return CommonAPIResponse.InternalServerError(new ArgumentNullException(nameof(requestModel)));
            }

            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();

                var websiteUserCollection = _kitsuneDatabase.GetCollection<KitsuneWebsiteUserCollection>(KitsuneWebsiteUserCollectionName);
                var kitsuneUserCollection = _kitsuneDatabase.GetCollection<UserModel>(KitsuneUserCollectionName);


                var isAuthorized = kitsuneUserCollection.Find(x => x._id == requestModel.DeveloperId).FirstOrDefault() != null;
                if (!isAuthorized)
                    return CommonAPIResponse.UnAuthorized(new System.ComponentModel.DataAnnotations.ValidationResult("Not Authorized"));

                var filterDefinitionBuilder = Builders<KitsuneWebsiteUserCollection>.Filter;
                var updateDefinitionBuilder = Builders<KitsuneWebsiteUserCollection>.Update;
                UpdateDefinition<KitsuneWebsiteUserCollection> updateDefinition = updateDefinitionBuilder.Set(x => x.UpdatedOn, DateTime.UtcNow);
                if (requestModel.ContactDetails != null)
                    updateDefinition = updateDefinition.Set(x => x.Contact, requestModel.ContactDetails);
                if (requestModel.DeveloperId != null)
                {
                    updateDefinition = updateDefinition.Set(x => x.WebsiteId, requestModel.WebsiteId)
                                                       .Set(x => x.AccessType, requestModel.AccessType);
                }
                if (requestModel.UserName != null)
                    updateDefinition = updateDefinition.Set(x => x.UserName, requestModel.UserName);

                var updateResult = websiteUserCollection.UpdateOne(filterDefinitionBuilder.Eq(x => x._id, requestModel.WebsiteUserId), updateDefinition);
                if (updateResult.IsAcknowledged && updateResult.MatchedCount > 0 && updateResult.ModifiedCount == 1)
                    return new CommonAPIResponse() { Response = requestModel.WebsiteId };

                return CommonAPIResponse.BadRequest(new System.ComponentModel.DataAnnotations.ValidationResult($"No website user found to update with user id '{requestModel.WebsiteUserId}'."));
            }
            catch (Exception ex)
            {
                return CommonAPIResponse.InternalServerError(ex);
            }
        }

        internal static CommonAPIResponse GetKitsuneWebsiteListForProject(string developerId, string projectId, int limit = 100, int skip = 0, bool includeUsers = false)
        {
            if (string.IsNullOrEmpty(developerId))
            {
                return CommonAPIResponse.UnAuthorized();
            }

            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();

                var websitesCollection = _kitsuneDatabase.GetCollection<KitsuneWebsiteCollection>(KitsuneWebsiteCollectionName);
                var websiteUsersCollection = _kitsuneDatabase.GetCollection<KitsuneWebsiteUserCollection>(KitsuneWebsiteUserCollectionName);
                var websiteDNSCollection = _kitsuneDatabase.GetCollection<WebsiteDNSInfo>(KitsuneWebsiteDNSInfoCollectionName);

                ProjectionDefinitionBuilder<KitsuneWebsiteCollection> project = new ProjectionDefinitionBuilder<KitsuneWebsiteCollection>();

                var result = websitesCollection.Find(x => x.ProjectId == projectId && x.IsArchived == false)
                                                .Project<KitsuneWebsiteCollection>(project.Include(x => x.WebsiteTag)
                                                                                                  .Include(x => x.WebsiteUrl)
                                                                                                  .Include(x => x.CreatedOn)
                                                                                                  .Include(x => x.UpdatedOn)).SortByDescending(x => x.CreatedOn).Limit(limit).Skip(skip);
                var totalCount = result.Count();


                List<WebsiteItem> websiteProjectList = new List<WebsiteItem>();
                if (totalCount > 0)
                {
                    var websites = result.ToList();
                    #region WebsitesDetails
                    foreach (var website in websites)
                    {
                        websiteProjectList.Add(new WebsiteItem
                        {
                            WebsiteId = website._id,
                            WebsiteTag = website.WebsiteTag,
                            WebsiteDomain = website.WebsiteUrl,
                            CreatedOn = website.CreatedOn,
                            UpdatedOn = website.UpdatedOn,
                            IsActive = website.IsActive,
                            WebsiteUsers = null
                        });

                    }
                    #endregion

                    if (includeUsers)
                    {
                        #region WebsiteUserDetails
                        //Get website users
                        var findObject = new FilterDefinitionBuilder<KitsuneWebsiteUserCollection>().In(x => x.WebsiteId, websites.Select(y => y._id));
                        var projectDocument = new ProjectionDefinitionBuilder<KitsuneWebsiteUserCollection>().Include(x => x.Contact).Include(x => x.WebsiteId);
                        var websiteUsers = websiteUsersCollection.Find(findObject).Project<KitsuneWebsiteUserCollection>(projectDocument).ToList();
                        if (websiteUsers != null && websiteUsers.Any())
                        {
                            foreach(var web in websiteProjectList)
                            {
                                web.WebsiteUsers = websiteUsers.Where(x => x.WebsiteId == web.WebsiteId).Select(x => new WebsiteUser { Address = x.Contact.Address?.AddressDetail, FullName = x.Contact?.FullName }).ToList();
                            }
                        }
                        #endregion
                    }

                        #region WebsiteDNSDetails
                        //Get website users
                        var findDNSObject = new FilterDefinitionBuilder<WebsiteDNSInfo>().In(x => x.WebsiteId, websites.Select(y => y._id));
                        var projectDNSDocument = new ProjectionDefinitionBuilder<WebsiteDNSInfo>().Include(x => x.DomainName).Include(x => x.IsSSLEnabled).Include(x => x.WebsiteId);
                        var websiteDNSList = websiteDNSCollection.Find(findDNSObject).Project<WebsiteDNSInfo>(projectDNSDocument).ToList();
                        if (websiteDNSList != null && websiteDNSList.Any())
                        {
                            foreach (var web in websiteProjectList)
                            {
                                web.IsSSLEnabled = websiteDNSList.FirstOrDefault(x => x.WebsiteId == web.WebsiteId && x.DomainName == web.WebsiteDomain)?.IsSSLEnabled ?? false;
                            }
                        }
                        #endregion


                }
                return new CommonAPIResponse()
                {
                    Response = new GetWebsitesResponseModel
                    {
                        Websites = websiteProjectList,
                        CurrentIndex = skip,
                        PageSize = limit,
                        TotalCount = totalCount
                    }
                };

            }
            catch (Exception ex)
            {
                return CommonAPIResponse.InternalServerError(ex);
            }
        }

        internal static CommonAPIResponse GetKitsuneWebsiteDetails(string websiteId)
        {
            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();

                var tempId = new ObjectId();
                if (!ObjectId.TryParse(websiteId, out tempId))
                    return CommonAPIResponse.BadRequest(new System.ComponentModel.DataAnnotations.ValidationResult("Invalid websiteid"));

                var websitesCollection = _kitsuneDatabase.GetCollection<KitsuneWebsiteCollection>(KitsuneWebsiteCollectionName);
                var websiteUsersCollection = _kitsuneDatabase.GetCollection<KitsuneWebsiteUserCollection>(KitsuneWebsiteUserCollectionName);
                var websiteDNSCollection = _kitsuneDatabase.GetCollection<WebsiteDNSInfo>(KitsuneWebsiteDNSInfoCollectionName);
                var projectCollection = _kitsuneDatabase.GetCollection<ProductionKitsuneProject>(KitsuneProjectsCollectionName);

                ProjectionDefinitionBuilder<KitsuneWebsiteCollection> projectDefinition = new ProjectionDefinitionBuilder<KitsuneWebsiteCollection>();


                var result = websitesCollection.Find(x => x._id == websiteId && x.IsArchived == false)
                                                .Project<KitsuneWebsiteCollection>(projectDefinition.Include(x => x._id)
                                                                                 .Include(x => x.WebsiteTag)
                                                                                 .Include(x => x.ProjectId)
                                                                                 .Include(x => x.WebsiteUrl)
                                                                                 .Include(x => x.CreatedOn)
                                                                                 .Include(x => x.KitsuneProjectVersion)
                                                                                 .Include(x => x.IsActive)
                                                                                 .Include(x => x.RootPath)
                                                                                 .Include(x => x.ClientId)
                                                                                 .Include(x => x.DeveloperId)
                                                                                 .Include(x => x.UpdatedOn)).Limit(1).FirstOrDefault();
                if (result != null)
                {
                    var CloudProviderCollection = _kitsuneDatabase.GetCollection<KitsuneCloudProviderCollectionModel>(KitsuneCloudProviderCollectionName);
                    KitsuneCloudProviderCollectionModel cloudProviderDetails = CloudProviderCollection.Find(x => x.projectid == result.ProjectId && x.isarchived == false).FirstOrDefault();
                    CloudProvider provider;
                    if (cloudProviderDetails == null)
                    {
                        provider = CloudProvider.AWS;
                    }
                    else
                    {
                        provider = cloudProviderDetails.provider;
                    }
                    var response = new WebsiteDetailsResponseModel
                    {
                        CreatedOn = result.CreatedOn,
                        IsActive = result.IsActive,
                        KitsuneProjectVersion = result.KitsuneProjectVersion,
                        ProjectId = result.ProjectId,
                        RootPath = result.RootPath,
                        UpdatedOn = result.UpdatedOn,
                        WebsiteId = result._id,
                        WebsiteTag = result.WebsiteTag,
                        WebsiteUrl = result.WebsiteUrl,
                        ClientId = result.ClientId,
                        DeveloperId = result.DeveloperId,
                        CloudProvider = provider
                    };

                    //Get project name
                    var projectName = projectCollection.Find(x => x.ProjectId.Equals(result.ProjectId)).Project(x => x.ProjectName).Limit(1).FirstOrDefault();
                    if (projectName != null)
                        response.ProjectName = projectName;

                    //Get dns info for IsSecure
                    var dnsInfo = websiteDNSCollection.Find(x => x.WebsiteId == websiteId && x.DomainName == response.WebsiteUrl).FirstOrDefault();
                    if (dnsInfo != null)
                        response.IsSSLEnabled = dnsInfo.IsSSLEnabled;

                    //Get website users
                    var websiteUsers = websiteUsersCollection.Find(x => x.WebsiteId == websiteId && x.IsActive == true);
                    if (websiteUsers != null && websiteUsers.Any())
                    {
                        response.WebsiteUsers = new List<WebsiteUserItem>();
                        foreach (var user in websiteUsers.ToList())
                        {
                            response.WebsiteUsers.Add(new WebsiteUserItem
                            {
                                AccessType = user.AccessType.ToString(),
                                Contact = user.Contact,
                                IsActive = user.IsActive,
                                LastLoginTimeStamp = user.LastLoginTimeStamp,
                                UserName = user.UserName,
                                UpdatedOn = user.UpdatedOn,
                                CreatedOn = user.CreatedOn
                            });
                        }
                    }

                    return new CommonAPIResponse() { Response = response };
                }
                else
                    return CommonAPIResponse.NotFound(new System.ComponentModel.DataAnnotations.ValidationResult($"Website with WebsiteId :' {websiteId}' does not exist."));


            }
            catch (Exception ex)
            {
                return CommonAPIResponse.InternalServerError(ex);
            }
        }

        internal static CommonAPIResponse GetKitsuneWebsiteDetailsByTag(string websiteTag)
        {
            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();

                var tempId = new ObjectId();

                var websitesCollection = _kitsuneDatabase.GetCollection<KitsuneWebsiteCollection>(KitsuneWebsiteCollectionName);
                var websiteUsersCollection = _kitsuneDatabase.GetCollection<KitsuneWebsiteUserCollection>(KitsuneWebsiteUserCollectionName);
                var websiteDNSCollection = _kitsuneDatabase.GetCollection<WebsiteDNSInfo>(KitsuneWebsiteDNSInfoCollectionName);
                var projectCollection = _kitsuneDatabase.GetCollection<ProductionKitsuneProject>(KitsuneProjectsCollectionName);

                ProjectionDefinitionBuilder<KitsuneWebsiteCollection> projectDefinition = new ProjectionDefinitionBuilder<KitsuneWebsiteCollection>();


                var result = websitesCollection.Find(x => x.WebsiteTag == websiteTag && x.IsArchived == false)
                                                .Project<KitsuneWebsiteCollection>(projectDefinition.Include(x => x._id)
                                                                                 .Include(x => x.WebsiteTag)
                                                                                 .Include(x => x.ProjectId)
                                                                                 .Include(x => x.WebsiteUrl)
                                                                                 .Include(x => x.CreatedOn)
                                                                                 .Include(x => x.KitsuneProjectVersion)
                                                                                 .Include(x => x.IsActive)
                                                                                 .Include(x => x.RootPath)
                                                                                 .Include(x => x.ClientId)
                                                                                 .Include(x => x.DeveloperId)
                                                                                 .Include(x => x.UpdatedOn)).Limit(1).FirstOrDefault();
                if (result != null)
                {
                    var CloudProviderCollection = _kitsuneDatabase.GetCollection<KitsuneCloudProviderCollectionModel>(KitsuneCloudProviderCollectionName);
                    KitsuneCloudProviderCollectionModel cloudProviderDetails = CloudProviderCollection.Find(x => x.projectid == result.ProjectId && x.isarchived == false).FirstOrDefault();
                    CloudProvider provider;
                    if (cloudProviderDetails == null)
                    {
                        provider = CloudProvider.AWS;
                    }
                    else
                    {
                        provider = cloudProviderDetails.provider;
                    }
                    var response = new WebsiteDetailsResponseModel
                    {
                        CreatedOn = result.CreatedOn,
                        IsActive = result.IsActive,
                        KitsuneProjectVersion = result.KitsuneProjectVersion,
                        ProjectId = result.ProjectId,
                        RootPath = result.RootPath,
                        UpdatedOn = result.UpdatedOn,
                        WebsiteId = result._id,
                        WebsiteTag = result.WebsiteTag,
                        WebsiteUrl = result.WebsiteUrl,
                        ClientId = result.ClientId,
                        DeveloperId = result.DeveloperId,
                        CloudProvider = provider
                    };

                    //Get project name
                    var projectName = projectCollection.Find(x => x.ProjectId.Equals(result.ProjectId)).Project(x => x.ProjectName).Limit(1).FirstOrDefault();
                    if (projectName != null)
                        response.ProjectName = projectName;

                    //Get dns info for IsSecure
                    var dnsInfo = websiteDNSCollection.Find(x => x.WebsiteId == result._id && x.DomainName == response.WebsiteUrl).FirstOrDefault();
                    if (dnsInfo != null)
                        response.IsSSLEnabled = dnsInfo.IsSSLEnabled;

                    //Get website users
                    var websiteUsers = websiteUsersCollection.Find(x => x.WebsiteId == result._id && x.IsActive == true);
                    if (websiteUsers != null && websiteUsers.Any())
                    {
                        response.WebsiteUsers = new List<WebsiteUserItem>();
                        foreach (var user in websiteUsers.ToList())
                        {
                            response.WebsiteUsers.Add(new WebsiteUserItem
                            {
                                AccessType = user.AccessType.ToString(),
                                Contact = user.Contact,
                                IsActive = user.IsActive,
                                LastLoginTimeStamp = user.LastLoginTimeStamp,
                                UserName = user.UserName,
                                UpdatedOn = user.UpdatedOn,
                                CreatedOn = user.CreatedOn
                            });
                        }
                    }

                    return new CommonAPIResponse() { Response = response };
                }
                else
                    return CommonAPIResponse.NotFound(new System.ComponentModel.DataAnnotations.ValidationResult($"Website with WebsiteId :' {websiteTag}' does not exist."));


            }
            catch (Exception ex)
            {
                return CommonAPIResponse.InternalServerError(ex);
            }
        }

        internal static string GetRootAliasUriFromWebsteDomain(string websiteId, string websiteDomain)
        {
            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();

                var websiteDNSCollection = _kitsuneDatabase.GetCollection<WebsiteDNSInfo>(KitsuneWebsiteDNSInfoCollectionName);
                var dns = websiteDNSCollection.Find(x => x.WebsiteId == websiteId && x.DomainName == websiteDomain && x.DNSStatus == DNSStatus.Active)?.FirstOrDefault();
                if (dns != null)
                {
                    return $"{(dns.IsSSLEnabled ? "https://" : "http://")}{websiteDomain}";
                }
                else
                {
                    return $"http://{websiteDomain}";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return null;
        }

        internal static CommonAPIResponse CheckKitsuneDomainAvailability(string domainName)
        {
            if (domainName == null)
            {
                return CommonAPIResponse.BadRequest(new System.ComponentModel.DataAnnotations.ValidationResult("Domain name can not be empty"));
            }

            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();

                var customersCollection = _kitsuneDatabase.GetCollection<KitsuneWebsiteCollection>(KitsuneWebsiteCollectionName);

                var domain = domainName.ToLower().Replace("http://", "").Replace("https://", "").Replace("www.", "");
                char[] trimChars = new char[] { ' ', '/', ':' };
                domain = domain.Trim(trimChars).ToUpper();

                var count = customersCollection.Count(x => x.WebsiteUrl.Equals(domain));
                if (count == 0)
                {
                    return CommonAPIResponse.NoContent();
                }
                else
                {
                    return CommonAPIResponse.NotFound();
                }
            }
            catch (Exception ex)
            {
                return CommonAPIResponse.InternalServerError(ex);
            }
        }

        internal static CommonAPIResponse CheckWebsiteTagAvailability(string websiteTag, string clientId)
        {
            if (string.IsNullOrWhiteSpace(websiteTag))
            {
                return CommonAPIResponse.BadRequest(new System.ComponentModel.DataAnnotations.ValidationResult("Website tag can not be empty"));
            }

            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();

                var websiteCollection = _kitsuneDatabase.GetCollection<KitsuneWebsiteCollection>(KitsuneWebsiteCollectionName);

                var tag = websiteTag.Trim().ToUpper();

                var count = websiteCollection.Count(x => x.WebsiteTag.Equals(tag) && x.ClientId == clientId);
                if (count > 0)
                {
                    return CommonAPIResponse.NoContent();
                }
                else
                {
                    return CommonAPIResponse.NotFound();
                }
            }
            catch (Exception ex)
            {
                return CommonAPIResponse.InternalServerError(ex);
            }
        }

        internal static CommonAPIResponse GetKitsuneLiveWebsites(string developerId, int limit = 100, int skip = 0)
        {
            if (string.IsNullOrEmpty(developerId))
            {
                return CommonAPIResponse.UnAuthorized();
            }
            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();

                var websiteCollection = _kitsuneDatabase.GetCollection<KitsuneWebsiteCollection>(KitsuneWebsiteCollectionName);
                var ProductionProjectCollection = _kitsuneDatabase.GetCollection<ProductionKitsuneProject>(ProductionProjectCollectionName);
                var user = _kitsuneDatabase.GetCollection<UserModel>(KitsuneUserCollectionName).Find(x => x._id == developerId).FirstOrDefault();

                if (user == null)
                    return CommonAPIResponse.NotFound(new System.ComponentModel.DataAnnotations.ValidationResult("Developer not found"));

                ProjectionDefinitionBuilder<KitsuneWebsiteCollection> projectWebsite = new ProjectionDefinitionBuilder<KitsuneWebsiteCollection>();
                var fdb = new FilterDefinitionBuilder<KitsuneWebsiteCollection>();
                var fd = fdb.Eq(x => x.DeveloperId, developerId) & fdb.Eq(x => x.IsArchived, false) & fdb.Eq(x => x.IsActive, true);
                var liveWebsites = websiteCollection.Find(fd).SortByDescending(x => x.CreatedOn).Skip(skip).Limit(limit).Project<KitsuneWebsiteCollection>(projectWebsite.Include(x => x.WebsiteUrl)
                                                                                                                .Include(x => x.CreatedOn)
                                                                                                                .Include(x => x.RootPath)
                                                                                                                .Include(x => x.ProjectId)
                                                                                                                .Include(x => x.KitsuneProjectVersion)
                                                                                                                .Include(x => x.WebsiteTag)
                                                                                                                .Include(x => x.UpdatedOn)
                                                                                                                .Include(x => x.DeveloperId)
                                                                                                                .Include(x => x.IsActive)
                                                                                                                .Include(x => x.KitsuneProjectVersion)
                                                                                                                .Include(x => x._id)).ToList();
                if (liveWebsites == null || liveWebsites.Count == 0)
                    return new CommonAPIResponse { Response = new GetLiveKitsuneWebsiteResponseModel { LiveWebsites = new List<LiveKitsuneWebsiteDetails>() } };
                ProjectionDefinitionBuilder<ProductionKitsuneProject> project = new ProjectionDefinitionBuilder<ProductionKitsuneProject>();
                var projectFdb = new FilterDefinitionBuilder<ProductionKitsuneProject>();
                var projectFd = projectFdb.In(x => x.ProjectId, liveWebsites.Select(x => x.ProjectId).ToList().Distinct());
                var projectNameList = ProductionProjectCollection.Find(projectFd).Project<ProductionKitsuneProject>(project.Include(x => x.ProjectId)
                                                                                                             .Include(x => x.ProjectName)
                                                                                                             .Include(x => x.PublishedOn)
                                                                                                             .Exclude(x => x._id)).ToList();

                List<LiveKitsuneWebsiteDetails> liveWebsiteList = new List<LiveKitsuneWebsiteDetails>();
                Dictionary<string, CloudProvider> projectCloudProviderMap = new Dictionary<string, CloudProvider>();
                var CloudProviderCollection = _kitsuneDatabase.GetCollection<KitsuneCloudProviderCollectionModel>(KitsuneCloudProviderCollectionName);
                
                foreach (var website in liveWebsites)
                {
                    var temp = projectNameList.FirstOrDefault(x => x.ProjectId == website.ProjectId);
                    if (temp != null)
                    {
                        CloudProvider provider;
                        if (projectCloudProviderMap.ContainsKey(website.ProjectId))
                        {
                            provider = projectCloudProviderMap[website.ProjectId];
                        }
                        else
                        {
                            KitsuneCloudProviderCollectionModel cloudProviderDetails = CloudProviderCollection.Find(x => x.projectid == website.ProjectId && x.isarchived == false).FirstOrDefault();
                            if (cloudProviderDetails == null)
                            {
                                provider = CloudProvider.AWS;
                            }
                            else
                            {
                                provider = cloudProviderDetails.provider;
                            }
                            projectCloudProviderMap.Add(website.ProjectId, provider);
                        }
                        liveWebsiteList.Add(new LiveKitsuneWebsiteDetails
                        {
                            ProjectId = website.ProjectId,
                            WebsiteId = website._id,
                            WebsiteUrl = website.WebsiteUrl,
                            RootPath = website.RootPath,
                            ProjectName = temp.ProjectName,
                            CreatedOn = website.CreatedOn,
                            PublishedOn = temp.PublishedOn,
                            IsActive = website.IsActive,
                            DeveloperId = website.DeveloperId,
                            UpdatedOn = website.UpdatedOn,
                            WebsiteTag = website.WebsiteTag,
                            ProjectVersion = website.KitsuneProjectVersion,
                            CloudProvider = provider
                        });

                    }
                }
                return new CommonAPIResponse
                {
                    Response = new GetLiveKitsuneWebsiteResponseModel
                    {
                        LiveWebsites = liveWebsiteList,
                        CurrentIndex = skip,
                        PageSize = limit,
                        TotalCount = liveWebsiteList.Count
                    }
                };

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        internal static CommonAPIResponse GetAllKitsuneLiveWebsites(int limit = 100, int skip = 0)
        {
            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();

                var websiteCollection = _kitsuneDatabase.GetCollection<KitsuneWebsiteCollection>(KitsuneWebsiteCollectionName);
                var ProductionProjectCollection = _kitsuneDatabase.GetCollection<ProductionKitsuneProject>(ProductionProjectCollectionName);

                ProjectionDefinitionBuilder<KitsuneWebsiteCollection> projectWebsite = new ProjectionDefinitionBuilder<KitsuneWebsiteCollection>();
                var fdb = new FilterDefinitionBuilder<KitsuneWebsiteCollection>();
                var fd = fdb.Eq(x => x.IsArchived, false) & fdb.Eq(x => x.IsActive, true);
                var totalCount = websiteCollection.Count(fd);
                var liveWebsites = websiteCollection.Find(fd).SortByDescending(x => x.CreatedOn).Skip(skip).Limit(limit).Project<KitsuneWebsiteCollection>(projectWebsite.Include(x => x.WebsiteUrl)
                                                                                                                .Include(x => x.CreatedOn)
                                                                                                                .Include(x => x.RootPath)
                                                                                                                .Include(x => x.ProjectId)
                                                                                                                .Include(x => x.KitsuneProjectVersion)
                                                                                                                .Include(x => x.WebsiteTag)
                                                                                                                .Include(x => x.UpdatedOn)
                                                                                                                .Include(x => x.DeveloperId)
                                                                                                                .Include(x => x.IsActive)
                                                                                                                .Include(x => x.KitsuneProjectVersion)
                                                                                                                .Include(x => x._id)
                                                                                                                .Include(x => x.ClientId)).ToList();
                if (liveWebsites == null || liveWebsites.Count == 0)
                    return new CommonAPIResponse { Response = new GetLiveKitsuneWebsiteResponseModel { LiveWebsites = new List<LiveKitsuneWebsiteDetails>() } };
                ProjectionDefinitionBuilder<ProductionKitsuneProject> project = new ProjectionDefinitionBuilder<ProductionKitsuneProject>();
                var projectFdb = new FilterDefinitionBuilder<ProductionKitsuneProject>();
                var projectFd = projectFdb.In(x => x.ProjectId, liveWebsites.Select(x => x.ProjectId).ToList().Distinct());
                var projectNameList = ProductionProjectCollection.Find(projectFd).Project<ProductionKitsuneProject>(project.Include(x => x.ProjectId)
                                                                                                             .Include(x => x.ProjectName)
                                                                                                             .Include(x => x.PublishedOn)
                                                                                                             .Exclude(x => x._id)).ToList();

                List<LiveKitsuneWebsiteDetails> liveWebsiteList = new List<LiveKitsuneWebsiteDetails>();
                Dictionary<string, CloudProvider> projectCloudProviderMap = new Dictionary<string, CloudProvider>();
                var CloudProviderCollection = _kitsuneDatabase.GetCollection<KitsuneCloudProviderCollectionModel>(KitsuneCloudProviderCollectionName);

                foreach (var website in liveWebsites)
                {
                    var temp = projectNameList.FirstOrDefault(x => x.ProjectId == website.ProjectId);
                    if (temp != null)
                    {
                        CloudProvider provider;
                        if (projectCloudProviderMap.ContainsKey(website.ProjectId))
                        {
                            provider = projectCloudProviderMap[website.ProjectId];
                        }
                        else
                        {
                            KitsuneCloudProviderCollectionModel cloudProviderDetails = CloudProviderCollection.Find(x => x.projectid == website.ProjectId && x.isarchived == false).FirstOrDefault();
                            if (cloudProviderDetails == null)
                            {
                                provider = CloudProvider.AWS;
                            }
                            else
                            {
                                provider = cloudProviderDetails.provider;
                            }
                            projectCloudProviderMap.Add(website.ProjectId, provider);
                        }
                        liveWebsiteList.Add(new LiveKitsuneWebsiteDetails
                        {
                            ProjectId = website.ProjectId,
                            WebsiteId = website._id,
                            WebsiteUrl = website.WebsiteUrl,
                            RootPath = website.RootPath,
                            ProjectName = temp.ProjectName,
                            CreatedOn = website.CreatedOn,
                            PublishedOn = temp.PublishedOn,
                            IsActive = website.IsActive,
                            DeveloperId = website.DeveloperId,
                            UpdatedOn = website.UpdatedOn,
                            WebsiteTag = website.WebsiteTag,
                            ProjectVersion = website.KitsuneProjectVersion,
                            ClientId = website.ClientId,
                            CloudProvider = provider
                        });

                    }
                }
                return new CommonAPIResponse
                {
                    Response = new GetLiveKitsuneWebsiteResponseModel
                    {
                        LiveWebsites = liveWebsiteList,
                        CurrentIndex = skip,
                        PageSize = limit,
                        TotalCount = totalCount
                    }
                };

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        internal static CommonAPIResponse CreateNewWebsiteUser(CreateOrUpdateWebsiteUserRequestModel requestModel)
        {
            if (requestModel == null)
            {
                return CommonAPIResponse.BadRequest(new System.ComponentModel.DataAnnotations.ValidationResult("Request can not be empty"));
            }
            if (string.IsNullOrEmpty(requestModel.DeveloperId))
                return CommonAPIResponse.UnAuthorized();
            if (string.IsNullOrEmpty(requestModel.UserName))
                return CommonAPIResponse.BadRequest(new System.ComponentModel.DataAnnotations.ValidationResult("Username can not be empty"));

            var userCollection = _kitsuneDatabase.GetCollection<KitsuneWebsiteUserCollection>(KitsuneWebsiteUserCollectionName);
            PasswordGenerator passwordGenerator = new PasswordGenerator();
            var password = passwordGenerator.GeneratePassword(true, true, true, 7);

            var websiteUser = new KitsuneWebsiteUserCollection
            {
                DeveloperId = requestModel.DeveloperId,
                WebsiteId = requestModel.WebsiteId,
                AccessType = requestModel.AccessType ?? KitsuneWebsiteAccessType.Admin, //Default user admin if not set
                UserName = requestModel.UserName,
                CreatedOn = DateTime.UtcNow,
                IsActive = true,
                Password = password,
                Contact = requestModel.ContactDetails,
            };
            userCollection.InsertOne(websiteUser);
            return new CommonAPIResponse { Response = websiteUser._id };
        }

        internal static CommonAPIResponse GetWebsiteUserDetails(string websiteId, string websiteUserId)
        {
            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();
                if (string.IsNullOrEmpty(websiteId))
                    return CommonAPIResponse.BadRequest(new System.ComponentModel.DataAnnotations.ValidationResult("Website id can not be empty"));

                if (string.IsNullOrWhiteSpace(websiteUserId))
                    return CommonAPIResponse.BadRequest(new System.ComponentModel.DataAnnotations.ValidationResult("Website user id can not be empty"));

                var websiteUsersCollection = _kitsuneDatabase.GetCollection<KitsuneWebsiteUserCollection>(KitsuneWebsiteUserCollectionName);

                ProjectionDefinitionBuilder<KitsuneWebsiteUserCollection> projectDefinition = new ProjectionDefinitionBuilder<KitsuneWebsiteUserCollection>();

                var result = websiteUsersCollection.Find(x => x._id == websiteUserId && x.WebsiteId == websiteId)
                                                .Project<KitsuneWebsiteUserCollection>(projectDefinition.Include(x => x._id)
                                                                                 .Include(x => x.AccessType)
                                                                                 .Include(x => x.Contact)
                                                                                 .Include(x => x.CreatedOn)
                                                                                 .Include(x => x.DeveloperId)
                                                                                 .Include(x => x.IsActive)
                                                                                 .Include(x => x.LastLoginTimeStamp)
                                                                                 .Include(x => x.Password)
                                                                                 .Include(x => x.WebsiteId)
                                                                                 .Include(x => x.UserName)).Limit(1).FirstOrDefault();

                if (result == null)
                    return CommonAPIResponse.NotFound(new System.ComponentModel.DataAnnotations.ValidationResult("User not found"));
                else
                    return new CommonAPIResponse { Response = result };
            }
            catch (Exception ex)
            {
                return CommonAPIResponse.InternalServerError(ex);
            }
        }

        internal static CommonAPIResponse LoginWebsiteUser(VerifyLoginRequestModel requestModel)
        {
            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();

                var websiteCollection = _kitsuneDatabase.GetCollection<KitsuneWebsiteCollection>(KitsuneWebsiteCollectionName);
                var websiteUserCollection = _kitsuneDatabase.GetCollection<KitsuneWebsiteUserCollection>(KitsuneWebsiteUserCollectionName);
                var projectCollection = _kitsuneDatabase.GetCollection<ProductionKitsuneProject>(KitsuneProjectsCollectionName);
                var kitsuneLanguagesCollection = _kitsuneSchemaDatabase.GetCollection<KLanguageModel>(KitsuneLanguageCollectionName);

                ProjectionDefinitionBuilder<KitsuneWebsiteCollection> project = new ProjectionDefinitionBuilder<KitsuneWebsiteCollection>();
                ProjectionDefinitionBuilder<ProductionKitsuneProject> projectKitsuneProject = new ProjectionDefinitionBuilder<ProductionKitsuneProject>();
                ProjectionDefinitionBuilder<KLanguageModel> projectLang = new ProjectionDefinitionBuilder<KLanguageModel>();

                var websiteResult = websiteCollection.Find(x => x.WebsiteUrl.ToLower() == requestModel.Domain.ToLower() && x.IsArchived == false)
                                                .Project<KitsuneWebsiteCollection>(project.Include(x => x._id)
                                                                                 .Include(x => x.WebsiteTag)
                                                                                 .Include(x => x.ProjectId)
                                                                                 .Include(x => x.CreatedOn)
                                                                                 .Include(x => x.GroupWebsiteId)
                                                                                 .Include(x => x.KitsuneProjectVersion)
                                                                                 .Include(x => x.RootPath)
                                                                                 .Include(x => x.UpdatedOn)
                                                                                 .Include(x => x.WebsiteUrl)
                                                                                 .Include(x => x.DeveloperId)).Limit(1).FirstOrDefault();


                if (websiteResult == null)
                    return CommonAPIResponse.BadRequest(new System.ComponentModel.DataAnnotations.ValidationResult("No website found for with the domain"));

                var userResultCursor = websiteUserCollection.Find(x => x.WebsiteId == websiteResult._id && x.UserName.ToLower() == requestModel.UserName.Trim().ToLower() && x.Password == requestModel.Pwd.Trim()).Limit(1);
                var userResult = (userResultCursor != null) ? userResultCursor.FirstOrDefault() : null;

                if (userResult != null)
                {
                    var result = new WebsiteLoginResponseModel()
                    {
                        WebsiteDetails = new WebsiteLogin_WebsiteDetais(),
                        UserDetails = new WebsiteLogin_UserDetais()
                        {
                            UserId = userResult._id,
                            AccessType = userResult.AccessType.ToString(),
                            Contact = userResult.Contact,
                            CreatedOn = userResult.CreatedOn,
                            IsActive = userResult.IsActive,
                            LastLoginTimeStamp = userResult.LastLoginTimeStamp,
                            UpdatedOn = userResult.UpdatedOn,
                            UserName = userResult.UserName
                        }
                    };

                    result.DeveloperId = websiteResult.DeveloperId;
                    if (!String.IsNullOrEmpty(websiteResult.DeveloperId))
                    {
                        try
                        {
                            var developerDetails = (GetDeveloperDetailsFromId(websiteResult.DeveloperId));

                            if (developerDetails != null)
                            {
                                result.DeveloperContactDetails = new ContactDetails()
                                {
                                    Email = developerDetails.Email,
                                    FullName = $"{developerDetails.FirstName} {developerDetails.LastName}",
                                    PhoneNumber = developerDetails.PhoneNumber,
                                    Address = developerDetails.Address
                                };

                                //To-Do get the details from project settings and override
                                result.SupportContactDetails = result.DeveloperContactDetails;
                            }
                        }
                        catch { }
                    }

                    var projectCollectionCursor = projectCollection.Find(x => x.ProjectId.Equals(websiteResult.ProjectId)).Project<ProductionKitsuneProject>(projectKitsuneProject.Include(x => x.ProjectName)
                                                                                                                                                             .Include(x => x.SchemaId)).Limit(1);
                    var projectDetails = (projectCollectionCursor != null) ? projectCollectionCursor.Limit(1).FirstOrDefault() : null;
                    if (projectDetails != null)
                    {
                        result.WebsiteDetails = new WebsiteLogin_WebsiteDetais()
                        {
                            CreatedOn = websiteResult.CreatedOn,
                            IsActive = websiteResult.IsActive,
                            KitsuneProjectVersion = websiteResult.KitsuneProjectVersion,
                            ProjectId = websiteResult.ProjectId,
                            ProjectName = projectDetails.ProjectName,
                            RootPath = websiteResult.RootPath,
                            UpdatedOn = websiteResult.UpdatedOn,
                            WebsiteId = websiteResult._id,
                            WebsiteTag = websiteResult.WebsiteTag,
                            WebsiteUrl = websiteResult.WebsiteUrl
                        };

                        result.SchemaId = projectDetails.SchemaId;
                    }

                    var languageCollectionCursor = kitsuneLanguagesCollection.Find(x => x._id.Equals(projectDetails.SchemaId)).Project<KLanguageModel>(projectLang.Include(x => x.Entity.EntityName)).Limit(1);
                    var languageDetails = (languageCollectionCursor != null) ? languageCollectionCursor.Limit(1).FirstOrDefault() : null;
                    if (languageDetails != null)
                    {
                        result.EntityName = languageDetails != null ? languageDetails.Entity.EntityName : null;
                    }

                    return new CommonAPIResponse { Response = result };
                }
                else
                {
                    return CommonAPIResponse.UnAuthorized();
                }
            }
            catch (Exception ex)
            {
                return CommonAPIResponse.InternalServerError(ex);
            }
        }

        internal static CommonAPIResponse ChangeWebsiteUserPassword(UpdateWebsiteUserPasswordRequestModel requestModel)
        {
            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();

                var websiteUserCollection = _kitsuneDatabase.GetCollection<KitsuneWebsiteUserCollection>(KitsuneWebsiteUserCollectionName);
                //  FilterDefinition<BsonDocument> filter = $@"{{ DeveloperId : '{requestModel.DeveloperId}', UserName : {{ $regex : /^{requestModel.UserName.Trim()}$/ }}, Password : '{requestModel.OldPassword}' }}";

                var filterDefinition = new FilterDefinitionBuilder<KitsuneWebsiteUserCollection>().And(
                    new FilterDefinitionBuilder<KitsuneWebsiteUserCollection>().Eq(x => x.DeveloperId, requestModel.DeveloperId),
                    new FilterDefinitionBuilder<KitsuneWebsiteUserCollection>().Eq(x => x._id, requestModel.WebsiteUserId),
                    new FilterDefinitionBuilder<KitsuneWebsiteUserCollection>().Eq(x => x.Password, requestModel.OldPassword));

                var result = websiteUserCollection.FindOneAndUpdate<KitsuneWebsiteUserCollection>(filterDefinition,
                    new UpdateDefinitionBuilder<KitsuneWebsiteUserCollection>()
                    .Set(x => x.Password, requestModel.NewPassword)
                    .Set(x => x.UpdatedOn, DateTime.UtcNow));

                if (result != null)
                    return CommonAPIResponse.NoContent();

                return CommonAPIResponse.BadRequest(new System.ComponentModel.DataAnnotations.ValidationResult("Old password is incorrect"));
            }
            catch (Exception ex)
            {
                return CommonAPIResponse.InternalServerError(ex);
            }
        }

        internal static CommonAPIResponse GetKSearchResult(KSearchRequestModel requestModel)
        {
            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();
                var websiteCollection = _kitsuneDatabase.GetCollection<KitsuneWebsiteCollection>(KitsuneWebsiteCollectionName);
                var ProductionResourceCollection = _kitsuneDatabase.GetCollection<ProductionKitsuneResource>(ProductionResorcesCollectionName);
                var ProductionProjectCollection = _kitsuneDatabase.GetCollection<ProductionKitsuneProject>(ProductionProjectCollectionName);
                var resourceCollection = _kitsuneDatabase.GetCollection<KitsuneResource>(KitsuneResourceCollectionName);

                var resourceInfo = new List<KitsuneResourceMetaDataDetails>();

                #region Get ProjectId

                var projectId = websiteCollection.Find(x => x._id.Equals(requestModel.CustomerId)).Project(x => x.ProjectId).Limit(1).FirstOrDefault();
                if (projectId == null)
                    return CommonAPIResponse.BadRequest(new System.ComponentModel.DataAnnotations.ValidationResult("Website not found"));

                #endregion

                #region Get details from KitsuneProjectResourcesPoduction

                if (!requestModel.IsDemo)
                {
                    var kitsuneResourceProj = Builders<ProductionKitsuneResource>.Projection;
                    resourceInfo = ProductionResourceCollection.Find(x => x.ProjectId.Equals(projectId) && x.ResourceType.Equals(ResourceType.LINK))
                                                                             .Project<KitsuneResourceMetaDataDetails>(kitsuneResourceProj.Include(x => x.SourcePath)
                                                                                                                        .Include(x => x.MetaData)
                                                                                                                        .Exclude(x => x._id)).ToList();
                }
                else
                {
                    var kitsuneResourceProj = Builders<KitsuneResource>.Projection;
                    resourceInfo = resourceCollection.Find(x => x.ProjectId.Equals(projectId) && x.ResourceType.Equals(ResourceType.LINK))
                                                                             .Project<KitsuneResourceMetaDataDetails>(kitsuneResourceProj.Include(x => x.SourcePath)
                                                                                                                        .Include(x => x.MetaData)
                                                                                                                        .Exclude(x => x._id)).ToList();
                }

                #endregion

                #region Get Favicon 

                string favicon = "";

                #endregion

                var searchModel = new KSearchResponseModel
                {
                    SearchObjects = new List<KSearchObject>(),
                    FaviconUrl = favicon
                };
                var searchKeyword = requestModel.Keyword.ToLower();

                foreach (var link in resourceInfo)
                {
                    try
                    {
                        if (searchModel.SearchObjects.FindIndex(url => url.SourcePath.Equals(link.SourcePath)) > -1)
                            continue;

                        int weight = 0;
                        List<string> selectedKeyword = new List<string>();
                        foreach (var keyword in link.MetaData.Keywords)
                        {
                            var count = searchKeyword.Split(' ').Count(searchStringSelector => keyword.ToLower().Split(' ').Contains(searchStringSelector));
                            if (count > 0)
                            {
                                selectedKeyword.Add(keyword);
                            }
                            weight = weight + count;
                        }

                        if (weight > 0)
                        {
                            searchModel.SearchObjects.Add(new KSearchObject { Count = weight, SourcePath = link.SourcePath, Keywords = selectedKeyword, Title = link.MetaData.Title, Description = link.MetaData.Description });
                        }
                    }
                    catch { }
                }
                searchModel.SearchObjects = searchModel.SearchObjects.OrderByDescending(x => x.Count).ToList();
                return new CommonAPIResponse() { Response = searchModel };
            }
            catch (Exception ex)
            {
                return CommonAPIResponse.InternalServerError(ex);
            }
        }

        internal static CommonAPIResponse GetCustomerIdFromDomain(GetCustomersIdFromDomainRequestModel requestModel)
        {
            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();
                if (requestModel.Domain.EndsWith(EnvConstants.Constants.KitsuneIdentifierKitsuneDemoDomain, StringComparison.InvariantCultureIgnoreCase))
                    return new CommonAPIResponse() { Response = requestModel.Domain.Replace(EnvConstants.Constants.KitsuneIdentifierKitsuneDemoDomain, "", StringComparison.InvariantCultureIgnoreCase) };
                var customersCollection = _kitsuneDatabase.GetCollection<KitsuneWebsiteCollection>(KitsuneWebsiteCollectionName);
                string websiteId = string.Empty;
                websiteId = customersCollection.Find(x => x.WebsiteUrl == requestModel.Domain.ToUpper() && x.IsActive == true).Project(x => x._id).Limit(1).FirstOrDefault();

                if (!string.IsNullOrEmpty(websiteId))
                    return new CommonAPIResponse() { Response = websiteId };
                return CommonAPIResponse.NotFound();
            }
            catch (Exception ex)
            {
                return CommonAPIResponse.InternalServerError(ex);
            }
        }
        internal static CommonAPIResponse GetProjectNameFromDomain(GetCustomersIdFromDomainRequestModel requestModel)
        {
            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();
                var websiteId = string.Empty;
                var projectId = string.Empty;
                if (requestModel.Domain.EndsWith(EnvConstants.Constants.KitsuneIdentifierKitsuneDemoDomain, StringComparison.InvariantCultureIgnoreCase))
                    websiteId = requestModel.Domain.Replace(EnvConstants.Constants.KitsuneIdentifierKitsuneDemoDomain, "", StringComparison.InvariantCultureIgnoreCase);
                var customersCollection = _kitsuneDatabase.GetCollection<KitsuneWebsiteCollection>(KitsuneWebsiteCollectionName);
                if (string.IsNullOrEmpty(websiteId))
                {
                    projectId = customersCollection.Find(x => x.WebsiteUrl == requestModel.Domain.ToUpper()).Project(x => x.ProjectId).Limit(1).FirstOrDefault();
                }
                else
                {
                    projectId = customersCollection.Find(x => x._id == websiteId).Project(x => x.ProjectId).Limit(1).FirstOrDefault();
                }

                if (!string.IsNullOrEmpty(projectId))
                {
                    var projectName = _kitsuneDatabase.GetCollection<KitsuneProject>(KitsuneProjectsCollectionName).Find(x => x.ProjectId == projectId).Project(x => x.ProjectName).FirstOrDefault();
                    if (!string.IsNullOrEmpty(projectName))
                        return new CommonAPIResponse() { Response = projectName };
                }
                return CommonAPIResponse.NotFound();
            }
            catch (Exception ex)
            {
                return CommonAPIResponse.InternalServerError(ex);
            }
        }
        internal static List<string> GetAllCustomerIdsPerDeveloperId(string developerId)
        {
            if (String.IsNullOrEmpty(developerId))
            {
                throw new ArgumentNullException(nameof(developerId));
            }

            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();

                var websitesCollection = _kitsuneDatabase.GetCollection<KitsuneWebsiteCollection>(KitsuneWebsiteCollectionName);

                ProjectionDefinitionBuilder<KitsuneWebsiteCollection> project = new ProjectionDefinitionBuilder<KitsuneWebsiteCollection>();

                var result = websitesCollection.Find(x => x.DeveloperId == developerId && x.IsActive && !x.IsArchived)
                                                .Project(x => x._id)
                                                .ToList();

                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        internal static double GetAmountToAddBeforeNextBillingCycleForUser(string developerId)
        {
            if (String.IsNullOrEmpty(developerId))
            {
                throw new ArgumentNullException(nameof(developerId));
            }

            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();

                var usagePredictionCollection = _kitsuneDatabase.GetCollection<KitsuneUsagePredictionCollection>(KitsuneUsagePredictionCollectionName);
                var result = usagePredictionCollection.Find(x => x.for_user == developerId).Limit(1).FirstOrDefault();

                return result.amount_to_add_before_next_billing_cycle;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        internal static DateTime? GetBalanceWentZeroDateForUser(string developerId)
        {
            if (String.IsNullOrEmpty(developerId))
            {
                throw new ArgumentNullException(nameof(developerId));
            }

            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();

                var balanceAlertCollection = _kitsuneDatabase.GetCollection<KitsuneBalanceAlertCollection>(KitsuneBalanceAlertCollectionName);
                var userBalanceAlert = balanceAlertCollection.Find(x => x.for_user == developerId).Limit(1).FirstOrDefault();

                var category = userBalanceAlert.category;
                var date_updated = userBalanceAlert.date_updated;

                DateTime? balanceWentZeroDate = null;

                if (category == 0)
                    balanceWentZeroDate = date_updated;

                return balanceWentZeroDate;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        internal static CommonAPIResponse ActivateWebsite(ActivateWebsiteRequestModel requestModel, KitsuneWebsiteCollection website = null)
        {
            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();
                var websiteCollection = _kitsuneDatabase.GetCollection<KitsuneWebsiteCollection>(KitsuneWebsiteCollectionName);
                var ProductionProjectCollection = _kitsuneDatabase.GetCollection<ProductionKitsuneProject>(ProductionProjectCollectionName);

                if (website == null)
                {
                    website = websiteCollection.Find(x => x._id == requestModel.WebsiteId).FirstOrDefault();

                    if (website == null)
                        return CommonAPIResponse.NotFound(new System.ComponentModel.DataAnnotations.ValidationResult($"Website with WebsiteId :' {requestModel.WebsiteId}' does not exist."));

                }
                ProductionKitsuneProject productionProject = ProductionProjectCollection.Find(x => x.ProjectId == website.ProjectId).FirstOrDefault();

                if (productionProject == null)
                    return CommonAPIResponse.NotFound(new System.ComponentModel.DataAnnotations.ValidationResult("Project does not exist."));

                var filterDefinitionBuilder = Builders<KitsuneWebsiteCollection>.Filter;
                var updateDefinitionBuilder = Builders<KitsuneWebsiteCollection>.Update;
                UpdateDefinition<KitsuneWebsiteCollection> updateDefinition = updateDefinitionBuilder.Set(x => x.UpdatedOn, DateTime.UtcNow)
                                                                                                     .Set(x => x.IsActive, true)
                                                                                                     .Set(x => x.KitsuneProjectVersion, productionProject.Version);

                var updateResult = websiteCollection.UpdateOne(filterDefinitionBuilder.Eq(x => x._id, website._id), updateDefinition);
                if (updateResult.IsAcknowledged && updateResult.MatchedCount > 0 && updateResult.ModifiedCount == 1)
                {
                    #region Send K-Admin Creds to the activated customers

                    EmailHelper emailHelper = new EmailHelper();
                    var websiteUserCollection = _kitsuneDatabase.GetCollection<KitsuneWebsiteUserCollection>(KitsuneWebsiteUserCollectionName);
                    ProjectionDefinitionBuilder<KitsuneWebsiteUserCollection> projectDefinition = new ProjectionDefinitionBuilder<KitsuneWebsiteUserCollection>();
                    var developer = _kitsuneDatabase.GetCollection<UserModel>(KitsuneUserCollectionName).Find(x => x.Email == productionProject.UserEmail).Project<UserModel>(new ProjectionDefinitionBuilder<UserModel>().Include(y => y.DisplayName)).FirstOrDefault();
                    //TODO: MIGRATION

                    var basePlugin = BasePluginConfigGenerator.GetBasePlugin(website.ClientId);
                    if (basePlugin.GetIsDefaultWebsiteAccessEmailEnabled())
                    {
                        var websiteUsers = websiteUserCollection.Find(x => x.WebsiteId == website._id).Project<WebsiteUserDetails>(projectDefinition.Include(x => x.Contact)
                                                                                                                                        .Include(x => x.UserName)
                                                                                                                                        .Include(x => x.Password)).ToList();
                        foreach (var user in websiteUsers)
                        {
                            Dictionary<string, string> optionalParameters = new Dictionary<string, string>
                            {
                                { EnvConstants.Constants.EmailParam_CustomerName, user.Contact.FullName},
                                { EnvConstants.Constants.EmailParam_DeveloperName, developer?.DisplayName},
                                { EnvConstants.Constants.EmailParam_KAdminUserName, user.UserName},
                                { EnvConstants.Constants.EmailParam_KAdminPassword, user.Password},
                                { EnvConstants.Constants.EmailParam_KAdminUrl, string.Format(EnvConstants.Constants.KAdminBaseUrl, website.WebsiteUrl).ToLower()}
                            };
                            emailHelper.SendGetKitsuneEmail(string.Empty, user.Contact.Email, MailType.CUSTOMER_KADMIN_CREDENTIALS, null, optionalParameters, website.ClientId);
                        }
                    }

                    #endregion

                    return new CommonAPIResponse() { Response = productionProject.Version };
                }

                throw new Exception($"Could not make website '{requestModel.WebsiteId}' active");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        internal static CommonAPIResponse DeactivateWebsites(DeActivateWebsitesRequestModel requestModel)
        {
            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();

                var websiteCollection = _kitsuneDatabase.GetCollection<KitsuneWebsiteCollection>(KitsuneWebsiteCollectionName);
                var websiteDNSCollection = _kitsuneDatabase.GetCollection<WebsiteDNSInfo>(KitsuneWebsiteDNSInfoCollectionName);

                var websiteFilterDefinationBuilder = new FilterDefinitionBuilder<KitsuneWebsiteCollection>();
                var websiteUpdateDefinationBuilder = new UpdateDefinitionBuilder<KitsuneWebsiteCollection>();

                var websiteDNSFilterDefinationBuilder = new FilterDefinitionBuilder<WebsiteDNSInfo>();
                var websiteDNSUpdateDefinationBuilder = new UpdateDefinitionBuilder<WebsiteDNSInfo>();

                DeActivateWebsitesResponseModel responseModel = new DeActivateWebsitesResponseModel()
                {
                    IsSuccess=true,
                    FailedWebsiteIds = new List<string>()
                };

                foreach (var websiteId in requestModel.WebsiteIds)
                {
                    try
                    {
                        var websiteFilter = websiteFilterDefinationBuilder.Eq(x => x._id, websiteId);
                        var websiteUpdate = websiteUpdateDefinationBuilder.Set(x => x.IsActive, false);

                        var websiteDNSFilter = websiteDNSFilterDefinationBuilder.Eq(x => x.WebsiteId, websiteId) &
                                               websiteDNSFilterDefinationBuilder.Eq(x=>x.DNSStatus,DNSStatus.Active);
                        var websiteDNSUpdate = websiteDNSUpdateDefinationBuilder.Set(x => x.DNSStatus, DNSStatus.InActive);

                        var websiteResult = websiteCollection.UpdateOne(websiteFilter, websiteUpdate);

                        if(websiteResult.IsAcknowledged && websiteResult.IsModifiedCountAvailable)
                        {
                            if(websiteResult.ModifiedCount>0)
                            {
                                websiteDNSCollection.UpdateMany(websiteDNSFilter, websiteDNSUpdate);
                            }
                            else
                            {
                                throw new Exception("Already Website Deactivated");
                            }
                        }
                        else
                        {
                            throw new Exception("Unable to Deactivate the Website");
                        }
                    }
                    catch(Exception ex)
                    {
                        responseModel.IsSuccess = false;
                        responseModel.FailedWebsiteIds.Add(websiteId);
                    }
                }

                return new CommonAPIResponse() { Response = responseModel };
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        
        public class KAdminLoginTokenModel
        {
            public string UserId { get; set; }
            public DateTime DateTime { get; set; }
            public int ExpiryTime { get; set; }
            public string Source { get; set; }
        }

        internal static CommonAPIResponse GenerateKAdminLoginToken(GenerateKAdminLoginTokenRequestModel requestModel)
        {
            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();
                var websiteUserCollection = _kitsuneDatabase.GetCollection<KitsuneWebsiteUserCollection>(KitsuneWebsiteUserCollectionName);
                var websiteUserDetails = websiteUserCollection.Find(x => x.WebsiteId == requestModel.WebsiteId && x.DeveloperId.Equals(requestModel.UserId)).FirstOrDefault();

                if (websiteUserDetails == null)
                    return CommonAPIResponse.NotFound(new System.ComponentModel.DataAnnotations.ValidationResult($"User with WebsiteId :' {requestModel.WebsiteId}' does not exist."));

                var websiteCollection = _kitsuneDatabase.GetCollection<KitsuneWebsiteCollection>(KitsuneWebsiteCollectionName);
                var websiteDetails = websiteCollection.Find(x => x._id.Equals(requestModel.WebsiteId)).FirstOrDefault();
                if (websiteDetails == null)
                    return CommonAPIResponse.NotFound(new System.ComponentModel.DataAnnotations.ValidationResult($"Website with WebsiteId :' {requestModel.WebsiteId}' does not exist"));

                var currentDateTime = DateTime.UtcNow;
                var message = new KAdminLoginTokenModel
                {
                    DateTime = currentDateTime,
                    UserId = websiteUserDetails._id,
                    ExpiryTime = requestModel.ExpiryTime ?? 2,
                    Source = requestModel.Source ?? "IDE"
                };
                string messageString = JsonConvert.SerializeObject(message);
                EncryptDecryptHelper encryptionHelper = new EncryptDecryptHelper("");
                var token = encryptionHelper.Encrypt(messageString);

                var urlEncodedToken = HttpUtility.UrlEncode(token);

                string redirectUrl = $"http://{websiteDetails.WebsiteUrl}/k-admin/Home/TokenLogin?token={urlEncodedToken}";

                KAdminLoginUrlResponseModel response = new KAdminLoginUrlResponseModel()
                {
                    RedirectUrl = redirectUrl
                };

                return new CommonAPIResponse { Response = response };
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        internal static CommonAPIResponse DecodeKAdminLoginToken(DecodeKAdminTokenRequestModel requestModel)
        {
            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();

                #region DeCrypt Token


                var messageObject = DecryptToken(requestModel.Token);
                if (messageObject == null)
                    return CommonAPIResponse.BadRequest(new System.ComponentModel.DataAnnotations.ValidationResult("Token Expired"));

                #endregion

                #region Get User Details

                var websiteUserCollection = _kitsuneDatabase.GetCollection<KitsuneWebsiteUserCollection>(KitsuneWebsiteUserCollectionName);
                var websiteUserDetails = websiteUserCollection.Find(x => x._id.Equals(messageObject.UserId)).FirstOrDefault();
                if (websiteUserDetails == null)
                    return CommonAPIResponse.NotFound(new System.ComponentModel.DataAnnotations.ValidationResult($"Website with given Token"));

                #endregion

                #region Get Website Details

                var websiteCollection = _kitsuneDatabase.GetCollection<KitsuneWebsiteCollection>(KitsuneWebsiteCollectionName);
                var websiteDetails = websiteCollection.Find(x => x._id.Equals(websiteUserDetails.WebsiteId)).FirstOrDefault();

                #endregion

                KAdminTokenLoginResponseModel response = new KAdminTokenLoginResponseModel
                {
                    Password = websiteUserDetails.Password,
                    UserName = websiteUserDetails.UserName,
                    WebsiteUrl = websiteDetails.WebsiteUrl,
                    Source = messageObject.Source
                };

                return new CommonAPIResponse { Response = response };
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        internal static KAdminLoginTokenModel DecryptToken(string token)
        {
            EncryptDecryptHelper decryptionHelper = new EncryptDecryptHelper("");
            var messageString = decryptionHelper.Decrypt(token);
            var messageObject = JsonConvert.DeserializeObject<KAdminLoginTokenModel>(messageString);
            if (messageObject == null || (messageObject.DateTime.AddMinutes(messageObject.ExpiryTime) < DateTime.UtcNow))
                return null;
            else
                return messageObject;
        }
        internal static GetLiveWebsiteForProjectResponseModel GetLiveWebsitesPerProject(GetLiveWebsiteForProjectRequestModel requestModel)
        {
            if (requestModel == null)
            {
                throw new ArgumentNullException(nameof(requestModel));
            }
            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();
                var pagination = new Pagination() { CurrentIndex = requestModel.Offset, PageSize = requestModel.Limit, TotalCount = 0 };

                var websiteCollection = _kitsuneDatabase.GetCollection<KitsuneWebsiteCollection>(KitsuneWebsiteCollectionName);
                var websiteUserCollection = _kitsuneDatabase.GetCollection<KitsuneWebsiteUserCollection>(KitsuneWebsiteUserCollectionName);
                var ProductionProjectCollection = _kitsuneDatabase.GetCollection<ProductionKitsuneProject>(ProductionProjectCollectionName);
                List<string> ProjectIdList;
                ProjectionDefinitionBuilder<ProductionKitsuneProject> project = new ProjectionDefinitionBuilder<ProductionKitsuneProject>();
                var LiveProductionProject = ProductionProjectCollection.Find(x => x.ProjectId == requestModel.ProjectId).SortByDescending(x => x.CreatedOn).Skip(requestModel.Offset).Limit(requestModel.Limit)
                                                                         .Project<ProductionKitsuneProject>(project.Include(x => x.PublishedOn)
                                                                                                                .Include(x => x.ProjectName)
                                                                                                                .Include(x => x.ProjectId)
                                                                                                                .Exclude(x => x._id)).ToList();
                if (LiveProductionProject == null || !LiveProductionProject.Any())
                    return new GetLiveWebsiteForProjectResponseModel { LiveWebsites = new List<LiveKitsuneWebsiteDetailsForProject>(), Pagination = pagination };

                ProjectIdList = LiveProductionProject.Select(x => x.ProjectId).ToList();

                ProjectionDefinitionBuilder<KitsuneWebsiteCollection> projectCustomer = new ProjectionDefinitionBuilder<KitsuneWebsiteCollection>();
                var fdb = new FilterDefinitionBuilder<KitsuneWebsiteCollection>();
                var fd = fdb.Eq(x => x.IsActive, true) & fdb.Eq(x => x.IsArchived, false) & fdb.In(x => x.ProjectId, ProjectIdList);
                pagination.TotalCount = websiteCollection.Count(fd);
                var liveWebsites = websiteCollection.Find(fd).Project<KitsuneWebsiteCollection>(projectCustomer.Include(x => x.WebsiteUrl)
                                                                                                                        .Include(x => x.CreatedOn)
                                                                                                                        .Include(x => x.ProjectId)
                                                                                                                        .Include(x => x.DeveloperId)
                                                                                                                        .Include(x => x.WebsiteTag)
                                                                                                                        .Include(x => x.IsActive)
                                                                                                                        .Include(x => x._id)).ToList();


                if (liveWebsites == null)
                    return new GetLiveWebsiteForProjectResponseModel { LiveWebsites = new List<LiveKitsuneWebsiteDetailsForProject>(), Pagination = pagination };

                //Get Websiteuser details
                var liveWebsiteIds = liveWebsites.Select(x => x._id).ToList();
                var websiteUsers = websiteUserCollection.Find(x => liveWebsiteIds.Contains(x.WebsiteId)).ToList();
                List<LiveKitsuneWebsiteDetailsForProject> LiveWebsiteList = new List<LiveKitsuneWebsiteDetailsForProject>();
                WebsiteUserDetais websiteUser = null;
                foreach (var website in liveWebsites)
                {
                    var temp = LiveProductionProject.FirstOrDefault(x => x.ProjectId == website.ProjectId);
                    if (temp != null)
                    {
                        var user = websiteUsers.FirstOrDefault(x => x.WebsiteId == website._id && x.AccessType == KitsuneWebsiteAccessType.Owner);
                        if (user != null)
                            websiteUser = new WebsiteUserDetais
                            {
                                AccessType = user.AccessType.ToString(),
                                Contact = user.Contact,
                                CreatedOn = user.CreatedOn.Date,
                                LastLoginTimeStamp = user.LastLoginTimeStamp,
                                UpdatedOn = user.UpdatedOn,
                                UserId = user._id,
                                UserName = user.UserName
                            };
                        else
                            websiteUser = null;
                        LiveWebsiteList.Add(new LiveKitsuneWebsiteDetailsForProject
                        {
                            CreatedOn = website.CreatedOn,
                            WebsiteUrl = website.WebsiteUrl,
                            WebsiteTag = website.WebsiteTag,
                            IsActive = website.IsActive,
                            DeveloperId = website.DeveloperId,
                            ProjectId = website.ProjectId,
                            ProjectName = LiveProductionProject.First(x => x.ProjectId == website.ProjectId).ProjectName,
                            WebsiteId = website._id,
                            PublishedOn = website.CreatedOn,
                            UpdatedOn = website.UpdatedOn,
                            WebsiteOwner = websiteUser
                        });

                    }
                }
                return new GetLiveWebsiteForProjectResponseModel
                {
                    LiveWebsites = LiveWebsiteList,
                    Pagination = pagination
                };

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        internal static List<string> GetLiveWebsiteIds(string projectid = null)
        {
            if (_kitsuneServer == null)
                InitializeConnection();
            var websiteCollection = _kitsuneDatabase.GetCollection<KitsuneWebsiteCollection>(KitsuneWebsiteCollectionName);
            IFindFluent<KitsuneWebsiteCollection, KitsuneWebsiteCollection> websites;

            if (!string.IsNullOrEmpty(projectid))
                websites = websiteCollection.Find(x => x.IsActive == true && x.ProjectId == projectid);
            else
                websites = websiteCollection.Find(x => x.IsActive == true);

            websites = websites.Project<KitsuneWebsiteCollection>(new ProjectionDefinitionBuilder<KitsuneWebsiteCollection>().Include(x => x._id).Include(x => x.ProjectId));
            if (websites != null && websites.Any())
                return websites.ToList().Select(x => $"{x._id}:{x.ProjectId}").ToList();
            return null;

        }

        internal static bool ChangeProjectId(string websiteid, string projectid)
        {
            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();
                var websiteCollection = _kitsuneDatabase.GetCollection<KitsuneWebsiteCollection>(KitsuneWebsiteCollectionName);
                var websiteUserCollection = _kitsuneDatabase.GetCollection<KitsuneWebsiteUserCollection>(KitsuneWebsiteUserCollectionName);
                var productionProjectCollection = _kitsuneDatabase.GetCollection<ProductionKitsuneProject>(ProductionProjectCollectionName);
                var existingWebsite = websiteCollection.Find(x => x.IsActive == true && x._id == websiteid).Project<KitsuneWebsiteCollection>(new ProjectionDefinitionBuilder<KitsuneWebsiteCollection>().Include(y => y.ProjectId)).FirstOrDefault();

                if (existingWebsite != null)
                {
                    if (existingWebsite.ProjectId == projectid)
                        return true;
                    else
                    {
                        var project = productionProjectCollection.Find(x => x.ProjectId == projectid)
                                                                 .Project<ProductionKitsuneProject>(new ProjectionDefinitionBuilder<ProductionKitsuneProject>().Include(y => y.Version).Include(y => y.UserEmail))
                                                                 .FirstOrDefault();
                        if (project == null)
                            throw new Exception("Projectid invalid");
                        var developerId = MongoConnector.GetUserIdFromUserEmail(new GetUserIdRequestModel { UserEmail = project.UserEmail });
                        var updateDefinitionWebsite = new UpdateDefinitionBuilder<KitsuneWebsiteCollection>()
                            .Set(x => x.ProjectId, projectid)
                            .Set(x => x.KitsuneProjectVersion, project.Version)
                            .Set(x => x.DeveloperId, developerId.Id)
                            .Set(x => x.UpdatedOn, DateTime.UtcNow);
                        var updateDefinitionWebsiteUser = new UpdateDefinitionBuilder<KitsuneWebsiteUserCollection>()
                           .Set(x => x.DeveloperId, developerId.Id)
                           .Set(x => x.UpdatedOn, DateTime.UtcNow);

                        var updateResultWebsite = websiteCollection.UpdateOne(new FilterDefinitionBuilder<KitsuneWebsiteCollection>().Eq(x => x._id, websiteid), updateDefinitionWebsite);
                        var updateResultWebsiteUser = websiteUserCollection.UpdateOne(new FilterDefinitionBuilder<KitsuneWebsiteUserCollection>().Eq(x => x.WebsiteId, websiteid), updateDefinitionWebsiteUser);


                        if (updateResultWebsite != null && updateResultWebsite.IsAcknowledged && updateResultWebsite.IsModifiedCountAvailable)
                        {
                            new CacheServices().InvalidateDataCache(websiteid);

                            return true;
                        }
                        return false;
                    }
                }
                else
                    throw new Exception($"Website not found with websiteid : {websiteid}");
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        internal static CommonAPIResponse IsCallTrackerEnabledForWebsite(string websiteId)
        {
            if (String.IsNullOrEmpty(websiteId))
            {
                throw new ArgumentNullException(nameof(websiteId));
            }

            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();
                var response = new IsCallTrackerEnabledForWebsiteResponse
                {
                    isActive = false
                };
                var websiteDetailsResponse = GetKitsuneWebsiteDetails(websiteId);
                if (websiteDetailsResponse?.Response != null)
                {
                    var websiteDetails = (WebsiteDetailsResponseModel)websiteDetailsResponse.Response;
                    var componentEnabled = MongoConnector.IsAppEnabled(websiteDetails.ProjectId, EnvConstants.Constants.ComponentId.callTracker);
                    if (componentEnabled.IsActive)
                    {
                        var callTrackerEnabledDomains = GetCallTrackerDomainsFromConfig(websiteId, websiteDetails.ProjectId);
                        if (callTrackerEnabledDomains.Contains(EnvConstants.Constants.callTrackerAllWebsitesIdentifier) || callTrackerEnabledDomains.Contains(websiteDetails.WebsiteUrl))
                            response.isActive = true;
                    }
                    return new CommonAPIResponse { Response = response };
                }
                return websiteDetailsResponse;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        internal static CommonAPIResponse GetCacheInvalidationStatus(string websiteId)
        {
            if (String.IsNullOrEmpty(websiteId))
            {
                throw new ArgumentNullException(nameof(websiteId));
            }

            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();
                var response = new GetWebsiteCacheInvalidationResult()
                {
                    IsCDNToggleAvailable = true
                };
                var websiteDetailsResponse = GetKitsuneWebsiteDetails(websiteId);
                if (websiteDetailsResponse?.Response != null)
                {
                    var websitesCacheCollection = _kitsuneDatabase.GetCollection<WebsiteCacheStatus>(KitsuneWebsiteCacheStatusCollectionName);

                    var websiteDetails = (WebsiteDetailsResponseModel)websiteDetailsResponse.Response;
                    var cloudProvider = MongoConnector.GetCloudProviderDetails(websiteDetails.ProjectId);

                    if(cloudProvider.provider != CloudProvider.AliCloud)
                    {
                        response.IsCDNToggleAvailable = false;
                    }

                    var cacheWebsiteEntry = websitesCacheCollection.Find(x => x.WebsiteId == websiteId && x.CloudProvider == cloudProvider.provider).FirstOrDefault();
                    if (cacheWebsiteEntry != null)
                    {
                        response.IsInvalidationEnabled = cacheWebsiteEntry.Enabled;
                        response.LastInvalidation = cacheWebsiteEntry.LastInvalidate;
                        response.NextInvalidation = cacheWebsiteEntry.NextInvalidate;
                    }

                    return new CommonAPIResponse { Response = response };
                }
                return websiteDetailsResponse;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        internal static CommonAPIResponse UpdateCacheInvalidationStatus(string websiteId, int nextInvalidationInSec = 0, bool? enable = null)
        {
            if (String.IsNullOrEmpty(websiteId))
            {
                throw new ArgumentNullException(nameof(websiteId));
            }

            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();
                var response = new GetWebsiteCacheInvalidationResult();
                var websiteDetailsResponse = GetKitsuneWebsiteDetails(websiteId);
                if (websiteDetailsResponse?.Response != null)
                {
                    var websitesCacheCollection = _kitsuneDatabase.GetCollection<WebsiteCacheStatus>(KitsuneWebsiteCacheStatusCollectionName);

                    var websiteDetails = (WebsiteDetailsResponseModel)websiteDetailsResponse.Response;
                    var cloudProvider = MongoConnector.GetCloudProviderDetails(websiteDetails.ProjectId);
                    var updateDefinitionBuild = new UpdateDefinitionBuilder<WebsiteCacheStatus>();
                    var updateDefinition = updateDefinitionBuild.SetOnInsert(x => x.WebsiteId, websiteId)
                        .SetOnInsert(x => x.CloudProvider, cloudProvider.provider);


                    if (enable != null)
                        updateDefinition = updateDefinition.Set(x => x.Enabled, enable);
                    else
                    {
                        updateDefinition = updateDefinition.Set(x => x.LastInvalidate, DateTime.UtcNow)
                        .Set(x => x.NextInvalidate, DateTime.UtcNow.AddSeconds(nextInvalidationInSec));
                    }

                    var cacheWebsiteEntry = websitesCacheCollection.UpdateOne((x => x.WebsiteId == websiteId && x.CloudProvider == CloudProvider.AliCloud), updateDefinition, new UpdateOptions
                    {
                        IsUpsert = true
                    });

                    return new CommonAPIResponse { Response = true };
                }
                return new CommonAPIResponse { Response = $"Website not found with the website id : {websiteId}", StatusCode = System.Net.HttpStatusCode.BadRequest };
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
