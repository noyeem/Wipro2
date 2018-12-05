using System.Collections.Generic;
 
namespace Wipro.Lib
{
    public interface ICrawler
    {
        IEnumerable<Link> GetPageImages(string html);
    }
}
