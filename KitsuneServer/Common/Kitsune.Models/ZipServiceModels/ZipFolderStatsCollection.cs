using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kitsune.Models.ZipServiceModels
{
    public enum TaskDownloadQueueStatus { Started, Completed, Error = -1 };
    public class KitsuneTaskDownloadQueueCollection : MongoEntity
    {
        public string ProjectId { get; set; }
        public string DownloadUrl { get; set; }
        public TaskDownloadQueueStatus Status { get; set; }
        public string Message { get; set; }
        public string UserEmail { get; set; }
        public DateTime CompletedOn { get; set; }
    }
}
