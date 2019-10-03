using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Text.RegularExpressions;

namespace Crawler.Helper
{
    public static class KrawlerUtility
    {
        public static string GenerateHtmlLocalPath(Uri uri)
        {
            if (uri == null)
                throw new Exception("Uri cannot be null");
            string localPath = String.Empty;
            try
            {
                if (uri.LocalPath.Trim().Equals("/"))
                {
                    if (uri.Query.Equals(String.Empty))
                        localPath = String.Format("{0}index.html", uri.LocalPath);
                    else
                    {
                        string decodeQuery = WebUtility.UrlEncode(uri.Query).Replace("%", "_");
                        localPath = String.Format("{0}index{1}.html", uri.LocalPath, decodeQuery);
                    }
                }
                else
                {
                    if (uri.Query.Equals(String.Empty))
                    {
                        if (uri.AbsoluteUri.EndsWith(".html", StringComparison.InvariantCultureIgnoreCase) || uri.AbsoluteUri.EndsWith(".htm", StringComparison.InvariantCultureIgnoreCase))
                        {
                            localPath = uri.LocalPath;
                        }
                        else
                        {
                            char[] trim = { '/' };
                            localPath = uri.LocalPath.TrimEnd(trim);
                            localPath = String.Format("{0}.html", localPath);
                        }
                    }
                    else
                    {
                        char[] trim = { '/' };
                        localPath = uri.LocalPath.TrimEnd(trim);
                        string localPathWithOutHtmlExtension = localPath.Replace(".html", String.Empty);
                        string decodeQuery = WebUtility.UrlEncode(uri.Query).Replace("%", "_");
                        localPath = String.Format("{0}.html", localPathWithOutHtmlExtension + decodeQuery);
                    }
                }
                return localPath;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Get the extension of the file from the content-type
        /// TODO: Not the best Solution(Check:https://stackoverflow.com/questions/1029740/get-mime-type-from-filename-extension/24200761#answer-14108040)
        /// </summary>
        /// <param name="contentType"></param>
        /// <returns></returns>
        public static string GetExtensionFromContentType(string path, string contentType)
        {
            string pathExtension = String.Empty;
            try
            {
                HashSet<string> knownExtensions = new HashSet<string>() { ".css", ".js", ".html", ".htm", ".jpg", ".jpeg", ".png", ".svg", ".gif", ".txt", ".pdf", ".xls" };
                pathExtension = Path.GetExtension(path).ToLower();
                if (knownExtensions.Contains(path))
                {
                    return pathExtension;
                }
                else
                {
                    ContentType type = new ContentType(contentType);
                    var format = type.MediaType.ToLower();
                    string savedExtension = String.Empty;
                    ExtensionDictionary.Extension.TryGetValue(format, out savedExtension);
                    if (String.IsNullOrEmpty(savedExtension))
                        return pathExtension;
                    return savedExtension;
                }
            }
            catch (Exception ex)
            {
                return pathExtension;
            }
        }

        /// <summary>
        /// Generate File Path
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="extension"></param>
        /// <returns></returns>
        public static string GenerateFileLocalPath(Uri uri, string extension)
        {
            try
            {
                if (uri == null)
                    throw new Exception("uri or extension cannot be empty");

                string localPath = uri.LocalPath;
                localPath = localPath.TrimEnd(new Char[] { ' ', '/' });
                localPath = Regex.Replace(localPath, @"\.\w+$", String.Empty);
                if (String.IsNullOrEmpty(localPath))
                    localPath = "/";

                if (uri.Query.Equals(String.Empty))
                {

                    localPath = String.Format("{0}{1}", localPath, extension);
                }
                else
                {
                    //TODO:Chnage the random number to query value
                    #region Generating Random Number
                    Random randomNumber = new Random();
                    // defining range for 4 digit random Number
                    int startRangeForRandomNumbers = 1000;
                    int endRangeForRandomNumbers = 9999;
                    var randomValue = randomNumber.Next(startRangeForRandomNumbers, endRangeForRandomNumbers);

                    #endregion
                    if (localPath.EndsWith(".min", StringComparison.InvariantCultureIgnoreCase))
                    {
                        localPath = localPath.Replace(".min", "");
                        localPath = String.Format("{0}{1}.min{2}", localPath, randomValue, extension);
                    }
                    else
                    {
                        localPath = String.Format("{0}{1}{2}", localPath, randomValue, extension);
                    }
                }
                return localPath;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
