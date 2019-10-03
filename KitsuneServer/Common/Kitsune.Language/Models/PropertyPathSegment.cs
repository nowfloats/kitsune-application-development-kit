using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kitsune.Language.Models
{
    public class PropertyPathSegment
    {
        public string PropertyName { get; set; }
        public string PropertyDataType { get; set; }
        public PropertyType Type { get; set; }
        public int? Index { get; set; }
        public int? Limit { get; set; }
        public Dictionary<string, int> Sort { get; set; }
        public Dictionary<string, object> Filter { get; set; }
        public Dictionary<string, bool> ObjectKeys { get; set; }
    }
}
