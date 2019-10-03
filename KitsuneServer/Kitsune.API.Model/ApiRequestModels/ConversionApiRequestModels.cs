using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Kitsune.Models.ZipServiceModels;
using Newtonsoft.Json;

namespace Kitsune.API.Model.ApiRequestModels
{
    public class ActivateSiteRequestModel : BaseModel
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
    public class ActivateSiteResponseModel : ErrorApiResponseModel
    {
        public string CNAMERecord { get; set; }
        public string ARecord { get; set; }
        public bool DomainVerified { get; set; }
        public string Domain { get; set; }
    }

    public class SendKitsuneConvertEmailRequestModel
    {
        public string UserName { get; set; }
        public string EmailID { get; set; }
        public MailType MailType { get; set; }
        public List<string> Attachments { get; set; }
        public Dictionary<string, string> OptionalParams { get; set; }
        //string UserName, string EmailID, MailType type
    }
    public enum MailType
    {
        CONVERSION_INSTANTIATED,
        CONVERSION_ERROR,
        CONVERSION_SUCCESS,
        WEBSITE_ACTIVATION,
        BALANACE_LOW_1,
        BALANCE_LOW_2,
        BALANACE_EMPTY,
        PAYMENT_INSTANTIATED,
        PAYMENT_SUCCESS,
        PAYMENT_ERROR,
        PAYMENT_INVOICE,
        DEFAULT_CUSTOMER_KADMIN_CREDENTIALS,
        CUSTOMER_KADMIN_CREDENTIALS,
        CUSTOMER_BILLING_NOT_ACTIVATED,
        CUSTOMER_ENQUIRY,
        CUSTOM_MESSAGE
    }

    public class ArchiveProjectRequestModel : BaseModel
    {
        public string CrawlId { get; set; }
        public override IEnumerable<ValidationResult> Validate()
        {
            var validationResultList = new List<ValidationResult>();

            if (string.IsNullOrEmpty(this.CrawlId))
                validationResultList.Add(new ValidationResult("CrawlId can not be empty"));

            return validationResultList;
        }
    }

    public class GetListOfLiveWebsitesRequestModel : BaseModel
    {
        public string CrawlId { get; set; }
        public override IEnumerable<ValidationResult> Validate()
        {
            var validationResultList = new List<ValidationResult>();

            if (string.IsNullOrEmpty(this.CrawlId))
                validationResultList.Add(new ValidationResult("CrawlId can not be empty"));

            return validationResultList;
        }
    }
    public class GetProjectDownloadStatusRequestModel : BaseModel
    {
        public string CrawlId { get; set; }
        public override IEnumerable<ValidationResult> Validate()
        {
            var validationResultList = new List<ValidationResult>();

            if (string.IsNullOrEmpty(this.CrawlId))
                validationResultList.Add(new ValidationResult("CrawlId can not be empty"));

            return validationResultList;
        }
    }

    public class GetProjectDownloadStatusResponseModel
    {
        public string LinkUrl { get; set; }
        public TaskDownloadQueueStatus Status { get; set; }
        public string StatusMessage { get; set; }
    }
    public class GetProjectDownloadStatusRequestModelv2 : BaseModel
    {
        public List<string> ProjectId { get; set; }
        public override IEnumerable<ValidationResult> Validate()
        {
            var validationResultList = new List<ValidationResult>();


            if (this.ProjectId == null || this.ProjectId.Count == 0)
                validationResultList.Add(new ValidationResult("ProejctId List can not be empty"));

            return validationResultList;
        }
    }
    public class ProjectDownloadStatus
    {
        [JsonIgnore]
        public string _id { get; set; }
        public string ProjectId { get; set; }
        public string LinkUrl { get; set; }
        public TaskDownloadQueueStatus Status { get; set; }
        public string StatusMessage { get; set; }
    }

    public class GetProjectDownloadStatusResponseModelv2
    {
        public List<ProjectDownloadStatus> ProjectDownloadStatusList { get; set; }
    }

    public class DownloadFolderRequestModel : BaseModel
    {
        public string CrawlId { get; set; }
        public override IEnumerable<ValidationResult> Validate()
        {
            var validationResultList = new List<ValidationResult>();

            if (string.IsNullOrEmpty(this.CrawlId))
                validationResultList.Add(new ValidationResult("CrawlId can not be empty"));

            return validationResultList;
        }
    }

    public class DownloadFolderResponseModel
    {
        public TaskDownloadQueueStatus Status { get; set; }
        public string Message { get; set; }
        public string DownloadUrl { get; set; }

    }

    public class GetListOfAllTasksRequestModel : BaseModel
    {
        public override IEnumerable<ValidationResult> Validate()
        {
            var validationResultList = new List<ValidationResult>();

            if (string.IsNullOrEmpty(this.UserEmail))
                validationResultList.Add(new ValidationResult("UserEmail can not be empty"));

            return validationResultList;
        }
    }

    public class GetListOfAllTasksResponseModel
    {
        public List<KitsuneTaskDownloadQueueCollection> ListOfTask = new List<KitsuneTaskDownloadQueueCollection>();
    }

    public class GetDomainDetailsRequestModel : BaseModel
    {
        public string Url { get; set; }
        public override IEnumerable<ValidationResult> Validate()
        {
            var validationResultList = new List<ValidationResult>();
            if (string.IsNullOrEmpty(this.Url))
                validationResultList.Add(new ValidationResult("Url can not be empty"));

            return validationResultList;
        }
    }
    public class GetCrawlIdOfSiteDomainDetails
    {
        public string ProjectId { get; set; }
        public string WebsiteUrl { get; set; }
        public string WebsiteTag { get; set; }
    }
    public class GetDomainDetailsResponseResult
    {
        public bool IsRedirect { get; set; }
        public string Domain { get; set; }
        public string CrawlId { get; set; }
        public bool IsError { get; set; }
        public string ErrorMessage { get; set; }
    }
    public class GetUrlForKeywordRequestModel : BaseModel
    {
        public string Domain { get; set; }
        public string Keyword { get; set; }
        public override IEnumerable<ValidationResult> Validate()
        {
            var validationResultList = new List<ValidationResult>();
            if (string.IsNullOrEmpty(this.Domain))
                validationResultList.Add(new ValidationResult("Domain can not be empty"));

            return validationResultList;
        }
    }
    public class SearchObject
    {
        public string S3Url { get; set; }
        public List<string> Keywords { get; set; }
        public double Count { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
    }
    public class KSearchModel
    {
        public string FaviconUrl { get; set; }
        public List<SearchObject> SearchObjects { get; set; }
    }

    public class GetSiteMapRequestModel : BaseModel
    {
        public string Domain { get; set; }
        public string ProjectId { get; set; }
        public string WebsiteId { get; set; }
        public override IEnumerable<ValidationResult> Validate()
        {
            var validationResultList = new List<ValidationResult>();
            if (string.IsNullOrEmpty(this.Domain))
                validationResultList.Add(new ValidationResult("Domain can not be empty"));
            if (string.IsNullOrEmpty(this.ProjectId))
                validationResultList.Add(new ValidationResult("Project id can not be empty"));
            if (string.IsNullOrEmpty(this.WebsiteId))
                validationResultList.Add(new ValidationResult("Website id can not be empty"));

            return validationResultList;
        }
    }
   
}