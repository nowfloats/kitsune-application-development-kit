using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AmazonAWSHelpers.SQSQueueHandler;
using Kitsune.API.Model.ApiRequestModels;
using Kitsune.API2.DataHandlers.Mongo;
using Kitsune.API2.EnvConstants;
using Kitsune.API2.Utils;
using Kitsune.BasePlugin.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Kitsune.API2.Controllers.Application
{
    [Produces("application/json")]
    [Route("api/AppAdmin")]
    public class AppAdminController : Controller
    {
        [HttpGet]
        [Route("PublishedWebsites")]
        public IActionResult GetCustomersPerProject(string clientid, string projectid = null)
        {
            if (!string.IsNullOrEmpty(clientid) && BasePluginConfigGenerator.GetBasePlugin(clientid).GetClientId() == clientid.Trim().ToUpper())
            {
                return Ok(MongoConnector.GetLiveWebsiteIds(projectid));
            }
            return Unauthorized();
        }
    }
}