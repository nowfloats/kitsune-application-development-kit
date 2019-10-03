using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kitsune.Models.ActivityModels
{
    //Activity Log DB Model
    public class KitsuneActivityLog: MongoEntity
    {
        //Unique Id of the Resource
        public string ResourceId { get; set; }
        //Activity Id from List of Present Activities
        public string ActivityId { get; set; }
        //Log Creation DateTime
        public DateTime ActivityCreatedOn { get; set; }
        //Data related to the Log
        public Dictionary<string, string> Params { get; set; }
    }

    //Activity DB Model
    public class KitsuneActivity:MongoEntity
    {
        //Types of Resources available in system(Ex-Web,WebAction,DB etc)
        public string ResourceType { get; set; }
        //Name of the Activity
        public string ActivityName { get; set; }
        //Type of activity created for a resource(unique across resource)
        public string ActivityType { get; set; }
        //Activity's Message to be soon to the user
        public string Message { get; set; }
    }

    public class CreateActivityLog
    {
        public string ResourceId { get; set; }
        public string ActivityId { get; set; }
        public DateTime ActivityCreatedOn { get; set; }
        public Dictionary<string, string> Params { get; set; }
    }

    public class CreateNewActivity
    {
        public string ActivityName { get; set; }
        public string ActivityType { get; set; }
        public string ResourceType { get; set; }
        public string Message { get; set; }
    }

}
