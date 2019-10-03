
using Amazon;
using AmazonAWSHelpers.SQSQueueHandler;
using AWS.Services.S3Helper;
using Kitsune.API.Model.ApiRequestModels;
using Kitsune.API.Model.ApiRequestModels.Application;
using Kitsune.API2.EnvConstants;
using Kitsune.API2.Models;
using Kitsune.API2.Utils;
using Kitsune.Helper;
using Kitsune.Models;
using Kitsune.Models.BuildAndRunModels;
using Kitsune.Models.Cloud;
using Kitsune.Models.CollectionModels;
using Kitsune.Models.Krawler;
using Kitsune.Models.Project;
using Kitsune.Models.ProjectModels;
using Kitsune.Models.PublishModels;
using Kitsune.Models.WebsiteModels;
using Kitsune.Models.ZipServiceModels;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using static Kitsune.API2.EnvConstants.Constants;
using static Kitsune.API2.Utils.SiteMapGenerator;

namespace Kitsune.API2.DataHandlers.Mongo
{
    /// <summary>
    /// Project related mongo functions
    /// </summary>
    public static partial class MongoConnector
    {
        #region IDE/Dashboard related

        internal static KitsuneProject GetProjectDetails(string projectId)
        {
            if (projectId == null)
            {
                throw new ArgumentNullException(nameof(projectId));
            }
            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();
                var ProjectCollection = _kitsuneDatabase.GetCollection<KitsuneProject>(KitsuneProjectsCollectionName);
                return ProjectCollection.Find(x => x.ProjectId == projectId).FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        internal static KitsuneAppProjectsResponse ListAppProjects(string filter = null, string sort = null, int skip = 0, int limit = 100)
        {
            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();

                var projectCollection = _kitsuneDatabase.GetCollection<KitsuneProject>(KitsuneProjectsCollectionName);

                var filterDefinitionBuilder = new FilterDefinitionBuilder<KitsuneProject>();
                var reqFilterDefinition = filterDefinitionBuilder.Empty;
                var sortDoc = new BsonDocument("CreatedOn", -1);
                var filterDoc = new BsonDocument();

                if (!string.IsNullOrEmpty(filter))
                {
                    if (BsonDocument.TryParse(filter, out filterDoc))
                        reqFilterDefinition = filterDoc;
                }
                if (!string.IsNullOrEmpty(sort))
                {
                    var tempSort = new BsonDocument();
                    if (BsonDocument.TryParse(sort, out tempSort))
                        sortDoc = tempSort;
                }
                var appFilterDef = new BsonDocument(nameof(KitsuneProject.ProjectType), ProjectType.APP.ToString());
                var finalFilter = filterDefinitionBuilder.And(appFilterDef, reqFilterDefinition);

                var count = projectCollection.Count(finalFilter);
                var projects = projectCollection.Find(finalFilter).Sort(sort).Skip(skip).Limit(limit).ToList();

                return new KitsuneAppProjectsResponse
                {
                    Pagination = new Pagination
                    {
                        CurrentIndex = skip,
                        PageSize = limit,
                        TotalCount = count
                    },
                    Projects = projects
                };
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        internal static KitsuneAppStatusResponse IsAppEnabled(string projectId, string componentId)
        {
            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();

                var component = MongoConnector.GetProjectDetails(componentId);
                if (component == null || component.ProjectType != Kitsune.Models.Project.ProjectType.APP)
                {
                    return new KitsuneAppStatusResponse
                    {
                        IsActive = false,
                        IsError = true,
                        Message = "Component not found!"
                    };
                }

                var project = MongoConnector.GetProjectDetails(projectId);
                if (project != null)
                {
                    var exists = project.Components?.Any(x => x.ProjectId == componentId);
                    if (exists == true)
                    {
                        return new KitsuneAppStatusResponse
                        {
                            IsActive = true,
                            IsError = false
                        };
                    }
                    return new KitsuneAppStatusResponse
                    {
                        IsActive = false,
                        IsError = false,
                    };
                }
                return new KitsuneAppStatusResponse
                {
                    IsActive = false,
                    IsError = true,
                    Message = "Project not found!"
                };
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        internal static void UpdateProjectComponents(string projectId, List<ProjectComponent> components)
        {
            if (projectId == null)
                throw new ArgumentNullException(nameof(projectId));

            if (components == null)
                throw new ArgumentNullException(nameof(components));

            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();

                var projectCollection = _kitsuneDatabase.GetCollection<KitsuneProject>(KitsuneProjectsCollectionName);
                var updateDef = new UpdateDefinitionBuilder<KitsuneProject>();
                var filterDef = new FilterDefinitionBuilder<KitsuneProject>();

                projectCollection.UpdateOne(filterDef.Eq(x => x.ProjectId, projectId), updateDef.Set(x => x.Components, components));


            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        internal static void DisableComponentChanges(string projectId, ProjectComponent components, string userId, string schemaId)
        {
            if (projectId == null)
                throw new ArgumentNullException(nameof(projectId));

            if (components == null)
                throw new ArgumentNullException(nameof(components));

            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();

                if (components.ProjectId == ComponentId.callTracker)
                {
                    var websitesToDisable = (GetWebsitesResponseModel)GetKitsuneWebsiteListForProject(userId, projectId).Response;
                    var projectDetails = MongoConnector.GetProjectDetails(projectId);
                    var schema = MongoConnector.GetLanguageEntity(new GetLanguageEntityRequestModel
                    {
                        EntityId = schemaId,
                        UserId = userId
                    });
                    if (websitesToDisable.Websites.Any() && schema != null)
                    {
                        DisableVMNs(null, websitesToDisable.Websites.Select(x => x.WebsiteId).ToList());
                        foreach (var website in websitesToDisable.Websites)
                        {
                            var callTrackerFieldsToUpdate = MongoConnector.PhoneNumberDatatTypeDetails(website.WebsiteId, userId, schema.EntityName);
                            List<UpdateDataItem> UpdateFieldsDetails = new List<UpdateDataItem>();
                            foreach (var field in callTrackerFieldsToUpdate)
                            {
                                var query = new PhoneNumberDatatTypeDetailsResponse
                                {
                                    _kid = field._kid,
                                    _parentClassId = field._parentClassId,
                                    _parentClassName = field._parentClassName,
                                    _propertyName = field._propertyName
                                };
                                var serializeQuery = JsonConvert.SerializeObject(query);
                                UpdateFieldsDetails.Add(new UpdateDataItem
                                {
                                    Query = serializeQuery,
                                    UpdateValue = new Dictionary<string, object> {
                                        { "calltrackernumber", string.Empty },
                                        { "isactive", false }
                                    }
                                });
                            }
                            var updateResult = MongoConnector.UpdateWebsiteData(new UpdateWebsiteDataRequestModel
                            {
                                BulkUpdates = UpdateFieldsDetails,
                                UserId = userId,
                                WebsiteId = website.WebsiteId,
                                SchemaName = schema.EntityName,
                            });
                        }
                    }
                }


            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        internal static string GetClientIdFromWebsiteId(string projectId)
        {
            if (projectId == null)
            {
                throw new ArgumentNullException(nameof(projectId));
            }
            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();

                var websiteCollection = _kitsuneDatabase.GetCollection<KitsuneWebsiteCollection>(KitsuneWebsiteCollectionName);
                return websiteCollection.Find(x => x.ProjectId == projectId && x.ProjectId != null).FirstOrDefault().ClientId;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        internal static string GetClientIdFromProjectId(string projectId)
        {
            if (projectId == null)
            {
                throw new ArgumentNullException(nameof(projectId));
            }
            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();

                var websiteCollection = _kitsuneDatabase.GetCollection<KitsuneProject>(KitsuneProjectsCollectionName);
                return websiteCollection.Find(x => x.ProjectId == projectId).Project<KitsuneProject>(Builders<KitsuneProject>.Projection.Include(x => x.ClientId))?.FirstOrDefault()?.ClientId;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        internal static GetProjectDetailsResponseModel GetProjectDetails(GetProjectDetailsRequestModel requestModel)
        {
            if (requestModel == null)
            {
                throw new ArgumentNullException(nameof(requestModel));
            }
            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();


                var ProjectCollection = _kitsuneDatabase.GetCollection<KitsuneProject>(KitsuneProjectsCollectionName);
                var ResourceCollection = _kitsuneDatabase.GetCollection<KitsuneResource>(KitsuneResourceCollectionName);
                var ProjectProject = new ProjectionDefinitionBuilder<KitsuneProject>();
                var ProjectResource = new ProjectionDefinitionBuilder<KitsuneResource>();
                Expression<Func<KitsuneProject, bool>> expr;
                if (!string.IsNullOrEmpty(requestModel.ProjectId) && requestModel.ProjectId != "null")
                    expr = (x) => x.ProjectId == requestModel.ProjectId && x.IsArchived == false && x.UserEmail == requestModel.UserEmail;
                else
                    expr = (x) => x.IsArchived == false && x.UserEmail == requestModel.UserEmail;
                var ProjectDetails = ProjectCollection.Find(expr).Project<GetProjectDetailsResponseModel>(ProjectProject.Exclude(x => x.UserEmail).Exclude(x => x.IsArchived)).SortByDescending(x => x.UpdatedOn).FirstOrDefault();
                if (ProjectDetails != null)
                {
                    List<ResourceItemMeta> Resources = new List<ResourceItemMeta>();
                    if (!requestModel.ExcludeResources)
                    {
                        Resources = ResourceCollection.Find(x => x.ProjectId == ProjectDetails.ProjectId && x.IsArchived == false).Project<ResourceItemMeta>(ProjectResource.Include(x => x.SourcePath)
                                                                                                                                            .Include(x => x.UrlPattern)
                                                                                                                                            .Include(x => x.ClassName)
                                                                                                                                            .Include(x => x.IsStatic)
                                                                                                                                            .Include(x => x.IsDefault)
                                                                                                                                            .Include(x => x.PageType)
                                                                                                                                            .Include(x => x.KObject)
                                                                                                                                            .Include(x => x.Errors)
                                                                                                                                            .Include(x => x.CreatedOn)
                                                                                                                                            .Include(x => x.ProjectId)
                                                                                                                                            .Include(x => x.UrlPatternRegex)
                                                                                                                                            .Include(x => x.Version)
                                                                                                                                            .Include(x => x.ResourceType)
                                                                                                                                            .Include(x => x.MetaData)
                                                                                                                                            .Include(x => x.UpdatedOn)
                                                                                                                                            .Include(x => x.Offset)
                                                                                                                                            .Include(x => x.CustomVariables)
                                                                                                                                            .Include(x => x.OptimizedPath)).SortBy(x => x.SourcePath).ToList();
                    }


                    ProjectDetails.Resources = Resources;
                    return ProjectDetails;
                }
                return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        internal static GetProjectStatusResponseModel GetProjectStatus(GetProjectStatusRequestModel requestModel)
        {
            if (requestModel == null)
            {
                throw new ArgumentNullException(nameof(requestModel));
            }
            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();


                var ProjectCollection = _kitsuneDatabase.GetCollection<KitsuneProject>(KitsuneProjectsCollectionName);
                var ProjectProject = new ProjectionDefinitionBuilder<KitsuneProject>();
                var ProjectResource = new ProjectionDefinitionBuilder<KitsuneResource>();
                Expression<Func<KitsuneProject, bool>> expr;

                expr = (x) => x.ProjectId == requestModel.ProjectId && x.IsArchived == false;
                var ProjectDetails = ProjectCollection.Find(expr).Project<GetProjectDetailsResponseModel>(ProjectProject.Exclude(x => x.UserEmail).Exclude(x => x.IsArchived)).SortByDescending(x => x.UpdatedOn).FirstOrDefault();
                if (ProjectDetails == null)
                    throw new Exception("ProjectId Not Found");
                return new GetProjectStatusResponseModel { ProjectStatus = ProjectDetails.ProjectStatus.ToString() };
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        internal static string CreateOrUpdateProject(CreateOrUpdateProjectRequestModel requestModel)
        {
            if (requestModel == null)
            {
                throw new ArgumentNullException(nameof(requestModel));
            }
            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();

                var ProjectCollection = _kitsuneDatabase.GetCollection<KitsuneProject>(KitsuneProjectsCollectionName);

                //if themeId is null then create new theme otherwise update the theme
                if (string.IsNullOrEmpty(requestModel.ProjectId))
                {

                    var projectId = MongoConnector.CreateNewProject(requestModel.ProjectName, requestModel.UserEmail, requestModel.ClientId, requestModel.CompilerVersion, ProjectType.NEWPROJECT, ProjectStatus.IDLE);


                    return projectId;
                }
                else
                {
                    //update existing theme
                    var Update = new UpdateDefinitionBuilder<KitsuneProject>();
                    var updateDef = Update.Set(x => x.UpdatedOn, DateTime.Now);
                    if (!string.IsNullOrEmpty(requestModel.ProjectName))
                        updateDef = updateDef.Set(x => x.ProjectName, requestModel.ProjectName);
                    if (requestModel.ProjectStatus != null)
                        updateDef = updateDef.Set(x => x.ProjectStatus, requestModel.ProjectStatus);

                    if (requestModel.CompilerVersion > 0)
                        updateDef = updateDef.Set(x => x.CompilerVersion, requestModel.CompilerVersion);

                    var Result = ProjectCollection.UpdateOne(x => x.ProjectId == requestModel.ProjectId && x.UserEmail == requestModel.UserEmail, updateDef);

                    if (Result.ModifiedCount == 1)
                        return requestModel.ProjectId;
                    return null;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        internal static string CreateWordPressProject(CreateWordPressProjectRequestModel requestModel)
        {
            if (requestModel == null)
            {
                throw new ArgumentNullException(nameof(requestModel));
            }
            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();

                var ProjectCollection = _kitsuneDatabase.GetCollection<KitsuneProject>(KitsuneProjectsCollectionName);
                var WordpressCollection = _kitsuneDatabase.GetCollection<KitsuneWordPress>(KitsuneWordPressCollection);

                #region Create new Project in DB

                var projectId = MongoConnector.CreateNewProject(requestModel.ProjectName, requestModel.UserEmail, requestModel.ClientId, 0, ProjectType.WORDPRESS);

                #endregion

                #region Create KitsuneWordPress Collection

                KitsuneWordPress wordpressObject = new KitsuneWordPress()
                {
                    CreatedOn = DateTime.Now,
                    ProjectId = projectId,
                    Stage = KitsuneWordPressStats.INITIALISING
                };
                WordpressCollection.InsertOne(wordpressObject);

                #endregion

                APIHelper.CreateNewtWordPressInstance(projectId);

                return projectId;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        internal static bool UpdateProjectVersion(UpdateProjectVersionRequestModel requestModel)
        {
            if (requestModel == null)
            {
                throw new ArgumentNullException(nameof(requestModel));
            }

            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();


                var ProjectCollection = _kitsuneDatabase.GetCollection<KitsuneProject>(KitsuneProjectsCollectionName);
                var Update = new UpdateDefinitionBuilder<KitsuneProject>();
                var UpdateDef = Update.Set(x => x.Version, requestModel.Version)
                                      .Set(x => x.UpdatedOn, DateTime.Now);
                if (requestModel.PublishedVersion != 0)
                    UpdateDef = UpdateDef.Set(x => x.PublishedVersion, requestModel.PublishedVersion);
                var updateResult = ProjectCollection.UpdateOne(x => x.ProjectId == requestModel.ProjectId && x.UserEmail == requestModel.UserEmail, UpdateDef);
                if (updateResult.ModifiedCount != 1)
                    return false;
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        internal static bool CreateOrUpdateResource(CreateOrUpdateResourceRequestModel requestModel)
        {
            if (requestModel == null)
            {
                throw new ArgumentNullException(nameof(requestModel));
            }

            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();

                var dateTimeNow = DateTime.Now;
                var ProjectCollection = _kitsuneDatabase.GetCollection<KitsuneProject>(KitsuneProjectsCollectionName);
                var ResourceCollection = _kitsuneDatabase.GetCollection<KitsuneResource>(KitsuneResourceCollectionName);

                var KitsuneProject = ProjectCollection.Find(x => x.ProjectId == requestModel.ProjectId && x.IsArchived == false && x.UserEmail == requestModel.UserEmail).FirstOrDefault();
                if (KitsuneProject == null)
                    return false;//Theme Doesn't exist

                var resourceType = requestModel.ResourceType;
                if (resourceType == null)
                {
                    var extension = requestModel.SourcePath.Split(new char[] { '.' }).LastOrDefault().Split(new char[] { '?', '#' }).FirstOrDefault().ToLower();
                    switch (extension)
                    {
                        case "html":
                        case "htm":
                        case "dl":
                            resourceType = ResourceType.LINK;
                            break;
                        case "css":
                            resourceType = ResourceType.STYLE;
                            break;
                        case "js":
                            resourceType = ResourceType.SCRIPT;
                            break;
                        case "zip":
                            resourceType = ResourceType.APPLICATION;
                            break;
                        default:
                            resourceType = ResourceType.FILE;
                            break;
                    }
                }
                string status = string.Empty;
                if (resourceType == ResourceType.APPLICATION && requestModel.PageType == KitsunePageType.LARAVEL)
                {
                    status = LaravelStatus.EMPTY.ToString();
                }

                requestModel.SourcePath = Kitsune.Helper.KitsuneCommonUtils.FormatResourceFileName(requestModel.SourcePath);
                if(requestModel.ByteArrayStream != null)
                {
                    var resourceS3Key = AmazonS3FileProcessor.SaveFileContentToS3(requestModel.PageType?.ToString() +"/"+ requestModel.ProjectId, AmazonAWSConstants.ApplicationBucketName, requestModel.SourcePath, null, byteArrayStream: requestModel.ByteArrayStream);
                    //Create json file with same name as application name to avoide build error in optimiser
                    AmazonS3FileProcessor.SaveFileContentToS3(requestModel.ProjectId, AmazonAWSConstants.SourceBucketName, requestModel.SourcePath, JsonConvert.SerializeObject(new MetaData() { Configuration = requestModel.Configuration, Status = status }));

                }
                else
                {
                    var resourceS3Key = AmazonS3FileProcessor.SaveFileContentToS3(requestModel.ProjectId, AmazonAWSConstants.SourceBucketName, requestModel.SourcePath, requestModel.FileContent);

                }
               
               
                var Update = new UpdateDefinitionBuilder<KitsuneResource>();
                var _idOBjectId = ObjectId.GenerateNewId();
                var UpDateDef = Update.SetOnInsert(x => x.SourcePath, requestModel.SourcePath)
                                      .SetOnInsert(x => x.ProjectId, requestModel.ProjectId)
                                      .SetOnInsert(x => x.CreatedOn, dateTimeNow)
                                      .Set(x => x.Errors, requestModel.Errors)
                                      .Set(x => x.IsArchived, false)
                                      .Set(x => x.IsStatic, requestModel.IsStatic)
                                      .Set(x => x.ResourceType, resourceType)
                                      .Set(x => x.KObject, requestModel.KObject)
                                      .Set(x => x.UpdatedOn, dateTimeNow)
                                      .Set(x => x.Offset, requestModel.Offset)
                                      .Set(x => x.CustomVariables, requestModel.CustomVariables)
                                      .Set(x => x.MetaData, new MetaData() { Configuration = requestModel.Configuration, Status = status })
                                      .Set(x => x.Version, KitsuneProject.Version);

                if (!string.IsNullOrEmpty(requestModel.UrlPattern))
                    UpDateDef = UpDateDef.Set(x => x.UrlPattern, requestModel.UrlPattern);
                if (requestModel.PageType != null)
                    UpDateDef = UpDateDef.Set(x => x.PageType, requestModel.PageType);
                if (!string.IsNullOrEmpty(requestModel.ClassName))
                    UpDateDef = UpDateDef.Set(x => x.ClassName, requestModel.ClassName);
                if (!string.IsNullOrEmpty(requestModel.UrlPatternRegex))
                    UpDateDef = UpDateDef.Set(x => x.UrlPatternRegex, requestModel.UrlPatternRegex);

              

                var Result = ResourceCollection.UpdateOne(x => x.ProjectId == requestModel.ProjectId && x.SourcePath == requestModel.SourcePath, UpDateDef, new UpdateOptions { IsUpsert = true });
                #region Push to Application SQS
                if (resourceType == ResourceType.APPLICATION && requestModel.ByteArrayStream != null)
                {
                    var applicationQueue = new AmazonSQSQueueHandlers<ApplicationBuildQueueModel>(AmazonAWSConstants.ApplicationBuildSQSUrl);
                    var queueresponse = applicationQueue.PushMessageToQueue(new ApplicationBuildQueueModel
                    {
                        BuildVersion = KitsuneProject.Version,
                        ProjectId = requestModel.ProjectId,
                        ApplicationId = requestModel.SourcePath,
                        UserEmail = requestModel.UserEmail,
                        ApplicationType = requestModel.PageType.ToString(),
                        SourceLocation = "https://s3.ap-south-1.amazonaws.com/" + AmazonAWSConstants.ApplicationBucketName + "/" + requestModel.PageType.ToString() + "/" + requestModel.ProjectId + requestModel.SourcePath,
                        Configuration = requestModel.Configuration
                    }, EnvironmentConstants.ApplicationConfiguration.AWSSQSConfiguration.AWS_AccessKey, EnvironmentConstants.ApplicationConfiguration.AWSSQSConfiguration.AWS_SecretKey, RegionEndpoint.GetBySystemName(EnvironmentConstants.ApplicationConfiguration.Defaults.AWSSQSRegion));


                }
                #endregion
                //TODO : Check the modified count or created
                return true;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        internal static GetProjectsListResponseModel GetProjectsList(GetProjectsListRequestModel requestModel)
        {
            if (requestModel == null)
            {
                throw new ArgumentNullException(nameof(requestModel));
            }

            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();

                var projectCollection = _kitsuneDatabase.GetCollection<KitsuneProject>(KitsuneProjectsCollectionName);

                //get all collections
                var pdb = new ProjectionDefinitionBuilder<KitsuneProject>();
                Expression<Func<KitsuneProject, bool>> expr;
                //if (User.getAll)
                //    expr = (x) => x.IsArchived == false;
                //else
                expr = (x) => x.UserEmail == requestModel.UserEmail && x.IsArchived == false;

                var projects = projectCollection.Find(expr).SortByDescending(x => x.CreatedOn).Project<GetProjectsList>(pdb
                                                                                                    .Include(x => x.ProjectId)
                                                                                                    .Include(x => x.ProjectName)
                                                                                                    .Include(x => x.ScreenShotUrl)
                                                                                                    .Include(x => x.CreatedOn)
                                                                                                    .Include(x => x.SchemaId)
                                                                                                    .Include(x => x.ProjectType)
                                                                                                    .Include(x => x.Version)
                                                                                                    .Include(x => x.ProjectStatus)).ToList();

                return new GetProjectsListResponseModel { Projects = projects };
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        internal static GetProjectsListResponseModelV2 GetProjectsListWithPagination(GetProjectsListRequestModelV2 requestModel)
        {
            if (requestModel == null)
            {
                throw new ArgumentNullException(nameof(requestModel));
            }

            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();

                var projectCollection = _kitsuneDatabase.GetCollection<KitsuneProject>(KitsuneProjectsCollectionName);

                //get all collections
                var pdb = new ProjectionDefinitionBuilder<KitsuneProject>();
                Expression<Func<KitsuneProject, bool>> expr;
                //if (User.getAll)
                //    expr = (x) => x.IsArchived == false;
                //else
                expr = (x) => x.UserEmail == requestModel.UserEmail && x.IsArchived == false;
                var total = projectCollection.CountDocuments(expr);

                var projects = projectCollection.Find(expr).SortByDescending(x => x.CreatedOn).Project<GetProjectsList>(pdb
                                                                                                    .Include(x => x.ProjectId)
                                                                                                    .Include(x => x.ProjectName)
                                                                                                    .Include(x => x.ScreenShotUrl)
                                                                                                    .Include(x => x.CreatedOn)
                                                                                                    .Include(x => x.SchemaId)
                                                                                                    .Include(x => x.ProjectType)
                                                                                                    .Include(x => x.Version)
                                                                                                    .Include(x => x.ProjectStatus))
                                                                                                .Skip(requestModel.Skip)
                                                                                                .Limit(requestModel.Limit)
                                                                                                .ToList();

                return new GetProjectsListResponseModelV2 { Projects = projects, Extra = new Pagination { CurrentIndex = requestModel.Skip, PageSize = requestModel.Limit, TotalCount = total } };
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        internal static GetResourceDetailsResponseModel GetResourceDetails(GetResourceDetailsRequestModel requestModel)
        {
            if (requestModel == null)
            {
                throw new ArgumentNullException(nameof(requestModel));
            }

            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();


                var ResourceCollection = _kitsuneDatabase.GetCollection<KitsuneResource>(KitsuneResourceCollectionName);

                var ResourceProject = new ProjectionDefinitionBuilder<KitsuneResource>();

                var resourceDetails = ResourceCollection.Find(x => x.ProjectId == requestModel.ProjectId && x.SourcePath == requestModel.SourcePath && x.IsArchived == false).Project<GetResourceDetailsResponseModel>(ResourceProject.Include(x => x.SourcePath)
                                                                                                                                                                            .Include(x => x.IsArchived)
                                                                                                                                                                            .Include(x => x.ClassName)
                                                                                                                                                                            .Include(x => x.OptimizedPath)
                                                                                                                                                                            .Include(x => x.Errors)
                                                                                                                                                                            .Include(x => x.CreatedOn)
                                                                                                                                                                            .Include(x => x.UrlPattern)
                                                                                                                                                                            .Include(x => x.IsStatic)
                                                                                                                                                                            .Include(x => x.PageType)
                                                                                                                                                                            .Include(x => x.ResourceType)
                                                                                                                                                                            .Include(x => x.ProjectId)
                                                                                                                                                                            .Include(x => x.IsDefault)
                                                                                                                                                                            .Include(x => x.KObject)
                                                                                                                                                                            .Include(x => x.UpdatedOn)).FirstOrDefault();
                if (resourceDetails != null)
                    resourceDetails.HtmlSourceString = AmazonS3FileProcessor.getFileFromS3(resourceDetails.SourcePath, requestModel.ProjectId, AmazonAWSConstants.SourceBucketName);
                return resourceDetails;
            }
            catch (Exception ex)
            {
                //logger.Error("Testig Logging");
                return null;
            }
        }
        internal static GetResourceDetailsResponseModel GetProductionResourceDetails(GetResourceDetailsRequestModel requestModel)
        {
            if (requestModel == null)
            {
                throw new ArgumentNullException(nameof(requestModel));
            }

            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();


                var ResourceCollection = _kitsuneDatabase.GetCollection<ProductionKitsuneResource>(ProductionResorcesCollectionName);

                var ResourceProject = new ProjectionDefinitionBuilder<ProductionKitsuneResource>();

                var resourceDetails = ResourceCollection.Find(x => x.ProjectId == requestModel.ProjectId && x.SourcePath == requestModel.SourcePath).Project<GetResourceDetailsResponseModel>(ResourceProject.Include(x => x.SourcePath)
                                                                                                                                                                            .Include(x => x.ClassName)
                                                                                                                                                                            .Include(x => x.OptimizedPath)
                                                                                                                                                                            .Include(x => x.Errors)
                                                                                                                                                                            .Include(x => x.CreatedOn)
                                                                                                                                                                            .Include(x => x.UrlPattern)
                                                                                                                                                                            .Include(x => x.IsStatic)
                                                                                                                                                                            .Include(x => x.PageType)
                                                                                                                                                                            .Include(x => x.ResourceType)
                                                                                                                                                                            .Include(x => x.ProjectId)
                                                                                                                                                                            .Include(x => x.IsDefault)
                                                                                                                                                                            .Include(x => x.KObject)
                                                                                                                                                                            .Include(x => x.Version)
                                                                                                                                                                            .Include(x => x.UpdatedOn)).FirstOrDefault();
                if (resourceDetails != null)
                    resourceDetails.HtmlSourceString = AmazonS3FileProcessor.getFileFromS3(resourceDetails.SourcePath, requestModel.ProjectId, AmazonAWSConstants.ProductionBucketName, version: resourceDetails.Version);
                return resourceDetails;
            }
            catch (Exception ex)
            {
                //logger.Error("Testig Logging");
                return null;
            }
        }

        internal static GetKitsuneResourceDetailsResponseModel GetKitsuneResourceDetails(GetResourceDetailsRequestModel requestModel)
        {
            if (requestModel == null)
            {
                throw new ArgumentNullException(nameof(requestModel));
            }

            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();


                var ResourceCollection = _kitsuneDatabase.GetCollection<KitsuneResource>(KitsuneResourceCollectionName);

                var ResourceProject = new ProjectionDefinitionBuilder<KitsuneResource>();

                var resourceDetails = ResourceCollection.Find(x => x.ProjectId == requestModel.ProjectId && x.SourcePath == requestModel.SourcePath && x.IsArchived == false).Project<GetKitsuneResourceDetailsResponseModel>(ResourceProject.Include(x => x.SourcePath)
                                                                                                                                                                            .Include(x => x.IsArchived)
                                                                                                                                                                            .Include(x => x.ClassName)
                                                                                                                                                                            .Include(x => x.OptimizedPath)
                                                                                                                                                                            .Include(x => x.Errors)
                                                                                                                                                                            .Include(x => x.CreatedOn)
                                                                                                                                                                            .Include(x => x.UrlPattern)
                                                                                                                                                                            .Include(x => x.IsStatic)
                                                                                                                                                                            .Include(x => x.PageType)
                                                                                                                                                                            .Include(x => x.ResourceType)
                                                                                                                                                                            .Include(x => x.IsDefault)
                                                                                                                                                                            .Include(x => x.KObject)
                                                                                                                                                                            .Include(x => x.MetaData)
                                                                                                                                                                            .Include(x => x.UpdatedOn)).FirstOrDefault();

                if (resourceDetails != null)
                    resourceDetails.File = AmazonS3FileProcessor.GetKitsuneFileFromS3(resourceDetails.SourcePath, requestModel.ProjectId, AmazonAWSConstants.SourceBucketName);
                return resourceDetails;
            }
            catch (Exception ex)
            {
                //logger.Error("Testig Logging");
                return null;
            }
        }

        internal static bool MakeResourceAsDefault(MakeResourceAsDefaultRequestModel requestModel)
        {
            if (requestModel == null)
            {
                throw new ArgumentNullException(nameof(requestModel));
            }

            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();
                var _resourceId = string.Empty;
                var projectCollection = _kitsuneDatabase.GetCollection<KitsuneProject>(KitsuneProjectsCollectionName);
                var ResourceCollection = _kitsuneDatabase.GetCollection<KitsuneResource>(KitsuneResourceCollectionName);

                var res = ResourceCollection.Find(x => x.ProjectId == requestModel.ProjectId && x.SourcePath == requestModel.SourcePath);
                if (res == null || !res.Any())
                    throw new Exception("Resource does not exist with SourcePath : " + requestModel.SourcePath);
                else
                    _resourceId = res.FirstOrDefault()._id;

                var updateResponse = ResourceCollection.UpdateMany((x => x.ProjectId == requestModel.ProjectId), new UpdateDefinitionBuilder<KitsuneResource>().Set(x => x.IsDefault, false));
                if (updateResponse.IsAcknowledged)
                {
                    var result = ResourceCollection.UpdateOne((x => x._id == _resourceId), new UpdateDefinitionBuilder<KitsuneResource>().Set(x => x.IsDefault, true));
                    if (result.IsAcknowledged && result.ModifiedCount == 1)
                        return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        internal static GetPartialPagesDetailsResponseModel GetPartialPagesDetails(GetPartialPagesDetailsRequestModel requestModel)
        {
            if (requestModel == null)
            {
                throw new ArgumentNullException(nameof(requestModel));
            }

            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();


                var ResourceCollection = _kitsuneDatabase.GetCollection<KitsuneResource>(KitsuneResourceCollectionName);
                var ResourceProject = new ProjectionDefinitionBuilder<KitsuneResource>();
                var fdb = new FilterDefinitionBuilder<KitsuneResource>();
                var fd = fdb.Eq(x => x.ProjectId, requestModel.ProjectId) & fdb.Eq(x => x.IsArchived, false) & fdb.Eq(x => x.PageType, KitsunePageType.PARTIAL);

                if (requestModel.SourcePaths != null)
                    fd = fd & fdb.In(x => x.SourcePath, requestModel.SourcePaths);

                var ResourceDetails = ResourceCollection.Find(fd).Project<ResourceDetails>(ResourceProject.Include(x => x.SourcePath)
                                                        .Include(x => x.IsArchived)
                                                        .Include(x => x.ClassName)
                                                        .Include(x => x.OptimizedPath)
                                                        .Include(x => x.Errors)
                                                        .Include(x => x.CreatedOn)
                                                        .Include(x => x.UrlPattern)
                                                        .Include(x => x.IsStatic)
                                                        .Include(x => x.PageType)
                                                        .Include(x => x.IsDefault)
                                                        .Include(x => x.KObject)
                                                        .Include(x => x.UpdatedOn)).ToList();
                foreach (var resource in ResourceDetails)
                {
                    resource.HtmlSourceString = AmazonS3FileProcessor.getFileFromS3(resource.SourcePath, requestModel.ProjectId, AmazonAWSConstants.SourceBucketName);
                }
                return new GetPartialPagesDetailsResponseModel { Resources = ResourceDetails };
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        internal static GetProjectWithResourcesDetailsResponseModel GetProjectWithResourceDetails(GetProjectWithResourcesDetailsRequestModel requestModel)
        {
            if (requestModel == null)
            {
                throw new ArgumentNullException(nameof(requestModel));
            }

            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();


                var projectCollection = _kitsuneDatabase.GetCollection<KitsuneProject>(KitsuneProjectsCollectionName);
                var ResourceCollection = _kitsuneDatabase.GetCollection<KitsuneResource>(KitsuneResourceCollectionName);
                var ProjectProject = new ProjectionDefinitionBuilder<KitsuneProject>();
                var ProjectResources = new ProjectionDefinitionBuilder<KitsuneResource>();
                var ProjectDetails = projectCollection.Find(x => x.ProjectId == requestModel.ProjectId && x.IsArchived == false && x.UserEmail == requestModel.UserEmail).Project<GetProjectWithResourcesDetailsResponseModel>(ProjectProject.Exclude(x => x.IsArchived)).FirstOrDefault(); //

                var Resources = ResourceCollection.Find(x => x.ProjectId == requestModel.ProjectId && x.IsArchived == false).Project<ResourceDetails>(ProjectResources.Include(x => x.SourcePath)
                                                                                                                                                        .Include(x => x.ClassName)
                                                                                                                                                        .Include(x => x.OptimizedPath)
                                                                                                                                                        .Include(x => x.IsStatic)
                                                                                                                                                        .Include(x => x.IsDefault)
                                                                                                                                                        .Include(x => x.PageType)
                                                                                                                                                        .Include(x => x.UrlPattern)
                                                                                                                                                        .Include(x => x.UpdatedOn)
                                                                                                                                                        .Include(x => x.KObject)
                                                                                                                                                        .Include(x => x.Errors)
                                                                                                                                                        .Include(x => x.ProjectId)
                                                                                                                                                        .Include(x => x.Version)
                                                                                                                                                        .Include(x => x.UrlPatternRegex)
                                                                                                                                                        .Include(x => x.ResourceType)).ToList();
                foreach (var Resource in Resources.Where(x => !x.IsStatic))
                {
                    Resource.HtmlSourceString = AmazonS3FileProcessor.getFileFromS3(Resource.SourcePath, requestModel.ProjectId, AmazonAWSConstants.SourceBucketName);
                }
                if (ProjectDetails != null)
                {
                    ProjectDetails.Resources = Resources;
                    return ProjectDetails;
                }
                return null;

            }
            catch (Exception ex)
            {
                return null;
            }
        }

        internal static bool DeleteProject(DeleteProjectRequestModel requestModel)
        {
            if (requestModel == null)
            {
                throw new ArgumentNullException(nameof(requestModel));
            }

            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();


                var ProjectCollection = _kitsuneDatabase.GetCollection<KitsuneProject>(KitsuneProjectsCollectionName);
                var ResourceCollection = _kitsuneDatabase.GetCollection<KitsuneResource>(KitsuneResourceCollectionName);
                //if themeId is null then create new theme otherwise update the theme


                //Delete existing theme
                var ProjectResult = ProjectCollection.UpdateOne(x => x.ProjectId == requestModel.ProjectId && x.UserEmail == requestModel.UserEmail,
                    new UpdateDefinitionBuilder<KitsuneProject>().Set(x => x.IsArchived, true));
                if (ProjectResult.ModifiedCount != 1)
                    return false;//theme couldnt be deleted

                //delete all pages of that particular theme
                var ReosurceResult = ResourceCollection.UpdateMany(x => x.ProjectId == requestModel.ProjectId,
                    new UpdateDefinitionBuilder<KitsuneResource>().Set(x => x.IsArchived, true));
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        internal static bool DeleteResource(DeleteResourceRequestModel requestModel)
        {
            if (requestModel == null)
            {
                throw new ArgumentNullException(nameof(requestModel));
            }

            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();


                var ProjectCollection = _kitsuneDatabase.GetCollection<KitsuneProject>(KitsuneProjectsCollectionName);
                var ResourceCollection = _kitsuneDatabase.GetCollection<KitsuneResource>(KitsuneResourceCollectionName);
                DateTime currentDateTime = DateTime.Now;

                //Delete existing theme
                if (!ProjectCollection.Find(x => x.ProjectId == requestModel.ProjectId && x.UserEmail == requestModel.UserEmail).Any())
                    return false;
                UpdateResult result = null;
                //Delete single resource
                if (!string.IsNullOrEmpty(requestModel.SourceName))
                    result = ResourceCollection.UpdateMany(new FilterDefinitionBuilder<KitsuneResource>()
                        .And(new FilterDefinitionBuilder<KitsuneResource>().Eq(x => x.ProjectId, requestModel.ProjectId),
                            new FilterDefinitionBuilder<KitsuneResource>().Eq(x => x.SourcePath, requestModel.SourceName)),
                    new UpdateDefinitionBuilder<KitsuneResource>().Set(x => x.IsArchived, true)
                                                                  .Set(x => x.UpdatedOn, currentDateTime));
                //Delete multiple/folder resource based on path starts with
                else if (!string.IsNullOrEmpty(requestModel.SourcePath))
                {
                    requestModel.SourcePath = requestModel.SourcePath.Trim().Trim('*');
                    result = ResourceCollection.UpdateMany(new FilterDefinitionBuilder<KitsuneResource>()
                        .And(new FilterDefinitionBuilder<KitsuneResource>().Eq(x => x.ProjectId, requestModel.ProjectId),
                            new FilterDefinitionBuilder<KitsuneResource>().Regex(x => x.SourcePath, new BsonRegularExpression(new Regex($"^{Regex.Escape(requestModel.SourcePath)}")))),
                       new UpdateDefinitionBuilder<KitsuneResource>().Set(x => x.IsArchived, true)
                                                                     .Set(x => x.UpdatedOn, currentDateTime));
                }

                if (result != null)
                    return result.IsAcknowledged && result.MatchedCount > 0;
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        internal static bool RenameResource(RenameResourceModel requestModel)
        {
            try
            {
                var existingResource = MongoConnector.GetResourceDetails(new GetResourceDetailsRequestModel { ProjectId = requestModel.ProjectId, UserEmail = requestModel.UserEmail, SourcePath = requestModel.OldSourceName });
                var newResourceCreated = MongoConnector.CreateOrUpdateResource(new CreateOrUpdateResourceRequestModel
                {
                    ClassName = existingResource.ClassName,
                    FileContent = existingResource.HtmlSourceString,
                    IsDefault = existingResource.IsDefault,
                    IsStatic = existingResource.IsStatic,
                    KObject = existingResource.KObject,
                    PageType = existingResource.PageType,
                    ProjectId = existingResource.ProjectId,
                    ResourceType = existingResource.ResourceType,
                    SourcePath = requestModel.NewSourceName,
                    UrlPattern = existingResource.UrlPattern,
                    UrlPatternRegex = existingResource.UrlPatternRegex,
                    UserEmail = requestModel.UserEmail
                });
                if (newResourceCreated)
                {
                    MongoConnector.DeleteResource(new DeleteResourceRequestModel
                    {
                        ProjectId = requestModel.ProjectId,
                        UserEmail = requestModel.UserEmail,
                        SourceName = requestModel.OldSourceName
                    });
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        internal static string UpdateProjectAccess(UpdateProjectAccessRequestModel requestModel)
        {
            if (requestModel == null)
            {
                throw new ArgumentNullException(nameof(requestModel));
            }

            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();

                var ProjectCollection = _kitsuneDatabase.GetCollection<KitsuneProject>(KitsuneProjectsCollectionName);
                var result = ProjectCollection.UpdateOne(x => x.ProjectId == requestModel.ProjectId,
                    new UpdateDefinitionBuilder<KitsuneProject>().Set(x => x.UserEmail, requestModel.UserEmail));
                if (result == null || result.MatchedCount == 0)
                    return "Project does not exist with the project id : " + requestModel.ProjectId;
                return "Success";
            }
            catch (Exception ex)
            {
                return "Something went wrong : " + ex.Message;
            }
        }

        internal static bool CreateOrUpdateKitsuneStatus(CreateOrUpdateKitsuneStatusRequestModel requestModel)
        {
            if (requestModel == null)
            {
                throw new ArgumentNullException(nameof(requestModel));
            }

            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();


                var ProjectCollection = _kitsuneDatabase.GetCollection<KitsuneProject>(KitsuneProjectsCollectionName);
                var ResourceCollection = _kitsuneDatabase.GetCollection<KitsuneResource>(KitsuneResourceCollectionName);
                var BuildStatusCollection = _kitsuneDatabase.GetCollection<KitsuneBuildStatus>(BuildStatusCollectionName);
                var projectIdFilter = Builders<KitsuneProject>.Filter.Eq(x => x.ProjectId, requestModel.ProjectId);
                var projectDetails = ProjectCollection.Find(projectIdFilter).FirstOrDefault();

                var update = Builders<KitsuneProject>.Update.Set(x => x.ProjectStatus, ProjectStatus.QUEUED);

                var buildExist = BuildStatusCollection.Find<KitsuneBuildStatus>((x => x.ProjectId == requestModel.ProjectId && x.BuildVersion == projectDetails.Version)).Limit(1).FirstOrDefault();


                var updateBuilder = new UpdateDefinitionBuilder<KitsuneBuildStatus>();


                var insertSuccess = false;

                if (requestModel.Stage == BuildStatus.Queued)
                {
                    //Dont process if the project is already processing
                    var projectActiveStatus = new List<ProjectStatus>() { ProjectStatus.ERROR, ProjectStatus.BUILDING, ProjectStatus.CRAWLING, ProjectStatus.PUBLISHING, ProjectStatus.QUEUED };
                    if (projectActiveStatus.Contains(projectDetails.ProjectStatus))
                        return false;

                    var insertBuildStatusDoc = new KitsuneBuildStatus { Stage = requestModel.Stage, BuildVersion = projectDetails.Version, CreatedOn = DateTime.UtcNow, ProjectId = requestModel.ProjectId, Error = new List<BuildError>(), Warning = new List<BuildError>() };
                    BuildStatusCollection.InsertOne(insertBuildStatusDoc);
                    insertSuccess = !string.IsNullOrEmpty(insertBuildStatusDoc._id);
                }
                else
                {
                    //var result = mongoHelper.BuildStatus.UpdateOne((x => x.ProjectId == requestModel.ProjectId && x.BuildVersion == projectDetails.Version), updateDef);
                    //insertSuccess = result.IsAcknowledged;
                    var updateDef = updateBuilder.Set(x => x.Stage, requestModel.Stage);
                    if (requestModel.Stage == BuildStatus.Error || requestModel.Stage == BuildStatus.Completed)
                        updateDef = updateDef.Set(x => x.IsCompleted, true);

                    if (requestModel.Error != null && requestModel.Error.Any())
                        updateDef = updateDef.Set(x => x.Error, requestModel.Error);
                    if (requestModel.Analyzer != null)
                        updateDef = updateDef.Set(x => x.Analyzer, requestModel.Analyzer);
                    if (requestModel.Compiler != null)
                        updateDef = updateDef.Set(x => x.Compiler, requestModel.Compiler);
                    if (requestModel.Optimizer != null)
                        updateDef = updateDef.Set(x => x.Optimizer, requestModel.Optimizer);
                    if (requestModel.Replacer != null)
                        updateDef = updateDef.Set(x => x.Replacer, requestModel.Replacer);
                    var sort = new SortDefinitionBuilder<KitsuneBuildStatus>().Descending(x => x.CreatedOn);

                    var result = BuildStatusCollection.FindOneAndUpdate<KitsuneBuildStatus>((x => x.ProjectId == requestModel.ProjectId && x.BuildVersion == projectDetails.Version), updateDef,
                            new FindOneAndUpdateOptions<KitsuneBuildStatus> { Sort = sort });
                    // var result = mongoHelper.BuildStatus.UpdateOne((x => x.ProjectId == requestModel.ProjectId && x.BuildVersion == projectDetails.Version), updateDef);
                    insertSuccess = result != null;
                }

                if (insertSuccess && requestModel.Stage == BuildStatus.Queued)
                {

                    #region Update Kitsune Project
                    var updateResult = ProjectCollection.UpdateOne(projectIdFilter, update);
                    if (!updateResult.IsAcknowledged || !updateResult.IsModifiedCountAvailable)
                        return false;

                    #endregion

                   

                    #region Push to Compiler SQS
                    var helper = new AmazonSQSQueueHandlers<CompilerServiceSQSModel>(AmazonAWSConstants.CompilerSQSUrl);
                    var response = helper.PushMessageToQueue(new CompilerServiceSQSModel
                    {
                        BuildVersion = projectDetails.Version,
                        ProjectId = requestModel.ProjectId,
                        Status = CompilerServiceStatus.Started,
                        UserEmail = requestModel.UserEmail
                    }, EnvironmentConstants.ApplicationConfiguration.AWSSQSConfiguration.AWS_AccessKey, EnvironmentConstants.ApplicationConfiguration.AWSSQSConfiguration.AWS_SecretKey, RegionEndpoint.GetBySystemName(EnvironmentConstants.ApplicationConfiguration.Defaults.AWSSQSRegion));

                    #endregion

                    if (!string.IsNullOrEmpty(response) && response.Equals("OK"))
                        return true;
                }
                return insertSuccess;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        internal static GetKitsuneBuildStatusResponseModel GetKitsuneBuildStatus(GetKitsuneBuildStatusRequestModel requestModel)
        {
            if (requestModel == null)
            {
                throw new ArgumentNullException(nameof(requestModel));
            }

            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();
                var BuildStatusCollection = _kitsuneDatabase.GetCollection<KitsuneBuildStatus>(BuildStatusCollectionName);
                Expression<Func<KitsuneBuildStatus, bool>> expr;
                //if (User.getAll)
                //    expr = (x) => x.IsArchived == false;
                //else
                expr = (x) => x.ProjectId == requestModel.ProjectId;
                if (requestModel.BuildVersion > 0)
                    expr = (x) => x.ProjectId == requestModel.ProjectId && x.BuildVersion == requestModel.BuildVersion;

                long buildStatusCount = BuildStatusCollection.Find(expr).Count();
                var result = BuildStatusCollection.Find(expr).SortByDescending(x => x.CreatedOn).FirstOrDefault();
                if (result != null)
                {
                    return new GetKitsuneBuildStatusResponseModel
                    {
                        Stage = result.Stage,
                        BuildVersion = result.BuildVersion,
                        Error = result.Error,
                        ProjectId = result.ProjectId,
                        Analyzer = result.Analyzer,
                        Optimizer = result.Optimizer,
                        Compiler = result.Compiler,
                        Replacer = result.Replacer,
                        IsCompleted = result.IsCompleted,
                        FirstBuild = (buildStatusCount == 1),
                        CreatedOn = result.CreatedOn
                    };
                }
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        internal static GetKitsuneBuildStatusResponseModel GetLastCompletedKitsuneBuildStatus(GetKitsuneBuildStatusRequestModel requestModel)
        {
            if (requestModel == null)
            {
                throw new ArgumentNullException(nameof(requestModel));
            }

            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();
                var BuildStatusCollection = _kitsuneDatabase.GetCollection<KitsuneBuildStatus>(BuildStatusCollectionName);
                Expression<Func<KitsuneBuildStatus, bool>> expr;
                expr = (x) => x.ProjectId == requestModel.ProjectId && x.Stage == BuildStatus.Completed;
                if (requestModel.BuildVersion > 0)
                    expr = (x) => x.ProjectId == requestModel.ProjectId && x.Stage == BuildStatus.Completed && x.BuildVersion == requestModel.BuildVersion;

                var result = BuildStatusCollection.Find(expr).SortByDescending(x => x.CreatedOn).FirstOrDefault();
                if (result != null)
                {
                    return new GetKitsuneBuildStatusResponseModel
                    {
                        Stage = result.Stage,
                        BuildVersion = result.BuildVersion,
                        Error = result.Error,
                        ProjectId = result.ProjectId,
                        Analyzer = result.Analyzer,
                        Optimizer = result.Optimizer,
                        Compiler = result.Compiler,
                        Replacer = result.Replacer,
                        IsCompleted = result.IsCompleted,
                        CreatedOn = result.CreatedOn
                    };
                }
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        internal static ProjectDetailsForBuildResponseModel ProjectDetailsForBuild(ProjectDetailsForBuildRequestModel requestModel)
        {
            if (requestModel == null)
            {
                throw new ArgumentNullException(nameof(requestModel));
            }

            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();
                var ProjectCollection = _kitsuneDatabase.GetCollection<KitsuneProject>(KitsuneProjectsCollectionName);
                var ResourceCollection = _kitsuneDatabase.GetCollection<KitsuneResource>(KitsuneResourceCollectionName);
                var BuildStatusCollection = _kitsuneDatabase.GetCollection<KitsuneBuildStatus>(BuildStatusCollectionName);
                var projectDetails = ProjectCollection.Find(x => x.ProjectId == requestModel.ProjectId && x.UserEmail == requestModel.UserEmail).FirstOrDefault();
                var resourceDetails = BuildStatusCollection.Find(x => x.ProjectId == requestModel.ProjectId && x.IsCompleted == true).SortByDescending(x => x.CreatedOn).FirstOrDefault();
                var date = resourceDetails == null ? projectDetails.CreatedOn : resourceDetails.CreatedOn;
                var project = Builders<BsonDocument>.Projection.Include("count");
                var resourceAggregate = ResourceCollection.Aggregate().Match(x => x.ProjectId == requestModel.ProjectId).Group(new BsonDocument { { "_id", "$ResourceType" }, { "count", new BsonDocument("$sum", 1) } }).Project<BuildStatsCountModel>(project); //.Sort("_id");
                var updatedResourceAggregate = ResourceCollection.Aggregate().Match(x => x.ProjectId == requestModel.ProjectId && x.UpdatedOn > date)
                    .Group(new BsonDocument { { "_id", "$ResourceType" }, { "count", new BsonDocument("$sum", 1) } })
                    .Project<BuildStatsCountModel>(project);
                var resourceAggregateList = resourceAggregate.ToList();
                var updatedResourceAggregateList = resourceAggregate.ToList();
                var buildMetadata = new ProjectDetailsForBuildResponseModel
                {
                    Total = new BaseBuildMetaData
                    {
                        LINK = resourceAggregateList.Where(x => x._id == ResourceType.LINK.ToString()).Select(x => x.count).FirstOrDefault(),
                        STYLE = resourceAggregateList.Where(x => x._id == ResourceType.STYLE.ToString()).Select(x => x.count).FirstOrDefault(),
                        FILE = resourceAggregateList.Where(x => x._id == ResourceType.FILE.ToString()).Select(x => x.count).FirstOrDefault(),
                        SCRIPT = resourceAggregateList.Where(x => x._id == ResourceType.SCRIPT.ToString()).Select(x => x.count).FirstOrDefault()
                    },
                    Modified = new BaseBuildMetaData
                    {
                        LINK = updatedResourceAggregateList.Where(x => x._id == ResourceType.LINK.ToString()).Select(x => x.count).FirstOrDefault(),
                        STYLE = updatedResourceAggregateList.Where(x => x._id == ResourceType.STYLE.ToString()).Select(x => x.count).FirstOrDefault(),
                        FILE = updatedResourceAggregateList.Where(x => x._id == ResourceType.FILE.ToString()).Select(x => x.count).FirstOrDefault(),
                        SCRIPT = updatedResourceAggregateList.Where(x => x._id == ResourceType.SCRIPT.ToString()).Select(x => x.count).FirstOrDefault()
                    }
                };
                return buildMetadata;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        internal static MakeProjectLiveResponseModel UpdateProductionDBDetailsOld(MakeProjectLiveRequestModel requestModel)
        {
            if (requestModel == null)
            {
                throw new ArgumentNullException(nameof(requestModel));
            }

            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();
                var ProjectCollection = _kitsuneDatabase.GetCollection<KitsuneProject>(KitsuneProjectsCollectionName);
                var ResourceCollection = _kitsuneDatabase.GetCollection<KitsuneResource>(KitsuneResourceCollectionName);

                #region Get Project Details

                var getProjectDetailsQuery = new GetProjectDetailsRequestModel
                {
                    ProjectId = requestModel.ProjectId,
                    UserEmail = requestModel.UserEmail
                };
                var project = GetProjectDetails(getProjectDetailsQuery);

                if (project == null)
                {
                    return new MakeProjectLiveResponseModel
                    {
                        status = "fail",
                        message = "Project doesn't exist in Kitsune Projects."
                    };
                }

                var customerInformationQuery = new GetWebsiteInformationQueryRequestModel
                {
                    UserEmail = requestModel.UserEmail,
                    WebsiteId = requestModel.WebsiteId
                };
                var domain = ((WebsiteDetailsResponseModel)GetKitsuneWebsiteDetails(requestModel.WebsiteId)?.Response)?.WebsiteUrl;

                #endregion

                #region Insert UrlPatternRegex

                // var projectUrlPatternRegex = string.Empty;
                if (project.Resources != null && project.Resources.Any())
                {
                    var regex = new Regex(EnvConstants.Constants.WidgetRegulerExpression);
                    Match match;
                    List<string> parts;
                    ResourceItemMeta resource;

                    for (int resourceIndex = 0; project.Resources.Count > resourceIndex; resourceIndex++)
                    {
                        resource = project.Resources[resourceIndex];
                        if (resource.ResourceType == ResourceType.LINK)
                        {
                            if (!string.IsNullOrEmpty(resource.UrlPattern))
                            {
                                parts = resource.UrlPattern.Split('/').ToList();
                                //else
                                //    throw new Exception("UrlPattern can not be null for page : " + resource.SourcePath);
                                resource.UrlPatternRegex = "";
                                if (parts.Count > 1)
                                {
                                    for (int i = 1; i < parts.Count; i++)
                                    {
                                        var replacementstring = parts[i];
                                        match = regex.Match(parts[i]);
                                        do
                                        {
                                            if (!string.IsNullOrEmpty(match.Value))
                                                replacementstring = replacementstring.Replace(match.Value, @"([a-zA-Z0-9\-\.\%]+)") + @"/";
                                            else
                                                replacementstring = replacementstring + @"/";

                                            match = match.NextMatch();

                                        } while (match != null && match.Success);
                                        resource.UrlPatternRegex += replacementstring;

                                    }
                                }
                                else
                                    resource.UrlPatternRegex = "";

                                resource.UrlPatternRegex = resource.UrlPatternRegex.TrimEnd('/');

                                // projectUrlPatternRegex += resource.UrlPatternRegex + " | ";
                            }
                        }
                    }
                }

                #endregion

                var dateTimeNow = DateTime.Now;
                #region Add the theme to production

                var prodResources = new List<ProductionKitsuneResource>();
                var auditResources = new List<AuditKitsuneResource>();
                foreach (var resource in project.Resources)
                {
                    prodResources.Add(new ProductionKitsuneResource
                    {
                        CreatedOn = dateTimeNow,
                        SourcePath = resource.SourcePath,
                        ClassName = resource.ClassName,
                        ProjectId = requestModel.ProjectId,
                        Version = resource.Version,
                        IsStatic = resource.IsStatic,
                        UrlPattern = resource.UrlPattern,
                        UrlPatternRegex = resource.UrlPatternRegex,
                        IsDefault = resource.IsDefault,
                        PageType = resource.PageType,
                        OptimizedPath = resource.OptimizedPath,
                        ResourceType = resource.ResourceType,
                        MetaData = resource.MetaData,
                        UpdatedOn = dateTimeNow,
                        KObject = resource.KObject
                    });
                    auditResources.Add(new AuditKitsuneResource
                    {
                        CreatedOn = resource.CreatedOn,
                        SourcePath = resource.SourcePath,
                        ClassName = resource.ClassName,
                        ProjectId = requestModel.ProjectId,
                        Version = resource.Version,
                        ProjectVersion = project.Version,
                        IsStatic = resource.IsStatic,
                        UrlPattern = resource.UrlPattern,
                        UrlPatternRegex = resource.UrlPatternRegex,
                        IsDefault = resource.IsDefault,
                        PageType = resource.PageType,
                        OptimizedPath = resource.OptimizedPath,
                        ResourceType = resource.ResourceType,
                        UpdatedOn = resource.UpdatedOn,
                        KObject = resource.KObject
                    });
                }

                var createProductionProjectResult = CreateProductionProject(new CreateProductionProjectRequestModel
                {
                    Project = new ProductionKitsuneProject
                    {
                        CreatedOn = dateTimeNow,
                        ProjectId = requestModel.ProjectId,
                        ProjectName = project.ProjectName,
                        UserEmail = requestModel.UserEmail,
                        Version = project.Version,
                        FaviconIconUrl = project.FaviconIconUrl,
                        ProjectStatus = project.ProjectStatus,
                        ProjectType = project.ProjectType,
                        UpdatedOn = dateTimeNow,
                        SchemaId = project.SchemaId,
                        BucketNames = project.BucketNames,
                        PublishedOn = dateTimeNow,
                        Components = project.Components,
                        CompilerVersion = project.CompilerVersion
                    },
                    Resources = prodResources
                });

                if (!createProductionProjectResult)
                {
                    return new MakeProjectLiveResponseModel
                    {
                        status = "fail",
                        message = "Copy from Source to Prod DB Failed."
                    };
                }

                #endregion

                #region Add theme to Audit

                var createAuditProjectResult = CreateAuditProject(new CreateAuditProjectRequestModel
                {
                    Project = new AuditKitsuneProject
                    {
                        CreatedOn = project.CreatedOn,
                        ProjectId = requestModel.ProjectId,
                        ProjectName = project.ProjectName,
                        UserEmail = requestModel.UserEmail,
                        Version = project.Version,
                        FaviconIconUrl = project.FaviconIconUrl,
                        ProjectStatus = project.ProjectStatus,
                        ProjectType = project.ProjectType,
                        UpdatedOn = project.UpdatedOn,
                        SchemaId = project.SchemaId,
                        BucketNames = project.BucketNames,
                        PublishedOn = dateTimeNow,
                        CompilerVersion = project.CompilerVersion
                    },
                    Resources = auditResources
                });

                if (!createAuditProjectResult)
                {
                    return new MakeProjectLiveResponseModel
                    {
                        status = "fail",
                        message = "Copy from Source to Demo bucket Failed."
                    };
                }

                #endregion

                #region Update Kitsune Project Version

                var updateVersionResult = UpdateProjectVersion(new UpdateProjectVersionRequestModel
                {
                    ProjectId = requestModel.ProjectId,
                    Version = project.Version + 1,
                    UserEmail = requestModel.UserEmail,
                    PublishedVersion = project.Version
                });

                if (!updateVersionResult)
                {
                    return new MakeProjectLiveResponseModel
                    {
                        status = "fail",
                        message = "Version Update Failed."
                    };
                }

                #endregion

                return new MakeProjectLiveResponseModel
                {
                    status = "success",
                    message = ""
                };
            }
            catch (Exception ex)
            {
                return new MakeProjectLiveResponseModel
                {
                    status = "fail",
                    message = "Exception Occured :" + ex.ToString()
                };
            }
        }

        internal static MakeProjectLiveResponseModel UpdateProductionDBDetails(MakeProjectLiveV2RequestModel requestModel)
        {
            if (requestModel == null)
            {
                throw new ArgumentNullException(nameof(requestModel));
            }

            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();
                var ProjectCollection = _kitsuneDatabase.GetCollection<KitsuneProject>(KitsuneProjectsCollectionName);
                var ResourceCollection = _kitsuneDatabase.GetCollection<KitsuneResource>(KitsuneResourceCollectionName);
                var PublishProjectStatsCollection = _kitsuneDatabase.GetCollection<PublishProjectModel>(KitsunePublishStatsCollectionName);
                var websiteCollection = _kitsuneDatabase.GetCollection<KitsuneWebsiteCollection>(KitsuneWebsiteCollectionName);
                var dateTimeNow = DateTime.Now;

                #region Get Publish Details

                var PublishProjectResult = PublishProjectStatsCollection.Find(x => x._id == requestModel.PublishStatsId).FirstOrDefault();
                if (PublishProjectResult == null)
                {
                    return new MakeProjectLiveResponseModel
                    {
                        status = "fail",
                        message = "Publish Stats document doesn't exist in Kitsune Publish Stats Collection."
                    };
                }

                #endregion

                #region Get Project Details

                var getProjectDetailsQuery = new GetProjectDetailsRequestModel
                {
                    ProjectId = PublishProjectResult.ProjectId,
                    UserEmail = requestModel.UserEmail
                };
                var project = GetProjectDetails(getProjectDetailsQuery);

                if (project == null)
                {
                    return new MakeProjectLiveResponseModel
                    {
                        status = "fail",
                        message = "Project doesn't exist in Kitsune Projects."
                    };
                }

                #endregion

                #region Add the theme to production

                var prodResources = new List<ProductionKitsuneResource>();
                var auditResources = new List<AuditKitsuneResource>();
                foreach (var resource in project.Resources)
                {
                    prodResources.Add(new ProductionKitsuneResource
                    {
                        CreatedOn = dateTimeNow,
                        SourcePath = resource.SourcePath,
                        ClassName = resource.ClassName,
                        ProjectId = PublishProjectResult.ProjectId,
                        Version = resource.Version,
                        IsStatic = resource.IsStatic,
                        UrlPattern = resource.UrlPattern,
                        UrlPatternRegex = resource.UrlPatternRegex,
                        IsDefault = resource.IsDefault,
                        PageType = resource.PageType,
                        OptimizedPath = resource.OptimizedPath,
                        ResourceType = resource.ResourceType,
                        MetaData = resource.MetaData,
                        KObject = resource.KObject,
                        UpdatedOn = dateTimeNow
                    });

                    auditResources.Add(new AuditKitsuneResource
                    {
                        CreatedOn = resource.CreatedOn,
                        SourcePath = resource.SourcePath,
                        ClassName = resource.ClassName,
                        ProjectId = PublishProjectResult.ProjectId,
                        Version = resource.Version,
                        ProjectVersion = project.Version,
                        IsStatic = resource.IsStatic,
                        UrlPattern = resource.UrlPattern,
                        UrlPatternRegex = resource.UrlPatternRegex,
                        IsDefault = resource.IsDefault,
                        PageType = resource.PageType,
                        OptimizedPath = resource.OptimizedPath,
                        ResourceType = resource.ResourceType,
                        KObject = resource.KObject,
                        UpdatedOn = resource.UpdatedOn
                    });
                }

                var createProductionProjectResult = CreateProductionProject(new CreateProductionProjectRequestModel
                {
                    Project = new ProductionKitsuneProject
                    {
                        CreatedOn = dateTimeNow,
                        ProjectId = PublishProjectResult.ProjectId,
                        ProjectName = project.ProjectName,
                        UserEmail = requestModel.UserEmail,
                        Version = project.Version,
                        FaviconIconUrl = project.FaviconIconUrl,
                        ProjectStatus = project.ProjectStatus,
                        ProjectType = project.ProjectType,
                        UpdatedOn = dateTimeNow,
                        SchemaId = project.SchemaId,
                        BucketNames = project.BucketNames,
                        PublishedOn = dateTimeNow,
                        Components = project.Components,
                        CompilerVersion = project.CompilerVersion
                        //CustomerId = requestModel.CustomerId,
                        //Domain = domain,
                        // UrlPatternRegex = projectUrlPatternRegex
                    },
                    Resources = prodResources
                });

                if (!createProductionProjectResult)
                {
                    return new MakeProjectLiveResponseModel
                    {
                        status = "fail",
                        message = "Copy from Source to Prod DB Failed."
                    };
                }

                #endregion

                #region Add theme to Audit

                var createAuditProjectResult = CreateAuditProject(new CreateAuditProjectRequestModel
                {
                    Project = new AuditKitsuneProject
                    {
                        CreatedOn = project.CreatedOn,
                        ProjectId = PublishProjectResult.ProjectId,
                        ProjectName = project.ProjectName,
                        UserEmail = requestModel.UserEmail,
                        Version = project.Version,
                        FaviconIconUrl = project.FaviconIconUrl,
                        ProjectStatus = project.ProjectStatus,
                        ProjectType = project.ProjectType,
                        UpdatedOn = project.UpdatedOn,
                        SchemaId = project.SchemaId,
                        BucketNames = project.BucketNames,
                        PublishedOn = dateTimeNow,
                        CompilerVersion = project.CompilerVersion
                        //CustomerId = requestModel.CustomerId,
                        //Domain = domain
                    },
                    Resources = auditResources
                });

                if (!createAuditProjectResult)
                {
                    return new MakeProjectLiveResponseModel
                    {
                        status = "fail",
                        message = "Copy from Source to Demo bucket Failed."
                    };
                }

                #endregion

                #region Update Kitsune Project Version

                var updateVersionResult = UpdateProjectVersion(new UpdateProjectVersionRequestModel
                {
                    ProjectId = PublishProjectResult.ProjectId,
                    Version = project.Version + 1,
                    UserEmail = requestModel.UserEmail,
                    PublishedVersion = project.Version
                });

                if (!updateVersionResult)
                {
                    return new MakeProjectLiveResponseModel
                    {
                        status = "fail",
                        message = "Version Update Failed."
                    };
                }

                var updateStatusResult = CreateOrUpdateProject(new CreateOrUpdateProjectRequestModel
                {
                    UserEmail = requestModel.UserEmail,
                    ProjectId = PublishProjectResult.ProjectId,
                    ProjectStatus = ProjectStatus.IDLE
                });
                if (string.IsNullOrEmpty(updateStatusResult))
                {
                    return new MakeProjectLiveResponseModel
                    {
                        status = "fail",
                        message = "Project Status Update Failed."
                    };
                }

                #endregion

                #region Get all websites to publish
                var websiteProjectionBuilder = new ProjectionDefinitionBuilder<KitsuneWebsiteCollection>();

                List<PublishedWebsite> websitesToPublish = null;
                var websiteProjection = websiteProjectionBuilder.Include(x => x._id).Include(x => x.WebsiteUrl);

                if (PublishProjectResult.PublishToAll)
                {
                    if (project.Version == 1)
                        websitesToPublish = websiteCollection.Find(x => x.ProjectId == PublishProjectResult.ProjectId).Project<PublishedWebsite>(websiteProjection).ToList();
                    else
                        websitesToPublish = websiteCollection.Find(x => x.ProjectId == PublishProjectResult.ProjectId && x.IsActive == true).Project<PublishedWebsite>(websiteProjection).ToList();
                    if (websitesToPublish != null)
                        PublishProjectResult.CustomerIds = websitesToPublish.Select(x => x._id).ToList();
                }
                else
                {
                    var filterDefinition = new FilterDefinitionBuilder<KitsuneWebsiteCollection>();

                    websitesToPublish = websiteCollection.Find(filterDefinition.In(x => x._id, PublishProjectResult.CustomerIds) & filterDefinition.Eq(x => x.IsActive, true)).Project<PublishedWebsite>(websiteProjection).ToList();
                }
                #endregion

                #region Get List of all Websites not Active 

                var filterBuilder = new FilterDefinitionBuilder<KitsuneWebsiteCollection>();
                var websiteFilterBuilder = filterBuilder.In(x => x._id, PublishProjectResult.CustomerIds) & filterBuilder.Eq(x => x.IsActive, false);


                var websitesForBillingActivationDetails = websiteCollection.Find(websiteFilterBuilder).Project<PublishedWebsite>(websiteProjection).ToList();
                var websitesForBillingActivation = websitesForBillingActivationDetails.Select(x => x._id).ToList();

                #endregion

                #region Update Version of the Websites

                var updateBuilder = new UpdateDefinitionBuilder<KitsuneWebsiteCollection>();
                websiteFilterBuilder = filterBuilder.In(x => x._id, PublishProjectResult.CustomerIds);
                var websiteUpdateBuilder = updateBuilder.Set(x => x.IsActive, true)
                                                        .Set(x => x.KitsuneProjectVersion, PublishProjectResult.Version)
                                                        .Set(x => x.UpdatedOn, dateTimeNow);

                var customerupdate = websiteCollection.UpdateMany(websiteFilterBuilder, websiteUpdateBuilder);

                #endregion

                #region UPDATE ROUTING TABLE

                try
                {
                    APIHelper.CreateProjectRoute(PublishProjectResult.ProjectId, false);
                }
                catch (Exception ex)
                {
                    //TODO: LOG 
                }

                #endregion

                #region Sending Mail to User and Billing Resistry and Sitemap

                if (customerupdate.IsAcknowledged && customerupdate.ModifiedCount > 0)
                {


#if DEBUG //As billing apis arent live on prod environment

                    string BillingRemarks = "Activated On Project Publish";
                    List<string> customersNotActivated = new List<string> { };
                    foreach (var websiteId in websitesForBillingActivation)
                    {
                        bool activationResult = BillingAPI.BillingProcess(websiteId, BillingRemarks, BillingComponents.WebRequests, BillingProcessUrl.Activate);
                        if (!activationResult)
                            customersNotActivated.Add(websiteId);
                    }

#endif

                    #region Send K-Admin Creds to the activated customers

                    EmailHelper emailHelper = new EmailHelper();
                    var websiteUserCollection = _kitsuneDatabase.GetCollection<KitsuneWebsiteUserCollection>(KitsuneWebsiteUserCollectionName);
                    ProjectionDefinitionBuilder<KitsuneWebsiteUserCollection> projectDefinition = new ProjectionDefinitionBuilder<KitsuneWebsiteUserCollection>();
                    var developer = _kitsuneDatabase.GetCollection<UserModel>(KitsuneUserCollectionName).Find(x => x.Email == project.UserEmail).Project<UserModel>(new ProjectionDefinitionBuilder<UserModel>().Include(y => y.DisplayName)).FirstOrDefault();
                    //TODO: MIGRATION
                    if (!string.IsNullOrEmpty(project.SchemaId))
                    {
                        foreach (var websiteId in websitesForBillingActivation)
                        {
                            var websiteUrl = websitesForBillingActivationDetails.Find(x => x._id == websiteId)?.WebsiteUrl;
                            var websiteUsers = websiteUserCollection.Find(x => x.WebsiteId == websiteId).Project<WebsiteUserDetails>(projectDefinition.Include(x => x.Contact)
                                                                                                                                                     .Include(x => x.UserName)
                                                                                                                                                     .Include(x => x.Password)).ToList();
                            foreach (var user in websiteUsers)
                            {
                                Dictionary<string, string> optionalParameters = new Dictionary<string, string>
                                {
                                    { EmailParam_CustomerName, user.Contact.FullName},
                                    { EmailParam_DeveloperName, developer?.DisplayName},
                                    { EmailParam_KAdminUserName, user.UserName},
                                    { EmailParam_KAdminPassword, user.Password},
                                    { EmailParam_KAdminUrl, string.Format(EnvConstants.Constants.KAdminBaseUrl, websiteUrl).ToLower()}
                                };
                                emailHelper.SendGetKitsuneEmail(string.Empty, requestModel.UserEmail, MailType.CUSTOMER_KADMIN_CREDENTIALS, null, optionalParameters);
                            }
                        }
                    }

                    #endregion

                    //TODO: MIGRATION
#if DEBUG //As billing apis arent live on prod environment
                    if (customersNotActivated.Any())
                    {
                        Dictionary<string, string> optionalParameters = new Dictionary<string, string>
                                            {
                                                { EnvConstants.Constants.BillingActivationFailedCustomers, "CustomerIds: " + string.Join(",", customersNotActivated.ToArray()) },
                                            };
                        emailHelper.SendGetKitsuneEmail(string.Empty, EnvConstants.Constants.BillingActivationFailedReportMail, MailType.CUSTOMER_BILLING_NOT_ACTIVATED, null, optionalParameters);
                    }
#endif

                    return new MakeProjectLiveResponseModel
                    {
                        status = "success",
                        message = ""
                    };
                }

                #endregion

                #region Update Sitemap and Cache Invalidation
                try
                {
                    //Not sure why publish project stage has only started state
                    if (PublishProjectResult != null)
                    {
                        //TODO : check if we need to get the configuration
                        //var websiteConfig = MongoConnector.GetProjectConfig(new GetProjectConfigRequestModel() { ProjectId = PublishProjectResult.ProjectId });


                        //Invalidate the project level cache(For nowfloats)

                        CacheServices cacheService = new CacheServices();
                        cacheService.InvalidateProjectCache(PublishProjectResult.ProjectId);


                        //TODO: Sitemap will be created via a CRONJOB running every 3 days, removing for every project publish
                        //Parallel.ForEach(websitesToPublish, new ParallelOptions { MaxDegreeOfParallelism = 5 },
                        //    website =>
                        //    {
                        //        try
                        //        {
                        //            //Exclude paths to be added
                        //            CommonHelpers.PushSitemapGenerationTaskToSQS(new SitemapGenerationTaskModel { WebsiteId = website._id, ProjectId = PublishProjectResult.ProjectId });
                        //            //Invalidate the domain level cache
                        //            cacheService.InvalidateKitsuneCache(website._id, website.WebsiteUrl.ToLower(), project.Version.ToString(), PublishProjectResult.ProjectId);
                        //        }
                        //        catch
                        //        {

                        //        }
                        //    });

                    }
                }
                catch (Exception ex)
                {

                }
                #endregion

                return new MakeProjectLiveResponseModel
                {
                    status = "fail",
                    message = "customer collection not updated"
                };
            }
            catch (Exception ex)
            {
                return new MakeProjectLiveResponseModel
                {
                    status = "fail",
                    message = "Exception Occured :" + ex.ToString()
                };
            }
        }

        internal static PublishCustomerResponseModel PublishCustomer(PublishCustomerRequestModel requestModel)
        {
            if (requestModel == null)
            {
                throw new ArgumentNullException(nameof(requestModel));
            }

            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();
                var ProjectCollection = _kitsuneDatabase.GetCollection<KitsuneProject>(KitsuneProjectsCollectionName);
                var ResourceCollection = _kitsuneDatabase.GetCollection<KitsuneResource>(KitsuneResourceCollectionName);
                var WebsiteCollection = _kitsuneDatabase.GetCollection<KitsuneWebsiteCollection>(KitsuneWebsiteCollectionName);

                #region Check Balance 

                //Removed as its already done in the PublishProject function
                #endregion

                List<string> customersToPublish = new List<string>();
                PublishWebsiteDetailsModel websitedetails = null;

                #region GetProjectId

                if (string.IsNullOrEmpty(requestModel.ProjectId))
                {
                    var projectDefinition = Builders<KitsuneWebsiteCollection>.Projection.Include(x => x.ProjectId)
                                                                .Include(x => x.WebsiteUrl)
                                                                .Exclude(x => x._id);

                    websitedetails = WebsiteCollection.Find(x => x._id.Equals(requestModel.WebsiteId)).Project<PublishWebsiteDetailsModel>(projectDefinition).FirstOrDefault();
                    if (websitedetails == null)
                        return new PublishCustomerResponseModel() { IsError = true, Message = "Error getting projectId" };
                    requestModel.ProjectId = websitedetails.ProjectId;
                }

                if (!requestModel.PublishToAll)
                    customersToPublish = new List<string>() { requestModel.WebsiteId };


                #endregion


                #region Publish Site

                var result = PublishProject(new PublishProjectRequestModel
                {
                    PublishToAll = requestModel.PublishToAll,
                    CustomerIds = customersToPublish,
                    ProjectId = requestModel.ProjectId,
                    UserEmail = requestModel.UserEmail,
                });

                if (result == null) throw new Exception("Unable to publish the project");
                if (result.IsError) throw new Exception(result.Message);

                #endregion


                #region CopyWebsiteData
                if (!string.IsNullOrEmpty(requestModel.CopyFromWebsiteId))
                    CopyWebsiteData(requestModel.CopyFromWebsiteId, requestModel.WebsiteId, requestModel.ProjectId);
                else if (requestModel.CopyFromDemoWebsite)
                    CopyWebsiteData(requestModel.CopyFromWebsiteId, requestModel.WebsiteId, requestModel.ProjectId, true);

                #endregion

                return new PublishCustomerResponseModel
                {
                    IsError = false,
                    Domain = websitedetails?.WebsiteUrl,
                    CNAMERecord = websitedetails?.WebsiteUrl,
                    DomainVerified = true,
                    ARecord = EnvironmentConstants.ApplicationConfiguration.KitsuneRedirectIPAddress
                };
            }
            catch (Exception ex)
            {
                return new PublishCustomerResponseModel
                {
                    IsError = true,
                    Message = ex.Message.ToString()
                };
            }
        }

        internal static PublishProjectResponseModel PublishProject(PublishProjectRequestModel requestModel)
        {
            if (requestModel == null)
            {
                throw new ArgumentNullException(nameof(requestModel));
            }

            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();
                var ProjectCollection = _kitsuneDatabase.GetCollection<KitsuneProject>(KitsuneProjectsCollectionName);
                var publishStatsCollection = _kitsuneDatabase.GetCollection<PublishProjectModel>(KitsunePublishStatsCollectionName);
                var BuildStatusCollection = _kitsuneDatabase.GetCollection<KitsuneBuildStatus>(BuildStatusCollectionName);
                var WebsiteCollection = _kitsuneDatabase.GetCollection<KitsuneWebsiteCollection>(KitsuneWebsiteCollectionName);

                #region Check BuildStatus
                //check last build status success
                var buildStatus = BuildStatusCollection.Find(x => x.ProjectId == requestModel.ProjectId).SortByDescending(x => x.CreatedOn).FirstOrDefault();
                if (buildStatus != null && buildStatus.Stage != BuildStatus.Completed)
                    return new PublishProjectResponseModel { IsError = true, Message = "Last build was not completed successfully" };
                #endregion

                #region Check Balance 

                var balanceCritical = MongoConnector.IsWalletCritical(requestModel.UserEmail);
                if (balanceCritical == true) throw new Exception("low balance");

                #endregion

                #region Check and update KitsuneProjectsCollection (Change the kitsune Url and set IsPublishing to true)

                var projectIdFilter = Builders<KitsuneProject>.Filter.Eq(x => x.ProjectId, requestModel.ProjectId);
                var project = Builders<KitsuneProject>.Projection.Include(x => x.ProjectStatus)
                                                                 .Include(x => x.Version)
                                                                 .Exclude(x => x._id);

                //Check Project Status
                var projectResult = ProjectCollection.Find(projectIdFilter).Project<KitsuneProjectPublishDetails>(project).FirstOrDefault();
                if (projectResult == null)
                    return new PublishProjectResponseModel { IsError = true, Message = "unable to fetch project details" };
                var projectActiveStatus = new List<ProjectStatus>() { ProjectStatus.ERROR, ProjectStatus.BUILDING, ProjectStatus.CRAWLING, ProjectStatus.PUBLISHING, ProjectStatus.QUEUED };
                if (projectActiveStatus.Contains(projectResult.ProjectStatus))
                    return new PublishProjectResponseModel { IsError = true, Message = $"project is {projectResult.ProjectStatus.ToString().ToLower()}" };


                //Update Project Status
                var update = Builders<KitsuneProject>.Update.Set(x => x.ProjectStatus, ProjectStatus.QUEUED);
                var result = ProjectCollection.UpdateOne(projectIdFilter, update);

                #endregion

                #region PublishToAll
                //TODO : Handle the publish to all for huge number of websites
                List<KitsuneWebsiteCollection> websitesToPublish = null;
                if (requestModel.PublishToAll)
                {
                    websitesToPublish = WebsiteCollection.Find(x => x.ProjectId == requestModel.ProjectId && x.IsActive)
                          .Project<KitsuneWebsiteCollection>(new ProjectionDefinitionBuilder<KitsuneWebsiteCollection>().Include(y => y._id).Include(y => y.WebsiteUrl)).ToList();
                    if (websitesToPublish != null && websitesToPublish.Any())
                    {
                        requestModel.CustomerIds = websitesToPublish.Select(x => x._id).ToList();
                    }
                }
                else
                {
                    websitesToPublish = WebsiteCollection.Find(x => requestModel.CustomerIds.Contains(x._id) && x.IsActive)
                          .Project<KitsuneWebsiteCollection>(new ProjectionDefinitionBuilder<KitsuneWebsiteCollection>().Include(y => y._id).Include(y => y.WebsiteUrl)).ToList();
                }
                #endregion


                #region Create document in PublishStats Collection

                var publishId = ObjectId.GenerateNewId().ToString();
                PublishProjectModel publishProjectObject = new PublishProjectModel()
                {
                    _id = publishId,
                    CreatedOn = DateTime.Now,
                    ProjectId = requestModel.ProjectId,
                    Stage = PublishProjectStage.STARTED,
                    Version = projectResult.Version,
                    CustomerIds = requestModel.CustomerIds,
                    PublishToAll = requestModel.PublishToAll
                };
                publishStatsCollection.InsertOne(publishProjectObject);


                #endregion

                if (result.IsAcknowledged && result.IsModifiedCountAvailable)
                {
                    #region Push into SQS

                    AmazonSQSQueueHandlers<PublishProjectSQSModel> sqsHanlder = new AmazonSQSQueueHandlers<PublishProjectSQSModel>(AmazonAWSConstants.PublishSQSUrl);
                    sqsHanlder.PushMessageToQueue(new PublishProjectSQSModel { PublishId = publishId },
                        EnvironmentConstants.ApplicationConfiguration.AWSSQSConfiguration.AWS_AccessKey, EnvironmentConstants.ApplicationConfiguration.AWSSQSConfiguration.AWS_SecretKey, RegionEndpoint.GetBySystemName(EnvironmentConstants.ApplicationConfiguration.Defaults.AWSSQSRegion));

                    return new PublishProjectResponseModel { IsError = false, Message = "publish started" };

                    #endregion
                }
                else
                {
                    return new PublishProjectResponseModel { IsError = true, Message = "error publishing, Error: Unable to change project status" };
                }
            }
            catch (Exception ex)
            {
                return new PublishProjectResponseModel { IsError = true, Message = "error publishing" };
            }
        }

        internal static DownloadProjectResponseModel DownloadProject(DownloadProjectRequestModel requestModel)
        {
            if (requestModel == null)
            {
                throw new ArgumentNullException(nameof(requestModel));
            }
            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();
                var ResourceCollection = _kitsuneDatabase.GetCollection<KitsuneResource>(KitsuneResourceCollectionName);
                var publishStatsCollection = _kitsuneDatabase.GetCollection<PublishProjectModel>(KitsunePublishStatsCollectionName);
                var ZipFolderStatsCollection = _kitsuneDatabase.GetCollection<KitsuneTaskDownloadQueueCollection>(TaskDownloadQueueCollectionName);

                #region Check download status

                //If in progress send the download status
                KitsuneTaskDownloadQueueCollection downloadDetails = ZipFolderStatsCollection.Find(x => x.ProjectId.Equals(requestModel.ProjectId)).SortByDescending(x => x.CreatedOn).FirstOrDefault();

                if (downloadDetails != null)
                {
                    switch (downloadDetails.Status)
                    {
                        case TaskDownloadQueueStatus.Started:
                            return new DownloadProjectResponseModel { IsError = false, Status = downloadDetails.Status, Message = downloadDetails.Message };
                        case TaskDownloadQueueStatus.Completed:
                            //Check and download
                            break;
                        case TaskDownloadQueueStatus.Error:
                            //download
                            break;
                    }
                }

                #endregion

                #region Check the Kitsune Resources last updated

                if (downloadDetails != null)
                {
                    var resourceUpdated = ResourceCollection.Find(x => x.ProjectId.Equals(requestModel.ProjectId) && x.UpdatedOn >= downloadDetails.CompletedOn).Any();
                    if (!resourceUpdated)
                    {
                        //send the Link
                        return new DownloadProjectResponseModel { IsError = false, DownloadUrl = downloadDetails.DownloadUrl, Status = TaskDownloadQueueStatus.Completed };
                    }
                }

                #endregion

                #region Download Project

                ObjectId objectId = ObjectId.GenerateNewId();
                KitsuneTaskDownloadQueueCollection collection = new KitsuneTaskDownloadQueueCollection
                {
                    ProjectId = requestModel.ProjectId,
                    Status = TaskDownloadQueueStatus.Started,
                    Message = "preparing your zip...",
                    CreatedOn = DateTime.UtcNow,
                    UserEmail = requestModel.UserEmail,
                    _id = objectId.ToString()
                };

                ZipFolderStatsCollection.InsertOne(collection);

                var amazonSqsQueueHandler = new AmazonSQSQueueHandlers<ZipSQSModel>(AmazonAWSConstants.DownloadSQSUrl);

                ZipSQSModel sqsModel = new ZipSQSModel
                {
                    ZippingId = objectId.ToString(),
                    ProjectId = requestModel.ProjectId
                };
                amazonSqsQueueHandler.PushMessageToQueue(sqsModel, EnvironmentConstants.ApplicationConfiguration.AWSSQSConfiguration.AWS_AccessKey,
                    EnvironmentConstants.ApplicationConfiguration.AWSSQSConfiguration.AWS_SecretKey, RegionEndpoint.GetBySystemName(EnvironmentConstants.ApplicationConfiguration.Defaults.AWSSQSRegion));


                return new DownloadProjectResponseModel { IsError = false, Status = TaskDownloadQueueStatus.Started };

                #endregion

            }
            catch (Exception ex)
            {
                return new DownloadProjectResponseModel { IsError = true, Message = "error publishing" };
            }
        }

        internal static bool KitsuneEnquiry(KitsuneEnquiryRequestModel requestModel)
        {
            if (requestModel == null)
            {
                throw new ArgumentNullException(nameof(requestModel));
            }

            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();


                var EnquiryCollection = _kitsuneDatabase.GetCollection<KitsuneEnquiry>(EnquiryCollectionName);
                var model = new KitsuneEnquiry
                {
                    EmailBody = requestModel.EmailBody,
                    Name = requestModel.Name,
                    Subject = requestModel.Subject,
                    From = requestModel.From,
                    Type = requestModel.Type
                };
                EnquiryCollection.InsertOne(model);
                return true;

            }
            catch (Exception ex)
            {
                return false;
            }
        }

        internal static GetAuditProjectAndResourcesDetailsResponseModel GetAuditProjectAndResourcesDetails(GetAuditProjectAndResourcesDetailsRequestModel requestModel)
        {
            if (requestModel == null)
            {
                throw new ArgumentNullException(nameof(requestModel));
            }

            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();

                var AuditProjectCollection = _kitsuneDatabase.GetCollection<AuditKitsuneProject>(AuditProjectCollectionName);
                var AuditResourceCollection = _kitsuneDatabase.GetCollection<AuditKitsuneResource>(AuditResourcesCollectionName);
                var asd = new FilterDefinitionBuilder<AuditKitsuneResource>();
                var pdb = new ProjectionDefinitionBuilder<AuditKitsuneResource>();
                Expression<Func<AuditKitsuneProject, bool>> exprProject;
                Expression<Func<AuditKitsuneResource, bool>> exprPage;
                exprProject = (x) => x.ProjectId == requestModel.ProjectId && x.Version == requestModel.Version;
                var project = AuditProjectCollection.Find(exprProject).SortByDescending(x => x.CreatedOn).FirstOrDefault();
                exprPage = (x) => x.ProjectId == requestModel.ProjectId && x.Version == requestModel.Version;
                var page = AuditResourceCollection.Find(exprPage).ToList();
                //foreach (var singlePage in page)
                //{
                //    singlePage.HtmlSourceString = AmazonS3FileProcessor.getFileFromS3(singlePage.HtmlSourceString);
                //    singlePage.HtmlCompiledString = AmazonS3FileProcessor.getFileFromS3(singlePage.HtmlCompiledString);
                //}
                return new GetAuditProjectAndResourcesDetailsResponseModel { Project = project, Resources = page };

            }
            catch (Exception ex)
            {
                return null;
            }
        }

        internal static bool CreateAuditProject(CreateAuditProjectRequestModel requestModel)
        {
            if (requestModel == null)
            {
                throw new ArgumentNullException(nameof(requestModel));
            }

            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();

                var AuditProjectCollection = _kitsuneDatabase.GetCollection<AuditKitsuneProject>(AuditProjectCollectionName);
                var AuditResourceCollection = _kitsuneDatabase.GetCollection<AuditKitsuneResource>(AuditResourcesCollectionName);
                var asd = new FilterDefinitionBuilder<AuditKitsuneResource>();
                var pdb = new ProjectionDefinitionBuilder<AuditKitsuneResource>();
                AuditProjectCollection.InsertOne(requestModel.Project);
                AuditResourceCollection.InsertMany(requestModel.Resources);
                return true;

            }
            catch (Exception ex)
            {
                return false;
            }
        }

        internal static bool CreateProductionProject(CreateProductionProjectRequestModel requestModel)
        {
            if (requestModel == null)
            {
                throw new ArgumentNullException(nameof(requestModel));
            }

            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();

                var ProductionProjectCollection = _kitsuneDatabase.GetCollection<ProductionKitsuneProject>(ProductionProjectCollectionName);
                var ProductionResourceCollection = _kitsuneDatabase.GetCollection<ProductionKitsuneResource>(ProductionResorcesCollectionName);
                var res = ProductionProjectCollection.Find(x => x.ProjectId == requestModel.Project.ProjectId);
                if (res != null && res.Any())
                    DeleteProductionProject(new DeleteProductionProjectRequestModel
                    {
                        ProjectId = requestModel.Project.ProjectId,
                        UserEmail = requestModel.UserEmail
                    });
                ProductionProjectCollection.InsertOne(requestModel.Project);
                ProductionResourceCollection.InsertMany(requestModel.Resources);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        internal static bool DeleteProductionProject(DeleteProductionProjectRequestModel requestModel)
        {
            if (requestModel == null)
            {
                throw new ArgumentNullException(nameof(requestModel));
            }

            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();

                var ProductionProjectCollection = _kitsuneDatabase.GetCollection<ProductionKitsuneProject>(ProductionProjectCollectionName);
                var ProductionResourceCollection = _kitsuneDatabase.GetCollection<ProductionKitsuneResource>(ProductionResorcesCollectionName);
                var res = ProductionProjectCollection.Find(x => x.ProjectId == requestModel.ProjectId).FirstOrDefault();
                if (res == null)
                    return false;
                ProductionProjectCollection.DeleteMany(x => x.ProjectId == requestModel.ProjectId);
                ProductionResourceCollection.DeleteMany(x => x.ProjectId == requestModel.ProjectId);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        internal static GetProductionProjectDetailsResponseModel GetProductionProjectDetails(GetProductionProjectDetailsRequestModel requestModel)
        {
            if (requestModel == null)
            {
                throw new ArgumentNullException(nameof(requestModel));
            }

            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();

                var ProductionProjectCollection = _kitsuneDatabase.GetCollection<ProductionKitsuneProject>(ProductionProjectCollectionName);
                var ProductionProjectResourceCollection = _kitsuneDatabase.GetCollection<ProductionKitsuneResource>(ProductionResorcesCollectionName);
                var res = ProductionProjectCollection.Find(x => x.ProjectId == requestModel.ProjectId).FirstOrDefault();
                var response = new GetProductionProjectDetailsResponseModel { Project = res };
                if (res != null && requestModel.IncludeResources)
                {
                    response.Resources = ProductionProjectResourceCollection.Find(x => x.ProjectId == requestModel.ProjectId)?.ToList();
                }
                return response;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        internal static GetProjectInProcessResponseModel GetProjectInProcess(GetProjectInProcessRequestModel requestModel)
        {
            if (requestModel == null)
            {
                throw new ArgumentNullException(nameof(requestModel));
            }

            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();

                var ProjectCollection = _kitsuneDatabase.GetCollection<KitsuneProject>(KitsuneProjectsCollectionName);
                var BuildStatusCollection = _kitsuneDatabase.GetCollection<KitsuneBuildStatus>(BuildStatusCollectionName);
                var krawlStatsCollection = _kitsuneDatabase.GetCollection<KitsuneKrawlerStats>(KitsuneKrawlStatsCollection);
                var finalList = new List<ProjectInProcessStatus>();
                var pdb = new ProjectionDefinitionBuilder<KitsuneProject>();
                var projects = ProjectCollection.Find(x => x.UserEmail == requestModel.UserEmail && x.IsArchived == false && x.ProjectStatus != ProjectStatus.IDLE).SortByDescending(x => x.CreatedOn)
                                                                    .Project<GetProjectInProcessDetail>(pdb.Include(x => x.ProjectId)
                                                                                                           .Include(x => x.ProjectStatus)
                                                                                                           .Include(x => x.Version)
                                                                                                           .Exclude(x => x._id)).ToList();
                if (projects != null && projects.Any())
                {
                    var crawlStatusProjectIdList = projects.Where(x => x.ProjectStatus == ProjectStatus.CRAWLING).Select(x => x.ProjectId).ToList();
                    var buildStatusProjectList = projects.Where(x => x.ProjectStatus == ProjectStatus.BUILDING).ToList();
                    var publishStatusProjectList = projects.Where(x => x.ProjectStatus == ProjectStatus.PUBLISHING).ToList();

                    var crawlFdb = new FilterDefinitionBuilder<KitsuneKrawlerStats>();
                    var fdb = crawlFdb.In(x => x.ProjectId, crawlStatusProjectIdList);
                    var crawlStatusProjectsResult = krawlStatsCollection.Find(fdb).ToList();

                    foreach (var project in buildStatusProjectList)
                    {
                        Expression<Func<KitsuneBuildStatus, bool>> expr;
                        expr = (x) => x.ProjectId == project.ProjectId;
                        if (project.Version > 0)
                            expr = (x) => x.ProjectId == project.ProjectId && x.BuildVersion == project.Version;

                        var result = BuildStatusCollection.Find(expr).SortByDescending(x => x.CreatedOn).FirstOrDefault();

                        if (result != null)
                        {
                            finalList.Add(new ProjectInProcessStatus
                            {
                                ProjectId = project.ProjectId,
                                ProjectStatus = project.ProjectStatus,
                                BuildStage = result.Stage,
                                CrawlStage = null,
                            });
                        }
                    }
                    foreach (var projectId in crawlStatusProjectIdList)
                    {
                        var crawlResult = crawlStatusProjectsResult.Where(x => x.ProjectId == projectId).FirstOrDefault();
                        if (crawlResult != null)
                        {
                            finalList.Add(new ProjectInProcessStatus
                            {
                                ProjectId = projectId,
                                ProjectStatus = ProjectStatus.CRAWLING,
                                BuildStage = null,
                                CrawlStage = crawlResult.Stage,
                            });
                        }
                    }
                    foreach (var project in publishStatusProjectList)
                    {
                        finalList.Add(new ProjectInProcessStatus
                        {
                            ProjectId = project.ProjectId,
                            ProjectStatus = project.ProjectStatus,
                            BuildStage = null,
                            CrawlStage = null,
                        });
                    }
                }

                return new GetProjectInProcessResponseModel
                {
                    Projects = finalList
                };
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        internal static ProjectUpdatedOrNotResponse CheckProjectUpdatedOrNot(GetProjectUpdatedOrNotRequest requestModel)
        {
            //Get last build datetime from buildstats
            //If Last build datetime not found (return projectupdated "true")
            //count number of resources updated after last build datetime
            //If the above count is more than 0 projectupdated true else false
            if (requestModel == null)
            {
                throw new ArgumentNullException(nameof(requestModel));
            }

            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();

                var resourceCollection = _kitsuneDatabase.GetCollection<KitsuneResource>(KitsuneResourceCollectionName);
                var buildStatsCollection = _kitsuneDatabase.GetCollection<KitsuneBuildStatus>(BuildStatusCollectionName);

                var buildDetails = buildStatsCollection.Find(x => x.ProjectId.Equals(requestModel.ProjectId)).SortByDescending(x => x.CreatedOn).First();
                //buildStats not found means new Project
                if (buildDetails == null)
                    return new ProjectUpdatedOrNotResponse { ProjectUpdated = true };

                var count = resourceCollection.Count(x => x.ProjectId.Equals(requestModel.ProjectId) && x.UpdatedOn >= buildDetails.CreatedOn);
                if (count > 0)
                    return new ProjectUpdatedOrNotResponse { ProjectUpdated = true };
                else
                    return new ProjectUpdatedOrNotResponse { ProjectUpdated = false };

            }
            catch (Exception ex)
            {
                return new ProjectUpdatedOrNotResponse { IsError = true, Message = ex.Message };
            }
        }

        internal static ResourceCompilationResult ResourceUpload(SubmitRequest requestModel)
        {
            if (requestModel == null)
            {
                throw new ArgumentNullException(nameof(requestModel));
            }

            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();

                requestModel.SourcePath = string.Format("/{0}", requestModel.SourcePath.Trim('/'));
                requestModel.ClassName = IdeApiHelper.GenerateClassName(requestModel.SourcePath);
                requestModel.UrlPattern = requestModel.SourcePath;
                var updateResourceRequest = new CreateOrUpdateResourceRequestModel
                {
                    Errors = null,
                    FileContent = requestModel.FileContent,
                    SourcePath = requestModel.SourcePath.Trim(),
                    ClassName = requestModel.ClassName,
                    ProjectId = requestModel.ProjectId,
                    UserEmail = requestModel.UserEmail,
                    UrlPattern = requestModel.UrlPattern,
                    IsStatic = requestModel.IsStatic,
                    PageType = requestModel.PageType,
                    KObject = requestModel.KObject,
                    ResourceType = requestModel.ResourceType
                };
                var isUploaded = CreateOrUpdateResource(updateResourceRequest);

                if (isUploaded)
                    return new ResourceCompilationResult { Success = true };
                else
                    return new ResourceCompilationResult { Success = false };
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        internal static List<ObjectReference> GetMetaInfo(GetResourceMetaInfoRequest requestModel)
        {
            if (requestModel == null)
            {
                throw new ArgumentNullException(nameof(requestModel));
            }

            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();


                var projectDetails = GetProjectDetails(requestModel.ProjectId);
                if (projectDetails == null)
                    throw new Exception("ProjectId does not Exist");
                var serializedMetaInfo = AmazonS3FileProcessor.getFileFromS3(string.Format(CompilerConstants.TreeFileName, requestModel.SourcePath), requestModel.ProjectId, projectDetails.BucketNames.demo);
                if (serializedMetaInfo == null)
                    throw new Exception("SoucePath meta Info file does not exist");
                var metaInfo = Helpers.ProtoDeserialize<List<ObjectReference>>(serializedMetaInfo);
                if (metaInfo == null)
                    throw new Exception("An error occured while deserializing meta info");
                return metaInfo;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        internal static CloudProviderSettings GetCloudProviderDetails(string projectId)
        {
            if (projectId == null)
            {
                throw new ArgumentNullException(nameof(projectId));
            }

            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();

                var CloudProviderCollection = _kitsuneDatabase.GetCollection<KitsuneCloudProviderCollectionModel>(KitsuneCloudProviderCollectionName);
                var cloudProviderDetails = CloudProviderCollection.Find(x => x.projectid == projectId && x.isarchived == false).FirstOrDefault();

                if (cloudProviderDetails == null)
                    return null;

                CloudProviderSettings cloudProviderSettings = CloudProviderFactory.GetInstance(cloudProviderDetails.provider);

                var configurationSettings = EnvironmentConstants.ApplicationConfiguration;
                string encryptionKey = configurationSettings.CloudProviderCredentialsEncryptionKey;
                EncryptDecryptHelper decryptionHelper = new EncryptDecryptHelper(encryptionKey, projectId, System.Security.Cryptography.PaddingMode.PKCS7);
                switch (cloudProviderDetails.provider)
                {
                    case CloudProvider.AliCloud:
                        AliCloudProviderSettings settings = (AliCloudProviderSettings)cloudProviderSettings;
                        settings.accountId = cloudProviderDetails.accountid;
                        settings.key = cloudProviderDetails.key;
                        settings.region = cloudProviderDetails.region;
                        //settings.secret = decryptionHelper.Decrypt(cloudProviderDetails.secret);
                        settings.secret = cloudProviderDetails.secret;
                        break;
                    case CloudProvider.GCP:
                        GCPProviderSettings gcpSettings = (GCPProviderSettings)cloudProviderSettings;
                        //gcpSettings.secret = decryptionHelper.Decrypt(cloudProviderDetails.secret);
                        gcpSettings.secret = cloudProviderDetails.secret;
                        break;
                }

                return cloudProviderSettings;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        internal static bool CreateUpdateCloudProviderDetails(string projectId, CloudProviderModel providerModel)
        {
            if (providerModel == null)
            {
                throw new ArgumentNullException(nameof(providerModel));
            }

            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();

                var CloudProviderCollection = _kitsuneDatabase.GetCollection<KitsuneCloudProviderCollectionModel>(KitsuneCloudProviderCollectionName);

                var update = Builders<KitsuneCloudProviderCollectionModel>.Update.Set(x => x.isarchived, true);
                var projectIdFilter = Builders<KitsuneCloudProviderCollectionModel>.Filter.Eq(x => x.projectid, projectId);
                var result = CloudProviderCollection.UpdateOne(projectIdFilter, update);

                var cloudProviderDetails = CloudProviderCollection.Find(x => x.projectid == projectId);

                var configurationSettings = EnvironmentConstants.ApplicationConfiguration;
                string encryptionKey = configurationSettings.CloudProviderCredentialsEncryptionKey;
                EncryptDecryptHelper encryptionHelper = new EncryptDecryptHelper(encryptionKey, projectId, System.Security.Cryptography.PaddingMode.PKCS7);
                KitsuneCloudProviderCollectionModel collection = new KitsuneCloudProviderCollectionModel();
                switch (providerModel.provider)
                {
                    case CloudProvider.AliCloud:
                        collection.projectid = projectId;
                        collection.provider = providerModel.provider;
                        collection.accountid = providerModel.accountId;
                        collection.key = providerModel.key;
                        collection.region = providerModel.region;
                        //collection.secret = encryptionHelper.Encrypt(providerModel.secret);
                        collection.secret = providerModel.secret;
                        break;
                    case CloudProvider.GCP:
                        collection.projectid = projectId;
                        collection.provider = providerModel.provider;
                        //collection.secret = encryptionHelper.Encrypt(providerModel.secret);
                        collection.secret = providerModel.secret;
                        break;
                }
                collection.isarchived = false;
                collection.createddate = DateTime.UtcNow;
                CloudProviderCollection.InsertOne(collection);
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return false;
        }
        #endregion

        #region Conversion related function

        internal static ActivateSiteResponseModel ActivateSite(ActivateSiteRequestModel requestModel)
        {
            if (requestModel == null)
            {
                throw new ArgumentNullException(nameof(requestModel));
            }

            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();

                var websiteCollection = _kitsuneDatabase.GetCollection<KitsuneWebsiteCollection>(KitsuneWebsiteCollectionName);
                var ProjectCollection = _kitsuneDatabase.GetCollection<KitsuneProject>(KitsuneProjectsCollectionName);
                var ResourceCollection = _kitsuneDatabase.GetCollection<KitsuneResource>(KitsuneResourceCollectionName);

                var customerDocument = websiteCollection.Find(x => x._id == requestModel.WebsiteId).FirstOrDefault();

                #region Check and update KitsuneProjectsCollection (Change the kitsune Url and set IsPublishing to true)

                var update = Builders<KitsuneProject>.Update.Set(x => x.ProjectStatus, ProjectStatus.QUEUED);
                var projectIdFilter = Builders<KitsuneProject>.Filter.Eq(x => x.ProjectId, customerDocument.ProjectId);

                //Get ProjectStatus
                var projectStatus = ProjectCollection.Find(projectIdFilter).Project(x => x.ProjectStatus).FirstOrDefault();

                var projectActiveStatus = new List<ProjectStatus>() { ProjectStatus.ERROR, ProjectStatus.BUILDING, ProjectStatus.CRAWLING, ProjectStatus.PUBLISHING, ProjectStatus.QUEUED };
                if (projectActiveStatus.Contains(projectStatus))
                    return new ActivateSiteResponseModel { IsError = true, Message = $"project is {projectStatus.ToString().ToLower()}" };

                #endregion


                #region Update KitsuneProjectsCollection (Change the kitsune Url and set IsPublishing to true)

                var result = ProjectCollection.UpdateOne(projectIdFilter, update);

                #endregion

                if (result.IsAcknowledged && result.IsModifiedCountAvailable)
                {
                    #region Push into SQS

                    AmazonSQSQueueHandlers<PublishSQS> sqsHanlder = new AmazonSQSQueueHandlers<PublishSQS>(AmazonAWSConstants.PublishSQSUrl);
                    sqsHanlder.PushMessageToQueue(new PublishSQS { CustomerId = customerDocument._id, ProjectId = customerDocument.ProjectId },
                        EnvironmentConstants.ApplicationConfiguration.AWSSQSConfiguration.AWS_AccessKey, EnvironmentConstants.ApplicationConfiguration.AWSSQSConfiguration.AWS_SecretKey, RegionEndpoint.GetBySystemName(EnvironmentConstants.ApplicationConfiguration.Defaults.AWSSQSRegion));

                    ActivateSiteResponseModel response = new ActivateSiteResponseModel
                    {
                        IsError = false,
                        DomainVerified = true,
                        CNAMERecord = customerDocument.WebsiteUrl,
                        ARecord = EnvConstants.Constants.KitsuneRedirectIPAddress,
                        Domain = customerDocument.WebsiteUrl
                    };

                    return response;

                    #endregion
                }
                else
                {
                    return new ActivateSiteResponseModel { IsError = true, Message = "some error occured." };
                }
            }
            catch (Exception ex)
            {
                return new ActivateSiteResponseModel { IsError = true, Message = ex.Message };
            }
        }

        internal static bool ArchiveProject(ArchiveProjectRequestModel requestModel)
        {
            if (requestModel == null)
            {
                throw new ArgumentNullException(nameof(requestModel));
            }

            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();

                var ProjectCollection = _kitsuneDatabase.GetCollection<KitsuneProject>(KitsuneProjectsCollectionName);
                // Archive the project in the KitsuneProjects
                var filter = Builders<KitsuneProject>.Filter.Eq(x => x.ProjectId, requestModel.CrawlId);
                var update = Builders<KitsuneProject>.Update.Set(x => x.IsArchived, true).Set(x => x.ArchivedOn, DateTime.Now);
                var result = ProjectCollection.UpdateOne(filter, update);
                if (result.ModifiedCount == 1)
                {
                    return true;
                }
                return false;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        internal static GetProjectDownloadStatusResponseModel GetProjectDownloadStatus(GetProjectDownloadStatusRequestModel requestModel)
        {
            if (requestModel == null)
            {
                throw new ArgumentNullException(nameof(requestModel));
            }

            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();
                var ZipFolderStatsCollection = _kitsuneDatabase.GetCollection<KitsuneTaskDownloadQueueCollection>(TaskDownloadQueueCollectionName);
                var result = ZipFolderStatsCollection.Find(x => x.ProjectId == requestModel.CrawlId).SortByDescending(x => x.CreatedOn).First();
                return new GetProjectDownloadStatusResponseModel { LinkUrl = result.DownloadUrl, Status = result.Status, StatusMessage = result.Message };
            }
            catch (Exception ex)
            {
                return new GetProjectDownloadStatusResponseModel { StatusMessage = "Error while getting value", Status = TaskDownloadQueueStatus.Error };
            }
        }

        internal static GetProjectDownloadStatusResponseModelv2 GetProjectDownloadStatusv2(GetProjectDownloadStatusRequestModelv2 requestModel)
        {
            if (requestModel == null)
            {
                throw new ArgumentNullException(nameof(requestModel));
            }

            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();
                var ZipFolderStatsCollection = _kitsuneDatabase.GetCollection<KitsuneTaskDownloadQueueCollection>(TaskDownloadQueueCollectionName);
                var project = Builders<BsonDocument>.Projection.Include("LinkUrl").Include("Status").Include("StatusMessage");
                var fdb = new FilterDefinitionBuilder<KitsuneTaskDownloadQueueCollection>();
                var KitsuneTaskDownloadFilter = fdb.In(x => x.ProjectId, requestModel.ProjectId);
                var downloadStatusAggregate = ZipFolderStatsCollection.Aggregate()
                                                     .Match(x => requestModel.ProjectId.Contains(x.ProjectId))
                                                     .SortByDescending(x => x.CreatedOn)
                                                     .Group(new BsonDocument { { "_id", "$ProjectId" },
                                                                               { "LinkUrl", new BsonDocument( "$first", "$DownloadUrl" ) },
                                                                               { "Status", new BsonDocument( "$first", "$Status" ) },
                                                                               { "StatusMessage", new BsonDocument( "$first", "$Message") } })
                                                     .Project<ProjectDownloadStatus>(project);
                var downloadStatusResultList = downloadStatusAggregate.ToList();
                List<ProjectDownloadStatus> ProjectDownloadStatusList = new List<ProjectDownloadStatus>();
                if (downloadStatusResultList.Count == 0)
                    return new GetProjectDownloadStatusResponseModelv2
                    {
                        ProjectDownloadStatusList = new List<ProjectDownloadStatus>() {new ProjectDownloadStatus
                                         {
                                             StatusMessage = "No Download Status Document Exist for given ProjectIds",
                                             Status = TaskDownloadQueueStatus.Error
                                         } }
                    };
                foreach (var downloadStatus in downloadStatusResultList)
                {
                    ProjectDownloadStatusList.Add(new ProjectDownloadStatus
                    {
                        ProjectId = downloadStatus._id,
                        LinkUrl = downloadStatus.LinkUrl,
                        Status = downloadStatus.Status,
                        StatusMessage = downloadStatus.StatusMessage
                    });
                }
                return new GetProjectDownloadStatusResponseModelv2
                {
                    ProjectDownloadStatusList = ProjectDownloadStatusList
                };
            }
            catch (Exception ex)
            {
                return new GetProjectDownloadStatusResponseModelv2
                {
                    ProjectDownloadStatusList = new List<ProjectDownloadStatus>() {new ProjectDownloadStatus
                                         {
                                             StatusMessage = "Error while getting value",
                                             Status = TaskDownloadQueueStatus.Error
                                         } }
                };
            }
        }

        internal static DownloadFolderResponseModel DownloadProject(DownloadFolderRequestModel requestModel)
        {
            if (requestModel == null)
            {
                throw new ArgumentNullException(nameof(requestModel));
            }

            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();
                var ZipFolderStatsCollection = _kitsuneDatabase.GetCollection<KitsuneTaskDownloadQueueCollection>(TaskDownloadQueueCollectionName);
                var kitsuneTaskCollection = ZipFolderStatsCollection.Count(x => x.ProjectId == requestModel.CrawlId && x.UserEmail == requestModel.UserEmail && (x.Status == TaskDownloadQueueStatus.Started));

                if (kitsuneTaskCollection == 0)
                {
                    ObjectId objectId = ObjectId.GenerateNewId();
                    KitsuneTaskDownloadQueueCollection collection = new KitsuneTaskDownloadQueueCollection
                    {
                        ProjectId = requestModel.CrawlId,
                        Status = TaskDownloadQueueStatus.Started,
                        Message = "preparing your zip...",
                        CreatedOn = DateTime.UtcNow,
                        UserEmail = requestModel.UserEmail,
                        _id = objectId.ToString()
                    };

                    ZipFolderStatsCollection.InsertOne(collection);

                    var amazonSqsQueueHandler = new AmazonSQSQueueHandlers<ZipSQSModel>(AmazonAWSConstants.DownloadSQSUrl);

                    ZipSQSModel sqsModel = new ZipSQSModel
                    {
                        ZippingId = objectId.ToString(),
                        ProjectId = requestModel.CrawlId
                    };
                    amazonSqsQueueHandler.PushMessageToQueue(sqsModel, EnvironmentConstants.ApplicationConfiguration.AWSSQSConfiguration.AWS_AccessKey,
                        EnvironmentConstants.ApplicationConfiguration.AWSSQSConfiguration.AWS_SecretKey, RegionEndpoint.GetBySystemName(EnvironmentConstants.ApplicationConfiguration.Defaults.AWSSQSRegion));

                    return new DownloadFolderResponseModel { Status = TaskDownloadQueueStatus.Started, Message = "creating a zip file" };
                }
                else
                {
                    return new DownloadFolderResponseModel { Status = TaskDownloadQueueStatus.Error, Message = "A similar task already exists in the queue, please wait" };
                }
            }
            catch (Exception ex)
            {
                return new DownloadFolderResponseModel { Status = TaskDownloadQueueStatus.Error, Message = "Error while downloading the folder" };
            }
        }

        internal static GetListOfAllTasksResponseModel GetListOfAllTasks(GetListOfAllTasksRequestModel requestModel)
        {
            if (requestModel == null)
            {
                throw new ArgumentNullException(nameof(requestModel));
            }

            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();
                var ZipFolderStatsCollection = _kitsuneDatabase.GetCollection<KitsuneTaskDownloadQueueCollection>(TaskDownloadQueueCollectionName);
                var BuildStatusCollection = _kitsuneDatabase.GetCollection<KitsuneBuildStatus>(BuildStatusCollectionName);
                var result = ZipFolderStatsCollection.Find(x => x.UserEmail == requestModel.UserEmail).SortByDescending(x => x.CompletedOn).ToList();

                var collectionResponse = result.ToList();
                var response = new List<KitsuneTaskDownloadQueueCollection>();

                for (int i = 0; i < collectionResponse.Count; i++)
                {
                    var isBuildTaskCreationGreaterThanZipFolder = BuildStatusCollection.Count(x => x.CreatedOn > collectionResponse[i].CreatedOn && x.ProjectId == collectionResponse[i].ProjectId);
                    if (isBuildTaskCreationGreaterThanZipFolder < 1)
                    {
                        switch (collectionResponse[i].Status)
                        {
                            case TaskDownloadQueueStatus.Started:
                                collectionResponse[i].Message = "preparing zip file";
                                break;

                            case TaskDownloadQueueStatus.Completed:
                                collectionResponse[i].Message = "download zip file, ready";
                                //Click to download once ready
                                break;

                            case TaskDownloadQueueStatus.Error:
                                collectionResponse[i].Message = "download zip failed, retry";
                                break;
                        }

                        if (response.Count(x => x.ProjectId == collectionResponse[i].ProjectId) < 1)
                            response.Add(collectionResponse[i]);
                    }
                }

                return new GetListOfAllTasksResponseModel { ListOfTask = response };
            }
            catch (Exception ex)
            {
                return new GetListOfAllTasksResponseModel { ListOfTask = null };
            }
        }

        internal static KSearchModel GetUrlForKeywords(GetUrlForKeywordRequestModel requestModel)
        {
            if (requestModel == null)
            {
                throw new ArgumentNullException(nameof(requestModel));
            }

            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();
                var websiteCollection = _kitsuneDatabase.GetCollection<KitsuneWebsiteCollection>(KitsuneWebsiteCollectionName);
                var ProductionResourceCollection = _kitsuneDatabase.GetCollection<ProductionKitsuneResource>(ProductionResorcesCollectionName);
                var ProductionProjectCollection = _kitsuneDatabase.GetCollection<ProductionKitsuneProject>(ProductionProjectCollectionName);
                var domain = requestModel.Domain.Trim(' ').ToUpper();

                #region Get ProjectId

                var projectId = websiteCollection.Find(x => x.WebsiteUrl.Equals(domain) && x.IsActive && !x.IsArchived)
                                                        .Project(x => x.ProjectId).FirstOrDefault();

                #endregion

                if (projectId == null)
                    return null;

                #region Get details from KitsuneProjectResourcesPoduction

                var kitsuneResourceProj = Builders<ProductionKitsuneResource>.Projection;
                var resourceInfo = ProductionResourceCollection.Find(x => x.ProjectId.Equals(projectId) && x.ResourceType.Equals(ResourceType.LINK))
                                                                         .Project<ProductionKitsuneResource>(kitsuneResourceProj.Include(x => x.SourcePath)
                                                                                                                    .Include(x => x.MetaData)
                                                                                                                    .Exclude(x => x._id)).ToList();

                #endregion

                #region Get details from KitsuneProjectPoduction

                var projectProj = Builders<ProductionKitsuneProject>.Projection;
                var projectInfo = ProductionProjectCollection.Find(x => x.ProjectId.Equals(projectId))
                                                                         .Project<ProductionKitsuneProject>(projectProj.Include(x => x.FaviconIconUrl)
                                                                                                                    .Exclude(x => x._id)).FirstOrDefault();

                #endregion

                var searchModel = new KSearchModel
                {
                    SearchObjects = new List<SearchObject>(),
                    FaviconUrl = projectInfo.FaviconIconUrl
                };
                var searchKeyword = requestModel.Keyword.ToLower();

                foreach (var link in resourceInfo)
                {
                    try
                    {
                        if (searchModel.SearchObjects.FindIndex(url => url.S3Url.Equals("https://" + requestModel.Domain + link.SourcePath)) > -1)
                            continue;

                        int weight = 0;
                        List<string> selectedKeyword = new List<string>();
                        foreach (var keyword in link.MetaData.Keywords)
                        {
                            //if (keyword.Split(' ').Contains(query.Keyword))
                            //{
                            var count = searchKeyword.Split(' ').Count(searchStringSelector => keyword.ToLower().Split(' ').Contains(searchStringSelector));
                            if (count > 0)
                            {
                                selectedKeyword.Add(keyword);
                            }
                            weight = weight + count;
                            //}
                        }

                        if (weight > 0)
                        {
                            searchModel.SearchObjects.Add(new SearchObject { Count = weight, S3Url = "http://" + requestModel.Domain + link.SourcePath, Keywords = selectedKeyword, Title = link.MetaData.Title, Description = link.MetaData.Description });
                        }
                    }
                    catch { }
                }
                searchModel.SearchObjects = searchModel.SearchObjects.OrderByDescending(x => x.Count).ToList();
                return searchModel;

            }
            catch (Exception ex)
            {
                return null;
            }
        }

        internal static string GetSiteMapOfWebite(GetSiteMapRequestModel requestModel)
        {
            if (requestModel == null)
            {
                throw new ArgumentNullException(nameof(requestModel));
            }
            try
            {
                var siteMap = AmazonS3FileProcessor.getFileFromS3(string.Format(CompilerConstants.SiteMapFileName, requestModel.WebsiteId), requestModel.ProjectId, AmazonAWSConstants.ProductionBucketName);
                if (siteMap != null)
                {
                    return siteMap;
                }
            }
            catch { }

            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();
                var websiteCollection = _kitsuneDatabase.GetCollection<KitsuneWebsiteCollection>(KitsuneWebsiteCollectionName);
                var ProductionResourceCollection = _kitsuneDatabase.GetCollection<ProductionKitsuneResource>(ProductionResorcesCollectionName);
                var ProductionProjectCollection = _kitsuneDatabase.GetCollection<ProductionKitsuneProject>(ProductionProjectCollectionName);
                var projection = new ProjectionDefinitionBuilder<KitsuneWebsiteCollection>();
                var domain = requestModel.Domain.Trim(' ').ToUpper();

                #region

                var projectId = websiteCollection.Find(x => x.WebsiteUrl.Equals(domain) && x.IsActive && !x.IsArchived)
                                                  .Project(x => x.ProjectId).FirstOrDefault();

                #endregion

                #region Get the Links
                var project = Builders<ProductionKitsuneResource>.Projection;
                var links = ProductionResourceCollection.Find(x => x.ProjectId.Equals(projectId) && x.ResourceType.Equals(ResourceType.LINK))
                                                                         .Project<SiteMapModel>(project.Include(x => x.UpdatedOn)
                                                                                         .Include(x => x.SourcePath)
                                                                                         .Exclude(x => x._id)).ToList();


                #endregion

                Uri uri = null;
                if (!(requestModel.Domain.StartsWith("https://", StringComparison.InvariantCultureIgnoreCase) || requestModel.Domain.StartsWith("http://", StringComparison.InvariantCultureIgnoreCase)))
                {
                    uri = new Uri("http://" + requestModel.Domain);
                }
                else
                {
                    uri = new Uri(requestModel.Domain);
                }

                ////quick fix 
                //links = links.Where(x => !x.SourcePath.EndsWith(".dl",StringComparison.InvariantCultureIgnoreCase)).ToList();
                var siteMap = SiteMapGenerator.Create(uri, links, new List<string> { ".dl" });
                return siteMap;

            }
            catch (Exception ex)
            {
                return null;
            }
        }


        internal static WordPressProjectStatusResponseModel GetWordPressProjectStatus(WordPressProjectStatusRequestModel requestModel)
        {
            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();

                var WordpressCollection = _kitsuneDatabase.GetCollection<KitsuneWordPress>(KitsuneWordPressCollection);
                var project = new ProjectionDefinitionBuilder<KitsuneWordPress>();
                var result = WordpressCollection.Find(x => x.ProjectId == requestModel.ProjectId).Project<WordPressProjectStatusResponseModel>(project.Include(x => x.Stage)
                                                                                                                                                      .Include(x => x.WordpressUser)
                                                                                                                                                      .Include(x => x.WordpressPassword)
                                                                                                                                                      .Include(x => x.isScheduled)
                                                                                                                                                      .Include(x => x.Domain)
                                                                                                                                                      .Exclude(x => x._id)).FirstOrDefault();
                if (result != null)
                {
                    if (result.Stage == KitsuneWordPressStats.IDLE)
                        return new WordPressProjectStatusResponseModel
                        {
                            Stage = result.Stage,
                            WordpressUser = result.WordpressUser,
                            WordpressPassword = result.WordpressPassword,
                            isScheduled = result.isScheduled,
                            Domain = result.Domain
                        };
                    else
                        return new WordPressProjectStatusResponseModel
                        {
                            Stage = result.Stage,
                        };
                }
                return new WordPressProjectStatusResponseModel();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region WebFormsHTML
        private static readonly string WebactionBaseUrl = "http://webactions.kitsune.tools/api/v1/webformsjsontext/";

        //for system use only
        private static readonly string DefaultKitsuneUserId = "5959ec985d643701d48ee8ab";

        internal static string AddWebformJSON(WebFormUpdateRequestModel requestModel)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(WebactionBaseUrl);
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(DefaultKitsuneUserId);
            var result = client.PostAsync("add-data", new StringContent($"{{WebsiteId : 'dashboard.kitsune.tools', ActionData : {{ webformid : '{requestModel.WebFormId}', json : \"{requestModel.JsonString}\" }} }}", Encoding.UTF8, "application/json")).Result;

            return result?.Content?.ReadAsStringAsync()?.Result;
        }

        internal static string UpdateWebformJSON(WebFormUpdateRequestModel requestModel)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(WebactionBaseUrl);
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(DefaultKitsuneUserId);
            var result = client.PostAsync("update-data", new StringContent($"{{Query : \"{{ webformid : '{requestModel.WebFormId}' }}\", UpdateValue : \"{{ $set : {{ json : '{HttpUtility.HtmlEncode(requestModel.JsonString)}' }} }}\" }}", Encoding.UTF8, "application/json")).Result;
            return result?.Content?.ReadAsStringAsync()?.Result;
        }

        internal static object GetWebformJSON(string webFormId)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(WebactionBaseUrl);
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(DefaultKitsuneUserId);
            var result = client.GetAsync($"get-data?query={{ webformid : '{webFormId}'}}").Result;

            var responseText = result?.Content?.ReadAsStringAsync()?.Result;
            if (!string.IsNullOrEmpty(responseText))
                return JsonConvert.DeserializeObject<dynamic>(HttpUtility.HtmlDecode(responseText));

            return null;
        }
        #endregion

        #region Project Config

        internal static string CreateOrUpdateProjectConfig(CreateOrUpdateProjectConfigRequestModel requestModel)
        {
            if (requestModel == null)
            {
                throw new ArgumentNullException(nameof(requestModel));
            }
            try
            {
                //Upload in s3
                //Update in KitsuneResource
                Byte[] byteData = Encoding.ASCII.GetBytes(requestModel.File?.Content);
                string filePath = String.Format(Kitsune.API2.EnvConstants.Constants.ProjectConfigurationS3FilePath, requestModel.ProjectId);
                var result = S3UploadHelper.SaveAssetToS3(EnvironmentConstants.ApplicationConfiguration.AWSS3Configuration.AWS_AccessKey,
                                                        EnvironmentConstants.ApplicationConfiguration.AWSS3Configuration.AWS_SecretKey,
                                                        AmazonAWSConstants.SourceBucketName,
                                                        filePath,
                                                        byteData,
                                                        contentType: string.IsNullOrEmpty(requestModel.File.ContentType) ? "application/json" : requestModel.File.ContentType,
                                                        isPublic: false);
                if (!result.IsSuccess)
                    throw new Exception("Error uploading file to s3");

                if (_kitsuneServer == null)
                    InitializeConnection();
                var resourceCollection = _kitsuneDatabase.GetCollection<KitsuneResource>(KitsuneResourceCollectionName);

                DateTime dateTime = DateTime.Now;
                var updateBuilder = new UpdateDefinitionBuilder<KitsuneResource>();
                var update = updateBuilder.SetOnInsert(x => x.Version, 1)
                                          .SetOnInsert(x => x.CreatedOn, dateTime)
                                          .Set(x => x.ProjectId, requestModel.ProjectId)
                                          .Set(x => x.SourcePath, Kitsune.API2.EnvConstants.Constants.ProjectConfigurationFilePath)
                                          .Set(x => x.OptimizedPath, Kitsune.API2.EnvConstants.Constants.ProjectConfigurationFilePath)
                                          .Set(x => x.IsArchived, false)
                                          .Set(x => x.IsDefault, false)
                                          .Set(x => x.IsStatic, false)
                                          .Set(x => x.PageType, KitsunePageType.DEFAULT)
                                          .Set(x => x.ResourceType, ResourceType.FILE)
                                          .Set(x => x.UpdatedOn, dateTime);
                var updateResult = resourceCollection.UpdateOne(x => x.ProjectId.Equals(requestModel.ProjectId), update, new UpdateOptions { IsUpsert = true });
                if (updateResult.IsAcknowledged)
                    return "Success";
                else
                    throw new Exception("Saved to s3 Successfully but unable to Update DB");
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        internal static GetProjectConfigResponseModel GetProjectConfig(GetProjectConfigRequestModel requestModel)
        {
            if (requestModel == null)
            {
                throw new ArgumentNullException(nameof(requestModel));
            }
            try
            {
                string filePath = EnvConstants.Constants.ProjectConfigurationFilePath;
                string bucketName = AmazonAWSConstants.ProductionBucketName;
                string fileS3Path = requestModel.ProjectId + filePath;

                if (requestModel.Level.Equals(FileLevel.PROD))
                {
                    //Get last published version
                    if (_kitsuneServer == null)
                        InitializeConnection();
                    var projectProductionCollection = _kitsuneDatabase.GetCollection<ProductionKitsuneProject>(ProductionProjectCollectionName);
                    var project = projectProductionCollection.Find(x => x.ProjectId.Equals(requestModel.ProjectId)).FirstOrDefault();
                    if (project == null)
                        throw new Exception("Error getting version from DB");
                    fileS3Path = requestModel.ProjectId + "/v" + project.Version + filePath;
                }

                switch (requestModel.Level)
                {
                    case FileLevel.SOURCE:
                        bucketName = AmazonAWSConstants.SourceBucketName;
                        fileS3Path = requestModel.ProjectId + filePath;
                        break;
                    case FileLevel.DEMO:
                        bucketName = AmazonAWSConstants.DemoBucketName;
                        fileS3Path = requestModel.ProjectId + "/cwd" + filePath;
                        break;
                }

                var result = S3UploadHelper.GetAssetFromS3(EnvironmentConstants.ApplicationConfiguration.AWSS3Configuration.AWS_AccessKey,
                                                        EnvironmentConstants.ApplicationConfiguration.AWSS3Configuration.AWS_SecretKey,
                                                        bucketName,
                                                        fileS3Path);
                if (result.IsSuccess)
                {
                    return new GetProjectConfigResponseModel { File = new ConfigFile { Content = result.File.Content, ContentType = result.File.ContentType } };
                }
                else
                {
                    throw new Exception(result.Message);
                }
            }
            catch (Exception ex)
            {
                //Log the Error
                throw ex;
            }
        }

        internal static string ValidateProjectConfig(ValidateConfigRequestModel requestModel)
        {
            if (requestModel == null)
            {
                throw new ArgumentNullException(nameof(requestModel));
            }
            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();

                var ProjectCollection = _kitsuneDatabase.GetCollection<KitsuneProject>(KitsuneProjectsCollectionName);
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        internal static string GetKitsuneSettings(string projectId, int version)
        {
            var configSourcePath = "/kitsune-settings.json";
            var projectSettings = AmazonS3FileProcessor.getFileFromS3(configSourcePath, projectId, AmazonAWSConstants.ProductionBucketName, version: version);
            if (!String.IsNullOrEmpty(projectSettings))
            {
                return JsonConvert.DeserializeObject<JObject>(projectSettings).ToString();
            }
            return null;
        }
        #endregion

        #region App/Component

        internal static GetAllProjectsWithConfigResponseModel GetProjectDetailsPerEnabledComponent(string componentId, string filter = null, string sort = null, int skip = 0, int limit = 100, bool includeEmptyConfig = false)
        {
            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();

                var ProjectCollection = _kitsuneDatabase.GetCollection<BsonDocument>(ProductionProjectCollectionName);

                var componentDetails = _kitsuneDatabase.GetCollection<ProductionKitsuneProject>(ProductionProjectCollectionName).Find(x => (x.ProjectId == componentId) && x.ProjectType == ProjectType.APP).Limit(1)?.ToList().FirstOrDefault();
                if (componentDetails != null)
                {
                    var filterDefinition = new FilterDefinitionBuilder<BsonDocument>().Empty;
                    var configSourcePath = "/kitsune-settings.json";

                    var sortDoc = new BsonDocument("CreatedOn", -1);
                    BsonDocument filterDoc = new BsonDocument();

                    if (!string.IsNullOrEmpty(filter))
                    {
                        if (BsonDocument.TryParse(filter, out filterDoc))
                        {
                            filterDefinition = filterDoc;
                        }
                    }

                    var filterDefinition2 = new FilterDefinitionBuilder<BsonDocument>().Eq("Components.ProjectId", componentId);
                    filterDefinition = filterDefinition & filterDefinition2;

                    if (!string.IsNullOrEmpty(sort))
                    {
                        BsonDocument tempSort = new BsonDocument();
                        if (BsonDocument.TryParse(sort, out tempSort))
                        {
                            sortDoc = tempSort;
                        }
                    }

                    var count = ProjectCollection.Count(filterDefinition);
                    var projects = ProjectCollection.Find(filterDefinition).Sort(sortDoc).Skip(skip).Limit(limit)
                        .Project(new ProjectionDefinitionBuilder<BsonDocument>()
                        .Include("ProjectId")
                        .Include("ProjectName")
                        .Include("SchemaId")
                        .Include("UpdatedOn")
                        .Include("CreatedOn")
                        .Include("UserEmail")
                        .Include("Version")
                        .Exclude("_id"));

                    var result = new List<ProjectComponentModel>();
                    if (count > 0)
                    {
                        result = projects.ToList().Select(x => BsonSerializer.Deserialize<ProjectComponentModel>(x)).ToList();
                    }
                    //Get the config file
                    foreach (var project in result)
                    {
                        var projectSettings = AmazonS3FileProcessor.getFileFromS3(configSourcePath, project.ProjectId, AmazonAWSConstants.ProductionBucketName, version: project.Version);
                        if (!String.IsNullOrEmpty(projectSettings))
                        {
                            project.Settings = JsonConvert.DeserializeObject<JObject>(projectSettings)?.GetValue(componentDetails.ProjectName)?.ToString();
                        }
                    }
                    return new GetAllProjectsWithConfigResponseModel
                    {
                        Pagination = new Pagination
                        {
                            CurrentIndex = skip,
                            PageSize = limit,
                            TotalCount = count
                        },
                        Projects = !includeEmptyConfig ? result.Where(x => !string.IsNullOrEmpty(x.Settings)).ToList() : result
                    };
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return null;
        }

        internal static List<string> GetAllProjectsPerUser(string emailId)
        {
            if (String.IsNullOrEmpty(emailId))
            {
                throw new ArgumentNullException(nameof(emailId));
            }

            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();

                var projectsCollection = _kitsuneDatabase.GetCollection<KitsuneProject>(KitsuneProjectsCollectionName);

                var result = projectsCollection.Find(x => x.UserEmail == emailId && !x.IsArchived)
                                                .Project(x => x.ProjectId)
                                                .ToList();

                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region Cache Related

        internal static bool AddCDNCacheInvalidationTask(string cacheTag)
        {
            if (string.IsNullOrWhiteSpace(cacheTag))
                return false;
            try
            {
                if (_logDatabase == null)
                    InitializeConnection();
                var cacheInvalidationTasksCollection = _logDatabase.GetCollection<CacheInvalidationTaskModel>("CacheInvalidationTasks");

                cacheInvalidationTasksCollection.InsertOne(new CacheInvalidationTaskModel()
                {
                    _id = ObjectId.GenerateNewId().ToString(),
                    CacheKey = cacheTag.Trim(),
                    Invalidated = false,
                    CreatedOn = DateTime.Now,
                    type = CacheType.AKAMAI_CACHE_TAG
                });

                return true;
            }
            catch(Exception ex)
            {
                throw new Exception($"AddCDNCacheInvalidationTask failed with exception {ex}");
            }
        }
        #endregion
    }
}
