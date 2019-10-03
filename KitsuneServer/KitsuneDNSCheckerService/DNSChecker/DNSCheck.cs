using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DnsClient.Protocol;
using DnsClient;

namespace KitsuneDNSCheckerService.DNSChecker
{
    public class DNSCheck
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="customer"></param>
        /// <returns></returns>
        public static bool CheckIfCNAMEMapped(string domain,string cname)
        {
            try
            {
                var client = new LookupClient();
                var result = client.Query(domain,QueryType.CNAME);
                var cnameRecords = result.AllRecords.CnameRecords();
                var record = cnameRecords.FirstOrDefault(
                    x => x.CanonicalName.ToString().TrimEnd('.').Equals(cname,StringComparison.OrdinalIgnoreCase)
                    );
                if(record == null)
                {
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        
    }
}
