using System;
using System.Collections.Generic;
using System.Text;

namespace Kitsune.Transpiler
{
    internal static class TranspilerConstants
    {
        public static string CompilerAPIBaseUrl = "api/Project/";

        public static string SaveFileContentToS3 { get { return CompilerAPIBaseUrl + "v1/Project/SaveFileContentToS3"; } }
        public static string GetFileFromS3 { get { return CompilerAPIBaseUrl + "v1/Project/GetFileFromS3"; } }
        public static string GetProjectDetailsQuery { get { return CompilerAPIBaseUrl + "v1/ProjectDetails/{0}?userEmail={1}"; } }
        public static string UpdateBuildStatus { get { return CompilerAPIBaseUrl + "v1/Build?user={0}"; } }
        public static string EventTrigger { get { return "api/event/v1/register"; } }
#if DEBUG
        public static string QueueUrl { get { return "https://sqs.ap-southeast-2.amazonaws.com/593693325525/TranspilerQueue"; } }
#else
        public static string QueueUrl { get { return "https://sqs.ap-south-1.amazonaws.com/949868548106/TranspilerQueue"; } }
#endif
    }
}
