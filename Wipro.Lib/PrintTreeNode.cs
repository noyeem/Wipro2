using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
namespace Wipro.Lib
{
    public class PrintTreeNode
    {
        StreamWriter sw;
        public PrintTreeNode()
        {
            //sw = new StreamWriter(@"C:\test.txt");
        }

        string path = null;
        /*
        public List<Link> GetSiteLinksTree(TreeNode<Link> links)
        {
            foreach (var item in links)
            {
                item.Children = GetSiteLinksChildren(links, item.Data).ToList();
            }

            //return links;

            return links.Children.ToList();
        }

        private List<Link> GetSiteLinksChildren(TreeNode<Link> allLinks, Link link)
        {
            //base case
            if (allLinks.All(b => b.Data.Href != link.Href)) return null;

            //recursive case
            link.Links = allLinks
                .Where(b => b.Data.Href == link.Href).toar
                .ToList();

            foreach (var item in link.Links)
            {
                item.Links = GetSiteLinksChildren(allLinks, item).ToList();
            }

            return link.Links;
        }*/
        public void Print(TreeNode<Link> parent)
        {
            var fileInfo = new System.IO.FileInfo(parent.Data.Href);
            path = path + fileInfo.Name + " | ";
            Console.WriteLine("Directory: " + path);
            Console.WriteLine("Listing links of URL : \"{0}\"\n{1}", parent.Data.Href, "".PadRight(30, '-'));
            foreach(var child in parent.Children)
            {
                Console.WriteLine("Parent: {0} -> ", child.Parent.Data.Href);
                Console.WriteLine("\tLink: {0}", child.Data.Href);
                Print(child);
            }
            Console.WriteLine("End of links for URL : \"{0}\"\n{1}", parent.Data.Href, "".PadRight(30, '='));
        }

       
    }
}
