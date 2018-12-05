using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Wipro.Web.Models
{
    public class CrawlWiproFormModel
    {
        public string url { get; set; }
        public int depth { get; set; }
    }
}
