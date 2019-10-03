
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Kitsune.Models.Project;
using Kitsune.API2.DataHandlers.Mongo;
using Kitsune.API.Model.ApiRequestModels;
using Kitsune.API2.Utils;

namespace Kitsune.API2.Controllers
{
    [Produces("application/json")]
    [Route("api/krawler")]
    public class KitsuneCrawlerController : Controller
    {
        [HttpPost]
        [Route("v1/updateprojectstatus")]
        public  IActionResult UpdateProjectStatus([FromQuery]string projectId,[FromQuery]ProjectStatus status)
        {
            try
            {
                var requestModel = new UpdateKitsuneProjectStatusRequestModel { ProjectId=projectId,ProjectStatus=status };
                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return BadRequest(validationResult);

                return Ok(MongoConnector.UpdateKitsuneProjectStatus(requestModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }

        }

        [HttpPost]
        [Route("v1/UpdateWebsiteDetails")]
        public  IActionResult UpdateWebsiteFavicon([FromBody]UpdateWebsiteDetailsRequestModel requestModel)
        {
            try
            {
                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return BadRequest(validationResult);

                return Ok(MongoConnector.UpdateWebsiteDetailsCommandHandler(requestModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
        
        [HttpPost]
        [Route("v1/startkrawl")]
        public  IActionResult StartKrawling([FromBody]StartKrawlRequestModel request)
        {
            try
            {
                var validationResult = request.Validate();
                if (validationResult.Any())
                    return BadRequest(validationResult);

                return Ok(MongoConnector.StartKrawling(request));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPost]
        [Route("v2/startkrawl")]
        public IActionResult StartCrawling([FromBody]StartKrawlRequestModel request)
        {
            try
            {
                var validationResult = request.Validate();
                if (validationResult.Any())
                    return BadRequest(validationResult);

                try
                {
                    request.Url = Helpers.GenerateUrl(request.Url);
                }
                catch(Exception ex)
                {
                    return BadRequest(ex.Message);
                }
                return Ok(MongoConnector.StartKrawling(request));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPost]
        [Route("v1/stopcrawl")]
        public IActionResult StopCrawling([FromBody]StopCrawlRequestModel request)
        {
            try
            {
                var validationResult = request.Validate();
                if (validationResult.Any())
                    return BadRequest(validationResult);

                return new CommonActionResult(MongoConnector.StopCrawling(request));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPost]
        [Route("v1/ReKrawl")]
        public IActionResult ReCrawl([FromQuery]string projectId,[FromQuery]string Url)
        {
            try
            {

                ReCrawlRequestModel request = new ReCrawlRequestModel()
                {
                    ProjectId=projectId,
                    Url=Url
                };

                var validationResult = request.Validate();
                if (validationResult.Any())
                    return BadRequest(validationResult);
                
                return Ok(MongoConnector.ReCrawl(request));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPost]
        [Route("v1/updatekrawlcomplete")]
        public  IActionResult KrawlingCompletedUpdateKitsuneProjects([FromQuery]string projectId)
        {
            try
            {
                var requestModel = new KrawlingCompletedRequestModel { ProjectId = projectId};
                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return BadRequest(validationResult);

                return Ok(MongoConnector.KrawlingCompletedUpdateKitsuneProjects(requestModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpGet]
        [Route("v1/GetListOfDomainsFound")]
        public  IActionResult GetListOfDomainsFound(string projectId)
        {
            try
            {
                var requestModel = new ListOfDomainsFoundRequestModel { ProjectId=projectId};
                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return BadRequest(validationResult);

                return Ok(MongoConnector.GetListOfDomainsFound(requestModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPost]
        [Route("v1/SaveSelectedDomain")]
        public  IActionResult SaveSelectedDomains([FromBody]SaveSelectedDomainRequestModel requestModel)
        {
            try
            {
                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return BadRequest(validationResult);

                return Ok(MongoConnector.SaveSelectedDomain(requestModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        #region Krawling Details for Ui

        [HttpGet]
        [Route("v1/GetAnalyseDetails")]
        public  IActionResult GetAnalyseDetails(string projectId)
        {
            try
            {
                var requestModel = new GetAnalyseDetailsRequestModel { ProjectId = projectId};
                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return BadRequest(validationResult);

                return Ok(MongoConnector.GetAnalyseDetails(requestModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpGet]
        [Route("v1/GetFilesDownloadDetails")]
        public  IActionResult GetFilesDownloadDetails(string projectId)
        {
            try
            {
                var requestModel = new FilesDownloadDetailsRequestModel { ProjectId = projectId};
                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return BadRequest(validationResult);

                return Ok(MongoConnector.GetFilesDownloadDetails(requestModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpGet]
        [Route("v1/GetNumberOfLinksReplacedQuery")]
        public  IActionResult GetListOfLinksReplaced(string projectId)
        {
            try
            {
                var requestModel = new GetNumberOfLinksReplacedRequestModel { ProjectId = projectId};
                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return BadRequest(validationResult);

                return Ok(MongoConnector.GetListOfLinksReplaced(requestModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        #endregion
    }
}