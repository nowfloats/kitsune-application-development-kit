using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kitsune.Models.Project
{
    public class KitsuneProject : BaseKitsuneProject
    {
        public DateTime ArchivedOn { get; set; }

        public bool IsArchived { get; set; }

        [BsonIgnoreIfNull]
        public string ScreenShotUrl { get; set; }
        public int PublishedVersion { get; set; }

    }
}
