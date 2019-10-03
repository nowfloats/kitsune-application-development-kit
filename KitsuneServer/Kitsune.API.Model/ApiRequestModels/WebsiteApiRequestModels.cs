using Kitsune.Models.Cloud;
using Kitsune.Models.Project;
using Kitsune.Models.WebsiteModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Kitsune.API.Model.ApiRequestModels
{
    public class CreateNewWebsiteRequestModel : BaseModel
    {
        public string DeveloperId { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string ClientId {get;set;}
        public string ProjectId { get; set; }
        public string WebsiteTag { get; set; }
        public string Domain { get; set; }
        public bool CopyDemoData { get; set; }
        public bool ActivateWebsite { get; set; }

        public CloudProvider CloudProviderType { get; set; }

        //FOR NF
        public string WebsiteId { get; set; }

        public override IEnumerable<ValidationResult> Validate()
        {
            var validationResultList = new List<ValidationResult>();

            if (string.IsNullOrEmpty(this.WebsiteTag))
                validationResultList.Add(new ValidationResult("Website tag can not be empty"));

            if (string.IsNullOrEmpty(this.Email))
                validationResultList.Add(new ValidationResult("Email can not be empty"));

            if (string.IsNullOrEmpty(this.DeveloperId))
                validationResultList.Add(new ValidationResult("Developer id can not be empty"));

            if (string.IsNullOrEmpty(this.ProjectId))
                validationResultList.Add(new ValidationResult("ProjectId can not be empty"));

            return validationResultList;
        }
    }

    public class UpdateWebsiteRequestModel
    {
        public string WebsiteId { get; set; }
        [JsonIgnore]
        public string DeveloperId { get; set; }
        public string ClientId { get; set; }
        //Required for sync service, ignore for other update
        public string ProjectId { get; set; }
        public string WebsiteUrl { get; set; }
        public string WebsiteTag { get; set; }
        public int? Version { get; set; }
        public bool? IsActive { get; set; }

        public IEnumerable<ValidationResult> Validate()
        {
            var validationResultList = new List<ValidationResult>();

            return validationResultList;
        }
    }

    public class CreateOrUpdateWebsiteUserRequestModel
    {
        [JsonIgnore]
        public string WebsiteUserId { get; set; }
        [JsonIgnore]
        public string DeveloperId { get; set; }
        public string ClientId {get;set;}
        public string UserName { get; set; }
        public string WebsiteId { get; set; }
        public KitsuneWebsiteAccessType? AccessType { get; set; }
        public bool? IsActive { get; set; }
        public ContactDetails ContactDetails { get; set; }

        public IEnumerable<ValidationResult> Validate()
        {
            var validationResultList = new List<ValidationResult>();
            if (!string.IsNullOrWhiteSpace(WebsiteId) && AccessType == null)
                validationResultList.Add(new ValidationResult("Website access type can not be empty."));
            return validationResultList;
        }
    }
    public class UpdateWebsiteUserPasswordRequestModel
    {
        public string DeveloperId { get; set; }
        public string WebsiteId { get; set; }
        public string WebsiteUserId { get; set; }
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
        public IEnumerable<ValidationResult> Validate()
        {
            var validationResultList = new List<ValidationResult>();
            if (string.IsNullOrWhiteSpace(WebsiteUserId))
                validationResultList.Add(new ValidationResult("Username can not be empty."));
            if (string.IsNullOrWhiteSpace(OldPassword))
                validationResultList.Add(new ValidationResult("Old password can not be empty."));
            if (string.IsNullOrWhiteSpace(NewPassword))
                validationResultList.Add(new ValidationResult("New password can not be empty."));
            if (!string.IsNullOrWhiteSpace(NewPassword) && NewPassword.Length < 5)
                validationResultList.Add(new ValidationResult("New password length must be at least 5 characters."));

            return validationResultList;
        }
    }


    public class GetWebsitesResponseModel : Pagination
    {
        public List<WebsiteItem> Websites { get; set; }
        
    }
    public class WebsiteItem
    {
        public string WebsiteId { get; set; }
        public string WebsiteTag { get; set; }
        public string WebsiteDomain { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public bool IsSSLEnabled { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<WebsiteUser> WebsiteUsers { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsActive { get; set; }

    }
    public class WebsiteUser
    {
        public string FullName { get; set; }
        public string Address { get; set; }
    }

    public class WebsiteDetailsResponseModel
    {
        public string WebsiteId { get; set; }
        public string ProjectId { get; set; }
        public string WebsiteTag { get; set; }
        public string ProjectName { get; set; }
        public string WebsiteUrl { get; set; }
        public string RootPath { get; set; }
        public bool IsActive { get; set; }
        public int KitsuneProjectVersion { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public List<WebsiteUserItem> WebsiteUsers { get; set; }
        public string ClientId { get; set; }
        public string DeveloperId { get; set; }
        public bool IsSSLEnabled{ get; set; }
        public CloudProvider CloudProvider { get; set; }
    }

    public class WebsiteUserItem
    {
        public string UserName { get; set; }
        public string AccessType { get; set; }
        public ContactDetails Contact { get; set; }
        public bool IsActive { get; set; }
        public DateTime LastLoginTimeStamp { get; set; }
        public DateTime UpdatedOn { get; set; }
        public DateTime CreatedOn { get; set; }
    }

    public class LiveKitsuneWebsiteDetails
    {
        public string WebsiteId { get; set; }
        public string DeveloperId { get; set; }
        public string ProjectId { get; set; }
        public string ClientId { get; set; }
        public string WebsiteUrl { get; set; }
        public string RootPath { get; set; }
        public string WebsiteTag { get; set; }
        public string ProjectName { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime PublishedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public bool IsActive { get; set; }
        public int ProjectVersion { get; set; }

        public CloudProvider CloudProvider { get; set; }
    }
    public class GetLiveKitsuneWebsiteResponseModel : Pagination
    {
        public List<LiveKitsuneWebsiteDetails> LiveWebsites { get; set; }

    }

    public class WebsiteLogin_WebsiteDetais
    {
        public string WebsiteId { get; set; }
        public string ProjectId { get; set; }
        public string WebsiteTag { get; set; }
        public string ProjectName { get; set; }
        public string WebsiteUrl { get; set; }
        public string RootPath { get; set; }
        public bool IsActive { get; set; }
        public int KitsuneProjectVersion { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
    }

    public class WebsiteLogin_UserDetais
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string AccessType { get; set; }
        public ContactDetails Contact { get; set; }
        public bool IsActive { get; set; }
        public DateTime LastLoginTimeStamp { get; set; }
        public DateTime UpdatedOn { get; set; }
        public DateTime CreatedOn { get; set; }
    }

    public class WebsiteLoginResponseModel
    {
        public string SchemaId { get; set; }
        public string EntityName { get; set; }
        public string DeveloperId { get; set; }
        public ContactDetails DeveloperContactDetails { get; set; }

        public ContactDetails SupportContactDetails { get; set; }

        public WebsiteLogin_UserDetais UserDetails { get; set; }
        public WebsiteLogin_WebsiteDetais WebsiteDetails { get; set; }
    }
    public class VerifyDeveloperToken
    {
        public string Token { get; set; }
    } 
    public class VerifyLoginRequestModel : BaseModel
    {
        public string Domain { get; set; }
        public string UserName { get; set; }
        public string Pwd { get; set; }

        public override IEnumerable<ValidationResult> Validate()
        {
            var validationResultList = new List<ValidationResult>();

            if (string.IsNullOrEmpty(this.Domain))
                validationResultList.Add(new ValidationResult("Domain can not be empty"));

            if (string.IsNullOrEmpty(this.UserName))
                validationResultList.Add(new ValidationResult("UserName can not be empty"));

            if (string.IsNullOrEmpty(this.Pwd))
                validationResultList.Add(new ValidationResult("Pwd can not be empty"));

            return validationResultList;
        }
    }
    public class KSearchRequestModel : BaseModel
    {
        public string Keyword { get; set; }
        public string CustomerId { get; set; }
        public bool IsDemo { get; set; }

        public override IEnumerable<ValidationResult> Validate()
        {
            var validationResultList = new List<ValidationResult>();

            if (string.IsNullOrEmpty(this.Keyword))
                validationResultList.Add(new ValidationResult("Keyword can not be empty"));

            if (string.IsNullOrEmpty(this.CustomerId))
                validationResultList.Add(new ValidationResult("CustomerId can not be empty"));

            return validationResultList;
        }
    }
    public class KSearchResponseModel
    {
        public string FaviconUrl { get; set; }
        public List<KSearchObject> SearchObjects { get; set; }
    }

    public class KSearchObject
    {
        public string SourcePath { get; set; }
        public List<string> Keywords { get; set; }
        public double Count { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
    }
    public class KitsuneResourceMetaDataDetails
    {
        public string SourcePath { get; set; }
        public MetaData MetaData { get; set; }
    }
    public class GetCustomersIdFromDomainRequestModel : BaseModel
    {
        public string Domain { get; set; }
        public override IEnumerable<ValidationResult> Validate()
        {
            var validationResultList = new List<ValidationResult>();

            if (string.IsNullOrEmpty(this.Domain))
                validationResultList.Add(new ValidationResult("Domain can not be empty"));

            return validationResultList;
        }
    }
    public class PublishWebsiteDetailsModel
    {
        public string WebsiteUrl { get; set; }
        public string ProjectId { get; set; }
    }
    public class GetWebsiteInformationQueryRequestModel : BaseModel
    {
        public string WebsiteId { get; set; }

        public override IEnumerable<ValidationResult> Validate()
        {
            var validationResultList = new List<ValidationResult>();

            if (string.IsNullOrEmpty(this.UserEmail))
                validationResultList.Add(new ValidationResult("UserEmail can not be empty"));

            if (string.IsNullOrEmpty(this.WebsiteId))
                validationResultList.Add(new ValidationResult("CustomerId can not be empty"));

            return validationResultList;
        }
    }
    public class ActivateWebsiteRequestModel : BaseModel
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

    public class DeActivateWebsitesRequestModel : BaseModel
    {
        public List<string> WebsiteIds { get; set; }
        public string UserId { get; set; }
        public override IEnumerable<ValidationResult> Validate()
        {
            var validationResultList = new List<ValidationResult>();

            if (this.WebsiteIds == null || this.WebsiteIds.Count == 0)
                validationResultList.Add(new ValidationResult("WebsiteIds can not be empty"));

            return validationResultList;
        }
    }
    
    public class DeActivateWebsitesResponseModel
    {
        public bool IsSuccess { get; set; }
        public List<string> FailedWebsiteIds { get; set; }
    }

    public class GenerateKAdminLoginTokenRequestModel : BaseModel
    {
        public string WebsiteId { get; set; }
        public string UserId { get; set; }
        public int? ExpiryTime { get; set; }
        public string Source { get; set; }
        public override IEnumerable<ValidationResult> Validate()
        {
            var validationResultList = new List<ValidationResult>();

            if (string.IsNullOrEmpty(this.WebsiteId))
                validationResultList.Add(new ValidationResult("WebsiteId can not be empty"));

            return validationResultList;
        }
    }
    public class DecodeKAdminTokenRequestModel : BaseModel
    {
        public string Token { get; set; }
        public override IEnumerable<ValidationResult> Validate()
        {
            var validationResultList = new List<ValidationResult>();

            if (string.IsNullOrEmpty(this.Token))
                validationResultList.Add(new ValidationResult("Token can not be empty"));

            return validationResultList;
        }
    }

    public class KAdminLoginUrlResponseModel
    {
        public string RedirectUrl { get; set; }
    }

    public class KAdminTokenLoginResponseModel
    {
        public string Source { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string WebsiteUrl { get; set; }
    }

    public class IsCallTrackerEnabledForWebsiteResponse
    {
        public bool isActive { get; set; }  
    }
    public class GetWebsiteCacheInvalidationResult
    {
        public bool IsCDNToggleAvailable { get; set; }
        public bool IsInvalidationEnabled { get; set; }
        public DateTime? LastInvalidation { get; set; }
        public DateTime? NextInvalidation { get; set; }
    }

}
