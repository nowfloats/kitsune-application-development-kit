using Kitsune.API.Model.ApiRequestModels;
using Kitsune.API2.DataHandlers.Mongo;
using Kitsune.API2.Utils;
using Kitsune.Compiler;
using Kitsune.Compiler.Helpers;
using Kitsune.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Kitsune.API2.Controllers
{
    [Produces("application/json")]
    [Route("api/IDE")]
    public class IDEController : Controller
    {
        private CompilerService _service = new CompilerService();
        [HttpGet]
        [Route("{projectId}")]
        public IActionResult GetProjectDetails(string projectId, [FromQuery]string userEmail)
        {
            try
            {
                var requestModel = new GetProjectDetailsRequestModel
                {
                    ProjectId = projectId,
                    ExcludeResources = false,
                    UserEmail = userEmail
                };
                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return BadRequest(validationResult);

                var projectDetails = MongoConnector.GetProjectDetails(requestModel);
                var themePagesTree = _service.ProjectResourceTree(projectDetails.Resources.Select(x => x.SourcePath).ToList(), projectDetails.ProjectName);

                GetIDEProjectDetailsResponseModel response = new GetIDEProjectDetailsResponseModel
                {
                    Assets = themePagesTree,
                    CreatedOn = projectDetails.CreatedOn,
                    ProjectName = projectDetails.ProjectName,
                    UpdatedOn = projectDetails.UpdatedOn,
                    UserEmail = projectDetails.UserEmail,
                    Version = projectDetails.Version,
                    ProjectId = projectDetails.ProjectId,
                    ArchivedOn = projectDetails.ArchivedOn,
                    BucketNames = projectDetails.BucketNames,
                    FaviconIconUrl = projectDetails.FaviconIconUrl,
                    IsArchived = projectDetails.IsArchived,
                    ProjectStatus = projectDetails.ProjectStatus,
                    ProjectType = projectDetails.ProjectType,
                    PublishedVersion = projectDetails.PublishedVersion,
                    SchemaId = projectDetails.SchemaId,
                    ScreenShotUrl = projectDetails.ScreenShotUrl,
                    Components = projectDetails.Components
                };

                return Ok(response);

            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPost]
        [Route("")]
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
        [HttpGet]
        [Route("List")]
        public IActionResult GetProjectList([FromQuery]string user)
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

                return Ok(MongoConnector.GetProjectsList(requestModel)?.Projects);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
        [HttpGet]
        [Route("v2/List")]
        public IActionResult GetProjectListWithPagination([FromQuery]string user, [FromQuery]int skip = 0, [FromQuery]int limit = 100)
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
        public IActionResult GetPageDetails(string projectId, [FromQuery]string sourcepath, [FromQuery]string user)
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

        [HttpPost]
        [Route("{projectid}/Resource")]
        public IActionResult UpdatePageDetails(string projectid, [FromBody]CompileResourceRequest req)
        {
            try
            {
                if (req == null)
                    return BadRequest("Request object can not be null");

                req.ProjectId = projectid;
                req.IsPublish = false;

                var compileResult = CompilerHelper.CompileProjectResource(req);

                var compilerService = new CompilerService();
                var updatePageRequest = new CreateOrUpdateResourceRequestModel
                {
                    Errors = null,
                    FileContent = req.FileContent,
                    SourcePath = req.SourcePath.Trim(),
                    ClassName = req.ClassName,
                    ProjectId = req.ProjectId,
                    UserEmail = req.UserEmail,
                    UrlPattern = req.UrlPattern,
                    IsStatic = req.IsStatic,
                    IsDefault = req.IsDefault,
                    PageType = req.PageType,
                    KObject = req.KObject,
                    ResourceType = null
                };

                var validationResult = updatePageRequest.Validate();
                if (validationResult.Any())
                    return BadRequest(validationResult);
                if (MongoConnector.CreateOrUpdateResource(updatePageRequest))
                    return Ok(compileResult);
                return BadRequest();
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
        [HttpPost]
        [Route("{projectid}/RenameResource")]
        public IActionResult RenameResource(string projectid, [FromQuery]string user, [FromQuery]string oldname, [FromQuery]string newname)
        {
            try
            {
                if (string.IsNullOrEmpty(user))
                {
                    return Unauthorized();
                }
                var requestModel = new RenameResourceModel
                {
                    NewSourceName = newname,
                    OldSourceName = oldname,
                    ProjectId = projectid,
                    UserEmail = user
                };
                if (string.IsNullOrEmpty(oldname) || string.IsNullOrEmpty(newname))
                    return BadRequest("Resource name can not be empty");
                if (MongoConnector.RenameResource(requestModel))
                {
                    return Ok(true);
                }

                return BadRequest();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [DisableRequestSizeLimit]
        [Route("{projectid}/ResourceUpload")]
        public IActionResult UploadTemplate(string projectid, [FromBody]CompileResourceRequest req)
        {
            try
            {
                if (req == null)
                    return BadRequest("Request object can not be null");

                req.ProjectId = projectid;

                var projectDetailsRequestModel = new GetProjectDetailsRequestModel
                {
                    ProjectId = req.ProjectId,
                    ExcludeResources = false,
                    UserEmail = req.UserEmail
                };

                var compileResult = CompilerHelper.CompileProjectResource(req);

                var compilerService = new CompilerService();
                var updatePageRequest = new CreateOrUpdateResourceRequestModel
                {
                    Errors = null,
                    FileContent = req.FileContent,
                    SourcePath = req.SourcePath.Trim(),
                    ClassName = req.ClassName,
                    ProjectId = req.ProjectId,
                    UserEmail = req.UserEmail,
                    UrlPattern = req.UrlPattern,
                    IsStatic = req.IsStatic,
                    IsDefault = req.IsDefault,
                    PageType = req.PageType,
                    KObject = req.KObject,
                    ResourceType = null,
                    Configuration = !string.IsNullOrEmpty(req.Configuration) ? JsonConvert.DeserializeObject<Dictionary<string, object>>(req.Configuration) : null,
                };

                var validationResult = updatePageRequest.Validate();
                if (validationResult.Any())
                    return BadRequest(validationResult);

                if (MongoConnector.CreateOrUpdateResource(updatePageRequest))
                    return Ok(compileResult);

                return BadRequest();
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
        [HttpPost]
        [DisableRequestSizeLimit]
        [Route("{projectid}/ApplicationUpload")]
        public IActionResult UploadMultipartResource(string projectid, [FromForm]ApplicationUploadModel req, IFormFile file = null)
        {
            try
            {
                if (req == null)
                    return BadRequest("Request object can not be null");
                byte[] byteArrayStream = null;
                if (file != null)
                {
                    Stream stream = Request.Body;
                    byteArrayStream = Kitsune.API2.EnvConstants.Constants.ReadInputStream(file);
                }


                req.ProjectId = projectid;

                var projectDetailsRequestModel = new GetProjectDetailsRequestModel
                {
                    ProjectId = req.ProjectId,
                    ExcludeResources = false,
                    UserEmail = req.UserEmail
                };

                var compilerService = new CompilerService();
                var updatePageRequest = new CreateOrUpdateResourceRequestModel
                {
                    Errors = null,
                    SourcePath = req.SourcePath.Trim(),
                    ProjectId = req.ProjectId,
                    UserEmail = req.UserEmail,
                    PageType = req._PageType,
                    ResourceType = req._ResourceType,
                    Configuration = !string.IsNullOrEmpty(req.Configuration) ? JsonConvert.DeserializeObject<Dictionary<string, object>>( req.Configuration) : null,
                    ByteArrayStream = byteArrayStream
                };

                var validationResult = updatePageRequest.Validate();
                if (validationResult.Any())
                    return BadRequest(validationResult);

                if (MongoConnector.CreateOrUpdateResource(updatePageRequest))
                    return Ok(new ResourceCompilationResult { Success = true });

                return BadRequest();
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
        [HttpDelete]
        [Route("{projectid}/Resource")]
        public IActionResult DeletePage(string projectid, [FromQuery]string user, [FromQuery]string resourcename = null, [FromQuery] string resourcepath = null)
        {
            try
            {
                var requestModel = new DeleteResourceRequestModel
                {
                    ProjectId = projectid,
                    UserEmail = user,
                    SourcePath = resourcepath,
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
        [HttpGet]
        [Route("{projectId}/Preview")]
        public IActionResult GetPreview(string projectId, [FromQuery]string sourcePath, [FromQuery]string dataset, [FromQuery] string userEmail)
        {
            try
            {
                if (string.IsNullOrEmpty(projectId) || string.IsNullOrEmpty(sourcePath) || string.IsNullOrEmpty(dataset) || string.IsNullOrEmpty(userEmail))
                    return BadRequest("Invalid request parameters");

                var projectDetailsRequestModel = new GetProjectDetailsRequestModel
                {
                    ProjectId = projectId,
                    ExcludeResources = false,
                    UserEmail = userEmail
                };
                var projectDetails = MongoConnector.GetProjectDetails(projectDetailsRequestModel);
                var requestModel = new GetResourceDetailsRequestModel
                {
                    ProjectId = projectId,
                    SourcePath = sourcePath,
                    UserEmail = userEmail
                };

                var resourceDetails = MongoConnector.GetResourceDetails(requestModel);
                var userId = MongoConnector.GetUserIdFromUserEmail(new GetUserIdRequestModel { UserEmail = userEmail });

                var languageEntity = MongoConnector.GetLanguageEntity(new GetLanguageEntityRequestModel
                {
                    EntityId = projectDetails.SchemaId,
                });
                var partialModel = new GetPartialPagesDetailsRequestModel
                {
                    ProjectId = projectId,
                    UserEmail = userEmail
                };

                GetPartialPagesDetailsResponseModel partialPages = MongoConnector.GetPartialPagesDetails(partialModel);

                languageEntity = new KLanguageBase().GetKitsuneLanguage(userEmail, projectId, projectDetails, languageEntity, userId.Id);

                //Compile and get the call the single page preview api
                var result = _service.GetPreviewAsync(userEmail, projectId, resourceDetails, projectDetails, languageEntity, partialPages, dataset, userId.Id);

                if (result != null)
                    return Ok(result);
                return BadRequest("Unable to get the preview");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }
        [HttpPost]
        [Route("{projectid}/Compile")]
        public IActionResult CompileKitsuneResource(string projectid, [FromQuery]string resourcename, [FromQuery]string user, [FromBody]CompileResourceRequest req)
        {
            try
            {
                req.UserEmail = user;
                req.ProjectId = projectid;
                req.SourcePath = resourcename;

                //Compile project dosent required partial pages
                var compileResult = CompilerHelper.CompileProjectResource(req);
                return Ok(compileResult);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }
        [HttpPost]
        [Route("{projectid}/MakePageAsDefault")]
        public IActionResult MakePageAsDefault(string projectid, [FromQuery]string user, [FromQuery]string pagename)
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
    }
}