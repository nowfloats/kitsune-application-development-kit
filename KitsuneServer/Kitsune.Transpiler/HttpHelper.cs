using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace Kitsune.Transpiler
{
    internal class HttpHelper
    {
        private static readonly HttpClient client = new HttpClient();
        public static Uri ServerBaseUri
        {
#if DEBUG
            //#error Replace with your IP address (the port is OK; i4t's part of the project)
            //get { return new Uri("http://localhost:41342/"); }
            get
            {
                return new Uri("https://api2.kitsunedev.com/");
            }
            //get { return new Uri("http://api2.kitsune.tools/"); }

#else

            get { return new Uri("https://api2.kitsune.tools/"); }
#endif
        }

        public static T Get<T>(string action, string authorization = null)
        {
            try
            {
                if (!string.IsNullOrEmpty(authorization))
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(authorization);

                var response = client.GetAsync(new Uri(ServerBaseUri, action)).Result;
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    throw new Exception("UnAuthorized");
                var rawResult = response.Content.ReadAsStringAsync().Result;
                T result = default(T);
                try
                {
                    result = JsonConvert.DeserializeObject<T>(rawResult);
                }
                catch
                {
                    throw new Exception($"GET-API : {action}");
                }
                //var result = JObject.Parse(rawResult).ToObject<T>();
                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
            // return default(T);
        }
        public static T PostSync<T>(string action, object obData, string authorization = null)
        {
            try
            {
                if (client.DefaultRequestHeaders.Accept != null && !client.DefaultRequestHeaders.Accept.Any(x => x.MediaType.ToLower() == "application/json"))
                    client.DefaultRequestHeaders
                      .Accept
                      .Add(new MediaTypeWithQualityHeaderValue("application/json"));
                if (!string.IsNullOrEmpty(authorization))
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(authorization);
                // client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GetToken());
                var jsonData = JsonConvert.SerializeObject(obData);
                //Log("Request : " + jsonData.Length);
                var response = client.PostAsync(new Uri(ServerBaseUri, action), new StringContent(jsonData, Encoding.UTF8, "application/json")).Result;
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    throw new Exception("UnAuthorized");
                else if (!((int)response.StatusCode >= 200 && (int)response.StatusCode <= 300))//If status code is not 20X, error.
                {
                    throw new Exception(String.Format("API call unsuccessful : {0}", action));
                }
                var rawResult = response.Content.ReadAsStringAsync().Result;

                T result = default(T);
                try
                {
                    result = JsonConvert.DeserializeObject<T>(rawResult);
                }
                catch
                {
                    throw new Exception($"POST-API : {action}, POST-API-BODY :  {jsonData}");
                }
                // Log("Success  : POST, Action : " + action + ", Result : " + rawResult);
                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
            //return default(T);
        }
    }
}
