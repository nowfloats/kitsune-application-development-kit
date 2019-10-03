using Crawler.Models;
using Kitsune.Models.Krawler;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace Crawler.Helper
{
    class DownloadHelper
    {
        public static IRestResponse Download(KeyValuePair<string, AssetDetails> resourceObject, FileDownloaderContext context)
        {
            try
            {
                Uri uri = null;
                string url = resourceObject.Value.LinkUrl;
                if (String.IsNullOrEmpty(url))
                {
                    if (Uri.TryCreate(url, UriKind.Absolute, out uri))
                    {
                        var result = HttpRequest.HttpRequestWithReadAndWriteTimeOut(uri, context.Configuration.ReadAndWriteTimeOut, context.Configuration.UserAgentString);
                        return result;
                    }
                    else
                    {
                        throw new Exception("Error Creating Url");
                    }
                }
                else
                {
                    throw new Exception("uri was null");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}
