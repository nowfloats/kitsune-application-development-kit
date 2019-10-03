using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kitsune.API.Model.ApiRequestModels;
using Kitsune.API2.DataHandlers.Mongo;
using Kitsune.API2.Utils;
using Kitsune.BasePlugin.Utils;
using Microsoft.AspNetCore.Mvc;

namespace Kitsune.API2.Controllers
{
    [Route("kitsune")]
    public class KitsuneController : Controller
    {

        [HttpGet]
        public IActionResult Get()
        {
            return Ok("Welcome to Kitsune Platform, please visit https://kitsune.tools for more info or to contact us");
        }

        [HttpGet]
        [Route("v1/ListWebAction/{projectId}")]
        public IActionResult GetWebActionsForProject(string projectId)
        {
            if (!string.IsNullOrEmpty(projectId))
            {
                return GetWebactions(projectId, false);
            }
            return BadRequest();
        }

        [HttpGet]
        [Route("v1/ListWebActionDetails/{projectId}")]
        public IActionResult GetWebActionsForProjectDetails(string projectId)
        {
            if (!string.IsNullOrEmpty(projectId))
            {
                return GetWebactions(projectId, true);
            }
            return BadRequest();

        }

        private IActionResult GetWebactions(string projectId, bool includeProperties)
        {
            try
            {
                var project = MongoConnector.GetProjectDetails(projectId);
                if (project != null)
                {
                    var userId = MongoConnector.GetUserIdFromUserEmail(new API.Model.ApiRequestModels.GetUserIdRequestModel { UserEmail = project.UserEmail });
                    if (userId != null && !string.IsNullOrEmpty(userId.Id))
                    {
                        var webactions = APIHelper.GetWebAction(userId.Id);
                        if (webactions != null && webactions.WebActions != null && webactions.WebActions.Any())
                        {
                            var result = new GetWebActionsForProjectResult();
                            result.Token = userId.Id;
                            var resultactions = new List<WebActionResultItem>();
                            foreach (var action in webactions.WebActions)
                            {
                                resultactions.Add(new WebActionResultItem
                                {
                                    ActionId = action.ActionId,
                                    Description = action.Description,
                                    DisplayName = action.DisplayName,
                                    Name = action.Name,
                                    Properties = includeProperties ? action.Properties : null,
                                    UpdatedOn = action.UpdatedOn,
                                    UserId = action.UserId,
                                    UserName = action.UserName,
                                    WebsiteId = action.WebsiteId
                                });
                            }
                            result.WebActions = resultactions;
                            return Ok(result);
                        }
                    }
                    return BadRequest();
                }

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return Ok(new GetWebActionsForProjectResult());
        }

        [HttpGet]
        [Route("v1/GetBasePlugin")]
        public IActionResult GetBasePluginDetails([FromQuery] string projectId, [FromQuery] string clientId)
        {
            if (String.IsNullOrEmpty(clientId))
            {
                //Use this till all data is filled in project - collections
                clientId = MongoConnector.GetClientIdFromWebsiteId(projectId);
            }

            var basePlugin = BasePluginConfigGenerator.GetBasePlugin(clientId);

            return Ok(new BasePluginResponseModel()
            {
                clientId = basePlugin.GetClientId(),
                clientName = basePlugin.GetClientName(),
                _defaultSubDomain = basePlugin.GetSubDomain(),
            });
        }
    }
}
