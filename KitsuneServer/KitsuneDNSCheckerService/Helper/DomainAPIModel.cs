using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KitsuneDNSCheckerService.Helper
{
    public class KitsuneUpdateDomainResult : Error
    {
        public string KitsuneUrl { get; set; }
        public string Domain { get; set; }
    }
    public class KitsuneCheckAndMapDomainResult : Error
    {
        public bool IsMapped { get; set; }
    }

    public class KitsuneMapDomainResult : Error
    {
    }

    public class KitsuneProjectsWhoseDomainNameNotMappedResult : Error
    {
        public List<DomainNotMapped> DomainList { get; set; }
    }
    public class DomainNotMapped
    {
        public string CustomerId { get; set; }
        public string Domain { get; set; }
        public string KitsuneUrl { get; set; }
    }
    public class Error
    {
        public bool IsError { get; set; }
        public string Message { get; set; }
    }
}
