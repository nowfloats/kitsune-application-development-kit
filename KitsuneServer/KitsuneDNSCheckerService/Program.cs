using KitsuneDNSCheckerService.DNSChecker;
using KitsuneDNSCheckerService.MapDomain;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KitsuneDNSCheckerService
{
    class Program
    {
        public static bool IsDebug { get { return true; } }
        public static string DaysToCheckDNS { get { return ConfigurationManager.AppSettings["DaysToCheckDNS"]; } }
        static void Main(string[] args)
        {
            try
            {
                int days = Convert.ToInt32(DaysToCheckDNS);
                KitsuneDomainMapper mapper = new KitsuneDomainMapper(days);
                mapper.Execute();
            }
            catch (Exception ex)
            {
                //log
            }
        }
    }
}
