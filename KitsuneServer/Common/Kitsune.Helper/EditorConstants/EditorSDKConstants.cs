using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kitsune.Helper.EditorConstants
{
    public class EditorSDKConstants
    {
        public static string GetProjectFileList { get { return "api/Editor/v1/CrawlId/{0}/GetProjectFileList"; } }
        public static string GetUserProjectList { get { return "api/Editor/v1/GetUserProjectList?userEmail={0}"; } }
        public static string CheckIfProjectIsPublishing { get { return "api/Editor/v1/CrawlId/{0}/IsProjectPublishing"; } }
        public static string DownloadProject { get { return "api/Conversion/v1/DownloadProject"; } }
        public static string CreateNewProject { get { return "api/Conversion/v1/CreateNewProject"; } }
        public static string GetListOfAllTasks { get { return "api/Conversion/v1/GetListOfAllTasks?userEmail={0}"; } }
        public static string PublishActivatedProject { get { return "api/Editor/v1/CrawlId/PublishProject"; } }
        
        public static string UploadAsset { get { return "api/Project/{0}/ResourceUpload"; } }
        public static string KitsuneDemoBucketAccelratedLink { get { return ConfigurationManager.AppSettings["KitsuneDemoBucketAccelratedLink"]; } }
        public static string DemoCloudFrontLink { get { return ConfigurationManager.AppSettings["DemoCloudFrontLink"]; } }
        public static string CloudFrontDistributionId { get { return ConfigurationManager.AppSettings["CloudFrontDistributionId"]; } }
        public static string ProjectIsPublished { get { return "api/Conversion/v1/ProjectPublishedOrNot?crawlId={0}"; } }
        public static string ArchiveProject { get { return "api/Conversion/v1/ArchiveProject"; } }

        #region Updated API
        public static string BuildAndRunProject { get { return "api/Project/v1/Build?user={0}"; } }
        public static string PRojectDetailsForBuild { get { return "api/Project/v1/Build?user={0}"; } }
        #endregion

    }
}
