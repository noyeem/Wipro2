using System;
using System.Collections.Generic;
using System.Diagnostics;
using Wipro.Lib;
using System.Linq;
using Microsoft.AspNetCore.StaticFiles;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            string url = "https://wiprodigital.com";
            int maxDepth = 3;
            Crawler crawler = new Crawler(url, maxDepth);
            var links = crawler.ExtractAll(url, true);

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            try
            {
                var agilityTotalRootLinks = Utilities.GetPageLinksViaAgility(url).ToList();

                Console.WriteLine("Starting crawling async...\nURL: {0}", url);
                crawler.ExtractAllAsync(null, true).Wait();
                Console.WriteLine("Total Time Took: {0}\n{1}", stopwatch.Elapsed.TotalSeconds, "-".PadRight(20, '-'));
                Console.WriteLine("Total Links: {0} \nAgility Links: {1}", crawler.SiteNode.Children.Count, agilityTotalRootLinks.Count);
                stopwatch.Stop();
                stopwatch.Reset();
                stopwatch.Start();
                Console.WriteLine("Starting crawling sync...\nURL: {0}", url);
                crawler = new Crawler(url, maxDepth);
                crawler.ExtractAll(null, true);
                stopwatch.Stop();
                Console.WriteLine("Total Time Took: {0}\n{1}", stopwatch.Elapsed.TotalSeconds, "-".PadRight(20, '-'));
                Console.WriteLine("Total Links: {0} \nAgility Links: {1}", crawler.SiteNode.Children.Count, agilityTotalRootLinks.Count);
            }
            catch(System.Exception ex)
            {

            }


            PrintTreeNode print = new PrintTreeNode();
            print.Print(crawler.SiteNode);


            Console.ReadLine();
        }
    }
}
