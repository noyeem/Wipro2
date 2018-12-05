using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using HtmlAgilityPack;
using Microsoft.AspNetCore.StaticFiles;
using System.IO;

namespace Wipro.Lib
{
    public static class Utilities
    {
        public static string GetHrefType(string extension)
        {
            string contentType;
            new FileExtensionContentTypeProvider().TryGetContentType(extension, out contentType);
            return contentType;
        }

        public static string RemoveTrailingSlash(string url)
        {
            if (url.EndsWith("/"))
                url = url.Substring(0, url.Length - 1);
            return url;
        }

        public static string GetFileNameFromUrl(string rootUrl, string href, out string extension)
        {
            Uri uri = null;
            Uri rootUri = new Uri(rootUrl);
            FileInfo fi = null;
            string filename = null;
            extension = null;

            if (href.Equals("/"))
                return "/";

            if (rootUrl == href)
                return "/";

            try
            {
                if (Uri.TryCreate(href, UriKind.RelativeOrAbsolute, out uri))
                {
                    if (rootUri.Host != uri.Host)
                        return null;

                    filename = uri.GetLeftPart(UriPartial.Path);
                    fi = new FileInfo(filename);
                    filename = fi.Name;
                    extension = fi.Extension;

                    filename = uri.Segments.Last();
                    if (filename == "/")
                    {
                        if (uri.AbsoluteUri.EndsWith("/"))
                        {
                            if (rootUrl == uri.AbsoluteUri)
                                return "/";
                            var tmpUrl = uri.AbsoluteUri.Substring(0, uri.AbsoluteUri.Length - 1);
                            if (Uri.TryCreate(tmpUrl, UriKind.RelativeOrAbsolute, out uri))
                            {
                                filename = uri.Segments.Last();
                                if (filename.EndsWith("/"))
                                    filename = filename.Substring(0, filename.Length - 1);
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {

                Console.WriteLine(ex.Message);
            }
             
            return filename;
        }

        /// <summary>
        /// Get only File Extension from URL
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string GetFileExtensionFromUrl(string url)
        {
            url = url.Split('?')[0];
            url = url.Split('/').Last();
            return url.Contains('.') ? url.Substring(url.LastIndexOf('.')) : "";
        }

        /// <summary>
        /// Get a list of all anchor tags via HtmlAgilityPacks
        /// </summary>
        /// <param name="url">URL to query for anchor tags</param>
        /// <returns>Anchor tags List</returns>
        public static IEnumerable<string> GetPageLinksViaAgility(string url)
        {
            var doc = new HtmlWeb().Load(url);
            var linkTags = doc.DocumentNode.Descendants("link");
            var linkedPages = doc.DocumentNode.Descendants("a")
                                              .Select(a => a.GetAttributeValue("href", null))
                                              .Where(u => !String.IsNullOrEmpty(u))
                                              .Distinct();
            return linkedPages;
        }
    }
}
