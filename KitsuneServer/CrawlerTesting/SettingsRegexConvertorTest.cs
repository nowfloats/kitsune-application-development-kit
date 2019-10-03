using KitsuneWebsiteCrawlerService.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace CrawlerTesting
{
    [TestClass]
    public class SettingsRegexConvertorTest
    {
        [TestMethod]
        public void RegexCOnvertor()
        {
            //Dictionary<string, string> dict = new Dictionary<string, string>
            //{
            //    { "*/blog/*", "^.*\\/blog\\/.*" },
            //    { "/blog/*","^\\/blog\\/.*"},
            //    { "/blog*","^\\/blog.*"}
            //};
            //var result = Utils.ConvertKitsuneUrlPatternToRegex("*/blog/*");
            //var result2 = Utils.ConvertKitsuneUrlPatternToRegex("/blog/*");
            //var result3 = Utils.ConvertKitsuneUrlPatternToRegex("/blog*");
            List<string> stringList = new List<string>()
            {
                //"*/blog/*","/hello/*","*.php"
            };
            var result = Utils.GetExcludeUrlsRegex(stringList);
        }

        [TestMethod]
        public void RegexConvertor()
        {
            List<string> stringList = new List<string>()
            {
                "*/blog/*","/hello/*","*.php"
            };
            var result = Utils.GetExcludeUrlsRegex(stringList);
        }
    }
}
