using Kitsune.Models.Krawler;
using Kitsune.Models.Project;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
//using NewtonSoft.Json;

namespace Kitsune.API.Model.ApiRequestModels
{
    public class ProjectResources
    {
        public List<AssetDetails> Links { get; set; }
        public List<AssetDetails> Styles { get; set; }
        public List<AssetDetails> Assets { get; set; }
        public List<AssetDetails> Scripts { get; set; }
    }

    public class UpdateKitsuneProjectStatusRequestModel : BaseModel
    {
        public string ProjectId { get; set; }
        public ProjectStatus ProjectStatus { get; set; }

        public override IEnumerable<ValidationResult> Validate()
        {
            var validationResultList = new List<ValidationResult>();

            if (string.IsNullOrEmpty(this.ProjectId))
                validationResultList.Add(new ValidationResult("ProjectId can not be empty"));

            return validationResultList;
        }
    }

    public class UpdateWebsiteDetailsRequestModel : BaseModel
    {
        public string ProjectId { get; set; }
        public string FaviconUrl { get; set; }
        public string ScreenShotUrl { get; set; }

        public override IEnumerable<ValidationResult> Validate()
        {
            var validationResultList = new List<ValidationResult>();

            if (string.IsNullOrEmpty(this.ProjectId))
                validationResultList.Add(new ValidationResult("CustomerId can not be empty"));
            if (String.IsNullOrEmpty(this.FaviconUrl) && String.IsNullOrEmpty(this.ScreenShotUrl))
                validationResultList.Add(new ValidationResult("Both FaviconIcon and ScreenshotUrl can not be empty"));

            return validationResultList;
        }
    }

    public class StartKrawlRequestModel : BaseModel
    {
        public bool IsDeepKrawl { get; set; }
        public string Url { get; set; }
        public string ClientId { get; set; }
        public string ProjectName { get; set; }
        public override IEnumerable<ValidationResult> Validate()
        {
            var validationResultList = new List<ValidationResult>();

            if (string.IsNullOrEmpty(this.Url))
                validationResultList.Add(new ValidationResult("Url can not be empty"));
            if(string.IsNullOrEmpty(this.UserEmail))
                validationResultList.Add(new ValidationResult("UserEmail can not be empty"));
            return validationResultList;
        }
    }
    
    public class StartKrawlResponseModel : ErrorApiResponseModel
    {
        public string ProjectId { get; set; }
    }


    public class StopCrawlRequestModel : BaseModel
    {
        public bool StopCrawl { get; set; }
        public int? LinksLimit { get; set; }
        public string ProjectId { get; set; }
        public override IEnumerable<ValidationResult> Validate()
        {
            var validationResultList = new List<ValidationResult>();

            if (!this.StopCrawl && this.LinksLimit == null)
                validationResultList.Add(new ValidationResult("Please, specify atleast one field"));
            if(String.IsNullOrEmpty( this.ProjectId))
                validationResultList.Add(new ValidationResult("ProjectId cannot be Null"));
            return validationResultList;
        }
    }

    public class StopCrawlResponseModel
    {
        public string Message { get; set; } 
    }

    public class ReCrawlRequestModel : BaseModel
    {
        public string ProjectId { get; set; }
        public string Url { get; set; }
        public override IEnumerable<ValidationResult> Validate()
            {
            var validationResultList = new List<ValidationResult>();

            if (string.IsNullOrEmpty(this.ProjectId))
                validationResultList.Add(new ValidationResult("ProjectId can not be empty"));
            if(string.IsNullOrEmpty(this.Url))
                validationResultList.Add(new ValidationResult("Url can not be empty"));

            return validationResultList;
        }
    }

    public class ReCrawlProjectProjection
    {
        public string ProjectId { get; set; }
        public ProjectStatus ProjectStatus { get; set; }
    }

    public class KrawlingCompletedRequestModel : BaseModel
    {
        public string ProjectId { get; set; }
        public override IEnumerable<ValidationResult> Validate()
        {
            var validationResultList = new List<ValidationResult>();

            if (string.IsNullOrEmpty(this.ProjectId))
                validationResultList.Add(new ValidationResult("ProjectId can not be empty"));

            return validationResultList;
        }
    }

    public class GetAnalyseDetailsRequestModel : BaseModel
    {
        public string ProjectId { get; set; }
        public override IEnumerable<ValidationResult> Validate()
        {
            var validationResultList = new List<ValidationResult>();

            if (string.IsNullOrEmpty(this.ProjectId))
                validationResultList.Add(new ValidationResult("ProjectId can not be empty"));

            return validationResultList;
        }
    }

    public class AnalyseDetailsResponseModel
    {
        public KitsuneKrawlerStatusCompletion Stage { get; set; }
        public int LinksFound { get; set; }
        public int StylesFound { get; set; }
        public int ScriptsFound { get; set; }
        public int AssetsFound { get; set; }
    }

    public class FilesDownloadDetailsRequestModel : BaseModel
    {
        public string ProjectId { get; set; }
        public override IEnumerable<ValidationResult> Validate()
        {
            var validationResultList = new List<ValidationResult>();

            if (string.IsNullOrEmpty(this.ProjectId))
                validationResultList.Add(new ValidationResult("ProjectId can not be empty"));

            return validationResultList;
        }
    }

    public class FilesDownloadDetailsResponseModel
    {
        public KitsuneKrawlerStatusCompletion Stage { get; set; }
        public int ScriptsDownloaded { get; set; }
        public int StylesDownloaded { get; set; }
        public int AssetsDownloaded { get; set; }
        public int StylesFound { get; set; }
        public int ScriptsFound { get; set; }
        public int AssetsFound { get; set; }
    }

    public class GetNumberOfLinksReplacedRequestModel : BaseModel
    {
        public string ProjectId { get; set; }
        public override IEnumerable<ValidationResult> Validate()
        {
            var validationResultList = new List<ValidationResult>();

            if (string.IsNullOrEmpty(this.ProjectId))
                validationResultList.Add(new ValidationResult("ProjectId can not be empty"));

            return validationResultList;
        }
    }

    public class GetNumberOfLinksReplacedResponseModel
    {
        public int LinksReplaced { get; set; }
        public int LinksFound { get; set; }
        //[JsonProperty("Status")]
        public KitsuneKrawlerStatusCompletion Stage { get; set; }
    }

    public class ListOfDomainsFoundRequestModel : BaseModel
    {
        public string ProjectId { get; set; }
        public override IEnumerable<ValidationResult> Validate()
        {
            var validationResultList = new List<ValidationResult>();

            if (string.IsNullOrEmpty(this.ProjectId))
                validationResultList.Add(new ValidationResult("ProjectId can not be empty"));

            return validationResultList;
        }
    }

    public class GetListOfDomainsResponseModel : ErrorApiResponseModel
    {
        public List<string> DomainList { get; set; }
    }

    public class SaveSelectedDomainRequestModel : BaseModel
    {
        public string ProjectId { get; set; }
        public List<String> Domains { get; set; }
        public override IEnumerable<ValidationResult> Validate()
        {
            var validationResultList = new List<ValidationResult>();

            if (string.IsNullOrEmpty(this.ProjectId))
                validationResultList.Add(new ValidationResult("ProjectId can not be empty"));

            return validationResultList;
        }
    }
}