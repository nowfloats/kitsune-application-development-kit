using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kitsune.Models.CollectionModels
{
    public enum KitsuneWordPressStats { IDLE = 1, INITIALISING = 2, ERROR = -1 }
    public class KitsuneWordPress : MongoEntity
    {
        public string ProjectId { get; set; }
        [BsonRepresentation(BsonType.String)]
        public KitsuneWordPressStats Stage { get; set; }
        public bool isArchived { get; set; }
        public bool isScheduled { get; set; }
        public string Status { get; set; }
        public string MasterAdmin { get; set; }
        public string MaterPassword { get; set; }
        public string DatabaseUser { get; set; }
        public string DatabaseName { get; set; }
        public string DatabasePassword { get; set; }
        public string WordpressUser { get; set; }
        public string WordpressPassword { get; set; }
        public string ImageId { get; set; }
        public string InstanceId { get; set; }
        public string PublicIP { get; set; }
        public string HostName { get; set; }
        public int Frequency { get; set; }
        public string CloudwatchRule { get; set; }
        public string Domain { get; set; }
    }
}
