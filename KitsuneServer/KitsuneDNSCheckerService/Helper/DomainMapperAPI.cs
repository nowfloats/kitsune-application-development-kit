using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KitsuneDNSCheckerService.Helper
{
    public class DomainMapperAPI
    {
        public Task<KitsuneProjectsWhoseDomainNameNotMappedResult> GetKitsuneProjectsWhoseDomainNameNotMapped(int days)
        {
            var response = HttpHelper.GetAsync<KitsuneProjectsWhoseDomainNameNotMappedResult>
                (String.Format(DomainMapperConstants.GetKitsuneProjectsWhoseDomainNotMappedApi, days));
            return response;
        }

        public Task<KitsuneMapDomainResult> KituneMapDomain(string customerId)
        {
            var response = HttpHelper.PostAsync<KitsuneMapDomainResult>
                (String.Format(DomainMapperConstants.KitsuneMapDomain, customerId), null);
            return response;
        }

        public Task<KitsuneCheckAndMapDomainResult> KitsuneCheckAndMapDomain(string customerId)
        {
            var response = HttpHelper.PostAsync<KitsuneCheckAndMapDomainResult>
                (String.Format(DomainMapperConstants.KitsuneCheckAndMapDomain, customerId), null);
            return response;
        }

        public Task<KitsuneUpdateDomainResult> KitsuneUpdateDomain(string customerId, string newDomain)
        {
            var response = HttpHelper.PostAsync<KitsuneUpdateDomainResult>
                (String.Format(DomainMapperConstants.KitsuneUpdateDomain, customerId, newDomain), null);
            return response;
        }
    }
}
