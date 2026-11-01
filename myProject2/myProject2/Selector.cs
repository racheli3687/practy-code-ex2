using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace myProject2
{
    public class Selector
    {
        public string TagName { get; set; }
        public string Id { get; set; }
        public List<string> Classes { get; set; } = new List<string>();

        public Selector Parent { get; set; }
        public Selector Child { get; set; }

        static public Selector FromQueryString(string query)
        {
            var levels = query.Split(" ", StringSplitOptions.RemoveEmptyEntries);
            Selector root = null;
            Selector current =null;

            foreach (var level in levels)
            {
                var parts = Regex.Split(level, @"(?=[#.])");
                Selector selector = new Selector();

                foreach (var part in parts)
                {
                    if (String.IsNullOrEmpty(part))
                        continue;
                    if (part.StartsWith("#"))
                    {
                        selector.Id = part.Substring(1);
                    }
                    else if (part.StartsWith("."))
                    {
                        selector.Classes.Add(part.Substring(1));
                    }
                    else if (HtmlHelper.Instance.HtmlTags.Contains(part.ToLower()))
                    {
                        selector.TagName = part.ToLower();

                    }
                }
                if (root == null)
                {
                    root = selector;
                    
                }
                else
                {
                    current.Child = selector;
                    selector.Parent = current;


                }
                current =selector;
            }
            return root;

        }

        public bool IsMatch(HtmlElement element)
        {
            if (element == null)
                return false;
            if (this.TagName != null && element.Name != this.TagName)
                return false;
            if (this.Id != null && element.Id != this.Id)
                return false;
            foreach (var cls in this.Classes)
            {
                if (!element.Classes.Contains(cls))
                    return false;
            }
            return true;
        }




    }
}
