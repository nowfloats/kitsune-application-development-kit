using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kitsune.Helper
{
    public class CustomerConstants
    {
        public static string KitsuneCreateNewCustomer { get { return "api/CustomerAPI/v1/CreateCustomer"; } }
        public static string KitsuneGetProjectCustomers { get { return "api/CustomerAPI/v1/GetProjectCustomers?crawlId={0}"; } }
        public static string GetCustomersList { get { return "api/CustomerAPI/v1/GetCustomersList?userEmail={0}"; } }
        public static string GetCustomerInformation { get { return "api/CustomerAPI/v1/GetCustomerInformation?userEmail={0}&customerId={1}"; } }
        public static string CustomerDomainPresentOrNot { get { return "api/CustomerAPI/v1/CheckCustomerDomainPresentOrNot?domain={0}"; } }
    }
}
