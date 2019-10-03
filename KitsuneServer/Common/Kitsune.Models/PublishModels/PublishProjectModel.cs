using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kitsune.Models.PublishModels
{
    public enum PublishProjectStage { STARTED};
    public class PublishProjectModel:MongoEntity
    {
        public string ProjectId { get; set; }
        public int Version { get; set; }
        public bool PublishToAll { get; set; }
        public PublishProjectStage Stage { get; set; }
        public List<string> CustomerIds { get; set; }
    }
}
