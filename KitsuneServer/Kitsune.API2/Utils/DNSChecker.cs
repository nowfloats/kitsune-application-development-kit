using DnsClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kitsune.API2.Utils
{
    public class DNSChecker
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="customer"></param>
        /// <returns></returns>
        public static bool CheckIfCNAMEMapped(string domain, List<string> cnames)
        {
            try
            {
                var client = new LookupClient();
                cnames = cnames.Select(x => x.ToLower()).ToList();
                var result = client.Query(domain, QueryType.CNAME);
                var cnameRecords = result.AllRecords.CnameRecords();
                var record = cnameRecords
                    .FirstOrDefault(x => cnames.Contains(x.CanonicalName.ToString().TrimEnd('.').ToLower()));

                if (record == null)
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
