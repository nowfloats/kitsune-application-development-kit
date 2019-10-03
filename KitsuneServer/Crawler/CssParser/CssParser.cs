using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Crawler
{
    public class CssParser
    {
        public static Dictionary<string, string> GetAllUris(string cssText)
        {
            try
            {
                Dictionary<string, string> listOfUrls = new Dictionary<string, string>();
                var urls = Regex.Matches(cssText, "url\\(['\"]?(?<url>[^)]+?)['\"]?\\)");
                foreach (var url in urls)
                {
                    string actualUrlString = url.ToString();
                    string urlString = actualUrlString.TrimStart("url".ToCharArray());
                    urlString = urlString.Trim(new char[] { ' ', '\'', '"', ')', '(' });
                    if (!listOfUrls.ContainsKey(urlString))
                    {
                        listOfUrls.Add(urlString, actualUrlString);
                    }
                }
                return listOfUrls;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static Dictionary<string, string> GetAllImportUris(string cssText)
        {
            try
            {
                Dictionary<string, string> listOfUrls = new Dictionary<string, string>();
                var urls = Regex.Matches(cssText, @"@import[\s]+([""'])(?<url>[^""']+)\1");
                foreach (var url in urls)
                {
                    string actualUrlString = url.ToString();
                    string urlString = actualUrlString.TrimStart("@import".ToCharArray());
                    urlString = urlString.Trim(new char[] { ' ', '\'', '"', ')', '(' });
                    if (!listOfUrls.ContainsKey(urlString))
                    {
                        listOfUrls.Add(urlString, actualUrlString);
                    }
                }
                return listOfUrls;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
