using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Kitsune.API.Model.ApiRequestModels;
using Kitsune.API2.DataHandlers.Mongo;
using Kitsune.BasePlugin.Utils;
using Kitsune.API2.Utils;

namespace Kitsune.API2.Controllers
{
    [Produces("application/json")]
    [Route("api/domainMapper")]
    public class DomainController : Controller
    {
        [HttpPost("v1/mapdomain")]
        public  IActionResult KitsuneMapDomain([FromQuery]string customerId)
        {
            try
            {
                var requestModel = new KitsuneMapDomainRequestModel { WebsiteId = customerId };
                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return BadRequest(validationResult);

                return Ok(MongoConnector.MapDomainToCustomer(requestModel));
            }
            catch(Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPost("v1/checkandmapdomain")]
        public  IActionResult KitsuneCheckAndMapDomain([FromQuery]string customerId)
        {
            try
            {
                var requestModel = new KitsuneCheckAndMapDomainRequestModel { WebsiteId = customerId };
                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return BadRequest(validationResult);

                return Ok(MongoConnector.VerifyAndUpdateDomainMappingForCustomer(requestModel));
            }
            catch(Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPost("v1/updatedomain")]
        public IActionResult KitsuneUpdateDomain([FromQuery]string customerId, [FromQuery]string newDomain)
        {
            try
            {
                var requestModel = new KitsuneUpdateDomainRequestModel { CustomerId = customerId, NewDomain = newDomain };
                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return BadRequest(validationResult);

                return Ok(MongoConnector.UpdateDomainName(requestModel));
            }
            catch(Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpPost("v2/updatedomain")]
        public IActionResult KitsuneUpdateDomain([FromQuery]string websiteid, [FromQuery]string newDomain, [FromQuery]string clientid)
        {
            try
            {
                if (string.IsNullOrEmpty(clientid) || BasePluginConfigGenerator.GetBasePlugin(clientid).GetClientId() != clientid.Trim().ToUpper())
                    return new CommonActionResult(CommonAPIResponse.UnAuthorized());

                var requestModel = new KitsuneUpdateDomainRequestModel { CustomerId = websiteid, NewDomain = newDomain };
                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return BadRequest(validationResult);

                return Ok(MongoConnector.UpdateDomainName(requestModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpGet("v1/requesteddomains")]
        public IActionResult KitsuneRequestedDomain([FromQuery]string websiteId)
        {
            try
            {
                var requestModel = new KitsuneRequestedDomainRequestModel { WebsiteId = websiteId };
                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return BadRequest(validationResult);

                return Ok(MongoConnector.RequestedDomain(requestModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        [HttpGet("v1/unmappeddomains")]
        public  IActionResult KitsuneDomainsNotMapped([FromQuery]int days)
        {
            try
            {
                var requestModel = new KitsuneProjectsWithDomainNameNotMappedRequestModel { Days = days };
                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return BadRequest(validationResult);

                return Ok(MongoConnector.GetDomainNotMappedProjects(requestModel));
            }
            catch(Exception ex)
            {
                return BadRequest(ex);
            }
        }   
    }
}