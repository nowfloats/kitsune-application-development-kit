using Kitsune.SyntaxParser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Kitsune.Helper
{
    public class KitsuneCommonUtils
    {
        public static string UpdateViewFunction(string pattern)
        {
            var tempPattern = pattern;
            var pageClassResult = Helper.Constants.ViewClassRegularExpression.Matches(tempPattern);
            if (pageClassResult.Count > 0)
            {
                tempPattern = pageClassResult[0].Value.Replace(pageClassResult[0].Groups[1].Value, "PAGECLASS");
            }
            return tempPattern;

        }
        public static string GenerateUrlPatternRegex(string urlPattern, string sourcePath)
        {
            try
            {
                var regex = Constants.WidgetRegulerExpression;
                string replacementString = string.Empty;
                string[] parts;
                string urlPatternRegex = string.Empty;

                if (!string.IsNullOrEmpty(urlPattern))
                {
                    urlPatternRegex = urlPattern;
                    var widget_matches = regex.Matches(urlPattern);
                    var match_list = widget_matches.Cast<Match>().Select(match => match.Value).ToList().Distinct();
                    foreach (var tempMatch in match_list)
                    {
                        var match = tempMatch;

                        replacementString = UpdateViewFunction(match);

                        if (!string.IsNullOrEmpty(match))
                        {
                            var object_matches = Parser.GetObjects(match.Trim('[').Trim(']'));
                            foreach (var mat in object_matches)
                            {
                                var trim_mat = mat;
                                while (trim_mat.EndsWith("]]"))
                                    trim_mat = trim_mat.Substring(0, trim_mat.Length - "]]".Length);
                                while (trim_mat.StartsWith("[["))
                                    trim_mat = trim_mat.Substring("[[".Length);
                                var inner_regex = new Regex(@"(" + Regex.Escape(trim_mat) + @".*?)([\+\-\/\*\^\\\]]{1})", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                                replacementString = inner_regex.Replace(replacementString, @"'([a-zA-Z0-9\-\.,\%_]+)'$2");
                            }

                            replacementString = Parser.Execute(replacementString.Trim('[', ']')).ToString();
                        }
                        urlPatternRegex = urlPatternRegex.Replace(tempMatch, replacementString);
                    }

                    urlPatternRegex = urlPatternRegex.Trim('/');
                    urlPatternRegex = Regex.Escape(urlPatternRegex);
                    urlPatternRegex = urlPatternRegex.Replace(Regex.Escape(@"([a-zA-Z0-9\-\.,\%_]+)"), @"([a-zA-Z0-9\-\.,\%_]+)");
                    return urlPatternRegex;
                }
                else
                    throw new Exception("Url Pattern Empty in Source Path: " + sourcePath);
            }
            catch
            {
                throw new Exception("Url Pattern Regex Generation Error in Source Path: " + sourcePath);
            }
        }
        public static string FormatResourceFileName(string resourceName)
        {
            var tempName = resourceName.Replace(" ", "-").Trim(new char[] { ' ' }).Replace("//", "/");
            tempName = Kitsune.Helper.Constants.SupportedResourcePathRegularExpression.Replace(tempName, "");
            return tempName;
        }
    }
}
