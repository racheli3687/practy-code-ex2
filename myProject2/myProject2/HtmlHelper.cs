using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace myProject2
{
    internal class HtmlHelper
    {
        private static readonly HtmlHelper _instance = new HtmlHelper();
        static public HtmlHelper Instance => _instance; 
        public string[] HtmlTags { get; set; }
        public string[] HtmlVoidTags { get; set; }


        private HtmlHelper()
        {
            string htmlTagsJson = File.ReadAllText("Seed/HtmlTags.json");
            string htmlVoidTagsJson = File.ReadAllText("Seed/HtmlVoidTags.json");

            this.HtmlTags= JsonSerializer.Deserialize<string[]>(htmlTagsJson);
            this.HtmlVoidTags = JsonSerializer.Deserialize<string[]>(htmlVoidTagsJson);
            
        }
    }
}
