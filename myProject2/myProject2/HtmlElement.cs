using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace myProject2
{
    public class HtmlElement
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public Dictionary<string, string> Attributes { get; set; }=new Dictionary<string, string>();
        public List<string> Classes { get; set; } = new List<string>();
        public string InnerHtml { get; set; }

        public HtmlElement Parent { get; set; }
        public List<HtmlElement> Children { get; set; } = new List<HtmlElement>();


        public IEnumerable<HtmlElement> Descendants()
        {
            
            Queue<HtmlElement> q = new Queue<HtmlElement>();
            q.Enqueue(this);
            while(q.Count!=0)
            {
                HtmlElement current = q.Dequeue();
                yield return current;
                if(current.Children!=null)
                {
                    foreach(HtmlElement c in current.Children )
                    {
                        q.Enqueue(c);
                    }
                }

            }
        }

        public IEnumerable<HtmlElement> Ancestors()
        {

            HtmlElement current = this;
            yield return current;
            while(current.Parent!=null)
            {
                yield return current.Parent;
                current = current.Parent;
            }
        }


    }
}

