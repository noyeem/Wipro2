using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Wipro.Api.Models
{
    public class CrawlWiproFormModel
    {
        public string url { get; set; }
        public int depth { get; set; }
    }
}
