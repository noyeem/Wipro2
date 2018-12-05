using System;
using System.Collections.Generic;

namespace Wipro.Lib
{
    public class Link
    {
        public string FileName { get; set; }
        public string Title { get; set; }
        public string Href { get; set; }
        public bool IsImage { get; set; }
        public bool IsRelativeUrl { get; set; }
        public bool IsJavaScript { get; set; }
        public string Raw { get; set; }
        public bool IsStatic { get; set;}
        public string Type { get; set; }

        public bool IsWipro { get; set; }

        public IEnumerable<Link> Links { get; set; }
    }
}
