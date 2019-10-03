using Kitsune.API.Model.ApiRequestModels;
using Kitsune.API2.DataHandlers.Mongo;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kitsune.API2.Controllers
{
    [Route("api/Event")]
    public class EventController : Controller
    {
        [HttpPost]
        [Route("v1/Register")]
        public IActionResult RegisterEvent([FromBody]CreateEventRequest requestModel)
        {
            try
            {
                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return BadRequest(validationResult);

                return Ok(MongoConnector.RegisterEvent(requestModel));
            }
            catch (Exception ex)
            {
                return BadRequest(ex);

            }
        }
    }
}
