using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Wipro.Lib
{
    public static class CrawlerParser
    {
        /// <summary>
        /// List of static file extensions
        /// todo: should be read from config
        /// </summary>
        private static readonly string[] staticFileExtensions = { ".css", ".ico", ".pdf", ".json", ".xml", ".jpg", ".jpeg", ".bmp", ".gif", ".png", ".js" }; //  etc

        /// <summary>
        /// Get a list of Link objects from html
        /// </summary>
        /// <param name="html">Page Html</param>
        /// <returns>List of Link objects</returns>
        public static IEnumerable<Link> GetPageLinks(string rootUrl, string html)
        {
            if (string.IsNullOrEmpty(html))
                return null;

            var list = new List<Link>();
            string strRegex = @"href\s*=\s*(?:\""(?<1>[^\""]*)\""|(?<1>\\S+))"; //regex: searches anchor tags
            var regex = new Regex(strRegex, RegexOptions.IgnoreCase | RegexOptions.Multiline);

            //On match found parse the link and store it as Link
            if (regex != null && regex.IsMatch(html))
            {
                foreach (Match match in regex.Matches(html))
                {
                    var link = CrawlerParser.ParseLink(rootUrl, match.Value);

                    //Only add if Link(:href) does not exist in the list 
                    if (!list.Exists(x => x.Href.Equals(link.Href)))
                        list.Add(link);
                }
            }

            return list;
        }
        /// <summary>
        /// Parse Link object from method Crawler.GetPageLinks() retunred html
        /// </summary>
        /// <param name="rootUri"></param>
        /// <param name="raw"></param>
        /// <returns></returns>
        public static Link ParseLink(string rootUri, string raw)
        {
            //Parse URI from  regex match value and perform some cleanups 
            var href = CrawlerParser.ParseTextFromQuote(raw);

            // sets true if wipro site
            bool isWipro = CrawlerParser.IsWipro(href);

            // sets true if url is relative 
            bool isRelative = CrawlerParser.IsRelativeUrl(href);

            // sets true javascript links
            bool isJs = CrawlerParser.IsJavaScriptOrHashLink(raw, href);

            // sets true if file is static
            bool isStatic = CrawlerParser.IsStatic(href);

            //get filename from url
            string extension = string.Empty;
            string fileName = null;
            if (isWipro && !isJs && !isStatic)
                fileName = Utilities.GetFileNameFromUrl(rootUri, href, out extension);

            //get file-type from extension 
            var type = Utilities.GetHrefType(extension);

            // group[1] value contains Title
            //var title = match.Groups.Count > 0 ? match.Groups[1].Value : "";

            //generate Link object 
            var link = new Link
            {
                Raw = raw,                          // store raw search 
                Href = href,                                // hyperlink
                IsRelativeUrl = isRelative,
                IsWipro = isWipro,
                IsJavaScript = isJs,
                IsStatic = isStatic,
                Type = type,
                FileName = fileName
            };
            return link;
        }

        /// <summary>
        /// Parses text from first occurrence of quoted text. 
        /// Example 1: String {This is a "Test"}  will return value {Test}
        /// Example 2: <a href="http://www.wipro.com">Wipro</a> will return value http://www.wipro.com
        /// </summary>
        /// <returns>Text inside the quotes</returns>
        /// <param name="input">string containing quoted text ie. <a href="http://www.wipro.com">Wipro</a>  
        /// Ex. <a href="/content/nexus/en.html" target="_blank" class="banner-button geolocbtn">Global Site</a> </param>
        public static string ParseTextFromQuote(string input)
        {
            //RegEx: Grabbing values between quotation marks
            var strRegex = @"([""'])(?:(?=(\\?))\2.)*?\1";
            var regex = new Regex(strRegex, RegexOptions.IgnoreCase | RegexOptions.Multiline);
            if (regex.IsMatch(input))
            {
                foreach (Match match in regex.Matches(input))
                {
                    //returns link & strips off quotes else return empty string
                    return match.Groups.Count > 0 && !match.Groups[0].Value.Contains("javascript:") ? match.Groups[0].Value.Replace("\"", "") : "";
                }
            }
            else
            {
                return input;
            }
            return null;
        }

        /// <summary>
        /// Determines if url is wipro
        /// </summary>
        /// <param name="href">Input url string</param>
        /// <returns>true if host == wipro domain</returns>
        public static bool IsWipro(string href)
        {
            if (IsRelativeUrl(href))
                return true;

            var url = new Uri(href);
            if (url.Host == "wiprodigital.com")
                return true;

            return false;
        }

        /// <summary>
        /// Checks using file extension againt a list of hardcoded static extension list 
        /// </summary>
        /// <param name="href">Input URL</param>
        /// <returns>true if url is static file</returns>
        public static bool IsStatic(string href)
        {
            bool result = false;
            var ext = Utilities.GetFileExtensionFromUrl(href);

            if (staticFileExtensions.Contains(ext))
            {
                return true;
            }

            return result;
        }

        public static string GetAbsoluteUri(string rootUrl, string relativeUrl)
        {
            if (!IsRelativeUrl(relativeUrl))
                return relativeUrl;

            if (relativeUrl.StartsWith("/"))
                relativeUrl = relativeUrl.Substring(1);
            return rootUrl + relativeUrl;
        }

        /// <summary>
        /// Check Link contains java script.
        /// </summary>
        /// <returns><c>true</c>, if link contains java script, <c>false</c> otherwise.</returns>
        /// <param name="raw">Raw string returned from Parsing Links from page html</param>
        public static bool IsJavaScriptOrHashLink(string raw, string href)
        {
            bool result = false;
            if (!string.IsNullOrEmpty(raw) && raw.Contains("javascript:"))
                return true;

            if (!string.IsNullOrEmpty(href) && href.StartsWith("#"))
                return true;

            return result;
        }

        /// <summary>
        /// Checks if URL is relative or absolute
        /// </summary>
        /// <param name="url">Input URL</param>
        /// <returns>return true if url is relative</returns>
        public static bool IsRelativeUrl(string url)
        {
            Uri result = null;
            return Uri.TryCreate(url, UriKind.Relative, out result);
        }

    }
}