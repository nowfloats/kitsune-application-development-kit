using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Crawler.Helper
{
    public class Utils
    {
        public static Uri GenerateUriToProcess(Uri uri, Regex ignoreFileRegex)
        {
            if (uri == null)
                throw new ArgumentNullException(nameof(uri));

            //Handle Query Parameter
            if (!String.IsNullOrEmpty(uri.Query) && ignoreFileRegex != null)
            {
                if (ignoreFileRegex.IsMatch(uri.AbsolutePath))
                {
                    string query = uri.Query;
                    string url = String.IsNullOrEmpty(query) ?
                        uri.AbsoluteUri : uri.AbsoluteUri.Replace(query, String.Empty);

                    Uri newUri = new Uri(url);
                    return newUri;
                }
            }

            return uri;
        }
    }
}
