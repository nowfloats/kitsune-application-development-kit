using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kitsune.Models.Project
{
    public class KitsuneResource : BaseKitsuneResource
    {
        public bool IsArchived { get; set; }
        [BsonIgnoreIfNull]
        public DateTime? ArchivedOn { get; set; }
    }
}
