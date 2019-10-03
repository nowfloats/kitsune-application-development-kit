using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kitsune.Helper.KitsuneAdminConstants
{
    public class KitsuneAdminSDKConstants
    {
        public static string ValidateCustomerUserNameAndPassword { get { return "api/k-Admin/v1/ValidateUser"; } }
        public static string UpdateCustomerDetails { get { return "api/k-Admin/v1/UpdateUserDetails"; } }
        public static string ResetCustomerPassword { get { return "api/k-Admin/v1/ResetPassword"; } }
        public static string GetCustomerDetails { get { return "api/k-Admin/v1/GetCustomerDetails?userName={0}"; } }
    }
}
