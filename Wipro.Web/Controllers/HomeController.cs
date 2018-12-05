using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Wipro.Lib;
using Wipro.Web.Models;

namespace Wipro.Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult CrawlWipro()
        {
            return View( );
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult CrawlWipro(CrawlWiproFormModel model)
        {
            //default to wiprodigital.com
            if(string.IsNullOrEmpty(model.url))
                model.url = "https://wiprodigital.com";

            //start crawler
            var crawler = new Crawler(model.url, model.depth);
            var result = crawler.ExtractAll(model.url, true).ToList();

            return View(result);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> CrawlWiproAsync(CrawlWiproFormModel model)
        {
            //default to wiprodigital.com
            if (string.IsNullOrEmpty(model.url))
                model.url = "https://wiprodigital.com";

            //start crawler
            var crawler = new Crawler(model.url, model.depth);
            var result = await crawler.GetAllPageLinksAsync(model.url, true);

            return View(result.ToList());
        }


        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
