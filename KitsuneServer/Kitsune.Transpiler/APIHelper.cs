using Kitsune.API.Model.ApiRequestModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kitsune.Transpiler
{
    internal class APIHelper
    {
        internal string SaveFileContentToS3(SaveFileContentToS3RequestModel resourceContent)
        {
            var response = HttpHelper.PostSync<string>(TranspilerConstants.SaveFileContentToS3, resourceContent);
            return response;
        }
        internal string GetFileFromS3(GetFileFromS3RequestModel fileRequest)
        {
            var response = HttpHelper.PostSync<string>(TranspilerConstants.GetFileFromS3, fileRequest);
            return response;
        }
        internal GetProjectDetailsResponseModel GetProjectDetailsApi(string userEmail, string projectId = "null")
        {
            var response = HttpHelper.Get<GetProjectDetailsResponseModel>(string.Format(TranspilerConstants.GetProjectDetailsQuery, projectId ?? "null", userEmail));
            if (response != null)
                return response;
            return null;
        }
        internal bool UpdateBuildStatus(string userEmail, CreateOrUpdateKitsuneStatusRequestModel createOrUpdateKitsuneStatusRequestModel)
        {
            var response = HttpHelper.PostSync<bool>(string.Format(TranspilerConstants.UpdateBuildStatus, userEmail), createOrUpdateKitsuneStatusRequestModel);
            return response;
        }
        internal ErrorApiResponseModel TriggerCompletedEvent(string projectid)
        {
            //Event 3 = Transpilation
            var response = HttpHelper.PostSync<ErrorApiResponseModel>(TranspilerConstants.EventTrigger, new { ProjectId = projectid, Event = 3 });
            return response;
        }
        internal ErrorApiResponseModel TriggerFailedEvent(string projectid)
        {
            //Event 4 = TranspilationFailed
            var response = HttpHelper.PostSync<ErrorApiResponseModel>(TranspilerConstants.EventTrigger, new { ProjectId = projectid, Event = 4 });
            return response;
        }
    }
}
