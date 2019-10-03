using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace Kitsune.KString.Service
{
    public class KStringHelper
    {
#if DEBUG
        private static readonly string KeywordExtractionAPI = "[[KIT_KEYWORD_EXTRACTOR_API]]";
        private static readonly string keywordDataUpdateAPI = "http://api2.kitsunedev.com/language/v1/";
#else
           private static readonly string KeywordExtractionAPI = "[[KIT_KEYWORD_EXTRACTOR_API]]";
        private static readonly string keywordDataUpdateAPI = "http://api2.kitsune.tools/language/v1/";
#endif
        public static List<string> ExtractKeyword(string dataString)
        {
            try
            {
                 HttpClient client = new HttpClient();

                client.BaseAddress = new Uri(KeywordExtractionAPI);
                client.DefaultRequestHeaders
                      .Accept
                      .Add(new MediaTypeWithQualityHeaderValue("application/json"));//ACCEPT header

                var result = client.PostAsync("", new StringContent(dataString,
                                                    Encoding.UTF8,
                                                    "application/json")).Result;
                if (result != null && result.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var response = result.Content.ReadAsStringAsync().Result;
                    if (!string.IsNullOrEmpty(response))
                    {
                        return JsonConvert.DeserializeObject<List<string>>(response);
                    }
                }
                else
                {
                    Console.WriteLine($"Keyword ExtractionAPI Unsuccessful : Status : {result?.StatusCode} : Response : {result?.Content?.ReadAsStringAsync().Result} ");
                }
                return null;
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Exception : ExtractKeyword : {ex.Message}");
                return null;
            }
            
        }
        public static string UpdateKeywordsToKitsuneDB(string schemaName, string userid, string kid, string referenceid, string dataString, List<string> keywords)
        {
            HttpClient client = new HttpClient();

            client.BaseAddress = new Uri(keywordDataUpdateAPI);
            client.DefaultRequestHeaders
                   .Accept
                   .Add(new MediaTypeWithQualityHeaderValue("application/json"));//ACCEPT header
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(userid); 
            var jsonData =      $"{{ " +
                                $"  'Query' : {{ " +
                                $"              '_kid' : '{kid}', " +
                                $"              'k_referenceid' : '{referenceid ?? ""}' " +
                                $"            }}, " + 
                                $"}} ";
            var result = client.PostAsync($"{schemaName.Trim().ToLower()}/update-data", new StringContent(jsonData,
                                                    Encoding.UTF8,
                                                    "application/json")).Result;
            if (result != null && result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var response = result.Content.ReadAsStringAsync().Result;
                if (!string.IsNullOrEmpty(response))
                {
                    return response;
                }
            }
            else
            {
                Console.WriteLine($"UpdateKeywordsToKitsuneDB Unsuccessful : Status : {result?.StatusCode} : Response : {result?.Content?.ReadAsStringAsync().Result} ");
            }
            return null;
        }
    }
    
}
