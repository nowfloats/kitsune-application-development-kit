//using Kitsune.Language.Models;
//using Kitsune.Models;
//using Kitsune.Models.BuildAndRunModels;
//using Kitsune.Models.Krawler;
//using Kitsune.Models.Project;
//using Kitsune.Models.PublishModels;
//using Kitsune.Models.ZipServiceModels;
//using KitsuneKrawler.Models;
//using System.Linq;
//using System.Text;
//using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;


namespace Kitsune.API.Model.ApiRequestModels
{

    public class BaseModel : IValidatableObject
    {
        public string UserEmail { get; set; }

        public virtual IEnumerable<ValidationResult> Validate()
        {
            //var validationResultList = new List<ValidationResult>();

            //if (string.IsNullOrEmpty(this.UserEmail))
            //    validationResultList.Add(new ValidationResult("User Name can not be empty"));

            //return validationResultList;

            return new List<ValidationResult>();
        }

        public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            return new List<ValidationResult>();
        }
    }

    public class ErrorApiResponseModel
    {
        public bool IsError { get; set; }
        public string Message { get; set; }
    }

    public class CommonAPIResponse
    {
        public CommonAPIResponse()
        {
            this.StatusCode = HttpStatusCode.OK;
        }
        private CommonAPIResponse(Exception ex)
        {
            this.Errors = new List<ValidationResult> { new ValidationResult(ex.Message), new ValidationResult(ex.StackTrace) };
            this.StatusCode = HttpStatusCode.InternalServerError;
        }
        private CommonAPIResponse(ValidationResult error)
        {
            this.Errors = new List<ValidationResult> { error };
            this.StatusCode = HttpStatusCode.BadRequest;
        }

        #region Standard Response Helper Functions
        public static CommonAPIResponse Created(object result)
        {
            var response = new CommonAPIResponse();
            response.Response = result;
            response.StatusCode = HttpStatusCode.Created;
            return response;
        }
        public static CommonAPIResponse OK(object result)
        {
            var response = new CommonAPIResponse();
            response.StatusCode = HttpStatusCode.OK;
            response.Response = result;
            return response;
        }
        public static CommonAPIResponse NoContent()
        {
            var response = new CommonAPIResponse();
            response.StatusCode = HttpStatusCode.NoContent;
            return response;
        }
        public static CommonAPIResponse InternalServerError(Exception ex)
        {
            return new CommonAPIResponse(ex);
        }
        public static CommonAPIResponse BadRequest(ValidationResult error)
        {
            return new CommonAPIResponse(error);
        }
        public static CommonAPIResponse NotFound(ValidationResult error = null)
        {
            var response = new CommonAPIResponse();
            if (error != null)
                response.Errors = new List<ValidationResult>() { error };
            else
                response.Errors = new List<ValidationResult>() { new ValidationResult("Not found") };
            response.StatusCode = HttpStatusCode.NotFound;
            return response;
        }
        public static CommonAPIResponse BadRequest(IEnumerable<ValidationResult> errors)
        {
            var response = new CommonAPIResponse();
            response.Errors = errors;
            response.StatusCode = HttpStatusCode.BadRequest;
            return response;
        }
        public static CommonAPIResponse UnAuthorized(ValidationResult error = null)
        {
            var response = new CommonAPIResponse();
            if (error != null)
                response.Errors = new List<ValidationResult>() { error };
            else
                response.Errors = new List<ValidationResult>() { new ValidationResult("UnAuthorized") };
            response.StatusCode = HttpStatusCode.Unauthorized;
            return response;
        }
        #endregion

        public IEnumerable<ValidationResult> Errors { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public object Response { get; set; }
    }

    #region Mongo Models

    public class PaymentTransactionLog
    {
        public string _id { get; set; }
        public double Amount { get; set; }
        public string Currency { get; set; }
        public string UserProfileId { get; set; }
        public string InvoiceId { get; set; }
        public string PaymentRequestId { get; set; }
        public string Status { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public String DebitDetail { get; set; }
    }

    #endregion

    #region Customer related api models

    //public class CreateNewCustomerRequestModel : BaseModel
    //{
    //    public string CustomerName { get; set; }
    //    public string CustomerEmail { get; set; }
    //    public long PhoneNumber { get; set; }
    //    public string CrawlId { get; set; }
    //    public string Domain { get; set; }

    //    public override IEnumerable<ValidationResult> Validate()
    //    {
    //        var validationResultList = new List<ValidationResult>();

    //        if (string.IsNullOrEmpty(this.Domain))
    //            validationResultList.Add(new ValidationResult("Domain can not be empty"));

    //        if (string.IsNullOrEmpty(this.UserEmail))
    //            validationResultList.Add(new ValidationResult("Name can not be empty"));

    //        return validationResultList;
    //    }
    //}

    //public class CreateNewCustomerResponseModel
    //{
    //    public string CustomerId { get; set; }
    //    public string UserName { get; set; }
    //    public string Password { get; set; }
    //    public bool Success { get; set; }
    //    public string Message { get; set; }
    //}

    //public class DeveloperProjectCustomersDetails
    //{
    //    [BsonId]
    //    [BsonRepresentation(BsonType.ObjectId)]
    //    public string CustomerId { get; set; }
    //    public string CustomerName { get; set; }
    //    public string KitsuneUrl { get; set; }
    //}

    //public class GetDevelopersProjectCustomersQueryRequestModel : BaseModel
    //{
    //    public string CrawlId { get; set; }

    //    public override IEnumerable<ValidationResult> Validate()
    //    {
    //        var validationResultList = new List<ValidationResult>();

    //        if (string.IsNullOrEmpty(this.CrawlId))
    //            validationResultList.Add(new ValidationResult("CrawlId can not be empty"));

    //        return validationResultList;
    //    }
    //}

    //public class GetDevelopersProjectCustomersQueryResponseModel
    //{
    //    public List<DeveloperProjectCustomersDetails> Customers { get; set; }
    //}

    //public class GetCustomersListQueryRequestModel : BaseModel
    //{
    //    public override IEnumerable<ValidationResult> Validate()
    //    {
    //        var validationResultList = new List<ValidationResult>();

    //        if (string.IsNullOrEmpty(this.UserEmail))
    //            validationResultList.Add(new ValidationResult("UserEmail can not be empty"));

    //        return validationResultList;
    //    }
    //}

    //public class GetCustomersListResponseModel
    //{
    //    public List<CustomerDetails> ActiveCustomers { get; set; }
    //}

    //public class CustomerDetails
    //{
    //    [BsonId]
    //    [BsonRepresentation(BsonType.ObjectId)]
    //    public string CustomerId { get; set; }
    //    public string CustomerName { get; set; }
    //    public string KitsuneUrl { get; set; }
    //    public string Domain { get; set; }
    //    public bool DomainVerified { get; set; }
    //}

    //public class GetCustomerInformationQueryRequestModel : BaseModel
    //{
    //    public string CustomerId { get; set; }

    //    public override IEnumerable<ValidationResult> Validate()
    //    {
    //        var validationResultList = new List<ValidationResult>();

    //        if (string.IsNullOrEmpty(this.UserEmail))
    //            validationResultList.Add(new ValidationResult("UserEmail can not be empty"));

    //        if (string.IsNullOrEmpty(this.CustomerId))
    //            validationResultList.Add(new ValidationResult("CustomerId can not be empty"));

    //        return validationResultList;
    //    }
    //}

    //public class GetCustomerInformationResponseModel
    //{
    //    [BsonId]
    //    [BsonRepresentation(BsonType.ObjectId)]
    //    public string CustomerId { get; set; }
    //    public string CrawlId { get; set; }
    //    public string CustomerName { get; set; }
    //    public string CustomerEmail { get; set; }
    //    public long CustomerPhoneNumber { get; set; }
    //    public string UserName { get; set; }
    //    public string Password { get; set; }
    //    public string KitsuneUrl { get; set; }
    //    public string ProjectName { get; set; }
    //    public string Domain { get; set; }
    //}

    //public class CustomerDomainPresentOrNotQueryRequestModel : BaseModel
    //{
    //    public string Domain { get; set; }

    //    public override IEnumerable<ValidationResult> Validate()
    //    {
    //        var validationResultList = new List<ValidationResult>();

    //        if (string.IsNullOrEmpty(this.UserEmail))
    //            validationResultList.Add(new ValidationResult("UserEmail can not be empty"));

    //        if (string.IsNullOrEmpty(this.Domain))
    //            validationResultList.Add(new ValidationResult("Domain can not be empty"));

    //        return validationResultList;
    //    }
    //}

    //public class CustomerDomainPresentOrNotResponseModel
    //{
    //    public bool IsSuccess { get; set; }
    //    public bool IsExist { get; set; }
    //    public string Message { get; set; }
    //}

    #endregion

    #region Developer related api models

    //public class GetDeveloperSummaryRequestModel : BaseModel
    //{
    //    public override IEnumerable<ValidationResult> Validate()
    //    {
    //        var validationResultList = new List<ValidationResult>();

    //        if (string.IsNullOrEmpty(this.UserEmail))
    //            validationResultList.Add(new ValidationResult("User Name can not be empty"));            

    //        return validationResultList;
    //    }
    //}

    //public class GetDeveloperSummaryResponseModel
    //{
    //    public long KitsuneUserCount { get; set; }
    //    public long KitsuneCustomerCount { get; set; }
    //    public long KitsunePageViewCount { get; set; }
    //}

    //public class GetUserIdRequestModel : BaseModel
    //{
    //    public override IEnumerable<ValidationResult> Validate()
    //    {
    //        var validationResultList = new List<ValidationResult>();

    //        if (string.IsNullOrEmpty(this.UserEmail))
    //            validationResultList.Add(new ValidationResult("User Name can not be empty"));

    //        return validationResultList;
    //    }
    //}

    //public class GetUserIdResult : ErrorApiResponseModel
    //{
    //    public string Id { get; set; }
    //}

    //public class GetDeveloperProfileRequestModel : BaseModel
    //{
    //    public override IEnumerable<ValidationResult> Validate()
    //    {
    //        var validationResultList = new List<ValidationResult>();

    //        if (string.IsNullOrEmpty(this.UserEmail))
    //            validationResultList.Add(new ValidationResult("UserEmail can not be empty"));

    //        return validationResultList;
    //    }
    //}

    //public class GetDeveloperProfileResponseModel
    //{
    //    public string DisplayName { get; set; }
    //    public string Email { get; set; }
    //    public string UserName { get; set; }
    //    public DateTime UpdatedOn { get; set; }
    //    public UserLevel Level { get; set; }
    //    public string About { get; set; }
    //    public string Twitter { get; set; }
    //    public string Facebook { get; set; }
    //    public string ProfilePic { get; set; }
    //    public string Github { get; set; }
    //    public string Google { get; set; }
    //    public int FollowersCount { get; set; }
    //    public int FollowingCount { get; set; }
    //    public List<ThemeItem> Themes { get; set; }
    //    public Wallet Wallet { get; set; }
    //    public string PhoneNumber { get; set; }
    //    public Address Address { get; set; }
    //}

    //public class ThemeItem
    //{
    //    public string ThemeName { get; set; }
    //    public string ThemeId { get; set; }
    //}

    //public class Address
    //{
    //    public string AddressDetail { get; set; }
    //    public string City { get; set; }
    //    public string State { get; set; }
    //    public string Country { get; set; }
    //    public string Pin { get; set; }
    //}

    //public class Wallet
    //{
    //    public double Balance { get; set; }
    //    public DateTime UpdatedOn { get; set; }
    //    public double UnbilledUsage { get; set; }
    //}

    //public enum UserLevel
    //{
    //    Beginner,
    //    Intermediate,
    //    Advanced
    //}

    //public class GetUserPaymentStatsRequestModel : BaseModel
    //{
    //    public override IEnumerable<ValidationResult> Validate()
    //    {
    //        var validationResultList = new List<ValidationResult>();

    //        if (string.IsNullOrEmpty(this.UserEmail))
    //            validationResultList.Add(new ValidationResult("UserEmail can not be empty"));

    //        return validationResultList;
    //    }
    //}

    //public class PaymentStats
    //{
    //    public string InvoiceId { get; set; }
    //    public Double Amount { get; set; }
    //    public DateTime UpdatedOn { get; set; }
    //    public string Status { get; set; }
    //    public string DebitDetail { get; set; }
    //}

    //public class GetUserPaymentStatsResponseModel
    //{
    //    public List<PaymentStats> WalletStats { get; set; }
    //}

    //public class GetDeveloperDebitDetailsRequestModel : BaseModel
    //{
    //    public string Component { get; set; }
    //    public DateTime MonthAndYear { get; set; }
    //    public override IEnumerable<ValidationResult> Validate()
    //    {
    //        var validationResultList = new List<ValidationResult>();

    //        if (string.IsNullOrEmpty(this.UserEmail))
    //            validationResultList.Add(new ValidationResult("UserEmail can not be empty"));

    //        return validationResultList;
    //    }
    //}

    //public class GetDeveloperDebitDetailsResponseModel<T1, T2>
    //{
    //    public List<T1> Usage { get; set; }
    //    public T2 Meta { get; set; }
    //}

    //public class DebitDetailsMetaData
    //{
    //    public string CurrentComponent { get; set; }
    //    public DateTime CurrentMonthAndYear { get; set; }
    //    public List<DateTime> MonthAndYears { get; set; }
    //    public List<string> ComponentsUsed { get; set; }
    //}

    //public class UpdateDeveloperWalletRequestModel : BaseModel
    //{
    //    public double Amount { get; set; }
    //    public bool Add { get; set; }

    //    public override IEnumerable<ValidationResult> Validate()
    //    {
    //        var validationResultList = new List<ValidationResult>();

    //        if (string.IsNullOrEmpty(this.UserEmail))
    //            validationResultList.Add(new ValidationResult("UserEmail can not be empty"));

    //        return validationResultList;
    //    }

    //}

    //public class UpdateDeveloperDetailsRequestModel : BaseModel
    //{
    //    public string PhoneNumber { get; set; }
    //    public Address Address { get; set; }
    //    public string Name { get; set; }

    //    public IEnumerable<ValidationResult> Validate()
    //    {
    //        var validationResultList = new List<ValidationResult>();

    //        if (string.IsNullOrEmpty(this.UserEmail))
    //            validationResultList.Add(new ValidationResult("UserEmail can not be empty"));

    //        return validationResultList;
    //    }
    //}

    #endregion

    #region Project related api models

    //public class GetProjectDetailsRequestModel : BaseModel
    //{
    //    public string ProjectId { get; set; }
    //    public bool ExcludeResources { get; set; }

    //    public override IEnumerable<ValidationResult> Validate()
    //    {
    //        List<ValidationResult> validationResultList = base.Validate()?.ToList();
    //        if(validationResultList == null)
    //         validationResultList = new List<ValidationResult>();

    //        if (string.IsNullOrEmpty(ProjectId))
    //            validationResultList.Add(new ValidationResult("ProjectId can not be empty"));

    //        return validationResultList;
    //    }
    //}

    //public class GetProjectDetailsResponseModel : KitsuneProjectItem
    //{
    //    public GetProjectDetailsResponseModel()
    //    {
    //        Resources = new List<ResourceItemMeta>();
    //    }
    //    //public DateTime LastPublishedOn { get; set; }
    //    public DateTime CreatedOn { get; set; }
    //    public IList<ResourceItemMeta> Resources { get; set; }
    //    public AssetChildren Assets { get; set; }
    //    public DateTime LastPublishedOn { get; set; }
    //}

    //public class CreateOrUpdateProjectRequestModel : BaseModel
    //{
    //    public string ProjectId { get; set; }
    //    public string DefaultProjectId { get; set; }
    //    public string ProjectName { get; set; }
    //    public bool IsDynamic { get; set; }
    //    public ProjectStatus? ProjectStatus { get; set; }
    //    public override IEnumerable<ValidationResult> Validate()
    //    {
    //        var validationResultList = new List<ValidationResult>();

    //        if (!string.IsNullOrEmpty(this.ProjectName))
    //            this.ProjectName = this.ProjectName.Trim();
    //        if (string.IsNullOrEmpty(this.ProjectId) && string.IsNullOrEmpty(this.ProjectName))
    //            validationResultList.Add(new ValidationResult("Project Name can not be empty"));

    //        return validationResultList;
    //    }
    //}

    //public class UpdateProjectVersionRequestModel : BaseModel
    //{
    //    public string ProjectId { get; set; }
    //    public int Version { get; set; }
    //    public int PublishedVersion { get; set; }
    //    public override IEnumerable<ValidationResult> Validate()
    //    {
    //        var validationResultList = new List<ValidationResult>();

    //        if (string.IsNullOrEmpty(this.ProjectId))
    //            validationResultList.Add(new ValidationResult("Project id can not be Empty"));
    //        else if (this.Version == 0)
    //            validationResultList.Add(new ValidationResult("Provide valid project version."));

    //        return validationResultList;
    //    }
    //}

    //public class GetProjectsListRequestModel : BaseModel
    //{
    //    public override IEnumerable<ValidationResult> Validate()
    //    {
    //        var validationResultList = new List<ValidationResult>();

    //        return validationResultList;
    //    }
    //}

    //public class GetProjectsListResponseModel: ErrorApiResponseModel
    //{
    //    public List<KitsuneProjectItem> Projects { get; set; }
    //}

    //public class GetResourceDetailsRequestModel : BaseModel
    //{
    //    public string ProjectId { get; set; }
    //    public string SourcePath { get; set; }
    //    public override IEnumerable<ValidationResult> Validate()
    //    {
    //        var validationResultList = new List<ValidationResult>();
    //        if (!string.IsNullOrEmpty(this.ProjectId) || !string.IsNullOrEmpty(this.SourcePath))
    //            this.ProjectId = this.ProjectId.Trim();
    //        else
    //            validationResultList.Add(new ValidationResult("ProjectId and SourcePath can not be Empty"));

    //        return validationResultList;
    //    }

    //}

    //public class ResourceDetails : ResourceItemMeta
    //{
    //    public string HtmlSourceString { get; set; }
    //    public bool IsArchived { get; set; }
    //}

    //public class GetResourceDetailsResponseModel: ResourceDetails
    //{
    //}

    //public class MakeResourceAsDefaultRequestModel : BaseModel
    //{
    //    public string ProjectId { get; set; }
    //    public string SourcePath { get; set; }
    //    public override IEnumerable<ValidationResult> Validate()
    //    {
    //        var validationResultList = new List<ValidationResult>();
    //        if (string.IsNullOrEmpty(this.ProjectId))
    //            validationResultList.Add(new ValidationResult("Project id can not be Empty"));
    //        if (string.IsNullOrEmpty(this.SourcePath))
    //            validationResultList.Add(new ValidationResult("SourcePath can not be Empty"));
    //        //var res = mongoHelper.KitsuneResourcesCollection.Find(x => x.ProjectId == command.ProjectId && x.SourcePath == command.SourcePath);
    //        //if (res == null || !res.Any())
    //        //    yield return new ValidationResult("MakeDefaultProject", "Resource does not exist with SourcePath : " + command.SourcePath);
    //        //else
    //        //    _resourceId = res.FirstOrDefault()._id;
    //        return validationResultList;
    //    }

    //}

    //public class GetPartialPagesDetailsRequestModel : BaseModel
    //{
    //    public string ProjectId { get; set; }
    //    public List<string> SourcePaths { get; set; }
    //    public override IEnumerable<ValidationResult> Validate()
    //    {
    //        var validationResultList = new List<ValidationResult>();

    //        if (string.IsNullOrEmpty(this.ProjectId))
    //            validationResultList.Add(new ValidationResult("Project id can not be Empty"));

    //        return validationResultList;
    //    }
    //}

    //public class GetPartialPagesDetailsResponseModel
    //{
    //    public List<ResourceDetails> Resources { get; set; }
    //}

    //public class GetProjectWithResourcesDetailsRequestModel : BaseModel
    //{
    //    public string ProjectId { get; set; }
    //    public override IEnumerable<ValidationResult> Validate()
    //    {
    //        var validationResultList = new List<ValidationResult>();
    //        if (!string.IsNullOrEmpty(this.ProjectId))
    //            this.ProjectId = this.ProjectId.Trim();
    //        else if (string.IsNullOrEmpty(this.ProjectId))
    //            validationResultList.Add(new ValidationResult("Project id can not be Empty"));

    //        return validationResultList;
    //    }
    //}
    //public class GetProjectWithResourcesDetailsResponseModel : MongoEntity
    //{
    //    public string UserEmail { get; set; }
    //    public string ProjectName { get; set; }
    //    public string ProjectId { get; set; }
    //    public int Version { get; set; }
    //    public IList<ResourceDetails> Resources { get; set; }
    //    public ProjectStatus ProjectStatus { get; set; }
    //    public string FaviconIconUrl { get; set; }
    //    public ProjectType ProjectType { get; set; }
    //    public DateTime UpdatedOn { get; set; }
    //    public string SchemaId { get; set; }
    //    public BucketNames BucketNames { get; set; }
    //    public DateTime ArchivedOn { get; set; }
    //    public bool IsArchived { get; set; }
    //    public string ScreenShotUrl { get; set; }
    //    public int PublishedVersion { get; set; }
    //}

    //public class DeleteProjectRequestModel : BaseModel
    //{
    //    public string ProjectId { get; set; }
    //    public override IEnumerable<ValidationResult> Validate()
    //    {
    //        var validationResultList = new List<ValidationResult>();
    //        if (!string.IsNullOrEmpty(this.ProjectId))
    //            this.ProjectId = this.ProjectId.Trim();
    //        else if (string.IsNullOrEmpty(this.ProjectId))
    //            validationResultList.Add(new ValidationResult("ProjectId can not be Empty"));
    //        else if (string.IsNullOrEmpty(this.UserEmail))
    //            validationResultList.Add(new ValidationResult("UserEmail can not be Empty"));

    //        return validationResultList;
    //    }
    //}

    //public class DeleteResourceRequestModel : BaseModel
    //{
    //    public string ProjectId { get; set; }
    //    public string SourePath { get; set; }
    //    public override IEnumerable<ValidationResult> Validate()
    //    {
    //        var validationResultList = new List<ValidationResult>();
    //        if (!string.IsNullOrEmpty(this.ProjectId) || !string.IsNullOrEmpty(this.SourePath))
    //            this.ProjectId = this.ProjectId.Trim();
    //        else
    //            validationResultList.Add(new ValidationResult("ProjectId can not be empty"));

    //        return validationResultList;
    //    }
    //}

    //public class UpdateProjectAccessRequestModel : BaseModel
    //{
    //    public string ProjectId { get; set; }
    //    public string UserName { get; set; }
    //    public override IEnumerable<ValidationResult> Validate()
    //    {
    //        var validationResultList = new List<ValidationResult>();

    //        if (!string.IsNullOrEmpty(this.ProjectId))
    //            this.ProjectId = this.ProjectId.Trim();
    //        else
    //            validationResultList.Add(new ValidationResult("ProjectId can not be empty"));
    //        return validationResultList;
    //    }
    //}

    //public class CreateOrUpdateKitsuneStatusRequestModel : BaseModel
    //{
    //    public string ProjectId { get; set; }
    //    public BuildStatus Stage { get; set; }
    //    public Dictionary<string, int> Analyzer { get; set; }
    //    public Dictionary<string, int> Optimizer { get; set; }
    //    public Dictionary<string, int> Compiler { get; set; }
    //    public Dictionary<string, int> Replacer { get; set; }
    //    public List<BuildError> Error { get; set; }
    //    public override IEnumerable<ValidationResult> Validate()
    //    {
    //        var validationResultList = new List<ValidationResult>();
    //        if (string.IsNullOrEmpty(this.ProjectId))
    //            validationResultList.Add(new ValidationResult("ProjectId can not be Empty"));

    //        return validationResultList;
    //    }
    //}

    //public class GetKitsuneBuildStatusRequestModel : BaseModel
    //{
    //    public string ProjectId { get; set; }
    //    public int BuildVersion { get; set; }
    //    public override IEnumerable<ValidationResult> Validate()
    //    {
    //        var validationResultList = new List<ValidationResult>();
    //        if (string.IsNullOrEmpty(this.ProjectId))
    //            validationResultList.Add(new ValidationResult("ProjectId can not be Empty"));

    //        return validationResultList;
    //    }
    //}

    //public class GetKitsuneBuildStatusResponseModel
    //{
    //    public string ProjectId { get; set; }
    //    public int BuildVersion { get; set; }
    //    public BuildStatus Stage { get; set; }
    //    public bool IsCompleted { get; set; }
    //    public Dictionary<string, int> Analyzer { get; set; }
    //    public Dictionary<string, int> Optimizer { get; set; }
    //    public Dictionary<string, int> Compiler { get; set; }
    //    public Dictionary<string, int> Replacer { get; set; }
    //    public List<BuildError> Error { get; set; }
    //}

    //public class ProjectDetailsForBuildRequestModel : BaseModel
    //{
    //    public string ProjectId { get; set; }
    //    public override IEnumerable<ValidationResult> Validate()
    //    {
    //        var validationResultList = new List<ValidationResult>();
    //        if (string.IsNullOrEmpty(this.ProjectId))
    //            validationResultList.Add(new ValidationResult("ProjectId can not be Empty"));

    //        return validationResultList;
    //    }
    //}

    //public class BaseBuildMetaData
    //{
    //    public int LINK { get; set; }
    //    public int SCRIPT { get; set; }
    //    public int STYLE { get; set; }
    //    public int FILE { get; set; }
    //}

    //public class ProjectDetailsForBuildResponseModel
    //{
    //    public BaseBuildMetaData Total { get; set; }
    //    public BaseBuildMetaData Modified { get; set; }
    //}

    //public class BuildStatsCountModel
    //{
    //    public string _id { get; set; }
    //    public int count { get; set; }
    //}

    //public class MakeProjectLiveRequestModel : BaseModel
    //{
    //    public string ProjectId { get; set; }
    //    public string CustomerId { get; set; }
    //    public override IEnumerable<ValidationResult> Validate()
    //    {
    //        var validationResultList = new List<ValidationResult>();
    //        if (string.IsNullOrEmpty(this.ProjectId))
    //            validationResultList.Add(new ValidationResult("ProjectId can not be Empty"));

    //        return validationResultList;
    //    }

    //}

    //public class MakeProjectLiveResponseModel
    //{
    //    public string message { get; set; }
    //    public string status { get; set; }
    //}

    //public class PublishProjectRequestModel : BaseModel
    //{ 
    //    public string CustomerId { get; set; }
    //    public override IEnumerable<ValidationResult> Validate()
    //    {
    //        var validationResultList = new List<ValidationResult>();
    //        if (string.IsNullOrEmpty(this.CustomerId))
    //            validationResultList.Add(new ValidationResult("CustomerId can not be Empty"));

    //        return validationResultList;
    //    }
    //}
    //public class PublishProjectResponseModel : ErrorApiResponseModel
    //{
    //    public string CNAMERecord { get; set; }
    //    public string ARecord { get; set; }
    //    public bool DomainVerified { get; set; }
    //    public string Domain { get; set; }
    //}


    //public class KitsuneEnquiryRequestModel : BaseModel
    //{
    //    public string EmailBody { get; set; }
    //    public string Name { get; set; }
    //    public string Subject { get; set; }
    //    public string From { get; set; }
    //    public int Type { get; set; }
    //    public override IEnumerable<ValidationResult> Validate()
    //    {
    //        var validationResultList = new List<ValidationResult>();
    //        if (!(string.IsNullOrEmpty(this.EmailBody) || string.IsNullOrEmpty(this.Name) || string.IsNullOrEmpty(this.From)))
    //            validationResultList.Add(new ValidationResult("Please provide Email body, Name and From email"));

    //        return validationResultList;
    //    }

    //}


    //public class GetAuditProjectAndResourcesDetailsRequestModel : BaseModel
    //{
    //    public string ProjectId { get; set; }
    //    public int Version { get; set; }
    //    public override IEnumerable<ValidationResult> Validate()
    //    {
    //        var validationResultList = new List<ValidationResult>();
    //        if (!string.IsNullOrEmpty(this.ProjectId))
    //            this.ProjectId = this.ProjectId.Trim();
    //        else
    //            validationResultList.Add(new ValidationResult("ProjectId can not be empty"));
    //        if (this.Version == 0)
    //            validationResultList.Add(new ValidationResult("Version can not be 0"));
    //        return validationResultList;
    //    }
    //}

    //public class GetAuditProjectAndResourcesDetailsResponseModel
    //{
    //    public AuditKitsuneProject Project { get; set; }
    //    public List<AuditKitsuneResource> Resources { get; set; }
    //}

    //public class CreateAuditProjectRequestModel : BaseModel
    //{ 
    //    public AuditKitsuneProject Project { get; set; }
    //    public List<AuditKitsuneResource> Resources { get; set; }

    //    public override IEnumerable<ValidationResult> Validate()
    //    {
    //        var validationResultList = new List<ValidationResult>();
    //        if (this?.Project == null)
    //            validationResultList.Add(new ValidationResult("Project is null"));
    //        if (this?.Resources == null)
    //            validationResultList.Add(new ValidationResult("Resource is null"));
    //        if (!this.Resources.Any())
    //            validationResultList.Add(new ValidationResult("Project doesn't have any Resource"));
    //        return validationResultList;
    //    }
    //}

    //public class CreateProductionProjectRequestModel : BaseModel
    //{
    //    public ProductionKitsuneProject Project { get; set; }
    //    public List<ProductionKitsuneResource> Resources { get; set; }
    //    public override IEnumerable<ValidationResult> Validate()
    //    {
    //        var validationResultList = new List<ValidationResult>();
    //        if (this?.Project == null)
    //            validationResultList.Add(new ValidationResult("Project is null"));
    //        if (this?.Resources == null)
    //            validationResultList.Add(new ValidationResult("Resource is null"));
    //        if (!this.Resources.Any())
    //            validationResultList.Add(new ValidationResult("Project doesn't have any Resource"));
    //        return validationResultList;
    //    }
    //}
    //public class DeleteProductionProjectRequestModel : BaseModel
    //{
    //    public string ProjectId { get; set; }
    //    public override IEnumerable<ValidationResult> Validate()
    //    {
    //        var validationResultList = new List<ValidationResult>();
    //        if (string.IsNullOrEmpty(this.ProjectId))
    //            validationResultList.Add(new ValidationResult("ProjectId can not be Empty"));

    //        return validationResultList;
    //    }
    //}
    //public class GetProductionProjectDetailsRequestModel : BaseModel
    //{
    //    public string ProjectId { get; set; }
    //    public override IEnumerable<ValidationResult> Validate()
    //    {
    //        var validationResultList = new List<ValidationResult>();
    //        if (string.IsNullOrEmpty(this.ProjectId))
    //            validationResultList.Add(new ValidationResult("ProjectId can not be Empty"));

    //        return validationResultList;
    //    }
    //}

    //public class GetProductionProjectDetailsResponseModel
    //{
    //    public ProductionKitsuneProject Project { get; set; }
    //}

    //public class CreateOrUpdateResourceRequestModel : BaseModel
    //{ 
    //    public string ProjectId { get; set; }
    //    public string SourcePath { get; set; }
    //    public KitsunePageType PageType { get; set; }
    //    public string ClassName { get; set; }
    //    public string UrlPattern { get; set; }
    //    public bool IsDefault { get; set; }
    //    public bool IsStatic { get; set; }
    //    public string FileContent { get; set; }
    //    public IList<CompilerError> Errors { get; set; }
    //    public string KObject { get; set; }
    //    public ResourceType ResourceType { get; set; }
    //    public override IEnumerable<ValidationResult> Validate()
    //    {
    //        var validationResultList = new List<ValidationResult>();

    //        if (!string.IsNullOrEmpty(this.SourcePath))
    //            this.SourcePath = this.SourcePath.Trim();
    //        else
    //            validationResultList.Add(new ValidationResult("SourcePath can not be empty"));

    //        return validationResultList;
    //    }

    //}


    //public class AssetChildren
    //{
    //    public string name { get; set; }
    //    public List<AssetChildren> children { get; set; }
    //    public bool? toggled { get; set; }

    //    public string Path { get; set; }
    //    public bool IsKitsune { get; set; }
    //}

    //public class ResourceItemMeta
    //{
    //    [BsonRepresentation(BsonType.ObjectId)]
    //    public string _id { get; set; }
    //    public string SourcePath { get; set; }
    //    public string OptimizedPath { get; set; }
    //    public string ClassName { get; set; }
    //    public string UrlPattern { get; set; }
    //    public bool IsStatic { get; set; }
    //    public bool IsDefault { get; set; }
    //    public string KObject { get; set; }
    //    public KitsunePageType PageType { get; set; }
    //    public int Version { get; set; }
    //    public IEnumerable<CompilerError> Errors { get; set; }
    //    public string ProjectId { get; set; }
    //    public string UserEmail { get; set; }
    //    public string UrlPatternRegex { get; set; }
    //    public ResourceType ResourceType { get; set; }
    //    public DateTime UpdatedOn { get; set; }
    //    public DateTime CreatedOn { get; set; }
    //    public MetaData MetaData { get; set; }

    //}

    //public class KitsuneProjectItem
    //{
    //    [BsonRepresentation(BsonType.ObjectId)]
    //    public string _id { get; set; }
    //    public string ProjectId { get; set; }
    //    public string ProjectName { get; set; }
    //    public int Version { get; set; }
    //    public DateTime UpdatedOn { get; set; }
    //    public ProjectStatus ProjectStatus { get; set; }
    //    public DateTime ArchivedOn { get; set; }
    //    public bool IsArchived { get; set; }
    //    public string ScreenShotUrl { get; set; }
    //    public int PublishedVersion { get; set; }
    //    public string UserEmail { get; set; }
    //    public string FaviconIconUrl { get; set; }
    //    public ProjectType ProjectType { get; set; }
    //    public string SchemaId { get; set; }
    //    public BucketNames BucketNames { get; set; }
    //}
    //public class InvalidateCacheRequestModel
    //{
    //    public List<string> PathList { get; set; }
    //}
    //public class SaveFileContentToS3RequestModel
    //{
    //    public string ProjectId { get; set; }
    //    public string BucketName { get; set; }
    //    public string SourcePath { get; set; }
    //    public string FileContent { get; set; }
    //    public bool Compiled { get; set; }
    //    public int Version { get; set; }
    //    public string ClientId { get; set; }
    //}
    //public class GetFileFromS3RequestModel
    //{
    //    public string ProjectId { get; set; }
    //    public string BucketName { get; set; }
    //    public string SourcePath { get; set; }
    //    public bool Compiled { get; set; }
    //    public int Version { get; set; }
    //    public string ClientId { get; set; }
    //}

    #endregion

    #region Domain related api models

    //public class DomainDetails
    //{
    //    public string Domain { get; set; }
    //    public string KitsuneUrl { get; set; }
    //    public bool DomainVerified { get; set; }
    //    public bool IsActive { get; set; }
    //    public bool IsArchived { get; set; }
    //}

    //public class DomainNotMapped
    //{
    //    [BsonId]
    //    [BsonRepresentation(BsonType.ObjectId)]
    //    public string CustomerId { get; set; }
    //    public string Domain { get; set; }
    //    public string KitsuneUrl { get; set; }
    //}

    //public class KitsuneMapDomainRequestModel : BaseModel
    //{
    //    public string CustomerId { get; set; }

    //    public override IEnumerable<ValidationResult> Validate()
    //    {
    //        var validationResultList = new List<ValidationResult>();

    //        if (string.IsNullOrEmpty(this.CustomerId))
    //            validationResultList.Add(new ValidationResult("CustomerId can not be empty"));

    //        return validationResultList;
    //    }
    //}

    //public class KitsuneMapDomainResponseModel : ErrorApiResponseModel
    //{
    //}

    //public class KitsuneCheckAndMapDomainRequestModel : BaseModel
    //{
    //    public string CustomerId { get; set; }
    //    public override IEnumerable<ValidationResult> Validate()
    //    {
    //        var validationResultList = new List<ValidationResult>();

    //        if (string.IsNullOrEmpty(this.CustomerId))
    //            validationResultList.Add(new ValidationResult("CustomerId can not be empty"));

    //        return validationResultList;
    //    }
    //}

    //public class KitsuneCheckAndMapDomainResponseModel : ErrorApiResponseModel
    //{
    //    public bool IsMapped { get; set; }
    //}

    //public class KitsuneUpdateDomainRequestModel : BaseModel
    //{
    //    public string CustomerId { get; set; }
    //    public string NewDomain { get; set; }
    //    public override IEnumerable<ValidationResult> Validate()
    //    {
    //        var validationResultList = new List<ValidationResult>();

    //        if (string.IsNullOrEmpty(this.CustomerId))
    //            validationResultList.Add(new ValidationResult("CustomerId can not be empty"));

    //        if (string.IsNullOrEmpty(this.NewDomain))
    //            validationResultList.Add(new ValidationResult("Domain Name can not be empty"));

    //        return validationResultList;
    //    }
    //}

    //public class KitsuneUpdateDomainResponseModel : ErrorApiResponseModel
    //{
    //    public string KitsuneUrl { get; set; }
    //    public string Domain { get; set; }
    //}

    //public class KitsuneProjectsWithDomainNameNotMappedRequestModel : BaseModel
    //{
    //    public int Days { get; set; }
    //}

    //public class KitsuneProjectsWithDomainNameNotMappedResponseModel : ErrorApiResponseModel
    //{
    //    public List<DomainNotMapped> DomainList { get; set; }
    //}

    #endregion

    #region Crawl related api models

    //public class ProjectResources
    //{
    //    public List<AssetDetails> Links { get; set; }
    //    public List<AssetDetails> Styles { get; set; }
    //    public List<AssetDetails> Assets { get; set; }
    //    public List<AssetDetails> Scripts { get; set; }
    //}

    //public class UpdateKitsuneProjectStatusRequestModel:BaseModel
    //{
    //    public string ProjectId { get; set; }
    //    public ProjectStatus ProjectStatus { get; set; }

    //    public override IEnumerable<ValidationResult> Validate()
    //    {
    //        var validationResultList = new List<ValidationResult>();

    //        if (string.IsNullOrEmpty(this.ProjectId))
    //            validationResultList.Add(new ValidationResult("ProjectId can not be empty"));

    //        return validationResultList;
    //    }
    //}

    //public class UpdateWebsiteDetailsRequestModel : BaseModel
    //{
    //    public string ProjectId { get; set; }
    //    public string FaviconUrl { get; set; }
    //    public string ScreenShotUrl { get; set; }

    //    public override IEnumerable<ValidationResult> Validate()
    //    {
    //        var validationResultList = new List<ValidationResult>();

    //        if (string.IsNullOrEmpty(this.ProjectId))
    //            validationResultList.Add(new ValidationResult("CustomerId can not be empty"));
    //        if(String.IsNullOrEmpty(this.FaviconUrl) && String.IsNullOrEmpty(this.ScreenShotUrl))
    //            validationResultList.Add(new ValidationResult("Both FaviconIcon and ScreenshotUrl can not be empty"));

    //        return validationResultList;
    //    }
    //}

    //public class StartKrawlRequestModel : BaseModel
    //{
    //    public bool IsDeepKrawl { get; set; }
    //    public string Url { get; set; }
    //    public override IEnumerable<ValidationResult> Validate()
    //    {
    //        var validationResultList = new List<ValidationResult>();

    //        if (string.IsNullOrEmpty(this.Url))
    //            validationResultList.Add(new ValidationResult("Url can not be empty"));

    //        return validationResultList;
    //    }
    //}

    //public class StartKrawlResponseModel:ErrorApiResponseModel
    //{
    //    public string ProjectId { get; set; }
    //}

    //public class KrawlingCompletedRequestModel : BaseModel
    //{
    //    public string ProjectId { get; set; }
    //    public override IEnumerable<ValidationResult> Validate()
    //    {
    //        var validationResultList = new List<ValidationResult>();

    //        if (string.IsNullOrEmpty(this.ProjectId))
    //            validationResultList.Add(new ValidationResult("ProjectId can not be empty"));

    //        return validationResultList;
    //    }
    //}

    //public class GetAnalyseDetailsRequestModel : BaseModel
    //{
    //    public string ProjectId { get; set; }
    //    public override IEnumerable<ValidationResult> Validate()
    //    {
    //        var validationResultList = new List<ValidationResult>();

    //        if (string.IsNullOrEmpty(this.ProjectId))
    //            validationResultList.Add(new ValidationResult("ProjectId can not be empty"));

    //        return validationResultList;
    //    }
    //}

    //public class AnalyseDetailsResponseModel
    //{
    //    public KitsuneKrawlerStatusCompletion Stage { get; set; }
    //    public int LinksFound { get; set; }
    //    public int StylesFound { get; set; }
    //    public int ScriptsFound { get; set; }
    //    public int AssetsFound { get; set; }
    //}

    //public class FilesDownloadDetailsRequestModel : BaseModel
    //{
    //    public string ProjectId { get; set; }
    //    public override IEnumerable<ValidationResult> Validate()
    //    {
    //        var validationResultList = new List<ValidationResult>();

    //        if (string.IsNullOrEmpty(this.ProjectId))
    //            validationResultList.Add(new ValidationResult("ProjectId can not be empty"));

    //        return validationResultList;
    //    }
    //}

    //public class FilesDownloadDetailsResponseModel
    //{
    //    public KitsuneKrawlerStatusCompletion Stage { get; set; }
    //    public int ScriptsDownloaded { get; set; }
    //    public int StylesDownloaded { get; set; }
    //    public int AssetsDownloaded { get; set; }
    //    public int StylesFound { get; set; }
    //    public int ScriptsFound { get; set; }
    //    public int AssetsFound { get; set; }
    //}

    //public class GetNumberOfLinksReplacedRequestModel : BaseModel
    //{
    //    public string ProjectId { get; set; }
    //    public override IEnumerable<ValidationResult> Validate()
    //    {
    //        var validationResultList = new List<ValidationResult>();

    //        if (string.IsNullOrEmpty(this.ProjectId))
    //            validationResultList.Add(new ValidationResult("ProjectId can not be empty"));

    //        return validationResultList;
    //    }
    //}

    //public class GetNumberOfLinksReplacedResponseModel
    //{
    //    public int LinksReplaced { get; set; }
    //    public int LinksFound { get; set; }
    //    //[JsonProperty("Status")]
    //    public KitsuneKrawlerStatusCompletion Stage { get; set; }
    //}

    //public class ListOfDomainsFoundRequestModel : BaseModel
    //{
    //    public string ProjectId { get; set; }
    //    public override IEnumerable<ValidationResult> Validate()
    //    {
    //        var validationResultList = new List<ValidationResult>();

    //        if (string.IsNullOrEmpty(this.ProjectId))
    //            validationResultList.Add(new ValidationResult("ProjectId can not be empty"));

    //        return validationResultList;
    //    }
    //}

    //public class GetListOfDomainsResponseModel:ErrorApiResponseModel 
    //{
    //    public List<string> DomainList { get; set; }
    //}

    //public class SaveSelectedDomainRequestModel : BaseModel
    //{
    //    public string ProjectId { get; set; }
    //    public List<String> Domains { get; set; }
    //    public override IEnumerable<ValidationResult> Validate()
    //    {
    //        var validationResultList = new List<ValidationResult>();

    //        if (string.IsNullOrEmpty(this.ProjectId))
    //            validationResultList.Add(new ValidationResult("ProjectId can not be empty"));

    //        return validationResultList;
    //    }
    //}

    #endregion

    #region Conversion related Api models

    //public class ActivateSiteRequestModel : BaseModel
    //{ 
    //    public string CustomerId { get; set; }
    //    public override IEnumerable<ValidationResult> Validate()
    //    {
    //        var validationResultList = new List<ValidationResult>();

    //        if (string.IsNullOrEmpty(this.CustomerId))
    //            validationResultList.Add(new ValidationResult("CustomerId can not be empty"));

    //        return validationResultList;
    //    }
    //}
    //public class ActivateSiteResponseModel : ErrorApiResponseModel
    //{
    //    public string CNAMERecord { get; set; }
    //    public string ARecord { get; set; }
    //    public bool DomainVerified { get; set; }
    //    public string Domain { get; set; }
    //}

    //public class SendKitsuneConvertEmailRequestModel
    //{
    //    public string UserName { get; set; }
    //    public string EmailID { get; set; }
    //    public MailType MailType { get; set; }
    //    public List<string> Attachments { get; set; }
    //    public Dictionary<string, string> OptionalParams { get; set; }
    //    //string UserName, string EmailID, MailType type
    //}
    //public enum MailType
    //{
    //    Conversion_Instantiated,
    //    Conversion_Error,
    //    Conversion_Success,
    //    Website_Activation,
    //    Balanace_Low_1,
    //    Balance_Low_2,
    //    Blanace_empty,
    //    Payment_Instantiated,
    //    Payment_Success,
    //    Payment_Error,
    //    Payment_Invoice,
    //    Customer_Enquiry,
    //    Custom_Message
    //}

    //public class ArchiveProjectRequestModel: BaseModel
    //{
    //    public string CrawlId { get; set; }
    //    public override IEnumerable<ValidationResult> Validate()
    //    {
    //        var validationResultList = new List<ValidationResult>();

    //        if (string.IsNullOrEmpty(this.CrawlId))
    //            validationResultList.Add(new ValidationResult("CrawlId can not be empty"));

    //        return validationResultList;
    //    }
    //}

    //public class GetListOfLiveWebsitesRequestModel : BaseModel
    //{ 
    //    public string CrawlId { get; set; }
    //    public override IEnumerable<ValidationResult> Validate()
    //    {
    //        var validationResultList = new List<ValidationResult>();

    //        if (string.IsNullOrEmpty(this.CrawlId))
    //            validationResultList.Add(new ValidationResult("CrawlId can not be empty"));

    //        return validationResultList;
    //    }
    //}
    //public class GetProjectDownloadStatusRequestModel : BaseModel
    //{
    //    public string CrawlId { get; set; }
    //    public override IEnumerable<ValidationResult> Validate()
    //    {
    //        var validationResultList = new List<ValidationResult>();

    //        if (string.IsNullOrEmpty(this.CrawlId))
    //            validationResultList.Add(new ValidationResult("CrawlId can not be empty"));

    //        return validationResultList;
    //    }
    //}

    //public class GetProjectDownloadStatusResponseModel
    //{
    //    public string LinkUrl { get; set; }
    //    public TaskDownloadQueueStatus Status { get; set; }
    //    public string StatusMessage { get; set; }
    //}

    //public class DownloadFolderRequestModel : BaseModel
    //{
    //    public string CrawlId { get; set; }
    //    public override IEnumerable<ValidationResult> Validate()
    //    {
    //        var validationResultList = new List<ValidationResult>();

    //        if (string.IsNullOrEmpty(this.CrawlId))
    //            validationResultList.Add(new ValidationResult("CrawlId can not be empty"));

    //        return validationResultList;
    //    }
    //}

    //public class DownloadFolderResponseModel
    //{
    //    public TaskDownloadQueueStatus Status { get; set; }
    //    public string Message { get; set; }
    //    public string DownloadUrl { get; set; }

    //}

    //public class GetListOfAllTasksRequestModel : BaseModel
    //{
    //    public override IEnumerable<ValidationResult> Validate()
    //    {
    //        var validationResultList = new List<ValidationResult>();

    //        if (string.IsNullOrEmpty(this.UserEmail))
    //            validationResultList.Add(new ValidationResult("UserEmail can not be empty"));

    //        return validationResultList;
    //    }
    //}

    //public class GetListOfAllTasksResponseModel
    //{
    //    public List<KitsuneTaskDownloadQueueCollection> ListOfTask = new List<KitsuneTaskDownloadQueueCollection>();
    //}

    //public class GetDomainDetailsRequestModel : BaseModel
    //{
    //    public string Url { get; set; }
    //    public override IEnumerable<ValidationResult> Validate()
    //    {
    //        var validationResultList = new List<ValidationResult>();
    //        if (string.IsNullOrEmpty(this.Url))
    //            validationResultList.Add(new ValidationResult("Url can not be empty"));

    //        return validationResultList;
    //    }
    //}
    //public class GetCrawlIdOfSiteDomainDetails
    //{
    //    public string CrawlId { get; set; }
    //    public string UserName { get; set; }
    //    public string KitsuneUrl { get; set; }
    //    public string Domain { get; set; }
    //    public bool DomainVerified { get; set; }
    //}
    //public class GetDomainDetailsResponseResult
    //{
    //    public bool IsRedirect { get; set; }
    //    public string Domain { get; set; }
    //    public string CrawlId { get; set; }
    //    public bool IsError { get; set; }
    //    public string ErrorMessage { get; set; }
    //}
    //public class GetUrlForKeywordRequestModel : BaseModel
    //{
    //    public string Domain { get; set; }
    //    public string Keyword { get; set; }
    //    public override IEnumerable<ValidationResult> Validate()
    //    {
    //        var validationResultList = new List<ValidationResult>();
    //        if (string.IsNullOrEmpty(this.Domain))
    //            validationResultList.Add(new ValidationResult("Domain can not be empty"));

    //        return validationResultList;
    //    }
    //}
    //public class SearchObject
    //{
    //    public string S3Url { get; set; }
    //    public List<string> Keywords { get; set; }
    //    public double Count { get; set; }
    //    public string Title { get; set; }
    //    public string Description { get; set; }
    //}
    //public class KSearchModel
    //{
    //    public string FaviconUrl { get; set; }
    //    public List<SearchObject> SearchObjects { get; set; }
    //}

    //public class GetSiteMapRequestModel : BaseModel
    //{ 
    //    public string Domain { get; set; }
    //    public override IEnumerable<ValidationResult> Validate()
    //    {
    //        var validationResultList = new List<ValidationResult>();
    //        if (string.IsNullOrEmpty(this.Domain))
    //            validationResultList.Add(new ValidationResult("Domain can not be empty"));

    //        return validationResultList;
    //    }
    //}
    //public class GetContactDetailsRequestModel : BaseModel
    //{
    //    public string Domain { get; set; }
    //    public override IEnumerable<ValidationResult> Validate()
    //    {
    //        var validationResultList = new List<ValidationResult>();
    //        if (string.IsNullOrEmpty(this.Domain))
    //            validationResultList.Add(new ValidationResult("Domain can not be empty"));

    //        return validationResultList;
    //    }
    //}

    #endregion

    #region Payment related Model
    //public class DeveloperModel
    //{
    //    public ObjectId _id { get; set; }
    //    public string UserName { get; set; }
    //    public string SecurityStamp { get; set; }
    //    public string Email { get; set; }
    //    public bool EmailConfirmed { get; set; }
    //    public string PhoneNumber { get; set; }
    //    public bool PhoneNumberConfirmed { get; set; }
    //    public bool TwoFactorEnabled { get; set; }
    //    public string LockoutEndDateUtc { get; set; }
    //    public bool LockoutEnabled { get; set; }
    //    public int AccessFailedCount { get; set; }
    //    public List<string> Roles { get; set; }
    //    //public List<LoginField> Logins { get; set; }
    //    public List<string> Claims { get; set; }
    //    public string DisplayName { get; set; }
    //    public bool IsArchived { get; set; }
    //    public BsonDateTime CreatedOn { get; set; }
    //    public BsonDateTime UpdatedOn { get; set; }
    //    public int Level { get; set; }
    //    public string About { get; set; }
    //    public string Twitter { get; set; }
    //    public string Facebook { get; set; }
    //    public string ProfilePic { get; set; }
    //    public string Github { get; set; }
    //    public string Google { get; set; }
    //    public string MonitorGroupId { get; set; }
    //    public Address Address { get; set; }
    //    public Wallet Wallet { get; set; }
    //}


    //public class CreatePaymentRequestModel
    //{
    //    public string username { get; set; }
    //    public double amount { get; set; }
    //    public string responseurl { get; set; }
    //}

    //public class InstamojoWebhookRequestModel
    //{
    //    public string payment_id { get; set; }
    //    public string status { get; set; }
    //    public object longurl { get; set; }
    //    public string buyer_name { get; set; }
    //    public string buyer_phone { get; set; }
    //    public string buyer { get; set; }
    //    public string currency { get; set; }
    //    public string amount { get; set; }
    //    public string fees { get; set; }
    //    public string mac { get; set; }
    //    public string payment_request_id { get; set; }
    //    public string purpose { get; set; }
    //    public string shorturl { get; set; }
    //}
    #endregion

    #region Lnaguage Related

    //public class CreateOrUpdateLanguageEntityRequestModel : BaseModel
    //{
    //    public string LanguageId { get; set; }
    //    public string UserId { get; set; }
    //    public KEntity Entity { get; set; }
    //    public override IEnumerable<ValidationResult> Validate()
    //    {
    //        var validationResultList = new List<ValidationResult>();

    //        if (this.Entity == null)
    //            validationResultList.Add(new ValidationResult("Entity can not be null"));
    //        if (this.Entity != null && string.IsNullOrEmpty(this.Entity.EntityName))
    //            validationResultList.Add(new ValidationResult("Entity name can not be empty"));
    //        else if (string.IsNullOrEmpty(this.UserId))
    //            validationResultList.Add(new ValidationResult("User id can not be empty"));

    //        return validationResultList;
    //    }

    //}
    //public class CreateOrUpdateLanguageEntityResponseModel : ErrorApiResponseModel
    //{
    //    public string ObjectId { get; set; }
    //}

    //public class GetLanguageSchemaRequestModel : BaseModel
    //{
    //    public string EntityId { get; set; }
    //    public string UserId { get; set; }
    //    public string WebsiteId { get; set; }
    //    public override IEnumerable<ValidationResult> Validate()
    //    {
    //        var validationResultList = new List<ValidationResult>();
    //        if (string.IsNullOrEmpty(this.WebsiteId) && string.IsNullOrEmpty(this.EntityId))
    //            validationResultList.Add(new ValidationResult("Entity id can not be null"));
    //        return validationResultList;
    //    }
    //}
    //public class GetLanguageSchemaResponseResult
    //{
    //    public KEntity Schema { get; set; }
    //    public string ProjectId { get; set; }
    //}

    //public class SchemaToWebSite : BaseModel
    //{
    //    public string SchemaId { get; set; }
    //    public string WebsiteId { get; set; }
    //    public string ProjectId { get; set; }
    //    public string UserId { get; set; }
    //}
    //public class MapSchemaToWebSiteRequestModel : SchemaToWebSite
    //{
    //    public override IEnumerable<ValidationResult> Validate()
    //    {
    //        var validationResultList = new List<ValidationResult>();

    //        return validationResultList;
    //    }
    //}
    //public class MapSchemaToWebSiteResponseModel : ErrorApiResponseModel
    //{

    //}
    //public class UnMapSchemaToWebSiteRequestModel : SchemaToWebSite
    //{
    //    public override IEnumerable<ValidationResult> Validate()
    //    {
    //        var validationResultList = new List<ValidationResult>();

    //        return validationResultList;
    //    }

    //}
    //public class UnMapSchemaToWebSiteResponseModel : ErrorApiResponseModel
    //{

    //}

    //public class UpdateWebsiteDataRequestModel : BaseModel
    //{
    //    public string Query { get; set; }
    //    public bool Multi { get; set; }
    //    public object UpdateValue { get; set; }
    //    public string SchemaName { get; set; }
    //    public string WebsiteId { get; set; }
    //    public string UserId { get; set; }
    //    public override IEnumerable<ValidationResult> Validate()
    //    {
    //        var validationResultList = new List<ValidationResult>();
    //        if (string.IsNullOrEmpty(this.SchemaName))
    //            validationResultList.Add(new ValidationResult("Action name can not be empty"));
    //        if (string.IsNullOrEmpty(this.UserId))
    //            validationResultList.Add(new ValidationResult("User Id can not be empty"));
    //        this.SchemaName = this.SchemaName.Replace(" ", "");
    //        return validationResultList;
    //    }
    //}
    //public class UpdateWebsiteDataResponseModel : ErrorApiResponseModel
    //{

    //}
    //public class GetWebsiteDataRequestModel : BaseModel
    //{
    //    public string WebsiteId { get; set; }
    //    public string UserId { get; set; }
    //    public string SchemaName { get; set; }
    //    public int Skip { get; set; }
    //    public int Limit { get; set; }
    //    public string Query { get; set; }
    //    public string Include { get; set; }
    //    public string Sort { get; set; }
    //    public string Aggrigate { get; set; }
    //    public override IEnumerable<ValidationResult> Validate()
    //    {
    //        var validationResultList = new List<ValidationResult>();
    //        if (string.IsNullOrEmpty(this.SchemaName))
    //            validationResultList.Add(new ValidationResult("Schema name can not be empty"));
    //        else
    //        {
    //            this.SchemaName = this.SchemaName.Trim().ToUpper();
    //            this.SchemaName = this.SchemaName.Replace(" ", "");

    //        }
    //        //if (string.IsNullOrEmpty(command.UserId))
    //        //    yield return new ValidationResult("CreateOrUpdateWebAction", "User Id can not be empty");

    //        return validationResultList;
    //    }
    //}
    //public class Pagination
    //{
    //    public int CurrentIndex { get; set; }
    //    public long TotalCount { get; set; }
    //    public int PageSize { get; set; }
    //}
    //public class GetWebsiteDataResponseModel: ErrorApiResponseModel
    //{
    //    public List<object> Data { get; set; }
    //    public Pagination Extra { get; set; }

    //}

    //public class WebsiteCommand : BaseModel
    //{
    //    public string WebsiteId { get; set; }
    //    public string UserId { get; set; }
    //    public string SchemaName { get; set; }
    //    public string IPAddress { get; set; }
    //    public Dictionary<string, object> Data { get; set; }
    //}
    //public class AddOrUpdateWebsiteRequestModel : WebsiteCommand
    //{
    //    public override IEnumerable<ValidationResult> Validate()
    //    {
    //        var validationResultList = new List<ValidationResult>();
    //        if (string.IsNullOrEmpty(this.UserId))
    //            validationResultList.Add(new ValidationResult("UserId can not be empty"));
    //        if (string.IsNullOrEmpty(this.SchemaName))
    //            validationResultList.Add(new ValidationResult("Schema name can not be empty"));
    //        else
    //        {
    //            this.SchemaName = this.SchemaName.Trim();
    //            this.SchemaName = this.SchemaName.Replace(" ", "").ToLower();
    //        };
    //        if (string.IsNullOrEmpty(this.WebsiteId))
    //            validationResultList.Add(new ValidationResult("Website Id can not be empty"));
    //        if (this.Data == null)
    //            validationResultList.Add(new ValidationResult("Data can not be empty"));

    //        return validationResultList;
    //    }
    //}
    //public class AddOrUpdateWebsiteResponseModel : ErrorApiResponseModel
    //{

    //}

    //public class GetLanguageEntityRequestModel: BaseModel
    //{
    //    public string EntityId { get; set; }
    //    public string UserId { get; set; }
    //    public override IEnumerable<ValidationResult> Validate()
    //    {
    //        var validationResultList = new List<ValidationResult>();

    //        if (string.IsNullOrEmpty(this.EntityId))
    //            validationResultList.Add(new ValidationResult("Entity id can not be null"));

    //        return validationResultList;
    //    }
    //}


    #endregion

}
