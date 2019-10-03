using KitsuneDNSCheckerService.DNSChecker;
using KitsuneDNSCheckerService.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KitsuneDNSCheckerService.MapDomain
{
    class KitsuneDomainMapper
    {
        private DomainMapperAPI api { get; set; }
        private int Days { get; set; }
        public KitsuneDomainMapper(int days=2)
        {
            try
            {
                api = new DomainMapperAPI();
                Days = days;
            }
            catch(Exception ex)
            {
                //log
            }
        }

        public void Execute()
        {
            try
            {
                if(Program.IsDebug) Console.WriteLine("Started......");
                var projectsList = api.GetKitsuneProjectsWhoseDomainNameNotMapped(Days).Result;

                if (projectsList == null) throw new Exception("Error while getting projects list.");
                if (projectsList.IsError) throw new Exception(projectsList.Message);

                var domains = projectsList.DomainList;
                foreach (var domain in domains)
                {
                    if (Program.IsDebug) Console.WriteLine(String.Format("Domain : {0}, KitsuneUrl : {1}", domain.Domain, domain.KitsuneUrl));
                    try
                    {
                        if (DNSCheck.CheckIfCNAMEMapped(domain.Domain, domain.KitsuneUrl))
                        {
                            var result=api.KituneMapDomain(domain.CustomerId).Result;
                            if (result.IsError) throw new Exception("Error Mapping Domain");

                            if (Program.IsDebug) Console.WriteLine("Mapped");
                            //log Log completed
                        }
                        else
                        {
                            //Not Mapped
                        }
                    }
                    catch (Exception ex)
                    {
                        if (Program.IsDebug) Console.WriteLine("Error:"+ex.Message);
                    }
                }
                if (Program.IsDebug) Console.WriteLine("Completed....");
            }
            catch(Exception ex)
            {

            }
        }
    }
}
