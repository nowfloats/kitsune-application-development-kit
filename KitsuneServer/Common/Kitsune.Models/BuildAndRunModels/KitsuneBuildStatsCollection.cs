using Kitsune.Models.BuildAndRunModels;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kitsune.Models.PublishModels
{
    
    public class BuildError
    {
        public int Line { get; set; }
        public int Column { get; set; }
        public string ErrorStackTrace { get; set; }
        public string Message { get; set; }
        public string SourceMethod { get; set; }
        public string SourcePath { get; set; }
    }

    public class KitsuneBuildStatus:MongoEntity
    {
        public string ProjectId { get; set; }
        public int BuildVersion { get; set; }
        [BsonRepresentation(BsonType.String)]
        public BuildStatus Stage { get; set; }
        public bool IsCompleted { get; set; }
        public Dictionary<string, int> Analyzer { get; set; }
        public Dictionary<string, int> Optimizer { get; set; }
        public Dictionary<string, int> Compiler { get; set; }
        public Dictionary<string, int> Replacer { get; set; }
        public List<BuildError> Error { get; set; }
        public List<BuildError> Warning { get; set; }
    }
}
