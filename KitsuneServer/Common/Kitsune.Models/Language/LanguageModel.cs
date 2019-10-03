using Kitsune.Language.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kitsune.Models
{
    public class KLanguageModel : MongoEntity
    {
        public KEntity Entity { get; set; }
        public DateTime UpdatedOn { get; set; }
        public bool IsArchived { get; set; }
        public string UserId { get; set; }
        public string ClientId { get; set; }
    }
    public class Link
    {
        public string description;
        public string url;
    }

    public class Image
    {
        public string description;
        public Link actualimage;
        public Link tileimage;
    }
   public class KLanguageModelProd : KLanguageModel
    {
        public string LanguageId { get; set; }
        public int Version { get; set; }
    }
}
