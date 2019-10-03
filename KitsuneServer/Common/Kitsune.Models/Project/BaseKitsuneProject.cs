using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kitsune.Models.Project
{
    public class BaseKitsuneProject : MongoEntity
    {
        public string ProjectId { get; set; }
        public string ProjectName { get; set; }
        public string UserEmail { get; set; }
        [BsonIgnoreIfNull]
        public string FaviconIconUrl { get; set; }
        public int Version { get; set; }
        [BsonRepresentation(BsonType.String)]
        public ProjectStatus ProjectStatus { get; set; }
        [BsonRepresentation(BsonType.String)]
        public ProjectType ProjectType { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string SchemaId { get; set; }
        public BucketNames BucketNames { get; set; }
        //0 will be the default version of the compiler
        public int CompilerVersion { get; set; }
        //Project extension with other apps
        [BsonIgnoreIfNull]
        public List<ProjectComponent> Components { get; set; }

        public string ClientId { get; set; }
    }
    //Refere the production/published project with version
    /// <summary>
    /// Should be validated before referencing and publishing
    /// </summary>
    public class ProjectComponent
    {
        public string ProjectId { get; set; }
        public string SchemaId { get; set; }
        public int Version { get; set; }
    }

    public class BucketNames
    {
        public string source { get; set; }
        public string demo { get; set; }
        public string placeholder { get; set; }
        public string production { get; set; }
    }

    public enum ProjectStatus
    {
        IDLE = 0,
        PUBLISHING = 1,
        BUILDING = 2,
        CRAWLING = 3,
        QUEUED = 4,
        PUBLISHINGERROR = -1,
        BUILDINGERROR = -2,
        ERROR = -3
    }

    public enum ProjectType
    {
        CRAWL = 0,
        DRAGANDDROP = 1,
        NEWPROJECT = 2,
        WORDPRESS = 3,
        APP = 4,
        TEMPLATE = 5,
        ERROR = -1
    }
}