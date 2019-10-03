using Kitsune.Models.WebsiteModels;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Kitsune.API.Model.ApiRequestModels
{
    public class DomainDetailsToMapDomain
    {
        public string Domain { get; set; }
        public string KitsuneUrl { get; set; }
    }

    public class DomainNotMapped
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string CustomerId { get; set; }
        public string Domain { get; set; }
        public string KitsuneUrl { get; set; }
    }

    public class KitsuneMapDomainRequestModel : BaseModel
    {
        public string WebsiteId { get; set; }

        public override IEnumerable<ValidationResult> Validate()
        {
            var validationResultList = new List<ValidationResult>();

            if (string.IsNullOrEmpty(this.WebsiteId))
                validationResultList.Add(new ValidationResult("WebsiteId can not be empty"));

            return validationResultList;
        }
    }

    public class KitsuneMapDomainResponseModel : ErrorApiResponseModel
    {
    }

    public class KitsuneCheckAndMapDomainRequestModel : BaseModel
    {
        public string WebsiteId { get; set; }
        public override IEnumerable<ValidationResult> Validate()
        {
            var validationResultList = new List<ValidationResult>();

            if (string.IsNullOrEmpty(this.WebsiteId))
                validationResultList.Add(new ValidationResult("CustomerId can not be empty"));

            return validationResultList;
        }
    }

    public class KitsuneCheckAndMapDomainResponseModel : ErrorApiResponseModel
    {
        public bool IsMapped { get; set; }
    }

    public class KitsuneUpdateDomainRequestModel : BaseModel
    {
        public string CustomerId { get; set; }
        public string NewDomain { get; set; }
        public override IEnumerable<ValidationResult> Validate()
        {
            var validationResultList = new List<ValidationResult>();

            if (string.IsNullOrEmpty(this.CustomerId))
                validationResultList.Add(new ValidationResult("CustomerId can not be empty"));

            if (string.IsNullOrEmpty(this.NewDomain))
                validationResultList.Add(new ValidationResult("Domain Name can not be empty"));

            return validationResultList;
        }
    }

    public class KitsuneRequestedDomainRequestModel : BaseModel
    {
        public string WebsiteId { get; set; }
        public override IEnumerable<ValidationResult> Validate()
        {
            var validationResultList = new List<ValidationResult>();

            if (string.IsNullOrEmpty(this.WebsiteId))
                validationResultList.Add(new ValidationResult("CustomerId can not be empty"));
            
            return validationResultList;
        }
    }

    public class KitsuneRequestedDomainResponseModel
    {
        public List<WebsiteDNSInfo> RequestedDomains { get; set; }
    }

    public class KitsuneUpdateDomainResponseModel : ErrorApiResponseModel
    {
        public string KitsuneUrl { get; set; }
        public string Domain { get; set; }
    }

    public class KitsuneProjectsWithDomainNameNotMappedRequestModel : BaseModel
    {
        public int Days { get; set; }
    }

    public class KitsuneProjectsWithDomainNameNotMappedResponseModel : ErrorApiResponseModel
    {
        public List<DomainNotMapped> DomainList { get; set; }
    }

}