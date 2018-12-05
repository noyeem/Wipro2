using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using Wipro.Lib;
using System.Linq;
using System;
using System.Diagnostics;

namespace UnitTestProject
{
    [TestClass]
    public class TestCrawler
    {
        private string rootUri = @"https://wiprodigital.com/";

       [TestMethod]
       public void Test_GetPageLinks()
        {
            string url = "https://wiprodigital.com";
            int maxDepth = 3;
            Crawler crawler = new Crawler(url, maxDepth);
            var html = crawler.GetPageHtml(url);
            var actualLinks = CrawlerParser.GetPageLinks(url, html);

            var actualHrefs = actualLinks.Where(x=> x.IsWipro && !x.IsJavaScript).Select(x => Utilities.RemoveTrailingSlash(x.Href)).Distinct().ToList();
            actualHrefs.Sort();
            var expectedHrefs = Utilities.GetPageLinksViaAgility(url).Where(x => CrawlerParser.IsWipro(x) && !CrawlerParser.IsJavaScriptOrHashLink(null, x)).Select(x=> Utilities.RemoveTrailingSlash(x)).Distinct().ToList();
            expectedHrefs.Sort();


            for(int i=0; i<expectedHrefs.Count; i++)
            {
                Debug.WriteLine("{0} |exp: {1}\n{0} |act: {2}", i, expectedHrefs[i], actualHrefs[i]);
                if(expectedHrefs[i] != actualHrefs[i])
                {

                }
            }
            Assert.AreEqual(actualHrefs, expectedHrefs);
        }
       
        [TestMethod]
        public void Test_GetRootPageHtml()
        {
            string url = "https://wiprodigital.com";
            int maxDepth = 3;
            Crawler crawler = new Crawler(url, maxDepth);
            var actuallHtml = crawler.GetRootPageHtml();

            var expectedHtml = new WebClient().DownloadString(url);

            Assert.AreEqual(actuallHtml, expectedHtml);
        }

        [TestMethod]
        public void Test_GetPageHtml()
        {
            string url = "https://wiprodigital.com";
            int maxDepth = 3;
            Crawler crawler = new Crawler(url, maxDepth);
            var actuallHtml = crawler.GetPageHtml(url);

            var expectedHtml = new WebClient().DownloadString(url);

            Assert.AreEqual(actuallHtml, expectedHtml);
        }

        [TestMethod]
        public void Test_GetPageImages()
        {

        }

        [TestMethod]
        public void Test_GetLinksAsync()
        {

        }

        [TestMethod]
        public void Test_GetRootPageHtmlAsync()
        {

        }

        [TestMethod]
        public void Test_GetPageAsync()
        {

        }
    }
}
