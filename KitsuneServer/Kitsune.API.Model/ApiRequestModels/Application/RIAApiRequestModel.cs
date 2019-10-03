using Kitsune.Models.WebsiteModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kitsune.API.Model.ApiRequestModels.Application
{
    public class GetAllProjectsWithConfigResponseModel
    {
        public IEnumerable<ProjectComponentModel> Projects { get; set; }
        public Pagination Pagination { get; set; }
    }

    public class ProjectComponentModel
    {
        public string ProjectId { get; set; }
        public string ProjectName { get; set; }
        public string UserEmail { get; set; }
        public int Version { get; set; }
        public DateTime UpdatedOn { get; set; }
        public DateTime CreatedOn { get; set; }
        public string SchemaId { get; set; }
        public string Settings { get; set; }
    }

    public class GetLiveWebsiteForProjectRequestModel
    {
        public int Offset { get; set; }
        public int Limit { get; set; }
        public string ProjectId { get; set; }
    }
    public class GetLiveWebsiteForProjectResponseModel
    {
        public List<LiveKitsuneWebsiteDetailsForProject> LiveWebsites { get; set; }

        public Pagination Pagination { get; set; }
    }
    public class LiveKitsuneWebsiteDetailsForProject
    {
        public string WebsiteId { get; set; }
        public string DeveloperId { get; set; }
        public string ProjectId { get; set; }
        public string WebsiteUrl { get; set; }
        public string RootPath { get; set; }
        public string WebsiteTag { get; set; }
        public string ProjectName { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime PublishedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public bool IsActive { get; set; }
        public WebsiteUserDetais WebsiteOwner { get; set; }
    }
    public class WebsiteUserDetais
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string AccessType { get; set; }
        public ContactDetails Contact { get; set; }
        public bool IsActive { get; set; }
        public DateTime LastLoginTimeStamp { get; set; }
        public DateTime UpdatedOn { get; set; }
        public DateTime CreatedOn { get; set; }
    }

}
