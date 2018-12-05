using System;
using System.Linq;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Net;
using System.Threading;
using System.Diagnostics;

namespace Wipro.Lib
{
    public class Crawler : ICrawler
    {
        public string RootUri { get; set; }
        private static readonly string[] blackListUrls = { "/" };
        private readonly int MAX_DEPTH_COUNT;

        private AsyncLocal<int> depthCounter = null;

        int depthCounter2 = 0;

        public List<Link> Links { get; set; }


        public TreeNode<Link> SiteNode { get; set; }

        public Crawler(string rootUri, int maxDepth)
        {
            this.RootUri = rootUri;
            this.depthCounter = new AsyncLocal<int>();
            //add a trailing slash to url
            if (!this.RootUri.EndsWith("/"))
                this.RootUri = this.RootUri + "/";

            MAX_DEPTH_COUNT = maxDepth;

            var wiproRoot = new Link { Href = rootUri };
            this.SiteNode = new TreeNode<Link>(wiproRoot);
            this.Links = new List<Link>();
        }


        #region Sync Methods
        public IEnumerable<Link> ExtractAll(string url, bool isRoot)
        {
            //sort, remove duplicates 
            IEnumerable<Link> links = null;
            if (isRoot)
            {
                var html = GetRootPageHtml();
                if (string.IsNullOrEmpty(html))
                    return links;

                links = FilterLinks(CrawlerParser.GetPageLinks(url, html));

                foreach (var link in links)
                {
                    if(!this.Links.Contains(link))
                        this.Links.Add(link);
                    SiteNode.AddChild(link);
                        ExtractAll(link.Href, false);
                }
            }
            else
            {
                url = CrawlerParser.GetAbsoluteUri(this.RootUri, url);

                var html = GetPageHtml(url);
                if (string.IsNullOrEmpty(html))
                    return links;

                links = CrawlerParser.GetPageLinks(this.RootUri, html);
                if (links != null)
                    links = FilterLinks(links);
                else
                    return links;

                foreach (var link in links)
                {
                    var node = this.SiteNode.FindTreeNode(x => x.Data.Href.Equals(link.Href));

                    if (node == null)
                    {
                        this.SiteNode.AddChild(link);
                        if (depthCounter2 < MAX_DEPTH_COUNT)
                        {
                            depthCounter2++;
                            this.Links.AddRange( ExtractAll(link.Href, false));
                        }
                    }
                    else
                    {
                        if (depthCounter2 < MAX_DEPTH_COUNT)
                        {
                            depthCounter2++;
                            node.AddChild(link);
                            link.Links = ExtractAll(link.Href, false);
                        }
                    }
                }
            }
            return links;
        }

        public IEnumerable<Link> FilterLinks(IEnumerable<Link> links)
        {
            links = links
                .Where(x => !string.IsNullOrEmpty(x.Href))                  // remove empty
                .Where(x => x.Href.ToLower() != this.RootUri.ToLower())     // remove any root url
                .Where(x => !blackListUrls.Contains(x.Href))                // remove blacklisted urls
                .Where(x => !x.IsJavaScript && !x.IsStatic && x.IsWipro)    // exclude JavaScripts, static files
                .GroupBy(x => x.FileName)                                   // group-by FileName 
                .Select(y => y.First())                                     // only select unique pages
                .Select(y => new Link {
                     FileName = Utilities.RemoveTrailingSlash(y.FileName),
                     Href = Utilities.RemoveTrailingSlash(y.Href),
                     IsImage = y.IsImage,
                     IsJavaScript = y.IsJavaScript,
                     IsRelativeUrl = y.IsRelativeUrl,
                     IsStatic = y.IsStatic,
                     IsWipro = y.IsWipro,
                     Links = y.Links,
                     Raw =y.Raw,
                     Title = y.Title,
                     Type = y.Type 
                }) ;                                    

             

            links = links.Where(x => x.FileName != "embed");
            return links;
        }
        public string GetRootPageHtml()
        {
            return GetPageAsync(this.RootUri).Result;
        }

        public string GetPageHtml(string url)
        {
            string html = string.Empty;

            using (var client = new HttpClient())
            {
                try
                {
                    html = client.GetStringAsync(url).Result;

                }
                catch (AggregateException ae)
                {
                    if (ae.InnerException != null && ae.InnerException is System.Net.Http.HttpRequestException)
                    {
                        var wex = (HttpRequestException)ae.InnerException;
                        //var res = (HttpWebResponse)wex.Response;
                        //if (res.StatusCode == HttpStatusCode.Moved)
                        //{
                        //    string movedUrl = res.Headers["Location"];
                        //    GetPageHtml(movedUrl);
                        //}
                        //else
                        //    throw ae;
                    }
                }
                catch (System.Exception ex)
                {
                    if (ex.InnerException != null && ex.InnerException is System.Net.WebException)
                    {
                        var wex = (WebException)ex.InnerException;
                        var res = (HttpWebResponse)wex.Response;
                        if (res.StatusCode == HttpStatusCode.Moved)
                        {
                            string movedUrl = res.Headers["Location"];
                            GetPageHtml(movedUrl);
                        }
                        else
                            throw ex;
                    }
                }
            }
            return html;
        }

        public IEnumerable<Link> GetPageImages(string html)
        {
            var list = new List<Link>();
            var strRegex = @"<img.+?src=[""'](.+?)[""'].*?>"; //regex: searches image tags
            var regex = new Regex(strRegex, RegexOptions.IgnoreCase | RegexOptions.Multiline);

            //On match found parse the link and store it as Link
            if (regex.IsMatch(html))
            {
                foreach (Match match in regex.Matches(html))
                {
                    list.Add(new Link
                    {
                        Raw = match.Value,                                  // store raw search 
                        Href = match.Groups[1].Value,                       // hyperlink
                        IsRelativeUrl = CrawlerParser.IsRelativeUrl(match.Groups[1].Value),           // set to true if wipro site
                        IsImage = true   // javascript links
                    });
                }
            }

            return list;
        }
        #endregion

        #region Async Methods

        private async Task<IEnumerable<Link>> GetRootPageLinksAsync()
        {
            IEnumerable<Link> links = null;
            var html = await GetRootPageHtmlAsync();
            links = CrawlerParser.GetPageLinks(this.RootUri, html);
            if (links != null)
                links = FilterLinks(links);
            else
                return null;

            foreach (var link in links)
            {
                try
                {
                    if (link != null && !string.IsNullOrEmpty(link.Href))
                    {
                        SiteNode.AddChild(link);
                    }
                }
                catch (Exception ex)
                {

                }
            }
            return links;
        }

        private async Task<IEnumerable<Link>> GetChildPagesLinksAsync(string url)
        {
            IEnumerable<Link> links = null;
            url = CrawlerParser.GetAbsoluteUri(this.RootUri, url);
            string html = string.Empty;

            try
            {
                html = await GetPageAsync(url);
            }
            catch (Exception ex)
            {

            }

            links = CrawlerParser.GetPageLinks(this.RootUri, html);
            if (links != null)
                links = FilterLinks(links);
            else
                return null;

            Parallel.ForEach(links, async link =>
            {
                try
                {
                    var node = this.SiteNode.FindTreeNode(x => x.Data.Href.Equals(link.Href));

                    if (node == null)
                    {
                        if (!this.SiteNode.Children.Any(x => x.Data.Href.Equals(link.Href)))
                            this.SiteNode.AddChild(link);
                        if (depthCounter2 < MAX_DEPTH_COUNT)
                        {
                            depthCounter2++;
                            link.Links = await GetChildPagesLinksAsync(link.Href);
                            Debug.WriteLine("Href: {0}\nChildren Count: {1}\nElements Index: {2}\nDepth: {3}", link.Href, SiteNode.Children.Count, SiteNode.ElementsIndex.Count, depthCounter2);
                        }
                    }
                    else
                    {
                        if (depthCounter2 < MAX_DEPTH_COUNT)
                        {
                            depthCounter2++;
                            if (!node.IsRoot && !node.Children.Any(x => x.Data.Href.Equals(link.Href)))
                                node.AddChild(link);
                            link.Links = await GetChildPagesLinksAsync(link.Href);
                            Debug.WriteLine("Href: {0}\nChildren Count: {1}\nElements Index: {2}\nDepth: {3}", link.Href, SiteNode.Children.Count, SiteNode.ElementsIndex.Count, depthCounter2);
                        }
                    }
                }
                catch (Exception ex)
                {

                }

            });

            return links;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="isRoot">
        /// True:
        ///     Get a List of top level anchor tags from the URL
        ///     
        /// False:
        ///     Recurse thru each links  
        /// </param>
        /// <returns></returns>
        public async Task<IEnumerable<Link>> GetAllPageLinksAsync(string url, bool isRoot = true)
        {
            if (isRoot)
            {
                return await GetRootPageLinksAsync();
            }
            else
            {
                return await GetChildPagesLinksAsync(url);
            }
        }

        public async Task ExtractAllAsync(string url, bool isRoot)
        {
            //sort, remove duplicates 
            IEnumerable<Link> links = null;
            if (isRoot)
            {
                var html = await GetRootPageHtmlAsync();
                links = CrawlerParser.GetPageLinks(this.RootUri, html);
                if (links != null)
                    links = links.Where(x => !x.IsJavaScript && !x.IsStatic && x.IsWipro);
                else
                    return;

                foreach (var link in links)
                {
                    try
                    {
                        if (link != null && !string.IsNullOrEmpty(link.Href))
                        {
                            SiteNode.AddChild(link);
                            if (!blackListUrls.Contains(link.Href))
                                await ExtractAllAsync(link.Href, false);
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }
            else
            {
                url = CrawlerParser.GetAbsoluteUri(this.RootUri, url);
                string html = string.Empty;

                try
                {
                    html = await GetPageAsync(url);
                }
                catch (Exception ex)
                {

                }

                links = CrawlerParser.GetPageLinks(this.RootUri, html);
                if (links != null)
                    links = links.Where(x => !x.IsJavaScript && !x.IsStatic && x.IsWipro);
                else
                    return;

                foreach (var link in links)
                {
                    try
                    {
                        var node = this.SiteNode.FindTreeNode(x => x.Data.Href.Equals(link.Href));

                        if (node == null)
                        {
                            if (!this.SiteNode.Children.Any(x => x.Data.Href.Equals(link.Href)))
                                this.SiteNode.AddChild(link);
                            if (!blackListUrls.Contains(link.Href) && link.Href.ToLower() != this.RootUri.ToLower() && depthCounter2 < MAX_DEPTH_COUNT)
                            {
                                depthCounter2++;
                                await ExtractAllAsync(link.Href, false);
                                Console.WriteLine("Href: {0}\nChildren Count: {1}\nElements Index: {2}", link.Href, SiteNode.Children.Count, SiteNode.ElementsIndex.Count);
                            }
                        }
                        else
                        {
                            if (!blackListUrls.Contains(link.Href) && link.Href.ToLower() != this.RootUri.ToLower() && depthCounter2 < MAX_DEPTH_COUNT)
                            {
                                depthCounter2++;
                                if (!node.IsRoot && !node.Children.Any(x => x.Data.Href.Equals(link.Href)))
                                    node.AddChild(link);
                                await ExtractAllAsync(link.Href, false);
                            }
                        }
                    }
                    catch (Exception ex)
                    {

                    }

                }
            }

        }

        public async Task<string> GetRootPageHtmlAsync()
        {
            return await GetPageAsync(this.RootUri);
        }
        public async Task<string> GetPageAsync(string url)
        {
            using (var client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(url);
                    if (response.StatusCode == HttpStatusCode.Moved || response.StatusCode == HttpStatusCode.MovedPermanently)
                    {
                        await GetPageAsync(response.Headers.Location.ToString());
                    }

                    return await client.GetStringAsync(url);
                }
                catch (System.Exception ex)
                {

                }
            }
            return null;
        }
        #endregion
    }
}