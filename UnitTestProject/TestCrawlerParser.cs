using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Wipro.Lib;


namespace UnitTestProject
{
    [TestClass]
    public class TestCrawlerParser
    {
        string rootUri = @"https://wiprodigital.com/";

        [TestMethod]
        public void Test_ParseTextFromQuote()
        {
            var inputHtml = @"<a href=""https://wiprodigital.com/cases/delivering-an-exceptional-mortgage-customer-experience-for-allied-irish-bank/"" target=""_self"">";
            string url = CrawlerParser.ParseTextFromQuote(inputHtml);
            Assert.AreEqual(url, "https://wiprodigital.com/cases/delivering-an-exceptional-mortgage-customer-experience-for-allied-irish-bank/");
        }

        [TestMethod]
        public void Test_IsWipro()
        {
            //True cases
            bool trueCase = CrawlerParser.IsWipro("https://wiprodigital.com/cases/delivering-an-exceptional-mortgage-customer-experience-for-allied-irish-bank/");

            //False cases
            bool falseCase = CrawlerParser.IsWipro("http://test.wiprodigital.com");

            Assert.AreEqual(trueCase, true);
            Assert.AreNotEqual(falseCase, true);
        }

        [TestMethod]
        public void Test_IsStatic()
        {
            //True cases
            bool trueCase = CrawlerParser.IsStatic("https://s17776.pcdn.co/wp-content/themes/wiprodigital/images/logo.png");

            //False cases
            bool falseCase = CrawlerParser.IsStatic("https://wiprodigital.com/cases/delivering-an-exceptional-mortgage-customer-experience-for-allied-irish-bank/");


            Assert.AreEqual(trueCase, true);
            Assert.AreNotEqual(falseCase, true);
        }

        [TestMethod]
        public void Test_GetAbsoluteUri()
        {
            string case1 = CrawlerParser.GetAbsoluteUri(this.rootUri, "cases/delivering-an-exceptional-mortgage-customer-experience-for-allied-irish-bank/");
           /// bool case2 = CrawlerParser.GetAbsoluteUri("https://wiprodigital.com/cases/delivering-an-exceptional-mortgage-customer-experience-for-allied-irish-bank/");

            Assert.AreEqual(case1, "https://wiprodigital.com/cases/delivering-an-exceptional-mortgage-customer-experience-for-allied-irish-bank/");
            ///   Assert.AreNotEqual(case2, true);
        }

        [TestMethod]
        public void Test_IsJavaScriptOrHashLink()
        {
            bool strHasHash = CrawlerParser.IsJavaScriptOrHashLink("href=\"#AllcontntAggr\"", "#AllcontntAggr");
            bool strHasJS = CrawlerParser.IsJavaScriptOrHashLink("<a href=\"javascript: void(0)\" id=\"loginlink\">login</a>", "javascript:void(0)");
            Assert.AreEqual(strHasHash, true);
            Assert.AreEqual(strHasJS, true);
        }

        [TestMethod]
        public void Test_IsRelativeUrl()
        {
            bool trueCase = CrawlerParser.IsRelativeUrl("/cases/delivering-an-exceptional-mortgage-customer-experience-for-allied-irish-bank/");
            bool falseCase = CrawlerParser.IsRelativeUrl("http://test.wiprodigital.com/cases/delivering-an-exceptional-mortgage-customer-experience-for-allied-irish-bank/");
            Assert.AreEqual(trueCase, true);
            Assert.AreNotEqual(falseCase, true);
        }
    }
}
