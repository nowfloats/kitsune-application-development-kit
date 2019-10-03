using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kitsune.Helper.KitsuneIdentifierConstants
{
    public class KitsuneIdentifierConstant
    {
        public static string KitsuneIdentiferDistributionId { get { return ConfigurationManager.AppSettings["KitsuneIdentifierDistributionId"]; } }

        public static string KitsuneRedirectIPAddress { get { return ConfigurationManager.AppSettings["KitsuneRedirectIPAddress"]; } }

        public static string KitsuneIdentifierKitsuneSubDomain { get { return ConfigurationManager.AppSettings["KitsuneIdentifierKitsuneSubDomain"]; } }

    }
}
