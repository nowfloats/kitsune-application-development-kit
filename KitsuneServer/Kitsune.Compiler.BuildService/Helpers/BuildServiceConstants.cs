using System;
using System.Collections.Generic;
using System.Text;

namespace Kitsune.Compiler.BuildService.Helpers
{
    internal static class BuildServiceConstants
    {
        public static string CompilerAPIBaseUrl = "api/Project/";

        public static string GetFileFromS3 { get { return CompilerAPIBaseUrl + "v1/Project/GetFileFromS3"; } }
        public static string GetLastCompletedBuild { get { return CompilerAPIBaseUrl + "v1/LastCompletedBuild?user={0}&projectid={1}"; } }
        public static string GetProjectDetailsQuery { get { return CompilerAPIBaseUrl + "v1/ProjectDetails/{0}?userEmail={1}"; } }
        public static string GetProjectDetailsByClientIdQuery { get { return CompilerAPIBaseUrl + "v1/ProjectDetails/{0}/{1}"; } }
        public static string GetProjectResourceDetailsQuery { get { return CompilerAPIBaseUrl + "v1/Project/{0}/Resource?sourcePath={1}&userEmail={2}"; } }
        public static string UpdateBuildStatus { get { return CompilerAPIBaseUrl + "v1/Build?user={0}"; } }
        public static string SaveFileContentToS3 { get { return CompilerAPIBaseUrl + "v1/Project/SaveFileContentToS3"; } }
        public static string CreateOrUpdateResourceCommand { get { return CompilerAPIBaseUrl + "v1/Resource"; } }
        public static string GetAuditProjectAndResourcesDetails { get { return CompilerAPIBaseUrl + "Admin/ProjectAndResources/{0}/{1}?userEmail={2}"; } }
        public static string GetPartialPagesDetailsQuery { get { return CompilerAPIBaseUrl + "v1/Project/{0}/PartialPages?sourcePath={1}&userEmail={2}"; } }

        public static string GetUserIdQuery { get { return "api/Developer/v1/GetUserId?useremail={0}"; } }
        public static string GetLanguage { get { return "Language/v1/{0}"; } }
        public static string GetLanguageByClientId { get { return "Language/v1/{0}/{1}"; } }



    }
}
