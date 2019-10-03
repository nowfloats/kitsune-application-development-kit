using Kitsune.Models;
using Kitsune.Models.BuildAndRunModels;
using Kitsune.Models.Krawler;
using Kitsune.Models.Project;
using Kitsune.Models.PublishModels;
using Kitsune.Models.ZipServiceModels;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Kitsune.Models.CollectionModels;
using Kitsune.Models.WebsiteModels;
using Kitsune.Models.Cloud;
using System.IO;

namespace Kitsune.API.Model.ApiRequestModels
{
    public class CreateWordPressProjectRequestModel : BaseModel
    {
        public string ProjectName { get; set; }
        public string ClientId { get; set; }

        public override IEnumerable<ValidationResult> Validate()
        {
            List<ValidationResult> validationResultList = base.Validate()?.ToList();
            if (validationResultList == null)
                validationResultList = new List<ValidationResult>();

            if (String.IsNullOrEmpty(this.UserEmail))
                validationResultList.Add(new ValidationResult("UserEmail cannot be null"));
            if (!string.IsNullOrEmpty(this.ProjectName))
                this.ProjectName = this.ProjectName.Trim();

            return validationResultList;
        }
    }

    public class GetProjectDetailsRequestModel : BaseModel
    {
        public string ProjectId { get; set; }
        public bool ExcludeResources { get; set; }

        public int CompilerVersion { get; set; }

        public override IEnumerable<ValidationResult> Validate()
        {
            List<ValidationResult> validationResultList = base.Validate()?.ToList();
            if (validationResultList == null)
                validationResultList = new List<ValidationResult>();

            if (string.IsNullOrEmpty(ProjectId))
                validationResultList.Add(new ValidationResult("ProjectId can not be empty"));

            return validationResultList;
        }
    }

    public class GetProjectStatusRequestModel : BaseModel
    {
        public string ProjectId { get; set; }

        public override IEnumerable<ValidationResult> Validate()
        {
            List<ValidationResult> validationResultList = base.Validate()?.ToList();
            if (validationResultList == null)
                validationResultList = new List<ValidationResult>();

            if (string.IsNullOrEmpty(ProjectId))
                validationResultList.Add(new ValidationResult("ProjectId can not be empty"));

            return validationResultList;
        }
    }

    public class GetProjectDetailsResponseModel : KitsuneProjectItem
    {
        //public GetProjectDetailsResponseModel()
        //{
        //    Resources = new List<ResourceItemMeta>();
        //}
        public List<ResourceItemMeta> Resources { get; set; }
    }

    public class GetProjectStatusResponseModel
    {
        public string ProjectStatus { get; set; }
    }


    public class CreateOrUpdateProjectRequestModel : BaseModel
    {
        public string ProjectId { get; set; }
        public string ClientId { get; set; }
        public string DefaultProjectId { get; set; }
        public string ProjectName { get; set; }
        public bool IsDynamic { get; set; }
        public int CompilerVersion { get; set; }
        public ProjectStatus? ProjectStatus { get; set; }
        public override IEnumerable<ValidationResult> Validate()
        {
            var validationResultList = new List<ValidationResult>();

            if (!string.IsNullOrEmpty(this.ProjectName))
                this.ProjectName = this.ProjectName.Trim();
            if (string.IsNullOrEmpty(this.ProjectId) && string.IsNullOrEmpty(this.ProjectName))
                validationResultList.Add(new ValidationResult("Project Name can not be empty"));

            return validationResultList;
        }
    }

    public class UpdateProjectVersionRequestModel : BaseModel
    {
        public string ProjectId { get; set; }
        public int Version { get; set; }
        public int PublishedVersion { get; set; }
        public override IEnumerable<ValidationResult> Validate()
        {
            var validationResultList = new List<ValidationResult>();

            if (string.IsNullOrEmpty(this.ProjectId))
                validationResultList.Add(new ValidationResult("Project id can not be Empty"));
            else if (this.Version == 0)
                validationResultList.Add(new ValidationResult("Provide valid project version."));

            return validationResultList;
        }
    }

    public class GetProjectsListRequestModel : BaseModel
    {
        public override IEnumerable<ValidationResult> Validate()
        {
            var validationResultList = new List<ValidationResult>();

            return validationResultList;
        }
    }
    public class GetProjectsListRequestModelV2 : BaseModel
    {
        public int Skip { get; set; }
        public int Limit { get; set; }
        public override IEnumerable<ValidationResult> Validate()
        {
            var validationResultList = new List<ValidationResult>();

            return validationResultList;
        }
    }

    public class GetProjectsList
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }
        public string ProjectId { get; set; }
        public int Version { get; set; }
        public string ProjectName { get; set; }
        public string ScreenShotUrl { get; set; }
        public DateTime CreatedOn { get; set; }
        public string SchemaId { get; set; }
        public ProjectType ProjectType { get; set; }
        public ProjectStatus ProjectStatus { get; set; }
    }
    public class GetProjectsListResponseModel
    {
        public List<GetProjectsList> Projects { get; set; }
    }
    public class GetProjectsListResponseModelV2 : GetProjectsListResponseModel
    {
        public Pagination Extra { get; set; }
    }

    public class GetResourceDetailsRequestModel : BaseModel
    {
        public string ProjectId { get; set; }
        public string SourcePath { get; set; }
        public override IEnumerable<ValidationResult> Validate()
        {
            var validationResultList = new List<ValidationResult>();
            if (!string.IsNullOrEmpty(this.ProjectId) || !string.IsNullOrEmpty(this.SourcePath))
                this.ProjectId = this.ProjectId.Trim();
            else
                validationResultList.Add(new ValidationResult("ProjectId and SourcePath can not be Empty"));

            return validationResultList;
        }

    }

    public class ResourceDetails : ResourceItemMeta
    {
        public string HtmlSourceString { get; set; }
        public bool IsArchived { get; set; }
    }

    public class KitsuneFile
    {
        public string Base64Data { get; set; }
        public string ContentType { get; set; }
    }

    public class GetResourceDetailsResponseModel : ResourceDetails
    {

    }

    public class GetKitsuneResourceDetailsResponseModel : ResourceDetails
    {
        public KitsuneFile File { get; set; }
    }

    public class MakeResourceAsDefaultRequestModel : BaseModel
    {
        public string ProjectId { get; set; }
        public string SourcePath { get; set; }
        public override IEnumerable<ValidationResult> Validate()
        {
            var validationResultList = new List<ValidationResult>();
            if (string.IsNullOrEmpty(this.ProjectId))
                validationResultList.Add(new ValidationResult("Project id can not be Empty"));
            if (string.IsNullOrEmpty(this.SourcePath))
                validationResultList.Add(new ValidationResult("SourcePath can not be Empty"));
            //var res = mongoHelper.KitsuneResourcesCollection.Find(x => x.ProjectId == command.ProjectId && x.SourcePath == command.SourcePath);
            //if (res == null || !res.Any())
            //    yield return new ValidationResult("MakeDefaultProject", "Resource does not exist with SourcePath : " + command.SourcePath);
            //else
            //    _resourceId = res.FirstOrDefault()._id;
            return validationResultList;
        }
    }


    public class GetPartialPagesDetailsRequestModel : BaseModel
    {
        public string ProjectId { get; set; }
        public List<string> SourcePaths { get; set; }
        public override IEnumerable<ValidationResult> Validate()
        {
            var validationResultList = new List<ValidationResult>();

            if (string.IsNullOrEmpty(this.ProjectId))
                validationResultList.Add(new ValidationResult("Project id can not be Empty"));

            return validationResultList;
        }
    }

    public class GetPartialPagesDetailsResponseModel
    {
        public List<ResourceDetails> Resources { get; set; }
    }

    public class GetProjectWithResourcesDetailsRequestModel : BaseModel
    {
        public string ProjectId { get; set; }
        public override IEnumerable<ValidationResult> Validate()
        {
            var validationResultList = new List<ValidationResult>();
            if (!string.IsNullOrEmpty(this.ProjectId))
                this.ProjectId = this.ProjectId.Trim();
            else if (string.IsNullOrEmpty(this.ProjectId))
                validationResultList.Add(new ValidationResult("Project id can not be Empty"));

            return validationResultList;
        }
    }
    public class GetProjectWithResourcesDetailsResponseModel : MongoEntity
    {
        public string UserEmail { get; set; }
        public string ProjectName { get; set; }
        public string ProjectId { get; set; }
        public int Version { get; set; }
        public IList<ResourceDetails> Resources { get; set; }
        public ProjectStatus ProjectStatus { get; set; }
        public string FaviconIconUrl { get; set; }
        public ProjectType ProjectType { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string SchemaId { get; set; }
        public BucketNames BucketNames { get; set; }
        public DateTime ArchivedOn { get; set; }
        public bool IsArchived { get; set; }
        public string ScreenShotUrl { get; set; }
        public int PublishedVersion { get; set; }
    }

    public class DeleteProjectRequestModel : BaseModel
    {
        public string ProjectId { get; set; }
        public override IEnumerable<ValidationResult> Validate()
        {
            var validationResultList = new List<ValidationResult>();
            if (!string.IsNullOrEmpty(this.ProjectId))
                this.ProjectId = this.ProjectId.Trim();
            else if (string.IsNullOrEmpty(this.ProjectId))
                validationResultList.Add(new ValidationResult("ProjectId can not be Empty"));
            else if (string.IsNullOrEmpty(this.UserEmail))
                validationResultList.Add(new ValidationResult("UserEmail can not be Empty"));

            return validationResultList;
        }
    }

    public class DeleteResourceRequestModel : BaseModel
    {
        public string ProjectId { get; set; }
        public string SourcePath { get; set; }
        public string SourceName { get; set; }
        public override IEnumerable<ValidationResult> Validate()
        {
            var validationResultList = new List<ValidationResult>();
            if (!string.IsNullOrEmpty(this.ProjectId))
                this.ProjectId = this.ProjectId.Trim();
            else
                validationResultList.Add(new ValidationResult("ProjectId can not be empty"));

            return validationResultList;
        }
    }

    public class UpdateProjectAccessRequestModel : BaseModel
    {
        public string ProjectId { get; set; }
        public string UserName { get; set; }
        public override IEnumerable<ValidationResult> Validate()
        {
            var validationResultList = new List<ValidationResult>();

            if (!string.IsNullOrEmpty(this.ProjectId))
                this.ProjectId = this.ProjectId.Trim();
            else
                validationResultList.Add(new ValidationResult("ProjectId can not be empty"));
            return validationResultList;
        }
    }

    public class CreateOrUpdateKitsuneStatusRequestModel : BaseModel
    {
        public string ProjectId { get; set; }
        public BuildStatus Stage { get; set; }
        public Dictionary<string, int> Analyzer { get; set; }
        public Dictionary<string, int> Optimizer { get; set; }
        public Dictionary<string, int> Compiler { get; set; }
        public Dictionary<string, int> Replacer { get; set; }
        public List<BuildError> Error { get; set; }
        public override IEnumerable<ValidationResult> Validate()
        {
            var validationResultList = new List<ValidationResult>();
            if (string.IsNullOrEmpty(this.ProjectId))
                validationResultList.Add(new ValidationResult("ProjectId can not be Empty"));

            return validationResultList;
        }
    }

    public class GetKitsuneBuildStatusRequestModel : BaseModel
    {
        public string ProjectId { get; set; }
        public int BuildVersion { get; set; }
        public override IEnumerable<ValidationResult> Validate()
        {
            var validationResultList = new List<ValidationResult>();
            if (string.IsNullOrEmpty(this.ProjectId))
                validationResultList.Add(new ValidationResult("ProjectId can not be Empty"));

            return validationResultList;
        }
    }

    public class GetKitsuneBuildStatusResponseModel
    {
        public string ProjectId { get; set; }
        public int BuildVersion { get; set; }
        public BuildStatus Stage { get; set; }
        public bool IsCompleted { get; set; }
        public Dictionary<string, int> Analyzer { get; set; }
        public Dictionary<string, int> Optimizer { get; set; }
        public Dictionary<string, int> Compiler { get; set; }
        public Dictionary<string, int> Replacer { get; set; }
        public List<BuildError> Error { get; set; }
        public bool FirstBuild { get; set; }
        public DateTime CreatedOn { get; set; }
    }

    public class ProjectDetailsForBuildRequestModel : BaseModel
    {
        public string ProjectId { get; set; }
        public override IEnumerable<ValidationResult> Validate()
        {
            var validationResultList = new List<ValidationResult>();
            if (string.IsNullOrEmpty(this.ProjectId))
                validationResultList.Add(new ValidationResult("ProjectId can not be Empty"));

            return validationResultList;
        }
    }

    public class BaseBuildMetaData
    {
        public int LINK { get; set; }
        public int SCRIPT { get; set; }
        public int STYLE { get; set; }
        public int FILE { get; set; }
    }

    public class ProjectDetailsForBuildResponseModel
    {
        public BaseBuildMetaData Total { get; set; }
        public BaseBuildMetaData Modified { get; set; }
    }

    public class BuildStatsCountModel
    {
        public string _id { get; set; }
        public int count { get; set; }
    }

    public class MakeProjectLiveRequestModel : BaseModel
    {
        public string ProjectId { get; set; }               //For old publish FLow
        public string WebsiteId { get; set; }              //For old publish FLow
        public override IEnumerable<ValidationResult> Validate()
        {
            var validationResultList = new List<ValidationResult>();
            if (String.IsNullOrEmpty(this.ProjectId))
                validationResultList.Add(new ValidationResult("ProjectId can not be Empty"));
            if (String.IsNullOrEmpty(this.WebsiteId))
                validationResultList.Add(new ValidationResult("CustomerId can not be Empty"));
            return validationResultList;
        }
    }

    public class MakeProjectLiveV2RequestModel : BaseModel
    {
        public string PublishStatsId { get; set; }
        public override IEnumerable<ValidationResult> Validate()
        {
            var validationResultList = new List<ValidationResult>();
            if (String.IsNullOrEmpty(this.PublishStatsId))
                validationResultList.Add(new ValidationResult("PublishStatsId can not be Empty"));
            return validationResultList;
        }
    }

    public class MakeProjectLiveResponseModel
    {
        public string message { get; set; }
        public string status { get; set; }
    }

    public class PublishCustomerRequestModel : BaseModel
    {
        public string WebsiteId { get; set; }
        public bool CopyFromDemoWebsite { get; set; }
        public string CopyFromWebsiteId { get; set; }
        public bool PublishToAll { get; set; }
        public string ProjectId { get; set; }

        public override IEnumerable<ValidationResult> Validate()
        {
            var validationResultList = new List<ValidationResult>();
            if (string.IsNullOrEmpty(this.WebsiteId) && !PublishToAll)
                validationResultList.Add(new ValidationResult("WebsiteId can not be Empty"));

            return validationResultList;
        }
    }
    public class PublishCustomerResponseModel : ErrorApiResponseModel
    {
        public string CNAMERecord { get; set; }
        public string ARecord { get; set; }
        public bool DomainVerified { get; set; }
        public string Domain { get; set; }
    }

    public class PublishProjectRequestModel : BaseModel
    {
        public List<string> CustomerIds { get; set; }
        public string ProjectId { get; set; }
        public bool PublishToAll { get; set; }
        public override IEnumerable<ValidationResult> Validate()
        {
            var validationResultList = new List<ValidationResult>();
            if (String.IsNullOrEmpty(this.ProjectId))
                validationResultList.Add(new ValidationResult("ProjectId can not be Empty"));
            if (String.IsNullOrEmpty(this.UserEmail))
                validationResultList.Add(new ValidationResult("UserEmail can not be Empty"));

            return validationResultList;
        }
    }

    public class PublishProjectResponseModel : ErrorApiResponseModel
    {

    }

    public class DownloadProjectRequestModel : BaseModel
    {
        public string ProjectId { get; set; }
        public override IEnumerable<ValidationResult> Validate()
        {
            var validationResultList = new List<ValidationResult>();
            if (String.IsNullOrEmpty(this.ProjectId))
                validationResultList.Add(new ValidationResult("ProjectId can not be Empty"));
            if (String.IsNullOrEmpty(this.UserEmail))
                validationResultList.Add(new ValidationResult("UserEmail can not be Empty"));

            return validationResultList;
        }
    }

    public class DownloadProjectResponseModel : ErrorApiResponseModel
    {
        public string DownloadUrl { get; set; }
        public TaskDownloadQueueStatus Status { get; set; }
    }



    public class KitsuneProjectPublishDetails
    {
        public ProjectStatus ProjectStatus { get; set; }
        public int Version { get; set; }
        public string SchemaId { get; set; }
    }


    public class KitsuneEnquiryRequestModel : BaseModel
    {
        public string EmailBody { get; set; }
        public string Name { get; set; }
        public string Subject { get; set; }
        public string From { get; set; }
        public int Type { get; set; }
        public override IEnumerable<ValidationResult> Validate()
        {
            var validationResultList = new List<ValidationResult>();
            if (!(string.IsNullOrEmpty(this.EmailBody) || string.IsNullOrEmpty(this.Name) || string.IsNullOrEmpty(this.From)))
                validationResultList.Add(new ValidationResult("Please provide Email body, Name and From email"));

            return validationResultList;
        }

    }


    public class GetAuditProjectAndResourcesDetailsRequestModel : BaseModel
    {
        public string ProjectId { get; set; }
        public int Version { get; set; }
        public override IEnumerable<ValidationResult> Validate()
        {
            var validationResultList = new List<ValidationResult>();
            if (!string.IsNullOrEmpty(this.ProjectId))
                this.ProjectId = this.ProjectId.Trim();
            else
                validationResultList.Add(new ValidationResult("ProjectId can not be empty"));
            if (this.Version == 0)
                validationResultList.Add(new ValidationResult("Version can not be 0"));
            return validationResultList;
        }
    }

    public class GetAuditProjectAndResourcesDetailsResponseModel
    {
        public AuditKitsuneProject Project { get; set; }
        public List<AuditKitsuneResource> Resources { get; set; }
    }

    public class CreateAuditProjectRequestModel : BaseModel
    {
        public AuditKitsuneProject Project { get; set; }
        public List<AuditKitsuneResource> Resources { get; set; }

        public override IEnumerable<ValidationResult> Validate()
        {
            var validationResultList = new List<ValidationResult>();
            if (this?.Project == null)
                validationResultList.Add(new ValidationResult("Project is null"));
            if (this?.Resources == null)
                validationResultList.Add(new ValidationResult("Resource is null"));
            if (!this.Resources.Any())
                validationResultList.Add(new ValidationResult("Project doesn't have any Resource"));
            return validationResultList;
        }
    }

    public class CreateProductionProjectRequestModel : BaseModel
    {
        public ProductionKitsuneProject Project { get; set; }
        public List<ProductionKitsuneResource> Resources { get; set; }
        public override IEnumerable<ValidationResult> Validate()
        {
            var validationResultList = new List<ValidationResult>();
            if (this?.Project == null)
                validationResultList.Add(new ValidationResult("Project is null"));
            if (this?.Resources == null)
                validationResultList.Add(new ValidationResult("Resource is null"));
            if (!this.Resources.Any())
                validationResultList.Add(new ValidationResult("Project doesn't have any Resource"));
            return validationResultList;
        }
    }
    public class DeleteProductionProjectRequestModel : BaseModel
    {
        public string ProjectId { get; set; }
        public override IEnumerable<ValidationResult> Validate()
        {
            var validationResultList = new List<ValidationResult>();
            if (string.IsNullOrEmpty(this.ProjectId))
                validationResultList.Add(new ValidationResult("ProjectId can not be Empty"));

            return validationResultList;
        }
    }
    public class GetProductionProjectDetailsRequestModel : BaseModel
    {
        public string ProjectId { get; set; }
        public string ClientId { get; set; }
        public bool IncludeResources { get; set; }
        public override IEnumerable<ValidationResult> Validate()
        {
            var validationResultList = new List<ValidationResult>();
            if (string.IsNullOrEmpty(this.ProjectId))
                validationResultList.Add(new ValidationResult("ProjectId can not be Empty"));

            return validationResultList;
        }
    }

    public class GetProductionProjectDetailsResponseModel : BaseModel
    {
        public ProductionKitsuneProject Project { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<ProductionKitsuneResource> Resources { get; set; }
        public override IEnumerable<ValidationResult> Validate()
        {
            var validationResultList = new List<ValidationResult>();
            if (this?.Project == null)
                validationResultList.Add(new ValidationResult("Project is null"));
            if (this?.Resources == null)
                validationResultList.Add(new ValidationResult("Resource is null"));
            if (!this.Resources.Any())
                validationResultList.Add(new ValidationResult("Project doesn't have any Resource"));
            return validationResultList;
        }
    }
    public class RenameResourceModel : BaseModel
    {
        public string ProjectId { get; set; }
        public string OldSourceName { get; set; }
        public string NewSourceName { get; set; }
        public override IEnumerable<ValidationResult> Validate()
        {
            var validationResultList = new List<ValidationResult>();
            if (string.IsNullOrEmpty(this.ProjectId))
                validationResultList.Add(new ValidationResult("ProjectId can not be Empty"));
            if (string.IsNullOrEmpty(this.OldSourceName))
                validationResultList.Add(new ValidationResult("OldSourceName can not be Empty"));
            if (string.IsNullOrEmpty(this.NewSourceName))
                validationResultList.Add(new ValidationResult("NewSourceName can not be Empty"));

            return validationResultList;
        }

    }
    public class CreateOrUpdateResourceRequestModel : BaseModel
    {
        public string ProjectId { get; set; }
        public string SourcePath { get; set; }
        public KitsunePageType? PageType { get; set; }
        public string ClassName { get; set; }
        public string UrlPattern { get; set; }
        public string UrlPatternRegex { get; set; }
        public bool IsDefault { get; set; }
        public bool IsStatic { get; set; }
        public string FileContent { get; set; }
        public byte[] ByteArrayStream { get; set; }
        public IList<CompilerError> Errors { get; set; }
        public string KObject { get; set; }
        public ResourceType? ResourceType { get; set; }
        public string Offset { get; set; }
        public Dictionary<string, int> CustomVariables { get; set; }
        public Dictionary<string, object> Configuration { get; set; }
        public override IEnumerable<ValidationResult> Validate()
        {
            var validationResultList = new List<ValidationResult>();

            if (!string.IsNullOrEmpty(this.SourcePath))
                this.SourcePath = this.SourcePath.Trim();
            else
                validationResultList.Add(new ValidationResult("SourcePath can not be empty"));

            return validationResultList;
        }

    }


    public class AssetChildren
    {
        public string name { get; set; }
        public List<AssetChildren> children { get; set; }
        public bool? toggled { get; set; }

        public string Path { get; set; }
        public bool IsKitsune { get; set; }
    }

    public class ResourceItemMeta
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }
        public string SourcePath { get; set; }
        public string OptimizedPath { get; set; }
        public string ClassName { get; set; }
        public string UrlPattern { get; set; }
        public bool IsStatic { get; set; }
        public bool IsDefault { get; set; }
        public string KObject { get; set; }
        public KitsunePageType PageType { get; set; }
        public int Version { get; set; }
        public IEnumerable<CompilerError> Errors { get; set; }
        public string ProjectId { get; set; }
        public string UserEmail { get; set; }
        public string UrlPatternRegex { get; set; }
        public ResourceType ResourceType { get; set; }
        public DateTime UpdatedOn { get; set; }
        public DateTime CreatedOn { get; set; }
        public MetaData MetaData { get; set; }
        public string Offset { get; set; }
        public Dictionary<string, int> CustomVariables { get; set; }

    }

    public class KitsuneProjectItem
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }
        public string ProjectId { get; set; }
        public string ProjectName { get; set; }
        public int Version { get; set; }
        public DateTime UpdatedOn { get; set; }
        public ProjectStatus ProjectStatus { get; set; }
        public DateTime ArchivedOn { get; set; }
        public bool IsArchived { get; set; }
        public string ScreenShotUrl { get; set; }
        public int PublishedVersion { get; set; }
        public string UserEmail { get; set; }
        public string FaviconIconUrl { get; set; }
        public ProjectType ProjectType { get; set; }
        public string SchemaId { get; set; }
        public BucketNames BucketNames { get; set; }
        public List<ProjectComponent> Components { get; set; }
        public DateTime CreatedOn { get; set; }
        public int CompilerVersion { get; set; }
        public string ClientId { get; set; }
    }
    public class InvalidateCacheRequestModel
    {
        public List<string> PathList { get; set; }
    }
    public class SaveFileContentToS3RequestModel
    {
        public string ProjectId { get; set; }
        public string BucketName { get; set; }
        public string SourcePath { get; set; }
        public string FileContent { get; set; }
        public bool Compiled { get; set; }
        public int Version { get; set; }
        public bool base64 { get; set; }
        public string ClientId { get; set; }
    }
    public class GetFileFromS3RequestModel
    {
        public string ProjectId { get; set; }
        public string BucketName { get; set; }
        public string SourcePath { get; set; }
        public bool Compiled { get; set; }
        public int Version { get; set; }
        public string ClientId { get; set; }
    }

    public class PublishedWebsite : MongoEntity
    {
        public string WebsiteUrl { get; set; }

    }
    public class WebsiteUserDetails : MongoEntity
    {
        public ContactDetails Contact { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
    }

    public class GetPageDataRequest : BaseModel
    {
        public string ProjectId { get; set; }
        public string SourcePath { get; set; }
        public string WebsiteId { get; set; }
        public string UserId { get; set; }
        public string SchemaName { get; set; }
        public string CurrentPageNumber { get; set; }
        public override IEnumerable<ValidationResult> Validate()
        {
            List<ValidationResult> validationResultList = base.Validate()?.ToList();
            if (validationResultList == null)
                validationResultList = new List<ValidationResult>();

            if (String.IsNullOrEmpty(this.ProjectId))
                validationResultList.Add(new ValidationResult("Project id can not be empty"));
            if (String.IsNullOrEmpty(this.SourcePath))
                validationResultList.Add(new ValidationResult("Source path can not be empty"));
            if (String.IsNullOrEmpty(this.WebsiteId))
                validationResultList.Add(new ValidationResult("Website id can not be empty"));
            if (String.IsNullOrEmpty(this.UserId))
                validationResultList.Add(new ValidationResult("User id can not be empty"));
            if (String.IsNullOrEmpty(this.SchemaName))
                validationResultList.Add(new ValidationResult("Schema name can not be empty"));

            return validationResultList;
        }
    }


    # region IDE related api models
    public class IdeGetProjectDetailsRequestModel : BaseModel
    {
        public string ProjectId { get; set; }
        public bool ExcludeResources { get; set; }

        public override IEnumerable<ValidationResult> Validate()
        {
            List<ValidationResult> validationResultList = base.Validate()?.ToList();
            if (validationResultList == null)
                validationResultList = new List<ValidationResult>();

            if (string.IsNullOrEmpty(ProjectId))
                validationResultList.Add(new ValidationResult("ProjectId can not be empty"));

            return validationResultList;
        }
    }
    public class IdeGetProjectDetailsResponseModel : KitsuneProjectItem
    {
        public DateTime LastPublishedOn { get; set; }
        public AssetChildren Assets { get; set; }
    }
    public class SubmitRequest: BaseModel
    {
        public string ProjectId { get; set; }
        public string ProjectName { get; set; }
        public IEnumerable<string> Categories { get; set; }
        public string FileContent { get; set; }
        public string UrlPattern { get; set; }
        public bool IsDev { get; set; }
        public bool IsStatic { get; set; }
        public bool IsDefault { get; set; }
        public string DefaultProjectId { get; set; }
        public string SourcePath { get; set; }
        public string ClassName { get; set; }
        public string KObject { get; set; }
        public bool IsPublish { get; internal set; }
        public KitsunePageType PageType { get; set; }
        public ProjectStatus ProjectStatus { get; set; }
        public ResourceType? ResourceType { get; set; }
        public bool IsPreview { get; set; }
        public override IEnumerable<ValidationResult> Validate()
        {
            List<ValidationResult> validationResultList = base.Validate()?.ToList();
            if (validationResultList == null)
                validationResultList = new List<ValidationResult>();

            if (string.IsNullOrEmpty(this.UserEmail))
                validationResultList.Add(new ValidationResult("UserEmail can not be empty"));
            if (string.IsNullOrEmpty(this.SourcePath))
                validationResultList.Add(new ValidationResult("SourcePath can not be empty"));

            return validationResultList;
        }
    }

    public class GetProjectInProcessRequestModel: BaseModel
    {
        public override IEnumerable<ValidationResult> Validate()
        {
            List<ValidationResult> validationResultList = base.Validate()?.ToList();
            if (validationResultList == null)
                validationResultList = new List<ValidationResult>();

            if (string.IsNullOrEmpty(this.UserEmail))
                validationResultList.Add(new ValidationResult("UserEmail can not be empty"));

            return validationResultList;
        }
    }
    public class GetProjectInProcessResponseModel
    {
        public List<ProjectInProcessStatus> Projects { get; set; }
    }
    public class GetProjectInProcessDetail
    {
        public string ProjectId { get; set; }
        public ProjectStatus ProjectStatus { get; set; }
        public int Version { get; set; }

    }
    public class ProjectInProcessStatus
    {
        public string ProjectId { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public ProjectStatus ProjectStatus { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public BuildStatus? BuildStage { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public KitsuneKrawlerStatusCompletion? CrawlStage { get; set; }
    }

    public class GetProjectUpdatedOrNotRequest : BaseModel
    {
        public string ProjectId { get; set; }
        public override IEnumerable<ValidationResult> Validate()
        {
            List<ValidationResult> validationResultList = base.Validate()?.ToList();
            if (validationResultList == null)
                validationResultList = new List<ValidationResult>();

            if(String.IsNullOrEmpty(this.ProjectId))
                validationResultList.Add(new ValidationResult("ProjectId can not be empty"));

            return validationResultList;
        }
    }

    public class ProjectUpdatedOrNotResponse:ErrorApiResponseModel
    {
        public bool ProjectUpdated { get; set; }
    }
    public class GetResourceMetaInfoRequest : BaseModel
    {
        public string ProjectId { get; set; }
        public string SourcePath { get; set; }
        public override IEnumerable<ValidationResult> Validate()
        {
            List<ValidationResult> validationResultList = base.Validate()?.ToList();
            if (validationResultList == null)
                validationResultList = new List<ValidationResult>();

            if (String.IsNullOrEmpty(this.ProjectId))
                validationResultList.Add(new ValidationResult("ProjectId can not be empty"));
            if (String.IsNullOrEmpty(this.SourcePath))
                validationResultList.Add(new ValidationResult("SourcePath can not be empty"));

            return validationResultList;
        }
    }
    #endregion

    #region Optimization Reports

    public class OptimizedPercentageRequestModel : BaseModel
    {
        public string ProjectId { get; set; }
        public override IEnumerable<ValidationResult> Validate()
        {
            List<ValidationResult> validationResultList = base.Validate()?.ToList();
            if (validationResultList == null)
                validationResultList = new List<ValidationResult>();

            if (string.IsNullOrEmpty(this.ProjectId))
                validationResultList.Add(new ValidationResult("ProjectId can not be empty"));

            return validationResultList;
        }
    }
    public class OptimizedPercentageResponseModel
    {
        public string Message { get; set; }
        public bool Success { get; set; }
        public int NextTick { get; set; }
    }

    public class WordPressProjectStatusRequestModel : BaseModel
    {
        public string ProjectId { get; set; }
        public override IEnumerable<ValidationResult> Validate()
        {
            List<ValidationResult> validationResultList = base.Validate()?.ToList();
            if (validationResultList == null)
                validationResultList = new List<ValidationResult>();

            if (string.IsNullOrEmpty(this.ProjectId))
                validationResultList.Add(new ValidationResult("ProjectId can not be empty"));

            return validationResultList;
        }
    }
    public class WordPressProjectStatusResponseModel
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public KitsuneWordPressStats? Stage { get; set; }
        public string WordpressUser { get; set; }
        public string WordpressPassword { get; set; }
        public bool? isScheduled { get; set; }
        public string Domain { get; set; }
    }

    #endregion
    #region WebformHtml
    public class WebFormUpdateRequestModel
    {
        public string JsonString { get; set; }
        public string WebFormId { get; set; }
        public string UserId { get; set; }

        public IEnumerable<ValidationResult> Validate()
        {
            if (string.IsNullOrEmpty(this.JsonString))
                yield return new ValidationResult("JSON string can not be empty");
            if (string.IsNullOrEmpty(this.UserId))
                yield return new ValidationResult("UserId can not be empty");
        }
    }
    #endregion

    #region Project Configuration

    public class ConfigFile
    {
        public string ContentType { get; set; }
        public string Content { get; set; }
    }

    public enum FileLevel { SOURCE,DEMO,PROD};

    public class CreateOrUpdateProjectConfigRequestModel : BaseModel
    {
        public string ProjectId { get; set; }
        public ConfigFile File { get; set; }
        public override IEnumerable<ValidationResult> Validate()
        {
            var validationResultList = new List<ValidationResult>();

            if (string.IsNullOrEmpty(this.ProjectId))
                validationResultList.Add(new ValidationResult("ProjectId Required"));
            if(this.File==null)
                validationResultList.Add(new ValidationResult("File object cannot be empty"));
            if (string.IsNullOrEmpty(this.File.Content))
                validationResultList.Add(new ValidationResult("file Content cannot be Empty"));

            return validationResultList;
        }
    }

    public class GetProjectConfigRequestModel : BaseModel
    {
        public string ProjectId { get; set; }
        public string ClientId { get; set; }
        public FileLevel Level { get; set; }
        public override IEnumerable<ValidationResult> Validate()
        {
            var validationResultList = new List<ValidationResult>();

            if (string.IsNullOrEmpty(this.ProjectId))
                validationResultList.Add(new ValidationResult("ProjectId Required"));

            return validationResultList;
        }
    }

    public class GetProjectConfigResponseModel
    {
        public ConfigFile File { get; set; }
    }

    public class ValidateConfigRequestModel : BaseModel
    {
        public ConfigFile File { get; set; }
        public override IEnumerable<ValidationResult> Validate()
        {
            var validationResultList = new List<ValidationResult>();

            if(File==null)
                validationResultList.Add(new ValidationResult("File required"));
            if(String.IsNullOrEmpty(File.Content))
                validationResultList.Add(new ValidationResult("File Content required"));
            return validationResultList;
        }
    }

    public class ValidateConfigResponseModel
    {
        public bool IsError { get; set; }
        public List<BuildError> Error { get; set; }
    }

    #endregion

    public class KitsuneAppProjectsResponse
    {
        public Pagination Pagination { get; set; }
        public IEnumerable<KitsuneProject> Projects { get; set; }
    }

    public class KitsuneAppStatusResponse : ErrorApiResponseModel
    {
        public bool IsActive { get; set; }
    }
    public class VMNAssignResponse
    {
        public string virtualNumber { get; set; }
        public string primaryNumber { get; set; }
    }
    public class VMNFetchResponse
    {
        public string CreatedOn { get; set; }
        public string HealthLastVerified { get; set; }
        public string virtualNumberISDCode { get; set; }
        public string primaryNumber { get; set; }
        public bool isNfStoreFront { get; set; }
        public string virtualNumberSource { get; set; }
        public string fpId { get; set; }
        public string fpTag { get; set; }
        public string _id { get; set; }
        public string customerId { get; set; }
        public string virtualNumber { get; set; }
        public bool isActive { get; set; }
    }

    public class callTrackerConfigModel
    {
        public List<string> domain { get; set; }
    }

    public class CloudProviderModel
    {
        public CloudProvider provider;
        public string accountId;
        public string key;
        public string region;
        public string secret;
        public string credentialsFile;

        public IEnumerable<ValidationResult> Validate()
        {
            switch(provider)
            {
                case CloudProvider.AliCloud:
                    if (string.IsNullOrEmpty(this.accountId))
                        yield return new ValidationResult("accountId can not be empty");
                    if (string.IsNullOrEmpty(this.key))
                        yield return new ValidationResult("key can not be empty");
                    if (string.IsNullOrEmpty(this.region))
                        yield return new ValidationResult("region can not be empty");
                    if (string.IsNullOrEmpty(this.secret))
                        yield return new ValidationResult("secret can not be empty");
                    break;
                case CloudProvider.GCP:
                    if (string.IsNullOrEmpty(this.credentialsFile))
                        yield return new ValidationResult("credentialsFile can not be empty");
                    break;
            }
        }
    }

}
