using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kitsune.Helper
{
    public class DomainMapperConstants
    {
        public static string GetKitsuneProjectsWhoseDomainNotMappedApi { get { return "api/domainmapper/v1/unmappeddomains?days={0}"; } }

        public static string KitsuneMapDomain { get { return "api/domainmapper/v1/mapdomain?customerId={0}"; } }

        public static string KitsuneCheckAndMapDomain { get { return "api/domainmapper/v1/checkandmapdomain?customerId={0}"; } }
        public static string KitsuneUpdateDomain { get { return "api/domainmapper/v1/updatedomain?customerId={0}&newDomain={1}"; } }
    }
}
