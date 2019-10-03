using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace Kitsune.Models.Project
{
    public class BaseKitsuneResource : MongoEntity
    {
        public string ProjectId { get; set; }
        public string SourcePath { get; set; }
        public string UserEmail { get; set; }
        public string OptimizedPath { get; set; }
        [BsonIgnoreIfNull]
        public string ClassName { get; set; }
        [BsonIgnoreIfNull]
        public string UrlPattern { get; set; }
        [BsonIgnoreIfNull]
        public string UrlPatternRegex { get; set; }
        public bool IsDefault { get; set; }
        public int Version { get; set; }
        public bool IsStatic { get; set; }
        [BsonRepresentation(BsonType.String)]
        public ResourceType ResourceType { get; set; }
        [BsonRepresentation(BsonType.String)]
        public KitsunePageType PageType { get; set; }
        public DateTime UpdatedOn { get; set; }
        [BsonIgnoreIfNull]
        public IList<CompilerError> Errors { get; set; }
        public string KObject { get; set; }
        public MetaData MetaData { get; set; }
        public string Offset { get; set; }
        public Dictionary<string, int> CustomVariables { get; set; }
    }
    public class SyntaxTableItem
    {
        public string VariableName { get; set; }
        public string DataPath { get; set; }
        public string Regex { get; set; }
        public string Type { get; set; }
    }

    public class MetaData
    {
        public string Status { get; set; }
        public Dictionary<string, object> Configuration { get; set; }

        public List<String> Keywords { get; set; }
        public List<MultiplePositionProperty> MetaInfo { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

    }
    //TODO : Update we
    public enum KitsunePageType { DEFAULT = 0, LIST = 1, DETAILS = 2, SEARCH = 3, PARTIAL = 4, LARAVEL = 5 }
    public enum ResourceType { LINK = 0, SCRIPT = 1, STYLE = 2, FILE = 3, APPLICATION = 4 }
    public enum LaravelStatus
    {
        EMPTY = 0,
        IMPORTINPROGRESS = 1,
        IMPORTCOMPLETE = 2,
        IMPORTFAIL = -1,
        VALIDATIONINPROGRESS = 3,
        READYFORBUILD = 4,
        VALIDATIONERROR = -2,
        BUILDINPROGRESS = 5,
        BUILDCOMPLETECONTAINERACTIVE = 6,
        BUILDCOMPLETECONTAINERASTOPED = 7,
        BUILDFAILED = -3

    }

}