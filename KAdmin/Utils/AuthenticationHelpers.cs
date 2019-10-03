using KitsuneAdminDashboard.Web.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;


namespace KitsuneAdminDashboard.Web.Utils
{
    public static class AuthenticationHelpers
    {
        internal static WebsiteLoginResponseModel VerifyLogin(AuthUser authUser)
        {
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(new Uri(String.Format(Constants.CustomerEndpoints.VerifyUserEndpoint, Constants.KitsuneServerUrl)));
                request.Method = "POST";
                request.ContentType = "application/json";

                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    string jsonData = JsonConvert.SerializeObject(authUser);
                    streamWriter.Write(jsonData);
                }
                var httpResponse = (HttpWebResponse)request.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    return JsonConvert.DeserializeObject<WebsiteLoginResponseModel>(streamReader.ReadToEnd());
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
