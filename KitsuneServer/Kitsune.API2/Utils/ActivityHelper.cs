using AmazonAWSHelpers.SQSQueueHandler;
using Kitsune.API.Model.ApiRequestModels;
using Kitsune.API2.EnvConstants;
using Kitsune.Models;
using Kitsune.Models.ActivityModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Kitsune.API2.Utils
{
    public class ActivityHelper
    {
        /// <summary>
        /// Create new ActivityLog
        /// Pushes the ActivityLogModel to SQS
        /// </summary>
        /// <param name="activityLog"></param>
        public void LogActivity(CreateActivityLog activityLog)
        {
            if (activityLog == null)
                throw new ArgumentNullException(nameof(activityLog));
            if (String.IsNullOrEmpty(activityLog.ResourceId))
                throw new ArgumentNullException(nameof(activityLog.ResourceId));
            if (String.IsNullOrEmpty(activityLog.ActivityId))
                throw new ArgumentNullException(nameof(activityLog.ActivityId));

            ActivityLogSQSModel sqsmodel = new ActivityLogSQSModel
            {
                ResourceId = activityLog.ResourceId,
                ActivityId = activityLog.ActivityId,
                CreatedOn = activityLog.ActivityCreatedOn,
                Params = activityLog.Params
            };
            AmazonSQSQueueHandlers<ActivityLogSQSModel> sqsHandler = new AmazonSQSQueueHandlers<ActivityLogSQSModel>(AmazonAWSConstants.ActivitySQSUrl);
            string result = sqsHandler.PushMessageToQueue(sqsmodel, AmazonAWSConstants.SQSAccessKey, AmazonAWSConstants.SQSSecretKey);
            if(result == null)
            {
                throw new Exception($"Error pushing ResourceId : {activityLog.ResourceId} and ActivityId : {activityLog.ActivityId} to SQS");
            }
        }

        /// <summary>
        /// Create new ActivityLog
        /// Pushes the ACtivityLogModel to SQS asynchronously
        /// For internal use
        /// </summary>
        /// <param name="activityLog"></param>
        public static async Task LogActivityAync(CreateActivityLog activityLog)
        {
            if (activityLog == null)
                throw new ArgumentNullException(nameof(activityLog));
            if (String.IsNullOrEmpty(activityLog.ResourceId))
                throw new ArgumentNullException(nameof(activityLog.ResourceId));
            if (String.IsNullOrEmpty(activityLog.ActivityId))
                throw new ArgumentNullException(nameof(activityLog.ActivityId));

            ActivityLogSQSModel sqsmodel = new ActivityLogSQSModel
            {
                ResourceId = activityLog.ResourceId,
                ActivityId = activityLog.ActivityId,
                CreatedOn = activityLog.ActivityCreatedOn,
                Params = activityLog.Params
            };
            try
            {
                AmazonSQSQueueHandlers<ActivityLogSQSModel> sqsHandler = new AmazonSQSQueueHandlers<ActivityLogSQSModel>(AmazonAWSConstants.ActivitySQSUrl);
                var result =await sqsHandler.PushMessageToQueueAsync(sqsmodel, AmazonAWSConstants.SQSAccessKey, AmazonAWSConstants.SQSSecretKey);
                if (!result.Equals(HttpStatusCode.OK))
                {
                    throw new Exception($"Error pushing ResourceId : {activityLog.ResourceId} and ActivityId : {activityLog.ActivityId} to SQS");    
                }
            }
            catch(Exception ex)
            {
                //TODO
                //Log Exception
            }
        }
    }
}
