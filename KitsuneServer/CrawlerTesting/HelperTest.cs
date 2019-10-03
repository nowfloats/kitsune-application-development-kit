using Crawler.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace CrawlerTesting
{
    [TestClass]
    public class HelperTest
    {
        [TestMethod]
        public void UrlGenerator()
        {
            try
            {
                Uri uri = new Uri("https://nowlfoats.com/abc.js?v=12#123");
                Regex regex = new Regex("abc\\.js");
                var result = Utils.GenerateUriToProcess(uri, regex);
            }
            catch(Exception ex)
            {

            }
        }
    }
}
