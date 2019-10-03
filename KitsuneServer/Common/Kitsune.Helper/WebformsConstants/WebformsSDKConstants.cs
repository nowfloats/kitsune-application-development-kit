using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kitsune.Helper.WebformsConstants
{
    public class WebformsSDKConstants
    {
        public static string GetWebforms { get { return "/api/webforms/v0/GetWebforms?emailid={0}"; } }
        public static string CreateWebforms { get { return "/api/webforms/v0/CreateWebforms"; } }
        public static string GetWebformName { get { return "/api/webforms/v0/GetWebfromDetails?name={0}"; } }
        public static string GetWebformActionid { get { return "/api/webforms/v0/GetWebfromDetails?actionid={0}"; } }
        public static string GetWebform { get { return "/api/webforms/v0/GetWebfromDetails?actionid={0}&name={1}"; } }

    }
}
