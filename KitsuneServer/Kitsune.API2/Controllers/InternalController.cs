using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kitsune.API2.DataHandlers.Mongo;
using Kitsune.API2.Models;
using Kitsune.API2.Utils;
using Kitsune.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Kitsune.API2.Controllers
{
    [Produces("application/json")]
    [Route("api/Internal")]
    public class InternalController : Controller
    {
        [HttpPost("v1/SendEmail")]
        public IActionResult SendEmail([FromQuery]string clientId, [FromQuery]string projectId, [FromBody]EmailRequestWithAttachments emailRequestModel, [FromQuery] EmailUserConfigType configType = EmailUserConfigType.WEBSITE_USER)
        {
            try
            {
                if (emailRequestModel == null)
                    return BadRequest();

                if (!String.IsNullOrEmpty(projectId) && String.IsNullOrEmpty(clientId))
                {
                    try
                    {
                        clientId = MongoConnector.GetClientIdFromWebsiteId(projectId);
                    }
                    catch { }
                }

                return Ok(new EmailHelper().SendEmail(emailRequestModel, configType, clientId));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
    }
}