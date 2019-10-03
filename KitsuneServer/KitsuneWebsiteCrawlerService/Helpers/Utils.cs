using KitsuneWebsiteCrawlerService.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace KitsuneWebsiteCrawlerService.Helpers
{
    public class Utils
    {
        public static string GetExcludeUrlsRegex(List<string> list)
        {
            if (list == null)
                throw new ArgumentNullException(nameof(list));
            try
            {
                List<string> listOfRegex = new List<string>();
                foreach (var str in list)
                {
                    if(!String.IsNullOrEmpty(str))
                    {
                        var regex = ConvertKitsuneUrlPatternToRegex(str);
                        listOfRegex.Add(regex);
                    }
                }
                string regexString = String.Join("|", listOfRegex.ToArray());
                return regexString;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static string ConvertKitsuneUrlPatternToRegex(string str)
        {
            str = str.Replace(".", "\\.");
            str = str.Replace("*", ".*");
            str = str.Replace("/", "\\/");
            str = $"^{str}$";
            return str;
        }

        public static List<string> GetAllStaticAssetList(List<ExternalAPIRequestModel> apiRequests)
        {
            List<string> listOfAssetUrls = new List<string>();

            foreach(var request in apiRequests)
            {
                var result = APIHelper.GetStaticAssetFromAPI(request);
                listOfAssetUrls.AddRange(result.assets);
            }
            return listOfAssetUrls;
        }
    }
}
