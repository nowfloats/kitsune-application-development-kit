using Kitsune.API.Model.ApiRequestModels;
using Kitsune.Models.ActivityModels;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kitsune.API2.DataHandlers.Mongo
{
    public static partial class MongoConnector
    {
        internal static string CreateNewActivity(CreateNewActivity activity)
        {
            try
            {
                if (_kitsuneServer == null)
                    InitializeConnection();
                var activityCollection = _kitsuneDatabase.GetCollection<KitsuneActivity>(KitsuneActivityCollection);

                string activityId= ObjectId.GenerateNewId().ToString();
                KitsuneActivity activityObject = new KitsuneActivity()
                {
                    ActivityName=activity.ActivityName,
                    ActivityType=activity.ActivityType,
                    Message=activity.Message,
                    ResourceType=activity.ResourceType,
                    CreatedOn = new DateTime(),
                };
                activityObject._id = activityId;
                activityCollection.InsertOne(activityObject);
                
                return activityId;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
