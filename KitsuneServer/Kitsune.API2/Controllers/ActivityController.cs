using AmazonAWSHelpers.SQSQueueHandler;
using Kitsune.API.Model.ApiRequestModels;
using Kitsune.API2.DataHandlers.Mongo;
using Kitsune.API2.Utils;
using Kitsune.Models;
using Kitsune.Models.ActivityModels;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kitsune.API2.Controllers
{
    [Route("api/Activity")]
    public class ActivityController : Controller
    {
        [HttpPost]
        [Route("v1/Log")]
        public IActionResult RegisterActivityLog([FromBody]CreateActivityLogRequest requestModel)
        {
            try
            {
                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return BadRequest(validationResult);

                CreateActivityLog createActivityLog = new CreateActivityLog()
                {
                    ActivityCreatedOn = requestModel.ActivityCreatedOn,
                    ActivityId = requestModel.ActivityId,
                    Params = requestModel.Params,
                    ResourceId = requestModel.ResourceId
                };
                ActivityHelper activityLogHelper = new ActivityHelper();
                activityLogHelper.LogActivity(createActivityLog);
                return Ok("Activity Pushed to sqs");
            }
            catch (Exception ex)
            {
                return StatusCode(500,ex.Message);
            }
        }

        [HttpPost]
        [Route("v1/Activity")]
        public IActionResult CreateNewActvity([FromBody]CreateActivityRequest requestModel)
        {
            try
            {
                var validationResult = requestModel.Validate();
                if (validationResult.Any())
                    return BadRequest(validationResult);

                CreateNewActivity newActivityModel = new CreateNewActivity()
                {
                    ActivityName=requestModel.ActivityName,
                    ActivityType=requestModel.ActivityType,
                    Message=requestModel.Message,
                    ResourceType=requestModel.ResourceType
                };
                var activityId=MongoConnector.CreateNewActivity(newActivityModel);

                return Ok($"ActivityId:{activityId}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
