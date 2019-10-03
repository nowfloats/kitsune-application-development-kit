using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kitsune.Helper
{
    public class AmazonSQSConstants
    {
        public static string DownloaderSQSUrl { get { return ConfigurationManager.AppSettings["DownloaderSQSUrl"]; } }

        public static string ReportGeneratorSQSUrl { get { return ConfigurationManager.AppSettings["ReportGeneratorSQSUrl"]; } }
    }
}
