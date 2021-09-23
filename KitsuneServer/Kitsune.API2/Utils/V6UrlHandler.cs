using Kitsune.API.Model.ApiRequestModels;
using Kitsune.API2.DataHandlers.Mongo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Kitsune.API2.Utils
{
    public class V6UrlHandler
    {
        public static string GetRedirectionUrlForV6Links(string webTemplateId, string url, string rootAliasUrl, string websiteTag, string websiteId)
        {
            try
            {
                var parameters = url.Split('/');
                var tempUrl = url.ToLower();

                var rootaliasUrl = string.IsNullOrEmpty(rootAliasUrl) ? string.Format("https://{0}.nowfloats.com/", websiteTag.ToLower()) : rootAliasUrl.Trim('/') + "/";

                #region BIZFLOATS
                if (url.ToLower().Contains("bizfloat"))
                {
                    tempUrl = tempUrl.Replace(rootaliasUrl, "");
                    parameters = tempUrl.Split('/');
                    if (parameters.Length != 3)
                    {
                        return null;
                    }
                    var bizFloatId = parameters[1];
                    var bizFloatDetails = MongoConnector.GetBizFloatsDetails(bizFloatId);
                    if (bizFloatDetails != null)
                    {
                        if (!string.IsNullOrEmpty(webTemplateId))
                        {
                            var urlPatternregex = MongoConnector.GetBizFloatUrlPattern(webTemplateId);
                            var title = System.Text.RegularExpressions.Regex.Replace(bizFloatDetails.Content.ToLower(), "[^0-9a-zA-Z]+", "-").Trim('-');
                            if (title.Length > 120)
                            {
                                title = title.Substring(0, 120);
                            }
                            var urlPattern = rootaliasUrl + title + "/u" + bizFloatDetails.Index;
                            if (!string.IsNullOrEmpty(urlPatternregex))
                            {
                                var urlPramsArray = urlPatternregex.ToLower().Split('/');
                                urlPattern = urlPatternregex.ToLower();
                                foreach (var urlParam in urlPramsArray)
                                {

                                    if (urlParam.Contains("rootaliasurl"))
                                    {
                                        urlPattern = urlPattern.Replace(urlParam, rootaliasUrl.Trim('/'));
                                    }
                                    else if (urlParam.Contains("title"))
                                    {
                                        urlPattern = urlPattern.Replace(urlParam, title);
                                    }
                                    else if (urlParam.Contains("id"))
                                    {
                                        urlPattern = urlPattern.Replace(urlParam, bizFloatId);
                                    }
                                    else if (urlParam.Contains("index"))
                                    {
                                        var firstIndex = urlParam.IndexOf("[[");
                                        var lastIndex = urlParam.IndexOf("]]");

                                        urlPattern = urlPattern.Replace(urlParam.Substring(firstIndex, lastIndex + 1), bizFloatDetails.Index.ToString());
                                    }
                                }
                            }
                            return urlPattern;
                        }
                    }
                }

                #endregion

                #region SEARCH
                else if (url.ToLower().Contains("search/"))
                {
                    var tempUrlParam = url.Split(new string[] { "search/" }, StringSplitOptions.None);
                    if (tempUrlParam.Length > 1)
                    {
                        var urlparams = tempUrlParam[1].Split('/');
                        if (urlparams.Length > 1)
                        {
                            return null;
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(webTemplateId))
                            {
                                var urlPattern = rootAliasUrl.ToLower().Trim('/') + "/search/" + parameters[parameters.Length - 1] + "/1";
                                return urlPattern;
                            }
                        }
                    }
                }

                #endregion

                #region PRODUCT
                else if (url.ToLower().Contains("product/"))
                {
                    tempUrl = tempUrl.Replace(rootaliasUrl, "");

                    parameters = tempUrl.Split('/');

                    if (parameters.Length != 3)
                    {
                        return null;
                    }
                    var productId = parameters[1];
                    var productDetails = MongoConnector.GetProductDetails(productId);
                    if (productDetails != null)
                    {
                        if (!string.IsNullOrEmpty(webTemplateId))
                        {
                            var title = System.Text.RegularExpressions.Regex.Replace(productDetails.Content.ToLower(), "[^0-9a-zA-Z]+", "-").Trim('-');
                            if (title.Length > 150)
                            {
                                title = title.Substring(0, 150);
                            }
                            var urlPattern = rootAliasUrl.ToLower().Trim('/') + "/" + title + "/p" + productDetails.Index;
                            return urlPattern;
                        }
                    }
                }
                #endregion

                tempUrl = tempUrl.Replace(rootaliasUrl, "");

                var bizfloatpattern1 = "([a-zA-Z0-9-%]+)/b[0-9]+";
                var bizfloatpattern2 = "([a-zA-Z0-9-%]+)/u[0-9]+";
                var bizfloatString = Regex.Match(tempUrl, bizfloatpattern2).Value;
                if (String.IsNullOrEmpty(bizfloatString))
                    bizfloatString = Regex.Match(tempUrl, bizfloatpattern1).Value;

                if (!String.IsNullOrEmpty(bizfloatString) && String.Compare(bizfloatString, tempUrl) == 0)
                {
                    var pageNumberString = Regex.Match(bizfloatString.Split('/')[1], @"\d+").Value;
                    var index = pageNumberString;
                    var bizFloatDetails = MongoConnector.GetBizFloatsDetailsByIndex(websiteId, index);
                    if (bizFloatDetails != null)
                    {
                        var urlStringFormatPattern = String.Empty;
                        var urlPatternregex = MongoConnector.GetBizFloatUrlPattern(webTemplateId);
                        var title = Regex.Replace(bizFloatDetails.Content.ToLower(), "[^0-9a-zA-Z]+", "-").Trim('-');
                        if (title.Length > 120)
                        {
                            title = title.Substring(0, 120);
                        }
                        var urlPattern = string.Empty;
                        if (urlPatternregex == null)
                            urlStringFormatPattern = "{0}{1}/b{2}";
                        else
                            urlStringFormatPattern = "{0}{1}/u{2}";

                        urlPattern = String.Format(urlStringFormatPattern, rootaliasUrl, title, bizFloatDetails.Index);

                        if (!string.IsNullOrEmpty(urlPatternregex))
                        {
                            var urlPramsArray = urlPatternregex.ToLower().Split('/');
                            urlPattern = urlPatternregex.ToLower();
                            foreach (var urlParam in urlPramsArray)
                            {
                                if (urlParam.Contains("rootaliasurl"))
                                {
                                    urlPattern = urlPattern.Replace(urlParam, rootaliasUrl.Trim('/'));
                                }
                                else if (urlParam.Contains("title"))
                                {
                                    urlPattern = urlPattern.Replace(urlParam, title);
                                }
                                else if (urlParam.Contains("index"))
                                {
                                    var firstIndex = urlParam.IndexOf("[[");
                                    var lastIndex = urlParam.IndexOf("]]");

                                    urlPattern = urlPattern.Replace(urlParam.Substring(firstIndex, lastIndex + 2), bizFloatDetails.Index.ToString());
                                }
                            }
                            if (String.Compare(urlPattern, url.Trim('/'), true) != 0 && !(Regex.Match(urlPattern, bizfloatpattern2).Success || (Regex.Match(urlPattern, bizfloatpattern1).Success)))
                                return urlPattern;
                        }
                        else if (!string.IsNullOrEmpty(webTemplateId))
                        {
                            if (String.Compare(urlPattern, url.Trim('/'), true) == 0)
                                return null;

                            //return String.Format(urlStringFormatPattern, rootaliasUrl, title, bizFloatDetails.MessageIndex);
                        }
                    }
                }

                var productPattern = "([a-zA-Z0-9-%]+)/p[0-9]+";
                var productString = Regex.Match(tempUrl, productPattern).Value;
                if (!String.IsNullOrEmpty(productString) && String.Compare(productString, tempUrl) == 0)
                {
                    var pageNumberString = Regex.Match(productString.Split('/')[1], @"\d+").Value;
                    var index = pageNumberString;
                    var productDetails = MongoConnector.GetProductDetailsByIndex(websiteId, index);
                    if (productDetails != null)
                    {
                        var urlPatternregex = MongoConnector.GetProductUrlPattern(webTemplateId);
                        var title = Regex.Replace(productDetails.Content.ToLower(), "[^0-9a-zA-Z]+", "-").Trim('-');
                        if (title.Length > 120)
                        {
                            title = title.Substring(0, 120);
                        }
                        var urlPattern = String.Format("{0}{1}/p{2}", rootaliasUrl, title, productDetails.Index);

                        if (!string.IsNullOrEmpty(urlPatternregex))
                        {
                            var urlPramsArray = urlPatternregex.ToLower().Split('/');
                            urlPattern = urlPatternregex.ToLower();
                            foreach (var urlParam in urlPramsArray)
                            {
                                if (urlParam.Contains("rootaliasurl"))
                                {
                                    urlPattern = urlPattern.Replace(urlParam, rootaliasUrl.Trim('/'));
                                }
                                else if (urlParam.Contains("name"))
                                {
                                    urlPattern = urlPattern.Replace(urlParam, title);
                                }
                                else if (urlParam.Contains("index"))
                                {
                                    var firstIndex = urlParam.IndexOf("[[");
                                    var lastIndex = urlParam.IndexOf("]]");

                                    urlPattern = urlPattern.Replace(urlParam.Substring(firstIndex, lastIndex + 2), productDetails.Index.ToString());
                                }
                            }

                            //var tempUrlPatternMatchComparisionValue = (String.Compare(urlPatternregex, Regex.Match(urlPatternregex, productPattern).Value, true));
                            if (String.Compare(urlPattern, url.Trim('/'), true) != 0 && !(Regex.Match(urlPattern, productPattern).Success))
                                return urlPattern;
                        }
                        else if (!string.IsNullOrEmpty(webTemplateId))
                        {
                            if (String.Compare(urlPattern, url.Trim('/'), true) == 0)
                                return null;

                            //return String.Format("{0}{1}/p{2}", rootaliasUrl, title, productDetails.ProductIndex);
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Console.Write(ex.ToString());
            }

            return null;
        }
    }
}
