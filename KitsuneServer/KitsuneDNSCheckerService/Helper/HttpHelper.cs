using Kitsune.Helper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace KitsuneDNSCheckerService.Helper
{
    public static class HttpHelper
    {
        private static readonly HttpClient client = new HttpClient();
        static bool _isDev = "True".Equals(ConfigurationManager.AppSettings["IsDev"]);


        public static Uri ServerBaseUri
        {


#if DEBUG
            //#error Replace with your IP address (the port is OK; i4t's part of the project)
            // get { return new Uri("http://localhost:6749/"); }
            get { return new Uri("http://api2.kitsune.tools/"); }
            // get { return new Uri("http://api2.kitsune.tools/"); }

#else

            get { return new Uri(_isDev ? "http://api2.kitsunedev.com/" : "https://api2.kitsune.tools/"); }
#endif

        }
        private static string GetToken()
        {
            //TODO : update once authencation done
            return null;
        }
        public async static Task<T> GetAsync<T>(string action)
        {
            try
            {
                // client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GetToken());
                var response = client.GetAsync(new Uri(ServerBaseUri, action)).Result;
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    throw new Exception("UnAuthorized");
                var rawResult = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<T>(rawResult);
                //var result = JObject.Parse(rawResult).ToObject<T>();
                return result;
            }
            catch (Exception ex)
            {
                Log(ex.Message);
                Log(ex.StackTrace);
                throw ex;
            }
            // return default(T);
        }
        public async static Task<T> GetAsyncWithError<T>(string action)
        {
            try
            {
                // client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GetToken());
                var response = client.GetAsync(new Uri(ServerBaseUri, action)).Result;
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    throw new Exception("UnAuthorized");
                else if (!((int)response.StatusCode >= 200 && (int)response.StatusCode <= 300))
                {
                    throw new Exception("API call unsuccessful : " + action);
                }
                var rawResult = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<T>(rawResult);
                //var result = JObject.Parse(rawResult).ToObject<T>();
                return result;
            }
            catch (Exception ex)
            {
                Log(ex.Message);
                Log(ex.StackTrace);
                throw ex;
            }
            // return default(T);
        }
        public async static Task<T> PostFileAsync<T>(string action, Stream fileStream, string fileName)
        {
            using (var client = new HttpClient())
            {
                using (var content = new MultipartFormDataContent("Upload----" + DateTime.Now.ToString(CultureInfo.InvariantCulture)))
                {
                    content.Add(new StreamContent(new MemoryStream(Kitsune.Helper.MediaUploadHelper.ReadInputStream(fileStream))), fileName, fileName);

                    using (var response = await client.PostAsync(new Uri(ServerBaseUri, action), content))
                    {
                        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                            throw new Exception("UnAuthorized");
                        else if (response.StatusCode != System.Net.HttpStatusCode.OK || response.StatusCode != System.Net.HttpStatusCode.Created)
                            Log(response?.Content?.ReadAsStringAsync()?.Result);
                        var rawResult = await response.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<T>(rawResult);
                        Log("Success  : PostFile, Action : " + action + ", Result : " + rawResult);
                        return result;
                    }
                }
            }
        }

        public static T PostFileSync<T>(string action, Stream fileStream, string fileName)
        {
            using (var client = new HttpClient())
            {
                using (var content = new MultipartFormDataContent("Upload----" + DateTime.Now.ToString(CultureInfo.InvariantCulture)))
                {
                    content.Add(new StreamContent(new MemoryStream(Kitsune.Helper.MediaUploadHelper.ReadInputStream(fileStream))), fileName, fileName);

                    using (var response = client.PostAsync(new Uri(ServerBaseUri, action), content).Result)
                    {
                        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                            throw new Exception("UnAuthorized");
                        else if (response.StatusCode != System.Net.HttpStatusCode.OK || response.StatusCode != System.Net.HttpStatusCode.Created)
                            Log(response?.Content?.ReadAsStringAsync()?.Result);
                        var rawResult = response.Content.ReadAsStringAsync().Result;
                        var result = JsonConvert.DeserializeObject<T>(rawResult);
                        //Log("Success  : PostFile, Action : " + action + ", Result : " + rawResult);
                        return result;
                    }
                }
            }
        }

        public async static Task<T> PostAsync<T>(string action, object obData)
        {
            try
            {
                if (client.DefaultRequestHeaders.Accept != null && !client.DefaultRequestHeaders.Accept.Any(x => x.MediaType.ToLower() == "application/json"))
                    client.DefaultRequestHeaders
                  .Accept
                  .Add(new MediaTypeWithQualityHeaderValue("application/json"));
                // client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GetToken());
                var jsonData = JsonConvert.SerializeObject(obData);
                //Log("Request : " + jsonData);
                var response = client.PostAsync(new Uri(ServerBaseUri, action), new StringContent(jsonData, Encoding.UTF8, "application/json")).Result;
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    throw new Exception("UnAuthorized");
                else if (!((int)response.StatusCode >= 200 && (int)response.StatusCode <= 300))//If status code is not 20X, error.
                {
                    Log(response?.Content?.ReadAsStringAsync()?.Result);
                    throw new Exception(String.Format("API call unsuccessful : {0}", action));
                }
                var rawResult = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<T>(rawResult);
                // Log("Success  : POST, Action : " + action + ", Result : " + rawResult);
                return result;
            }
            catch (Exception ex)
            {
                Log(ex.Message);
                Log(ex.StackTrace);
                throw ex;
            }
            //return default(T);
        }
        public static T PostSync<T>(string action, object obData)
        {
            try
            {
                if (client.DefaultRequestHeaders.Accept != null && !client.DefaultRequestHeaders.Accept.Any(x => x.MediaType.ToLower() == "application/json"))
                    client.DefaultRequestHeaders
                      .Accept
                      .Add(new MediaTypeWithQualityHeaderValue("application/json"));
                // client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GetToken());
                var jsonData = JsonConvert.SerializeObject(obData);
                //Log("Request : " + jsonData.Length);
                var response = client.PostAsync(new Uri(ServerBaseUri, action), new StringContent(jsonData, Encoding.UTF8, "application/json")).Result;
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    throw new Exception("UnAuthorized");
                else if (!((int)response.StatusCode >= 200 && (int)response.StatusCode <= 300))//If status code is not 20X, error.
                {
                    Log(response?.Content?.ReadAsStringAsync()?.Result);
                    throw new Exception(String.Format("API call unsuccessful : {0}", action));
                }
                var rawResult = response.Content.ReadAsStringAsync().Result;
                var result = JsonConvert.DeserializeObject<T>(rawResult);
                // Log("Success  : POST, Action : " + action + ", Result : " + rawResult);
                return result;
            }
            catch (Exception ex)
            {
                Log(ex.Message);
                Log(ex.StackTrace);
                throw ex;
            }
            //return default(T);
        }

        public async static Task<T> PostAsync<T>(string action, object obData, string serverBaseUri)
        {
            try
            {
                Uri ServerBaseUri = new Uri(serverBaseUri);
                if (client.DefaultRequestHeaders.Accept != null && !client.DefaultRequestHeaders.Accept.Any(x => x.MediaType.ToLower() == "application/json"))
                    client.DefaultRequestHeaders
                  .Accept
                  .Add(new MediaTypeWithQualityHeaderValue("application/json"));
                // client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GetToken());
                var jsonData = JsonConvert.SerializeObject(obData);
                //Log("Request : " + jsonData);
                var response = client.PostAsync(new Uri(ServerBaseUri, action), new StringContent(jsonData, Encoding.UTF8, "application/json")).Result;
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    throw new Exception("UnAuthorized");
                else if (!((int)response.StatusCode >= 200 && (int)response.StatusCode <= 300))//If status code is not 20X, error.
                {
                    Log(response?.Content?.ReadAsStringAsync()?.Result);
                    throw new Exception(String.Format("API call unsuccessful : {0}", action));
                }
                var rawResult = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<T>(rawResult);
                //Log("Success  : POST, Action : " + action + ", Result : " + rawResult);
                return result;
            }
            catch (Exception ex)
            {
                Log(ex.Message);
                Log(ex.StackTrace);
                throw ex;
            }
            //return default(T);
        }

        //public static string SerializeObject<T>(this T toSerialize)
        //{
        //    XmlSerializer xmlSerializer = new XmlSerializer(toSerialize.GetType(), XMLNameSpace);
        //    using (StringWriter textWriter = new StringWriter())
        //    {
        //        xmlSerializer.Serialize(textWriter, toSerialize);
        //        return textWriter.ToString();
        //    }
        //}
        public async static Task<T> DeleteAsync<T>(string action)
        {
            try
            {
                var response = client.DeleteAsync(new Uri(ServerBaseUri, action)).Result;
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    throw new Exception("UnAuthorized");
                else if (response.StatusCode != System.Net.HttpStatusCode.OK || response.StatusCode != System.Net.HttpStatusCode.Created)
                    Log(response?.Content?.ReadAsStringAsync()?.Result);
                var rawResult = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<T>(rawResult);
                return result;
            }
            catch (Exception ex)
            {
                Log(ex.Message);
                Log(ex.StackTrace);
                throw ex;
            }
            //return default(T);
        }
        private static void Log(string message)
        {
            try
            {
                //TODO: Update log
            }
            catch
            {

            }
        }

        internal static object GetAsync<T>(object p)
        {
            throw new NotImplementedException();
        }

    }

}
