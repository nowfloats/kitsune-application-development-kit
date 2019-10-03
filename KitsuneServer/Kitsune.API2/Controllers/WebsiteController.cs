using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AmazonAWSHelpers.SQSQueueHandler;
using Kitsune.API.Model.ApiRequestModels;
using Kitsune.API2.DataHandlers.Mongo;
using Kitsune.API2.EnvConstants;
using Kitsune.API2.Utils;
using Kitsune.API2.Validators;
using Kitsune.BasePlugin.Utils;
using Kitsune.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using MongoDB.Bson;
using Newtonsoft.Json;

namespace Kitsune.API2.Controllers
{
    [Produces("application/json")]
    [Route("api/Website")]
    public class WebsiteController : Controller
    {
        private readonly IDistributedCache _distributedCache;

        public WebsiteController(IDistributedCache distributedCache)
        {
            _distributedCache = distributedCache;
        }

        [HttpGet]
        public IActionResult ClearCache(string key)
        {
            _distributedCache.Remove(key);

            return Ok();
        }

        #region Website APIs
        /// <summary>
        /// Create new website with following paramater and also creates new default user for that website
        /// </summary>
        /// <param name="requestModel">
        /// DeveloperId, PhoneNumber, Email, ProjectId, WebsiteTag
        /// </param>
        /// <returns>WebsiteId</returns>
        [HttpPost("v1")]
        public IActionResult CreateNewWebsite([FromBody]CreateNewWebsiteRequestModel requestModel)
        {
            try
            {
                var developerId = AuthHelper.AuthorizeRequest(Request);
                if (string.IsNullOrEmpty(developerId))
                {
                    return new CommonActionResult(CommonAPIResponse.UnAuthorized());
                }

                try
                {
                    if (String.IsNullOrEmpty(requestModel.ClientId) && !String.IsNullOrEmpty(requestModel.ProjectId))
                    {
                        requestModel.ClientId = MongoConnector.GetClientIdFromProjectId(requestModel.ProjectId);
                    }
                }
                catch { }

                requestModel.DeveloperId = developerId;
                requestModel.ActivateWebsite = false;
                if (requestModel.CopyDemoData)
                    requestModel.ActivateWebsite = true;
                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return new CommonActionResult(CommonAPIResponse.BadRequest(validationResult));

                return new CommonActionResult(MongoConnector.CreateNewWebsite(requestModel));
            }
            catch (Exception ex)
            {
                return new CommonActionResult(CommonAPIResponse.InternalServerError(ex));
            }
        }

        /// <summary>
        /// Create new website with following paramater and also creates new default user for that website
        /// </summary>
        /// <param name="requestModel">
        /// DeveloperId, PhoneNumber, Email, ProjectId, WebsiteTag
        /// </param>
        /// <returns>WebsiteId</returns>
        [HttpPost("v2")]
        public IActionResult CreateNewWebsite([FromBody]CreateNewWebsiteRequestModel requestModel, [FromQuery] string clientid = null)
        {
            try
            {
                var authId = AuthHelper.AuthorizeRequest(Request);

                try
                {
                    if (String.IsNullOrEmpty(clientid) && !String.IsNullOrEmpty(requestModel.DeveloperId))
                    {
                        requestModel.ClientId = MongoConnector.GetClientIdFromProjectId(requestModel.ProjectId);
                    }
                }
                catch { }

                requestModel.ClientId = BasePluginConfigGenerator.GetBasePlugin(clientid).GetClientId();
                if (string.IsNullOrEmpty(clientid) || requestModel.ClientId != clientid.Trim().ToUpper())
                    return new CommonActionResult(CommonAPIResponse.UnAuthorized());

                requestModel.ActivateWebsite = false;

                if (requestModel.ClientId == Constants.ClientIdConstants.NowFloatsClientId)
                    requestModel.ActivateWebsite = true;


                requestModel.DeveloperId = MongoConnector.GetUserIdFromProjectId(requestModel.ProjectId);

                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return new CommonActionResult(CommonAPIResponse.BadRequest(validationResult));

                return new CommonActionResult(MongoConnector.CreateNewWebsite(requestModel));
            }
            catch (Exception ex)
            {
                return new CommonActionResult(CommonAPIResponse.InternalServerError(ex));
            }
        }

        /// <summary>
        /// Update kitsune website with the website id
        /// Developer can update WebsiteUrl, WebsiteTag, Version, IsActive
        /// </summary>
        /// <param name="WebsiteId"></param>
        /// <param name="requestModel"></param>
        /// <returns>WebsiteId</returns>
        [HttpPost("v1/{WebsiteId}")]
        public IActionResult UpdateKitsuneWebsite(string WebsiteId, [FromBody]UpdateWebsiteRequestModel requestModel)
        {
            var developerId = AuthHelper.AuthorizeRequest(Request);
            if (string.IsNullOrEmpty(developerId))
            {
                return new CommonActionResult(CommonAPIResponse.UnAuthorized());
            }
            try
            {
                requestModel.DeveloperId = developerId;
                if (!string.IsNullOrEmpty(WebsiteId))
                    requestModel.WebsiteId = WebsiteId;

                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return new CommonActionResult(CommonAPIResponse.BadRequest(validationResult));
                return new CommonActionResult(MongoConnector.UpdateWebsiteDetails(requestModel));
            }
            catch (Exception ex)
            {
                return new CommonActionResult(CommonAPIResponse.InternalServerError(ex));
            }
        }

        /// <summary>
        /// Get website details
        /// </summary>
        /// <param name="WebsiteId"></param>
        /// <returns>Website details</returns>
        [HttpGet("v1/{WebsiteId}")]
        public IActionResult GetKitsuneWebsiteDetails(string WebsiteId, [FromQuery]string clientid = null)
        {
            try
            {
                var developerId = AuthHelper.AuthorizeRequest(Request);
                if (string.IsNullOrEmpty(developerId) && (string.IsNullOrEmpty(clientid) || BasePluginConfigGenerator.GetBasePlugin(clientid).GetClientId() != clientid.Trim().ToUpper()))
                {
                    return new CommonActionResult(CommonAPIResponse.UnAuthorized());
                }
                return new CommonActionResult(MongoConnector.GetKitsuneWebsiteDetails(WebsiteId));
            }
            catch (Exception ex)
            {
                return new CommonActionResult(CommonAPIResponse.InternalServerError(ex));
            }
        }

        /// <summary>
        /// Get website list for project 
        /// </summary>
        /// <param name="ProjectId"></param>
        /// <returns></returns>
        [HttpGet("v1")]
        public IActionResult GetKitsuneWebsiteListForProject([FromQuery]string projectId, int limit = 100, int skip = 0, bool includeusers = false)
        {
            var developerId = AuthHelper.AuthorizeRequest(Request);
            if (string.IsNullOrEmpty(developerId))
            {
                return new CommonActionResult(CommonAPIResponse.UnAuthorized());
            }
            try
            {
                return new CommonActionResult(MongoConnector.GetKitsuneWebsiteListForProject(developerId, projectId, limit, skip, includeusers));
            }
            catch (Exception ex)
            {
                return new CommonActionResult(CommonAPIResponse.InternalServerError(ex));
            }
        }

        [HttpGet("v1/CheckDomain")]
        public IActionResult CheckDomainAvailability([FromQuery]string domainName)
        {
            var developerId = AuthHelper.AuthorizeRequest(Request);
            if (string.IsNullOrEmpty(developerId))
            {
                return new CommonActionResult(CommonAPIResponse.UnAuthorized());
            }
            try
            {
                return new CommonActionResult(MongoConnector.CheckKitsuneDomainAvailability(domainName));
            }
            catch (Exception ex)
            {
                return new CommonActionResult(CommonAPIResponse.InternalServerError(ex));
            }

        }

        [HttpGet("v1/WebsiteTagExists/{websitetag}")]
        public IActionResult CheckWebsiteTagAvailability(string websitetag, [FromQuery]string clientId, [FromQuery] string projectId)
        {
            var developerId = AuthHelper.AuthorizeRequest(Request);
            if (string.IsNullOrEmpty(developerId))
            {
                return new CommonActionResult(CommonAPIResponse.UnAuthorized());
            }

            try
            {
                try
                {
                    if (String.IsNullOrEmpty(clientId) && !String.IsNullOrEmpty(projectId))
                    {
                        clientId = MongoConnector.GetClientIdFromProjectId(projectId);
                    }
                }
                catch { }

                return new CommonActionResult(MongoConnector.CheckWebsiteTagAvailability(websitetag, clientId));
            }
            catch (Exception ex)
            {
                return new CommonActionResult(CommonAPIResponse.InternalServerError(ex));
            }
        }

        [HttpGet("v1/Live")]
        public IActionResult GetLiveWebsites([FromQuery]int limit = 100, [FromQuery]int skip = 0)
        {
            var developerId = AuthHelper.AuthorizeRequest(Request);
            if (string.IsNullOrEmpty(developerId))
            {
                return new CommonActionResult(CommonAPIResponse.UnAuthorized());
            }
            try
            {
                return new CommonActionResult(MongoConnector.GetKitsuneLiveWebsites(developerId, limit, skip));
            }
            catch (Exception ex)
            {
                return new CommonActionResult(CommonAPIResponse.InternalServerError(ex));
            }

        }

        [HttpGet("v1/GetAllLiveWebsites")]
        public IActionResult GetAllLiveWebsites([FromQuery]int limit = 100, [FromQuery]int skip = 0)
        {
            var authId = AuthHelper.AuthorizeRequest(Request);
            if (String.IsNullOrEmpty(authId) || String.Compare(authId, "4C627432590419E9CF79252291B6A3AE25D7E2FF13347C6ACD1587C47C6ACDD") != 0)
            {
                return new CommonActionResult(CommonAPIResponse.UnAuthorized());
            }

            try
            {
                return new CommonActionResult(MongoConnector.GetAllKitsuneLiveWebsites(limit, skip));
            }
            catch (Exception ex)
            {
                return new CommonActionResult(CommonAPIResponse.InternalServerError(ex));
            }

        }
        [HttpGet("v1/{websiteid}/ChangeProjectId/{projectid}")]
        public IActionResult ChangeProjectId([FromRoute]string websiteid, [FromRoute]string projectid, [FromQuery]string clientid)
        {

            if (string.IsNullOrEmpty(clientid) || BasePluginConfigGenerator.GetBasePlugin(clientid).GetClientId() != clientid.Trim().ToUpper())
                return new CommonActionResult(CommonAPIResponse.UnAuthorized());
            if (string.IsNullOrEmpty(websiteid) || string.IsNullOrEmpty(projectid))
            {
                return new CommonActionResult(CommonAPIResponse.BadRequest(new ValidationResult("Invalid input paramater")));
            }
            try
            {
                return new CommonActionResult(CommonAPIResponse.OK(MongoConnector.ChangeProjectId(websiteid, projectid)));
            }
            catch (Exception ex)
            {
                return new CommonActionResult(CommonAPIResponse.InternalServerError(ex));
            }

        }
        [HttpGet("v1/k-search")]
        public IActionResult KSearch([FromQuery]string customerId, [FromQuery]string keyword, [FromQuery]bool IsDemo = false)
        {
            try
            {
                KSearchRequestModel requestModel = new KSearchRequestModel()
                {
                    CustomerId = customerId,
                    Keyword = keyword,
                    IsDemo = IsDemo
                };
                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return new CommonActionResult(CommonAPIResponse.BadRequest(validationResult));

                return new CommonActionResult(MongoConnector.GetKSearchResult(requestModel));
            }
            catch (Exception ex)
            {
                return new CommonActionResult(CommonAPIResponse.InternalServerError(ex));
            }
        }

        [HttpGet("v1/GetWebsiteId")]
        public IActionResult GetCustomerIdFromDomain([FromQuery]string domain)
        {
            try
            {
                GetCustomersIdFromDomainRequestModel requestModel = new GetCustomersIdFromDomainRequestModel()
                {
                    Domain = domain
                };
                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return new CommonActionResult(CommonAPIResponse.BadRequest(validationResult));

                return new CommonActionResult(MongoConnector.GetCustomerIdFromDomain(requestModel));
            }
            catch (Exception ex)
            {
                return new CommonActionResult(CommonAPIResponse.InternalServerError(ex));
            }
        }
        [HttpGet("v1/GetProjectName")]
        public IActionResult GetProjectNameFromDomain([FromQuery]string domain)
        {
            try
            {
                GetCustomersIdFromDomainRequestModel requestModel = new GetCustomersIdFromDomainRequestModel()
                {
                    Domain = domain
                };
                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return new CommonActionResult(CommonAPIResponse.BadRequest(validationResult));

                return new CommonActionResult(MongoConnector.GetProjectNameFromDomain(requestModel));
            }
            catch (Exception ex)
            {
                return new CommonActionResult(CommonAPIResponse.InternalServerError(ex));
            }
        }

        [HttpGet("v1/ActivateWebsite")]
        public IActionResult ActivateWebsite([FromQuery]string websiteId)
        {
            try
            {
                var userId = AuthHelper.AuthorizeRequest(Request);
                if (string.IsNullOrEmpty(userId))
                {
                    return new CommonActionResult(CommonAPIResponse.UnAuthorized());
                }
                ActivateWebsiteRequestModel requestModel = new ActivateWebsiteRequestModel
                {
                    WebsiteId = websiteId
                };
                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return new CommonActionResult(CommonAPIResponse.BadRequest(validationResult));

                return new CommonActionResult(MongoConnector.ActivateWebsite(requestModel));
            }
            catch (Exception ex)
            {
                return new CommonActionResult(CommonAPIResponse.InternalServerError(ex));
            }
        }

        [HttpPost("v1/DeactivateWebsites")]
        public IActionResult DeactivateWebsites([FromBody]DeActivateWebsitesRequestModel deactivateRequestModel)
        {
            try
            {
                var userId = AuthHelper.AuthorizeRequest(Request);
                if (string.IsNullOrEmpty(userId))
                {
                    return new CommonActionResult(CommonAPIResponse.UnAuthorized());
                }
                DeActivateWebsitesRequestModel requestModel = new DeActivateWebsitesRequestModel
                {
                    WebsiteIds = deactivateRequestModel.WebsiteIds,
                    UserId = userId
                };
                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return new CommonActionResult(CommonAPIResponse.BadRequest(validationResult));

                return new CommonActionResult(MongoConnector.DeactivateWebsites(requestModel));
            }
            catch (Exception ex)
            {
                return new CommonActionResult(CommonAPIResponse.InternalServerError(ex));
            }
        }

        [HttpGet("v1/KdaminLogin")]
        public IActionResult GenerateKAdminLoginToken([FromQuery]string websiteId, [FromQuery]string source = "IDE")
        {
            try
            {
                var userId = AuthHelper.AuthorizeRequest(Request);
                if (string.IsNullOrEmpty(userId))
                {
                    return new CommonActionResult(CommonAPIResponse.UnAuthorized());
                }
                GenerateKAdminLoginTokenRequestModel requestModel = new GenerateKAdminLoginTokenRequestModel
                {
                    WebsiteId = websiteId,
                    UserId = userId,
                    ExpiryTime = source == "IDE" ? 2 : 30,
                    Source = source
                };

                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return new CommonActionResult(CommonAPIResponse.BadRequest(validationResult));

                return new CommonActionResult(MongoConnector.GenerateKAdminLoginToken(requestModel));
            }
            catch (Exception ex)
            {
                return new CommonActionResult(CommonAPIResponse.InternalServerError(ex));
            }
        }

        [HttpGet("v1/KAdminDecodeToken")]
        public IActionResult DecodeKAdminToken([FromQuery]string token)
        {
            try
            {
                DecodeKAdminTokenRequestModel requestModel = new DecodeKAdminTokenRequestModel
                {
                    Token = token
                };

                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return new CommonActionResult(CommonAPIResponse.BadRequest(validationResult));

                return new CommonActionResult(MongoConnector.DecodeKAdminLoginToken(requestModel));
            }
            catch (Exception ex)
            {
                return new CommonActionResult(CommonAPIResponse.InternalServerError(ex));
            }
        }
        [HttpGet("v1/KAdminValidateToken")]
        public IActionResult ValidateToken([FromQuery] string token)
        {
            var tokenObject = MongoConnector.DecryptToken(token);
            if (tokenObject == null)
                return Unauthorized();
            return Ok();
        }
        /// <summary>
        /// this will upload necessary files for a website like sitemap, google-verification etc
        /// </summary>
        /// <param name="websiteId"></param>
        /// <param name="assetFileName"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        [HttpPost("v1/{websiteid}/upload-resource-file")]
        [ValidateMimeMultipartContentFilter]
        public IActionResult UploadFiles(string websiteId, [FromQuery]string assetFileName, IFormFile file)
        {
            Stream stream = Request.Body;
            var byteArrayStream = Kitsune.API2.EnvConstants.Constants.ReadInputStream(file);

            if (string.IsNullOrEmpty(websiteId))
                return BadRequest("WebsiteId cannot be empty");

            if (string.IsNullOrEmpty(assetFileName))
                return BadRequest("File name cannot be empty");

            if (byteArrayStream == null || byteArrayStream.Length == 0)
                return BadRequest("File cannot be empty");

            websiteId = websiteId.Replace(" ", "");

            var userid = Request.Headers.ContainsKey("Authorization") ? Request.Headers["Authorization"].ToString() : null;

            var projectId = MongoConnector.GetProjectIdFromWebsiteId(websiteId);

            var bucketName = "kitsune-resource-production";
            var temporaryFileName = Helper.KitsuneCommonUtils.FormatResourceFileName(assetFileName)?.ToLower();

            var result = AmazonS3Helper.SaveAssetsAndReturnObjectkey($"{projectId}/websiteresources/{websiteId}/{temporaryFileName}", byteArrayStream, bucketName);
            if (!string.IsNullOrEmpty(result))
            {
                return Ok("Uploaded successfully");
            }

            // do something witht his stream now
            return BadRequest("Invalid website id");
        }

        [HttpPost("v1/copys3")]
        public IActionResult CopyS3Weebhook([FromBody]string message_id, [FromBody]string run_time)
        {
            try
            {
                return new CommonActionResult(CommonAPIResponse.OK("ok"));
            }
            catch (Exception ex)
            {
                return new CommonActionResult(CommonAPIResponse.InternalServerError(ex));
            }
        }

        [HttpPost]
        [Route("PushToSitemapQueue")]
        public IActionResult PushToSitemapQueue([FromQuery]string clientid, [FromQuery]string websiteId)
        {
            try
            {
                if (string.IsNullOrEmpty(clientid) || string.IsNullOrEmpty(websiteId) || BasePluginConfigGenerator.GetBasePlugin(clientid).GetClientId() != clientid.Trim().ToUpper())
                {
                    return Unauthorized();
                }
                if (websiteId.ToLower() == "all")
                {
                    var liveWebsites = MongoConnector.GetLiveWebsiteIds();
                    if (liveWebsites != null && liveWebsites.Any())
                    {
                        var result = string.Empty;
                        foreach (var website in liveWebsites.Select(x => new KeyValuePair<string, string>(x.Substring(0, x.IndexOf(':')), x.Substring(x.IndexOf(':') + 1))))
                        {
                            result = CommonHelpers.PushSitemapGenerationTaskToSQS(new SitemapGenerationTaskModel() { WebsiteId = website.Key, ProjectId = website.Value });
                        }
                        if (!string.IsNullOrEmpty(result))
                            return Ok();
                    }
                }
                else
                {
                    var result = CommonHelpers.PushSitemapGenerationTaskToSQS(new SitemapGenerationTaskModel() { WebsiteId = websiteId });
                    if (!string.IsNullOrEmpty(result))
                        return Ok();
                }

                return new CommonActionResult(CommonAPIResponse.BadRequest(new ValidationResult("Invalid sitemap request")));
            }
            catch (Exception ex)
            {
                return new CommonActionResult(CommonAPIResponse.InternalServerError(ex));
            }
        }

        /// <summary>
        /// Is Call Tracker Enabled
        /// </summary>
        /// <param name="WebsiteId"></param>
        /// <returns>IsActive true or false</returns>
        [HttpGet("v1/CallTrackerEnabled")]
        public IActionResult IsCallTrackerEnabled([FromQuery]string WebsiteId)
        {
            try
            {
                var developerId = AuthHelper.AuthorizeRequest(Request);
                if (string.IsNullOrEmpty(developerId))
                {
                    return new CommonActionResult(CommonAPIResponse.UnAuthorized());
                }
                return new CommonActionResult(MongoConnector.IsCallTrackerEnabledForWebsite(WebsiteId));
            }
            catch (Exception ex)
            {
                return new CommonActionResult(CommonAPIResponse.InternalServerError(ex));
            }
        }

        #endregion

        #region WebsiteUser APIs (K-ADMIN)
        [HttpGet("v1/{websiteid}/cacheinvalidationstatus")]
        public IActionResult GetCacheInvalidationStatus([FromRoute]string websiteid, [FromQuery] string clientid)
        {
            try
            {
                if (string.IsNullOrEmpty(clientid) || BasePluginConfigGenerator.GetBasePlugin(clientid).GetClientId() != clientid.Trim().ToUpper())
                {
                    return Unauthorized();
                }

                if (string.IsNullOrEmpty(websiteid))
                    return new CommonActionResult(CommonAPIResponse.BadRequest(new ValidationResult("Invalid websiteid")));

                return new CommonActionResult(MongoConnector.GetCacheInvalidationStatus(websiteid));
            }
            catch (Exception ex)
            {
                return new CommonActionResult(CommonAPIResponse.InternalServerError(ex));
            }
        }

        [HttpPost("v1/{websiteid}/updatecacheinvalidationstatus")]
        public IActionResult UpdateCacheInvalidationStatus([FromRoute]string websiteid,  [FromQuery] string clientid,  [FromQuery] int nextInvalidationInSec)
        {
            try
            {
                if (string.IsNullOrEmpty(clientid) || BasePluginConfigGenerator.GetBasePlugin(clientid).GetClientId() != clientid.Trim().ToUpper())
                {
                    return Unauthorized();
                }

                if (string.IsNullOrEmpty(websiteid))
                    return new CommonActionResult(CommonAPIResponse.BadRequest(new ValidationResult("Invalid websiteid")));
                if(nextInvalidationInSec==0)
                    return new CommonActionResult(CommonAPIResponse.BadRequest(new ValidationResult("Provide valid next invalidation in secnd paramater")));

                return new CommonActionResult(MongoConnector.UpdateCacheInvalidationStatus(websiteid, nextInvalidationInSec));
            }
            catch (Exception ex)
            {
                return new CommonActionResult(CommonAPIResponse.InternalServerError(ex));
            }
        }

        [HttpPost("v1/{websiteid}/togglecdncache")]
        public IActionResult ToggleCDNCache([FromRoute]string websiteid, [FromQuery] string clientid, [FromQuery] bool enable)
        {
            try
            {
                if (string.IsNullOrEmpty(clientid) || BasePluginConfigGenerator.GetBasePlugin(clientid).GetClientId() != clientid.Trim().ToUpper())
                {
                    return Unauthorized();
                }

                if (string.IsNullOrEmpty(websiteid))
                    return new CommonActionResult(CommonAPIResponse.BadRequest(new ValidationResult("Invalid websiteid")));

                return new CommonActionResult(MongoConnector.UpdateCacheInvalidationStatus(websiteid, enable: enable));
            }
            catch (Exception ex)
            {
                return new CommonActionResult(CommonAPIResponse.InternalServerError(ex));
            }
        }

        [HttpPost("v1/{WebsiteId}/WebsiteUser")]
        public IActionResult UpdateKitsuneWebsiteUser(string WebsiteId, [FromBody]CreateOrUpdateWebsiteUserRequestModel requestModel)
        {
            try
            {
                requestModel.WebsiteId = WebsiteId;
                var developerId = AuthHelper.AuthorizeRequest(Request);
                if (string.IsNullOrEmpty(developerId))
                {
                    return new CommonActionResult(CommonAPIResponse.UnAuthorized());
                }
                requestModel.DeveloperId = developerId;
                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return new CommonActionResult(CommonAPIResponse.BadRequest(validationResult));

                return new CommonActionResult(MongoConnector.CreateNewWebsiteUser(requestModel));
            }
            catch (Exception ex)
            {
                return new CommonActionResult(CommonAPIResponse.InternalServerError(ex));
            }
        }

        [HttpPost("v1/{WebsiteId}/WebsiteUser/{WebsiteUserId}")]
        public IActionResult UpdateKitsuneWebsiteUser(string WebsiteId, string WebsiteUserId, [FromBody]CreateOrUpdateWebsiteUserRequestModel requestModel)
        {
            try
            {
                requestModel.WebsiteId = WebsiteId;
                requestModel.WebsiteUserId = WebsiteUserId;
                var developerId = AuthHelper.AuthorizeRequest(Request);
                if (string.IsNullOrEmpty(developerId))
                {
                    return new CommonActionResult(CommonAPIResponse.UnAuthorized());
                }
                requestModel.DeveloperId = developerId;
                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return new CommonActionResult(CommonAPIResponse.BadRequest(validationResult));

                return new CommonActionResult(MongoConnector.UpdateWebsiteUserDetails(requestModel));
            }
            catch (Exception ex)
            {
                return new CommonActionResult(CommonAPIResponse.InternalServerError(ex));
            }
        }

        [HttpPost("v1/{WebsiteId}/WebsiteUser/{WebsiteUserId}/ChangePassword")]
        public IActionResult ChangeKitsuneWebsiteUserPassword(string WebsiteId, string WebsiteUserId, [FromBody]UpdateWebsiteUserPasswordRequestModel requestModel)
        {
            try
            {
                requestModel.WebsiteId = WebsiteId;
                requestModel.WebsiteUserId = WebsiteUserId;
                var developerId = AuthHelper.AuthorizeRequest(Request);
                if (string.IsNullOrEmpty(developerId))
                {
                    return new CommonActionResult(CommonAPIResponse.UnAuthorized());
                }
                requestModel.DeveloperId = developerId;
                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return new CommonActionResult(CommonAPIResponse.BadRequest(validationResult));

                return new CommonActionResult(MongoConnector.ChangeWebsiteUserPassword(requestModel));
            }
            catch (Exception ex)
            {
                return new CommonActionResult(CommonAPIResponse.InternalServerError(ex));
            }
        }

        [HttpGet("v1/{WebsiteId}/WebsiteUser/{WebsiteUserId}")]
        public IActionResult GetKitsuneWebsiteUser(string WebsiteId, string WebsiteUserId)
        {
            try
            {
                var developerId = AuthHelper.AuthorizeRequest(Request);
                if (string.IsNullOrEmpty(developerId))
                {
                    return new CommonActionResult(CommonAPIResponse.UnAuthorized());
                }
                return new CommonActionResult(MongoConnector.GetWebsiteUserDetails(WebsiteId, WebsiteUserId));
            }
            catch (Exception ex)
            {
                return new CommonActionResult(CommonAPIResponse.InternalServerError(ex));
            }
        }

        [HttpPost("v1/Login")]
        public IActionResult LoginWebsiteUser([FromBody]VerifyLoginRequestModel requestModel)
        {
            try
            {
                return new CommonActionResult(MongoConnector.LoginWebsiteUser(requestModel));
            }
            catch (Exception ex)
            {
                return new CommonActionResult(CommonAPIResponse.InternalServerError(ex));
            }
        }

        [HttpGet("v1/GetV6RedirectUrl")]
        public IActionResult GetV6RedirectUrl([FromQuery] string fpTag, [FromQuery] string requestUrl)
        {
            try
            {
                if (!String.IsNullOrEmpty(fpTag))
                {
                    var cacheKey = String.Format("GetV6RedirectUrl-{0}-{1}", fpTag, requestUrl);
                    try
                    {
                        var cachedItem = _distributedCache.GetString(cacheKey);
                        if (!String.IsNullOrEmpty(cachedItem) && String.Compare(cachedItem, "null") == 0)
                        {
                            return null;
                        }
                        else if (!String.IsNullOrEmpty(cachedItem))
                        {
                            return Ok(cachedItem);
                        }
                    }
                    catch { }

                    var website = (WebsiteDetailsResponseModel)MongoConnector.GetKitsuneWebsiteDetailsByTag(fpTag)?.Response;

                    if (website != null)
                    {
                        if (!website.IsSSLEnabled)
                            requestUrl = requestUrl.Replace("https://", "http://");

                        var rootAliasUri = MongoConnector.GetRootAliasUriFromWebsteDomain(website.WebsiteId, website.WebsiteUrl)?.ToLower();

                        if (!String.IsNullOrEmpty(rootAliasUri))
                        {
                            var response = V6UrlHandler.GetRedirectionUrlForV6Links(website.ProjectId, requestUrl, rootAliasUri, website.WebsiteTag, website.WebsiteId);
                            try
                            {
                                _distributedCache.SetString(cacheKey, JsonConvert.SerializeObject(response), new DistributedCacheEntryOptions()
                                {
                                    AbsoluteExpiration = DateTime.Now.AddDays(7)
                                });
                            }
                            catch { }

                            return Ok(response);
                        }
                    }
                }

                return new CommonActionResult(CommonAPIResponse.BadRequest(ValidationResult.Success));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion
    }
}