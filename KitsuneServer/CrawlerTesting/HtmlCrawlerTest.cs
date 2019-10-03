using Crawler.Helper;
using Crawler.Krawler;
using Crawler.Models;
using Kitsune.Models.Krawler;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace CrawlerTesting
{
    class KrawlerException
    {
        public LOGTYPE ErrorType { get; set; }
        public string Message { get; set; }
        public Exception Ex { get; set; }
    }

    [TestClass]
    public class HtmlCrawlerTest
    {
        [TestMethod]
        public void OnePageCrawl()
        {   
            string url = "https://www.religarehealthinsurance.com/";
            Uri uri = new Uri(url);

            List<KrawlerException> Errors = new List<KrawlerException>();
            Krawler krawler = new Krawler(uri);


            try
            {
                
                krawler.KrawlContext.Resources.UniqueWebpagesDictionary.TryAdd(uri.AbsoluteUri, new AssetDetails { LinkUrl = uri.AbsoluteUri, PlaceHolder = "[Kitsune_" + uri.AbsoluteUri + "]" });
                krawler.KrawlContext.ErrorLogMethod = (LOGTYPE x, string y, Exception ex) => Errors.Add(new KrawlerException { ErrorType = x, Ex = ex, Message = y });
                krawler.ProcessUri(uri);
            }
            catch(Exception ex)
            {

            }

        }

        [TestMethod]
        public void WebsiteCrawl()
        {
            List<KrawlerException> Errors = new List<KrawlerException>();
            string url = "https://giphy.com/";
            Uri uri = new Uri(url);

            Krawler krawler = new Krawler(uri);
            krawler.KrawlContext.ErrorLogMethod = (LOGTYPE x, string y, Exception ex) => Errors.Add(new KrawlerException { ErrorType = x, Ex = ex, Message = y });
            //krawler.KrawlContext.SaveHtmlMethod = (x, y) => SaveInS3("test1"+x, y);
            krawler.Krawl();
        }

        [TestMethod]
        public void KrawlerUtilityTest()
        {
            var xyz=KrawlerUtility.GenerateFileLocalPath(new Uri("http://13.127.86.77/abc.css?sccss=1&ver=4.9.5"), ".css");
        }
    }
}
