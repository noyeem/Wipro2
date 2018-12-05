using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wipro.Lib;

namespace Wipro.Web.ViewComponents
{
    [ViewComponent(Name = "DisplayLinks")]
    public class DisplayLinksViewComponent : ViewComponent
    {
        public DisplayLinksViewComponent( )
        {
        }

        public async Task<IViewComponentResult> InvokeAsync(string url, bool isRoot=true)
        {
            var crawler = new Crawler(url, 3);
            var links = await crawler.GetAllPageLinksAsync(url, isRoot);
            List<Link> _links = null;
            
                if (links != null) _links = links.ToList();
             
            return View(_links);
        }
    }
}