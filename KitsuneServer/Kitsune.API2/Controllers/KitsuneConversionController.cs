using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Kitsune.API.Model.ApiRequestModels;
using Kitsune.API2.DataHandlers.Mongo;
//using System.Web.Http;
using Kitsune.API2.Utils;

namespace Kitsune.API2.Controllers
{
    [Produces("application/json")]
    [Route("api/Conversion")]
    public class KitsuneConversionController : Controller
    {
        [HttpPost]
        [Route("v1/ActivateSite")]
        public IActionResult ActivateSite([FromBody]ActivateSiteRequestModel requestModel)
        {
            try
            {
                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return BadRequest(validationResult);

                return Ok(MongoConnector.ActivateSite(requestModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPost]
        [Route("v1/ArchiveProject")]
        public IActionResult ArchiveProject([FromBody]ArchiveProjectRequestModel requestModel)
        {
            try
            {
                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return BadRequest(validationResult);

                return Ok(MongoConnector.ArchiveProject(requestModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpGet]
        [Route("v1/GetProjectDownloadstatus")]
        public IActionResult GetProjectDownloadStatus([FromQuery]string crawlId)
        {
            try
            {
                var requestModel = new GetProjectDownloadStatusRequestModel
                {
                    CrawlId = crawlId,
                };
                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return BadRequest(validationResult);

                return Ok(MongoConnector.GetProjectDownloadStatus(requestModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpGet]
        [Route("v2/GetProjectDownloadstatus")]
        public IActionResult GetProjectDownloadStatusv2([FromQuery]string projectId)
        {
            try
            {
                var requestModel = new GetProjectDownloadStatusRequestModelv2();
                if (!string.IsNullOrEmpty(projectId))
                    requestModel.ProjectId = projectId.Split(',').ToList();
                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return BadRequest(validationResult);

                return Ok(MongoConnector.GetProjectDownloadStatusv2(requestModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
        
        [HttpPost]
        [Route("v1/DownloadProject")]
        public IActionResult DownlaodProject([FromBody]DownloadFolderRequestModel requestModel)
        {
            try
            {
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
        [Route("v1/GetListOfAllTasks")]
        public IActionResult GetListOfAllTasks([FromQuery]string userEmail)
        {
            try
            {
                var requestModel = new GetListOfAllTasksRequestModel
                {
                    UserEmail = userEmail,
                };
                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return BadRequest(validationResult);

                return Ok(MongoConnector.GetListOfAllTasks(requestModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

       
        //TODO:Favicon Icon for search page
        [HttpGet]
        [Route("v1/GetUrlsForKeywords")]
        public IActionResult GetUrlForKeywords([FromQuery]string domain, [FromQuery]string keyword)
        {
            try
            {
                var requestModel = new GetUrlForKeywordRequestModel
                {
                    Domain = domain,
                    Keyword = keyword,
                    UserEmail = null
                };
                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return BadRequest(validationResult);

                return Ok(MongoConnector.GetUrlForKeywords(requestModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpGet]
        [Route("v1/GetSiteMapOfWebite")]
        public IActionResult GetSiteMapOfWebite([FromQuery]string domain, [FromQuery]string projectId, [FromQuery]string websiteId)
        {
            try
            {
                var requestModel = new GetSiteMapRequestModel
                {
                    Domain = domain,
                    ProjectId = projectId,
                    WebsiteId = websiteId,
                    UserEmail = null
                };
                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return BadRequest(validationResult);

                return Ok(MongoConnector.GetSiteMapOfWebite(requestModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        
        
        [HttpPost]
        [Route("v1/SendKitsuneConversionMail")]
        public IActionResult SendKitsuneConversionEmail(SendKitsuneConvertEmailRequestModel requestModel)
        {
            try
            {
                EmailHelper emailHelper = new EmailHelper();
                emailHelper.SendGetKitsuneEmail(requestModel.UserName, requestModel.EmailID, requestModel.MailType, requestModel.Attachments, requestModel.OptionalParams);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }
    }
}