using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kitsune.Models
{
    public class ActivityLogSQSModel
    {
        public string ResourceId { get; set; }
        public string ActivityId { get; set; }
        public DateTime CreatedOn { get; set; }
        public Dictionary<string,string> Params { get; set; }
    }
}
