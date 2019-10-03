using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Kitsune.API.Model.ApiRequestModels
{
    public class CreateActivityLogRequest : BaseModel
    {
        public string ResourceId { get; set; }
        public string ActivityId { get; set; }
        public DateTime ActivityCreatedOn { get; set; }
        public Dictionary<string,string> Params { get; set; }

        public override IEnumerable<ValidationResult> Validate()
        {
            var validationResultList = new List<ValidationResult>();

            if (string.IsNullOrEmpty(this.ResourceId))
                validationResultList.Add(new ValidationResult("ResourceId can not be empty"));
            if(string.IsNullOrEmpty(this.ActivityId))
                validationResultList.Add(new ValidationResult("ActivityId can not be empty"));
            
            return validationResultList;
        }
    }

    public class CreateActivityLogResponse
    {

    }

    public class CreateActivityRequest : BaseModel
    {
        public string ActivityName { get; set; }
        public string ActivityType { get; set; }
        public string ResourceType { get; set; }
        public string Message { get; set; }

        public override IEnumerable<ValidationResult> Validate()
        {
            var validationResultList = new List<ValidationResult>();

            if (string.IsNullOrEmpty(this.ActivityName))
                validationResultList.Add(new ValidationResult("ActivityName can not be empty"));
            if (string.IsNullOrEmpty(this.ActivityType))
                validationResultList.Add(new ValidationResult("ActivityType can not be empty"));
            if (string.IsNullOrEmpty(this.ResourceType))
                validationResultList.Add(new ValidationResult("ResourceType can not be empty"));
            if (string.IsNullOrEmpty(this.Message))
                validationResultList.Add(new ValidationResult("Message can not be empty"));

            return validationResultList;
        }
    }
}
