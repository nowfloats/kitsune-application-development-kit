using Kitsune.API.Model.ApiRequestModels;
using Kitsune.API2.DataHandlers.Mongo;
using Kitsune.API2.EnvConstants;
using Kitsune.API2.Models;
using Kitsune.API2.Utils;
using Kitsune.API2.Validators;
using Kitsune.BasePlugin.Utils;
using Kitsune.Compiler.Helpers;
using Kitsune.Models.PublishModels;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Kitsune.API2.Controllers
{
    /// <summary>
    /// API's related to Project And Resources operations
    /// </summary>
    [Route("api/Project")]
    public class ProjectController : Controller
    {
        /// <summary>
        /// Create a new project or update the existing project
        /// </summary>
        /// <param name = "requestModel" ></ param >
        /// < returns ></ returns >
        [HttpPost]
        [Route("v1/Project")]
        public IActionResult CreateOrUpdateProject([FromBody]CreateOrUpdateProjectRequestModel requestModel)
        {
            try
            {
                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return BadRequest(validationResult);

                return Ok(MongoConnector.CreateOrUpdateProject(requestModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPost]
        [Route("v1/CreateWordPress")]
        public IActionResult CreateOrUpdateWordPressProject([FromBody]CreateWordPressProjectRequestModel requestModel)
        {
            try
            {
                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return BadRequest(validationResult);

                return Ok(MongoConnector.CreateWordPressProject(requestModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        /// <summary>
        /// Updates the version of the project
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="version"></param>
        /// <param name="userEmail"></param>
        /// <param name="publishedVersion"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("v1/Project/{projectId}/UpdateVersion")]
        public IActionResult UpdateProjectVersion([FromRoute]string projectId, [FromQuery]int version, [FromQuery]string userEmail, [FromQuery]int publishedVersion = 0)
        {
            try
            {
                var requestModel = new UpdateProjectVersionRequestModel
                {
                    UserEmail = userEmail,
                    ProjectId = projectId,
                    PublishedVersion = publishedVersion,
                    Version = version
                };
                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return BadRequest(validationResult);

                return Ok(MongoConnector.UpdateProjectVersion(requestModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        /// <summary>
        /// Create or update the resource 
        /// </summary>
        /// <param name="requestModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("v1/Resource")]
        public IActionResult CreateOrUpdateResource([FromBody]CreateOrUpdateResourceRequestModel requestModel)
        {
            try
            {
                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return BadRequest(validationResult);

                return Ok(MongoConnector.CreateOrUpdateResource(requestModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        /// <summary>
        /// Get the list of project for a particular user
        /// </summary>
        /// <param name="userEmail"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("v1/Projects")]
        public IActionResult GetProjects([FromQuery]string userEmail)  //bool getAll = false
        {
            try
            {
                var requestModel = new GetProjectsListRequestModel
                {
                    UserEmail = userEmail
                };
                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return BadRequest(validationResult);

                return Ok(MongoConnector.GetProjectsList(requestModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
        /// <summary>
		/// Get the list of project for a particular user
		/// </summary>
		/// <param name="userEmail"></param>
		/// <returns></returns>
		[HttpGet]
        [Route("v2/Projects")]
        public IActionResult GetProjectsWithPagination([FromQuery]string userEmail, [FromQuery]int skip = 0, [FromQuery]int limit = 100)  //bool getAll = false
        {
            try
            {
                var requestModel = new GetProjectsListRequestModelV2
                {
                    UserEmail = userEmail,
                    Skip = skip,
                    Limit = limit
                };
                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return BadRequest(validationResult);

                return Ok(MongoConnector.GetProjectsListWithPagination(requestModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
        /// <summary>
        /// Gives the project details of the given projectId
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="userEmail"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("v1/ProjectDetails/{projectId}")]
        public IActionResult GetProjectDetails([FromRoute]string projectId, [FromQuery]string userEmail, [FromQuery]bool excludeResources = false)
        {
            try
            {
                var requestModel = new GetProjectDetailsRequestModel
                {
                    ProjectId = projectId,
                    ExcludeResources = excludeResources,
                    UserEmail = userEmail
                };
                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return BadRequest(validationResult);

                return Ok(MongoConnector.GetProjectDetails(requestModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpGet]
        [Route("v1/ProjectStatus/{projectId}")]
        public IActionResult GetProjectStatus([FromRoute]string projectId, [FromHeader]string ClientId)
        {
            try
            {
                if (String.IsNullOrEmpty(ClientId))
                    return Unauthorized();
                if (!ClientId.Equals("d5d8fb95-006f-41c5-85a0-a9c7ae54bb17"))
                    return Unauthorized();

                var requestModel = new GetProjectStatusRequestModel
                {
                    ProjectId = projectId
                };
                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return BadRequest(validationResult);

                return Ok(MongoConnector.GetProjectStatus(requestModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Gives the project details of the given projectId
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="clientid"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("v1/ProjectDetails/{projectId}/{clientid}")]
        public IActionResult GetProjectDetailsByClientId([FromRoute]string projectId, [FromRoute]string clientid, [FromQuery]bool excludeResources = false)
        {
            try
            {
                if (!string.IsNullOrEmpty(clientid) && BasePluginConfigGenerator.GetBasePlugin(clientid).GetClientId() == clientid.Trim().ToUpper())
                {
                    return Ok(MongoConnector.GetProjectDetails(projectId));
                }
                return Unauthorized();
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        /// <summary>
        /// Get page details of the given page
        /// To support pagename with / for folder structure
        /// </summary>
        /// <param name="ProjectId"></param>
        /// <param name="PageName"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("v1/Project/{projectId}/Resource")]
        public IActionResult GetResourceDetailsByName([FromRoute]string projectId, [FromQuery]string sourcePath, [FromQuery]string userEmail = null, [FromQuery]string clientid = null)
        {
            try
            {
                if (string.IsNullOrEmpty(userEmail) && (string.IsNullOrEmpty(clientid) || BasePluginConfigGenerator.GetBasePlugin(clientid).GetClientId() == clientid.Trim().ToUpper()))
                {
                    return Unauthorized();
                }
                var requestModel = new GetResourceDetailsRequestModel
                {
                    ProjectId = projectId,
                    SourcePath = sourcePath
                };
                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return BadRequest(validationResult);

                return Ok(MongoConnector.GetResourceDetails(requestModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        /// <summary>
        /// Makes a page default for a particular projectId
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="userEmail"></param>
        /// <param name="sourcePath"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("v1/Project/{projectId}/MakeResourceDefault")]
        public IActionResult MakeResourceDefault([FromRoute]string projectId, [FromQuery]string userEmail, [FromQuery]string sourcePath)
        {
            try
            {
                var requestModel = new MakeResourceAsDefaultRequestModel
                {
                    ProjectId = projectId,
                    SourcePath = sourcePath,
                    UserEmail = userEmail
                };
                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return BadRequest(validationResult);

                return Ok(MongoConnector.MakeResourceAsDefault(requestModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        /// <summary>
        /// Get list of partial Pages
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="userEmail"></param>
        /// <param name="sourcePath"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("v1/Project/{projectId}/PartialPages")]
        public IActionResult GetPartialPagesDetails([FromRoute]string projectId, [FromQuery]string userEmail, [FromQuery]string sourcePath = null)
        {
            try
            {
                var requestModel = new GetPartialPagesDetailsRequestModel
                {
                    ProjectId = projectId,
                    UserEmail = userEmail
                };
                if (!string.IsNullOrEmpty(sourcePath))
                    requestModel.SourcePaths = sourcePath.Split(',').ToList();
                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return BadRequest(validationResult);

                return Ok(MongoConnector.GetPartialPagesDetails(requestModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        /// <summary>
        /// Get Project details and the resources related to the project
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="userEmail"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("v1/ProjectAndResources/{projectId}")]
        public IActionResult GetProjectWithResourcesDetails([FromRoute]string projectId, [FromQuery]string userEmail)
        {
            try
            {
                var requestModel = new GetProjectWithResourcesDetailsRequestModel
                {
                    ProjectId = projectId,
                    UserEmail = userEmail
                };
                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return BadRequest(validationResult);

                return Ok(MongoConnector.GetProjectWithResourceDetails(requestModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        /// <summary>
        /// Delete the project(Archive the project)
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="userEmail"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("v1/Projects/{projectId}")]
        public IActionResult DeleteProject([FromRoute]string projectId, [FromQuery]string userEmail)
        {
            try
            {
                var requestModel = new DeleteProjectRequestModel
                {
                    ProjectId = projectId,
                    UserEmail = userEmail
                };
                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return BadRequest(validationResult);

                return Ok(MongoConnector.DeleteProject(requestModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        /// <summary>
        /// Delete the given resource of the Project
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="sourcePath"></param>
        /// <param name="userEmail"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("v1/Projects/{projectId}/Resources")]
        public IActionResult DeleteResources([FromRoute]string projectId, [FromQuery]string userEmail, [FromQuery]string sourcePath = null, [FromQuery]string folderPath = null)
        {
            try
            {
                var requestModel = new DeleteResourceRequestModel
                {
                    ProjectId = projectId,
                    UserEmail = userEmail,
                    SourceName = sourcePath,
                    SourcePath = folderPath
                };
                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return BadRequest(validationResult);

                return Ok(MongoConnector.DeleteResource(requestModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
        /// <summary>
        /// Rename resource of the Project
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="oldPath"></param>
        /// <param name="newPath"></param>
        /// <param name="userEmail"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("v1/Projects/{projectId}/RenameResource")]
        public IActionResult RenameResources([FromRoute]string projectId, [FromQuery]string userEmail, [FromQuery]string oldPath, [FromQuery]string newPath)
        {
            try
            {
                var requestModel = new RenameResourceModel
                {
                    ProjectId = projectId,
                    UserEmail = userEmail,
                    NewSourceName = newPath,
                    OldSourceName = oldPath
                };
                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return BadRequest(validationResult);

                return Ok(MongoConnector.RenameResource(requestModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
        /// <summary>
        /// Update the project Access level(userEmail)
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="useremail"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("v1/Project/{projectId}/UpdateProjectAccess")]
        public IActionResult UpdateProjectAccess([FromRoute]string projectId, [FromQuery]string useremail)
        {
            try
            {
                var requestModel = new UpdateProjectAccessRequestModel
                {
                    ProjectId = projectId,
                    UserEmail = useremail,
                };
                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return BadRequest(validationResult);

                return Ok(MongoConnector.UpdateProjectAccess(requestModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        /// <summary>
        /// Starts build or update the build status
        /// </summary>
        /// <param name="user"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("v1/Build")]
        public IActionResult StartBuild([FromQuery] string user, [FromBody]CreateOrUpdateKitsuneStatusRequestModel requestModel)
        {
            try
            {
                requestModel.UserEmail = user;
                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return BadRequest(validationResult);

                return Ok(MongoConnector.CreateOrUpdateKitsuneStatus(requestModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        /// <summary>
        /// Get the details of the Build Status(processing details,Like "links updated" or "styles optimized" )
        /// </summary>
        /// <param name="user"></param>
        /// <param name="projectid"></param>
        /// <param name="buildversion"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("v1/Build")]
        public IActionResult GetBuildDetails([FromQuery]string user, [FromQuery]string projectid, [FromQuery]int buildversion = 0)
        {
            try
            {
                var requestModel = new GetKitsuneBuildStatusRequestModel
                {
                    ProjectId = projectid,
                    UserEmail = user,
                    BuildVersion = buildversion
                };
                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return BadRequest(validationResult);

                return Ok(MongoConnector.GetKitsuneBuildStatus(requestModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpGet]
        [Route("v1/LastCompletedBuild")]
        public IActionResult LastCompletedBuildDetails([FromQuery]string user, [FromQuery]string projectid, [FromQuery]int buildversion = 0)
        {
            try
            {
                var requestModel = new GetKitsuneBuildStatusRequestModel
                {
                    ProjectId = projectid,
                    UserEmail = user,
                    BuildVersion = buildversion
                };
                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return BadRequest(validationResult);

                return Ok(MongoConnector.GetLastCompletedKitsuneBuildStatus(requestModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        /// <summary>
        /// Get the Build overall details (total number of links , assets updated for build)
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="userEmail"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("v1/ProjectDetailForBuild/{projectId}")]
        public IActionResult ProjectDetailForBuild([FromRoute]string projectId, [FromQuery]string userEmail)
        {
            try
            {
                var requestModel = new ProjectDetailsForBuildRequestModel
                {
                    ProjectId = projectId,
                    UserEmail = userEmail,
                };
                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return BadRequest(validationResult);

                return Ok(MongoConnector.ProjectDetailsForBuild(requestModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        /// <summary>
        /// Update the Production
        /// </summary>
        /// <param name="userEmail"></param>
        /// <param name="projectId"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("MakeProjectLive")]
        public IActionResult UpdateProductionDBDetails([FromBody]MakeProjectLiveRequestModel requestModel)
        {
            try
            {
                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return BadRequest(validationResult);

                return Ok(MongoConnector.UpdateProductionDBDetailsOld(requestModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPost]
        [Route("v2/MakeProjectLive")]
        public IActionResult UpdateProductionDBDetailsV2([FromBody]MakeProjectLiveV2RequestModel requestModel)
        {
            try
            {
                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return BadRequest(validationResult);

                return Ok(MongoConnector.UpdateProductionDBDetails(requestModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpGet]
        [Route("v1/GetCloudProviderDetails")]
        public IActionResult GetCloudProviderDetails([FromQuery] string projectId)
        {
            try
            {
                var userId = string.Empty;
                if (Request.Headers.ContainsKey("Authorization"))
                    userId = Request.Headers["Authorization"].ToString();

                if (string.IsNullOrWhiteSpace(userId))
                    return Unauthorized();

                if (string.IsNullOrEmpty(projectId))
                    return BadRequest("Invalid projectId");
                
                var projectUserId = MongoConnector.GetUserIdFromProjectId(projectId);

                if (projectUserId == null)
                    return BadRequest("Invalid projectId");

                if (projectUserId != userId)
                    return Unauthorized();

                return Ok(MongoConnector.GetCloudProviderDetails(projectId));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPost]
        [Route("v1/CreateUpdateCloudProvider")]
        public IActionResult CreateUpdateCloudProvider([FromQuery] string projectId, [FromBody] CloudProviderModel providerModel)
        {
            try
            {
                var userId = string.Empty;
                if (Request.Headers.ContainsKey("Authorization"))
                    userId = Request.Headers["Authorization"].ToString();

                if (string.IsNullOrWhiteSpace(userId))
                    return Unauthorized();

                if (string.IsNullOrEmpty(projectId))
                    return BadRequest("Invalid projectId");

                var projectUserId = MongoConnector.GetUserIdFromProjectId(projectId);

                if (projectUserId == null)
                    return BadRequest("Invalid projectId");

                if (projectUserId != userId)
                    return Unauthorized();

                if (providerModel.provider == Kitsune.Models.Cloud.CloudProvider.AliCloud && string.IsNullOrEmpty(providerModel.accountId))
                {
                    Helpers.PopulateDefaultAliCloudCreds(providerModel);
                }
                else
                {
                    var validationResult = providerModel.Validate();
                    if (validationResult.Any())
                        return BadRequest(validationResult);
                }
                
                return Ok(MongoConnector.CreateUpdateCloudProviderDetails(projectId, providerModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPost]
        [Route("v1/Publish")]
        public IActionResult PublishProject([FromQuery]string userEmail,
            [FromQuery]string copyFromWebsite = null,
            [FromQuery]bool copyDataFromDemoWebsite = false,
            [FromQuery]string customerId = null, //TODO : Remove customerId once every app updated with websiteId
            [FromQuery]string websiteId = null,
            [FromQuery]bool publishToAll = false,
            [FromQuery]string projectId = null)
        {
            try
            {
                var requestModel = new PublishCustomerRequestModel
                {
                    WebsiteId = customerId ?? websiteId,
                    UserEmail = userEmail,
                    CopyFromDemoWebsite = !publishToAll ? copyDataFromDemoWebsite : false,
                    CopyFromWebsiteId = !publishToAll ? copyFromWebsite : null,
                    PublishToAll = publishToAll,
                    ProjectId = projectId
                };
                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return BadRequest(validationResult);

                return Ok(MongoConnector.PublishCustomer(requestModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPost]
        [Route("v1/PublishProject")]
        public IActionResult PublishCustomers([FromBody]PublishProjectRequestModel requestModel)
        {
            try
            {
                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return BadRequest(validationResult);

                return Ok(MongoConnector.PublishProject(requestModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPost]
        [Route("v1/Download")]
        public IActionResult DownloadProject([FromQuery]string projectId, [FromQuery]string userEmail)
        {
            try
            {
                var requestModel = new DownloadProjectRequestModel
                {
                    ProjectId = projectId,
                    UserEmail = userEmail
                };
                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return BadRequest(validationResult);

                return Ok(MongoConnector.DownloadProject(requestModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpGet]
        [Route("v1/ProjectsInProcess")]
        public IActionResult ProjectDetailForBuild([FromQuery]string userEmail)
        {
            try
            {
                var requestModel = new GetProjectInProcessRequestModel
                {
                    UserEmail = userEmail,
                };
                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return BadRequest(validationResult);

                return Ok(MongoConnector.GetProjectInProcess(requestModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpGet]
        [Route("v1/CheckProjectUpdatedOrNot")]
        public IActionResult ProjectUpdatedOrNot([FromQuery]string projectId)
        {
            try
            {
                var requestModel = new GetProjectUpdatedOrNotRequest
                {
                    ProjectId = projectId,
                };
                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return BadRequest(validationResult);

                return Ok(MongoConnector.CheckProjectUpdatedOrNot(requestModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpGet]
        [Route("v1/MetaInfo")]
        public IActionResult GetResourceMetaInfo([FromQuery]string projectId, [FromQuery] string sourcePath)
        {
            try
            {
                var requestModel = new GetResourceMetaInfoRequest
                {
                    ProjectId = projectId,
                    SourcePath = sourcePath
                };
                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return BadRequest(validationResult);

                return Ok(MongoConnector.GetMetaInfo(requestModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }


        /// <summary>
        /// API to invalidate Akamai CDN cache by Edge-Cache-Tag
        /// </summary>
        /// <param name="cacheTag"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("v1/InvalidateCache")]
        public IActionResult InvalidateCache([FromQuery]string cacheTag)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(cacheTag))
                    return BadRequest("Incorrect Cache Tag");

                return Ok(MongoConnector.AddCDNCacheInvalidationTask(cacheTag));
            }
            catch(Exception ex)
            {
                return BadRequest(ex);
            }
        }

        #region GetKitsune

        [HttpPost]
        [Route("v1/KitsuneEnquiry")]
        public IActionResult KitsuneEnquiry([FromBody]KitsuneEnquiryRequestModel requestModel)
        {
            try
            {
                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return BadRequest(validationResult);

                return Ok(MongoConnector.KitsuneEnquiry(requestModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        #endregion

        #region Admin APIs

        /// <summary>
        /// Get Audit Project and Resource details
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="version"></param>
        /// <param name="userEmail"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("Admin/ProjectAndResources/{projectId}/{version}")]
        public IActionResult AuditProjectAndResourceDetails([FromRoute]string projectId, [FromRoute]int version, [FromQuery]string userEmail)
        {
            try
            {
                var requestModel = new GetAuditProjectAndResourcesDetailsRequestModel
                {
                    ProjectId = projectId,
                    UserEmail = userEmail,
                    Version = version
                };
                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return BadRequest(validationResult);

                return Ok(MongoConnector.GetAuditProjectAndResourcesDetails(requestModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        /// <summary>
        /// Create a Audit Project
        /// </summary>
        /// <param name="requestModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Admin/Create/Project")]
        public IActionResult CreateAuditProject([FromBody]CreateAuditProjectRequestModel requestModel)
        {
            try
            {
                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return BadRequest(validationResult);

                return Ok(MongoConnector.CreateAuditProject(requestModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        #region Production

        /// <summary>
        /// Create a Produdtion Project
        /// </summary>
        /// <param name="requestModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Admin/Live/Project")]
        public IActionResult CreateProductionProject([FromBody]CreateProductionProjectRequestModel requestModel)
        {
            try
            {
                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return BadRequest(validationResult);

                return Ok(MongoConnector.CreateProductionProject(requestModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        /// <summary>
        /// Delete a Production Project
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="userEmail"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("Admin/UnLive/Project/{projectId}")]
        public IActionResult DeleteProductionProject([FromRoute]string projectId, [FromQuery]string userEmail)
        {
            try
            {
                var requestModel = new DeleteProductionProjectRequestModel
                {
                    ProjectId = projectId,
                    UserEmail = userEmail
                };
                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return BadRequest(validationResult);

                return Ok(MongoConnector.DeleteProductionProject(requestModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        /// <summary>
        /// Get Production Project Details
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="userEmail"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("Admin/ProductionProject/{projectId}")]
        public IActionResult GetProductionProjectDetails([FromRoute]string projectId, [FromQuery]string userEmail, [FromQuery]string clientid = null, [FromQuery]bool includeResources = false)
        {
            try
            {
                if (string.IsNullOrEmpty(userEmail) && (string.IsNullOrEmpty(clientid) || BasePluginConfigGenerator.GetBasePlugin(clientid).GetClientId() != clientid.Trim().ToUpper()))
                {
                    return Unauthorized();
                }
                var requestModel = new GetProductionProjectDetailsRequestModel
                {
                    ProjectId = projectId,
                    UserEmail = userEmail,
                    ClientId = clientid,
                    IncludeResources = includeResources
                };
                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return BadRequest(validationResult);

                return Ok(MongoConnector.GetProductionProjectDetails(requestModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpGet]
        [Route("Admin/{projectId}/ProductionResource")]
        public IActionResult GetProductionResourceDetails(string projectId, string sourcepath, string clientid)
        {
            try
            {
                if (string.IsNullOrEmpty(clientid) || BasePluginConfigGenerator.GetBasePlugin(clientid).GetClientId() != clientid.Trim().ToUpper())
                {
                    return Unauthorized();
                }
                var requestModel = new GetResourceDetailsRequestModel
                {
                    ProjectId = projectId,
                    SourcePath = sourcepath
                };
                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return BadRequest(validationResult);

                return Ok(MongoConnector.GetProductionResourceDetails(requestModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
        [HttpGet]
        [Route("v1/WordPressProjectStatus")]
        public IActionResult GetWordPressProjectStatus([FromQuery]string projectId)
        {
            try
            {
                var requestModel = new WordPressProjectStatusRequestModel
                {
                    ProjectId = projectId,
                };
                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return BadRequest(validationResult);

                return Ok(MongoConnector.GetWordPressProjectStatus(requestModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        #endregion

        #endregion

        #region AWS Theme Assets

        [HttpGet]
        [Route("v1/Project/{projectId}/GetProjectResources")]
        public async Task<IEnumerable<string>> GetProjectResources([FromRoute]string projectId)
        {

            var result = await AmazonS3FileProcessor.GetKitsuneAssets(projectId);
            if (result != null)
                return result;
            return new List<string>();
        }

        [HttpGet]
        [Route("v2/Theme/{ProjectId}/GetThemeAssets")]
        public async Task<IEnumerable<string>> GetThemeAssetsTreeFormat([FromRoute]string ProjectId)
        {

            var result = await AmazonS3FileProcessor.GetKitsuneAssets(ProjectId);
            if (result != null)
                return result;
            return new List<string>();
        }

        [HttpPost]
        [Route("v1/InvalidateCDNCache")]
        public async Task<bool> InvalidateCDNCache([FromBody]InvalidateCacheRequestModel requestModel)
        {
            return AmazonS3FileProcessor.InvalidateFiles(EnvironmentConstants.ApplicationConfiguration.CloudFrontDistributionId, requestModel.PathList);
        }

        [HttpPost]
        [Route("v1/Project/SaveFileContentToS3")]
        public async Task<string> SaveFileContentToS3([FromBody]SaveFileContentToS3RequestModel requestModel)
        {
            var result = AmazonS3FileProcessor.SaveFileContentToS3(requestModel.ProjectId,
                                                                   requestModel.BucketName,
                                                                   requestModel.SourcePath,
                                                                   requestModel.FileContent,
                                                                   requestModel.Compiled,
                                                                   requestModel.Version,
                                                                   requestModel.base64,
                                                                   requestModel.ClientId);
            return result;
        }

        [HttpPost]
        [Route("v1/Project/GetFileFromS3")]
        public async Task<string> GetFileFromS3([FromBody]GetFileFromS3RequestModel requestModel)
        {
            var result = AmazonS3FileProcessor.getFileFromS3(requestModel.SourcePath,
                                                             requestModel.ProjectId,
                                                             requestModel.BucketName,
                                                             requestModel.Compiled,
                                                             requestModel.Version,
                                                             requestModel.ClientId);
            return result;
        }

        #endregion

        #region IDE APis

        private bool _isDev;
        public ProjectController()
        {
            _isDev = "True".Equals(EnvironmentConstants.ApplicationConfiguration.IsDev);

        }
        [HttpGet]
        [Route("{projectId}")]
        public IActionResult IdeGetProjectDetails([FromRoute]string projectId, [FromQuery]string userEmail)
        {
            try
            {
                //var result = GetProjectDetails(userEmail, projectId);
                var requestModel = new GetProjectDetailsRequestModel
                {
                    ProjectId = projectId,
                    UserEmail = userEmail
                };
                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return BadRequest(validationResult);

                var result = MongoConnector.GetProjectDetails(requestModel);
                var final = new AssetChildren
                {
                    children = new List<AssetChildren>(),
                    name = "Files",
                    toggled = false
                };

                var themePagesTree = IdeApiHelper.ProjectResourceTree(result.Resources.Select(x => x.SourcePath).ToList(), result.ProjectName);
                IdeGetProjectDetailsResponseModel response = new IdeGetProjectDetailsResponseModel
                {
                    Assets = themePagesTree,
                    CreatedOn = result.CreatedOn,
                    //LastPublishedOn = result.LastPublishedOn,
                    ProjectName = result.ProjectName,
                    UpdatedOn = result.UpdatedOn,
                    UserEmail = result.UserEmail,
                    Version = result.Version,
                    ProjectId = result.ProjectId,
                    ArchivedOn = result.ArchivedOn,
                    BucketNames = result.BucketNames,
                    FaviconIconUrl = result.FaviconIconUrl,
                    IsArchived = result.IsArchived,
                    ProjectStatus = result.ProjectStatus,
                    ProjectType = result.ProjectType,
                    PublishedVersion = result.PublishedVersion,
                    SchemaId = result.SchemaId,
                    ScreenShotUrl = result.ScreenShotUrl,
                    CompilerVersion = result.CompilerVersion
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }


        }

        [HttpGet]
        [Route("List")]
        public IActionResult GetProjectList(string user)
        {
            try
            {
                var requestModel = new GetProjectsListRequestModel
                {
                    UserEmail = user
                };
                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return BadRequest(validationResult);

                return Ok(MongoConnector.GetProjectsList(requestModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
        [HttpGet]
        [Route("v2/List")]
        public IActionResult GetProjectListWithPagination(string user, int skip = 0, int limit = 100)
        {
            try
            {
                var requestModel = new GetProjectsListRequestModelV2
                {
                    UserEmail = user,
                    Limit = limit,
                    Skip = skip
                };
                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return BadRequest(validationResult);

                return Ok(MongoConnector.GetProjectsListWithPagination(requestModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
        [HttpGet]
        [Route("{projectId}/Resource")]
        public IActionResult GetResourceDetails(string projectId, string sourcepath, string user)
        {
            try
            {
                var requestModel = new GetResourceDetailsRequestModel
                {
                    ProjectId = projectId,
                    SourcePath = sourcepath,
                    UserEmail = user
                };
                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return BadRequest(validationResult);

                return Ok(MongoConnector.GetResourceDetails(requestModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpGet]
        [Route("v2/{projectId}/Resource")]
        public IActionResult GetKitsuneResourceDetails(string projectId, string sourcepath, string user)
        {
            try
            {
                var requestModel = new GetResourceDetailsRequestModel
                {
                    ProjectId = projectId,
                    SourcePath = sourcepath,
                    UserEmail = user
                };
                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return BadRequest(validationResult);

                return Ok(MongoConnector.GetKitsuneResourceDetails(requestModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        //[HttpPost]
        //[Route("{projectid}/Resource")]
        //public IActionResult UpdateResourceDetails(string projectid, SubmitRequest req)
        //{
        //    try
        //    {
        //        var requestModel = new Kitsune.Compiler.Core.Helpers.SubmitRequest {
        //            FileContent = req.FileContent,
        //            IsStatic = req.IsStatic,
        //            SourcePath = req.SourcePath,
        //            UserEmail = req.UserEmail,
        //            ProjectId = projectid,
        //            IsDev = _isDev
        //        };
        //        //var validationResult = requestModel.Validate();
        //        //if (validationResult.Any())
        //        //    return BadRequest(validationResult);

        //        return Ok(CompilerService.SubmitTemplate(requestModel));
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ex);
        //    }
        //    req.IsDev = _isDev;
        //    req.ProjectId = projectid;
        //    return _service.SubmitTemplate(req);
        //}
        [HttpPost]
        [DisableRequestSizeLimit]
        [Route("{projectid}/ResourceUpload")]
        public IActionResult ResourceUpload([FromRoute]string projectid, [FromBody]SubmitRequest requestModel)
        {
            try
            {
                requestModel.IsDev = _isDev;
                requestModel.ProjectId = projectid;
                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return BadRequest(validationResult);

                return Ok(MongoConnector.ResourceUpload(requestModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpDelete]
        [Route("{projectid}/Resource")]
        public IActionResult DeleteResource(string projectid, string resourcename, string user)
        {
            try
            {
                var requestModel = new DeleteResourceRequestModel
                {
                    ProjectId = projectid,
                    UserEmail = user,
                    SourceName = resourcename
                };
                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return BadRequest(validationResult);

                return Ok(MongoConnector.DeleteResource(requestModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        //[HttpGet]
        //[Route("{projectId}/Preview")]
        //public string GetPreview(string projectId, string sourcePath, string dataset, string userEmail)
        //{
        //    var result = _service.GetPreview(userEmail, projectId, sourcePath, null, dataset);
        //    if (result != null)
        //        return result.HtmlString;
        //    return null;

        //}

        //[HttpPost]
        //[Route("{projectid}/Compile/{resourcename}")]
        //public ThemeValidationResult ValidateHtml(string projectid, string resourcename, string user, SubmitRequest req)
        //{
        //    req.IsDev = _isDev;
        //    req.UserEmail = user;
        //    req.ProjectId = projectid;
        //    req.SourcePath = resourcename;
        //    return _service.ValidateHtml(req);
        //}
        [HttpPost]
        [Route("{projectid}/MakePageAsDefault")]
        public IActionResult MakeResourceAsDefault(string projectid, string user, string pagename)
        {
            try
            {
                var requestModel = new MakeResourceAsDefaultRequestModel
                {
                    ProjectId = projectid,
                    SourcePath = pagename,
                    UserEmail = user
                };
                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return BadRequest(validationResult);

                return Ok(MongoConnector.MakeResourceAsDefault(requestModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        #endregion

        #region Optimization Reports

        [HttpGet]
        [Route("v1/OptimizedValue")]
        public IActionResult OptimizedPercentage([FromQuery]string projectId)
        {
            try
            {
                var requestModel = new OptimizedPercentageRequestModel
                {
                    ProjectId = projectId,
                };
                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return BadRequest(validationResult);

                return Ok(MongoConnector.GetOptimizedPercentage(requestModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        #endregion

        #region WebformHtml

        [HttpPost]
        [Route("webform/{webformid}/add-data")]
        public IActionResult AddWebform(string webformid, [FromBody]WebFormUpdateRequestModel requestModel)
        {
            try
            {
                if (requestModel != null)
                {
                    requestModel.WebFormId = webformid;
                    var validationResult = requestModel.Validate();
                    if (validationResult.Any())
                        return BadRequest(validationResult);

                    return Ok(MongoConnector.AddWebformJSON(requestModel));
                }
                return BadRequest("Invalid request model");
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPost]
        [Route("webform/{webformid}/update-data")]
        public IActionResult UpdateUpdateWebform(string webformid, [FromBody]WebFormUpdateRequestModel requestModel)
        {
            try
            {
                if (requestModel != null)
                {
                    requestModel.WebFormId = webformid;
                    var validationResult = requestModel.Validate();
                    if (validationResult.Any())
                        return BadRequest(validationResult);

                    return Ok(MongoConnector.UpdateWebformJSON(requestModel));
                }
                return BadRequest("Invalid request model");
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpGet]
        [Route("webform/{webformid}/get-data")]
        public IActionResult GetWebformdata(string webformid)
        {
            try
            {
                if (!string.IsNullOrEmpty(webformid))
                {
                    return Ok(MongoConnector.GetWebformJSON(webformid));
                }
                return BadRequest("Invalid request model");
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
        #endregion

        #region Project Config

        [HttpPost]
        [Route("v1/ProjectConfig")]
        public IActionResult CreateOrUpdateProjectConfig([FromBody]CreateOrUpdateProjectConfigRequestModel requestModel, [FromQuery]string clientId)
        {
            try
            {
                if (string.IsNullOrEmpty(clientId) && Request.Headers != null && Request.Headers.Keys.Contains("clientId"))
                {
                    clientId = Request.Headers.FirstOrDefault(x => x.Key == "clientId").Value;
                }
                //TODO : remove 394D599C21104007901DE762E9E290B9 clientid once Optimizere and Payment service updates 
                if (string.IsNullOrEmpty(clientId) || (BasePluginConfigGenerator.GetBasePlugin(clientId).GetClientId() != clientId.Trim().ToUpper() && clientId != "394D599C21104007901DE762E9E290B9"))
                {
                    return Unauthorized();
                }

                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return BadRequest(validationResult);

                return Ok(MongoConnector.CreateOrUpdateProjectConfig(requestModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpGet]
        [Route("v1/ProjectConfig")]
        public IActionResult GetProjectConfig([FromQuery]string projectId, [FromQuery] string clientId, [FromQuery]FileLevel level = FileLevel.PROD)
        {
            try
            {
                if (string.IsNullOrEmpty(clientId) && Request.Headers != null && Request.Headers.Keys.Contains("clientId"))
                {
                    clientId = Request.Headers.FirstOrDefault(x => x.Key == "clientId").Value;
                }
                //TODO : remove 394D599C21104007901DE762E9E290B9 clientid once Optimizere and Payment service updates 
                if (string.IsNullOrEmpty(clientId) || (BasePluginConfigGenerator.GetBasePlugin(clientId).GetClientId() != clientId.Trim().ToUpper() && clientId != "394D599C21104007901DE762E9E290B9"))
                {
                    return Unauthorized();
                }

                GetProjectConfigRequestModel requestModel = new GetProjectConfigRequestModel()
                {
                    ProjectId = projectId,
                    Level = level
                };
                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return BadRequest(validationResult);

                return Ok(MongoConnector.GetProjectConfig(requestModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route("v1/ValidateProjectConfig")]
        public IActionResult ValidateConfig([FromBody]ValidateConfigRequestModel requestModel)
        {
            try
            {
                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return BadRequest(validationResult);

                var result = KitsuneCompiler.ValidateJsonConfig(requestModel);
                if (result != null && !result.Success)
                    return Ok(new ValidateConfigResponseModel
                    {
                        Error = result.ErrorMessages.Select(x => new BuildError
                        {
                            Column = x.LinePosition,
                            Line = x.LineNumber,
                            Message = x.Message,
                        }).ToList(),
                        IsError = true
                    });

                return Ok(new ValidateConfigResponseModel
                {
                    IsError = false
                });
            }
            catch (JsonReaderException ex)
            {
                return Ok(new ValidateConfigResponseModel
                {
                    IsError = true,
                    Error = new List<BuildError>
                    {
                        new BuildError
                        {
                            Line =ex.LineNumber,Column=ex.LinePosition,Message=ex.Message,SourcePath=EnvConstants.Constants.ProjectConfigurationFilePath
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500);
            }
        }

        [HttpGet]
        [Route("v1/GetKitsuneSettings")]
        public IActionResult GetKitsuneSettings([FromQuery] string projectId, [FromQuery] int version)
        {
            try
            {
                if (string.IsNullOrEmpty(projectId))
                    return BadRequest("Invalid ProjectId");

                return Ok(MongoConnector.GetKitsuneSettings(projectId, version));
            }
            catch (Exception ex)
            {
                return StatusCode(500);
            }
        }
        #endregion

        #region KApps/Components

        [HttpGet]
        [Route("v1/UpdateProjectAppModule")]
        [Route("v1/UpdateProjectComponents")]
        public IActionResult UpdateProjectAppModule([FromQuery]string projectId, [FromQuery]string componentId, [FromQuery]bool enable, [FromQuery] string kAppProjectId)
        {
            //TO support backward compatibility 
            if (string.IsNullOrEmpty(componentId) && !string.IsNullOrEmpty(kAppProjectId))
                componentId = kAppProjectId;

            if (componentId == null)
            {
                throw new ArgumentNullException(nameof(componentId));
            }

            var userId = string.Empty;
            if (Request.Headers.ContainsKey("Authorization"))
                userId = Request.Headers["Authorization"].ToString();

            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            var projectUserId = MongoConnector.GetUserIdFromProjectId(projectId);
            if (projectUserId != userId)
                return Unauthorized();

            var component = MongoConnector.GetProjectDetails(componentId);
            if (component == null || component.ProjectType != Kitsune.Models.Project.ProjectType.APP)
            {
                return BadRequest(new
                {
                    Message = "component not found"
                });
            }

            var project = MongoConnector.GetProjectDetails(projectId);
            if (project != null)
            {
                if (enable)
                {
                    var exists = project.Components?.Any(x => x.ProjectId == componentId);
                    if (exists != true)
                    {
                        if (project.Components == null)
                            project.Components = new List<Kitsune.Models.Project.ProjectComponent>();

                        project.Components.Add(new Kitsune.Models.Project.ProjectComponent
                        {
                            ProjectId = componentId,
                            SchemaId = component.SchemaId
                        });
                        MongoConnector.UpdateProjectComponents(projectId, project.Components);
                        return Ok(new
                        {
                            Message = $"Component {component.ProjectName} is enabled for the given project"
                        });
                    }

                    return Ok(new
                    {
                        Message = $"Component {component.ProjectName} is already enabled for the given project"
                    });
                }
                else
                {
                    var existingComponent = project.Components?.FirstOrDefault(x => x.ProjectId == componentId);
                    if (existingComponent != null)
                    {
                        project.Components.Remove(existingComponent);
                        MongoConnector.UpdateProjectComponents(projectId, project.Components);
                        MongoConnector.DisableComponentChanges(projectId, existingComponent, userId, project.SchemaId);
                        return Ok(new
                        {
                            Message = $"Component {component.ProjectName} is disabled for the given project"
                        });
                    }
                    return Ok(new
                    {
                        Message = $"Component {component.ProjectName} is already disabled for the given project"
                    });
                }
            }

            return BadRequest(new
            {
                Message = "Cannot find the project with given id"
            });
        }

        [HttpGet]
        [Route("v1/ListAvailableKApps")]
        public IActionResult ListAvailableKApps([FromQuery]string filter, [FromQuery]string sort, [FromQuery]int skip = 0, [FromQuery]int limit = 100)
        {
            var userId = string.Empty;
            if (Request.Headers.ContainsKey("Authorization"))
                userId = Request.Headers["Authorization"].ToString();

            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            return Ok(MongoConnector.ListAppProjects(filter, sort, skip, limit));
        }

        [HttpGet]
        [Route("v1/GetComponentSettingsPerProject")]
        public IActionResult GetComponentSettingsPerProject([FromQuery]string filter, [FromQuery]string sort, [FromQuery]int skip = 0, [FromQuery]int limit = 100)
        {
            var userId = string.Empty;
            if (Request.Headers.ContainsKey("Authorization"))
                userId = Request.Headers["Authorization"].ToString();

            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            return Ok(MongoConnector.ListAppProjects(filter, sort, skip, limit));
        }

        [HttpGet]
        [Route("GetProjectDetailsPerEnabledComponent")]
        public IActionResult GetProjectDetailsPerEnabledComponent([FromQuery]string componentId, [FromQuery]string filter, [FromQuery]string sort, [FromQuery]int skip = 0, [FromQuery]int limit = 100, bool includeEmptyConfig = false)
        {
            try
            {
                if (String.IsNullOrEmpty(componentId))
                    return BadRequest("componentId is required");

                var result = MongoConnector.GetProjectDetailsPerEnabledComponent(componentId, filter, sort, skip, limit, includeEmptyConfig);
                if (result != null)
                    return Ok(result);

                return BadRequest();
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        [HttpGet]
        [Route("GetLiveWebsitesPerProject/{projectId}")]
        public IActionResult GetLiveWebsitesPerProject([FromRoute]string projectId, [FromQuery]int skip = 0, [FromQuery]int limit = 100)
        {
            if (String.IsNullOrEmpty(projectId))
                return BadRequest("projectId is required");

            return Ok(MongoConnector.GetLiveWebsitesPerProject(new API.Model.ApiRequestModels.Application.GetLiveWebsiteForProjectRequestModel
            {
                Limit = limit,
                Offset = skip,
                ProjectId = projectId
            }));
        }

        #endregion
    }
}
