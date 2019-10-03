using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kitsune.Models.Cloud
{
    public class CloudConfiguration
    {
        public string _id;
        public string ClientId;

        public string ProjectId;
        public string DeveloperId;

        public string DomainName;

        public string[] AlternativeCname;
    }

    public class AWSCloudConfiguration : CloudConfiguration
    {
        public string CloudFrontId;
    }

    public class AWSCloudConfigurationAuditLogModel : CloudConfiguration
    {
        public string RequestObject;
        public string ResponseObject;
    }
}
