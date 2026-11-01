using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace myProject2
{
    internal class HtmlSerializer
    {


        string[] htmlTags = HtmlHelper.Instance.HtmlTags;
        string[] htmlVoidTags = HtmlHelper.Instance.HtmlVoidTags;
        public async Task<string> Load(string url)
        {
            HttpClient client = new HttpClient();
            var response = await client.GetAsync(url);
            var html = await response.Content.ReadAsStringAsync();
            return html;
        }
        
            
        public List<string> SplitToParts(string html)
        {
            var cleanHtml = Regex.Replace(html, @"\s+"," ");
            var parts = Regex.Split(cleanHtml, @"(<[^>]*>)")
                .Select(p=>p.Trim())
                .Where(t=>t.Length>0)
                .ToList();
            return parts;
        }



        public HtmlElement Serialize(string html)
        {
            ArgumentNullException.ThrowIfNull(html);

            var root = new HtmlElement { Name = "root" };
            var stack = new Stack<HtmlElement>();
            stack.Push(root);

            var parts = SplitToParts(html);

            foreach (var part in parts)
            {
                if (string.IsNullOrWhiteSpace(part))
                    continue;

                var current = stack.Peek();

                // טקסט רגיל
                if (!part.StartsWith("<"))
                {
                    current.InnerHtml += part;
                    continue;
                }

                // הערות ו-DOCTYPE
                if (part.StartsWith("<!--") ||
                    part.StartsWith("<!", StringComparison.Ordinal) ||
                    part.StartsWith("<?", StringComparison.Ordinal))
                {
                    continue;
                }

                // הגנה מפני חלק שאינו תגית שלמה
                if (!part.EndsWith(">"))
                {
                    current.InnerHtml += part;
                    continue;
                }

                var content = part.Substring(1, part.Length - 2).Trim();

                if (content.Length == 0)
                {
                    current.InnerHtml += part;
                    continue;
                }

                // תגית סוגרת
                if (content.StartsWith("/"))
                {
                    var closingName = content.Substring(1)
                        .Split((char[])null, StringSplitOptions.RemoveEmptyEntries)
                        .FirstOrDefault()?
                        .ToLowerInvariant();

                    if (closingName == null)
                        continue;

                    // סוגרים רק אם התגית תואמת לראש המחסנית
                    if (stack.Count > 1 &&
                        stack.Peek().Name.Equals(
                            closingName,
                            StringComparison.OrdinalIgnoreCase))
                    {
                        stack.Pop();
                    }

                    continue;
                }

                // חילוץ שם התגית
                var tagName = content
                    .Split((char[])null, StringSplitOptions.RemoveEmptyEntries)
                    .FirstOrDefault();

                if (string.IsNullOrEmpty(tagName))
                {
                    current.InnerHtml += part;
                    continue;
                }

                bool hasSelfClosingSlash = content.TrimEnd().EndsWith("/");

                tagName = tagName.TrimEnd('/').ToLowerInvariant();

                if (tagName.Length == 0)
                {
                    current.InnerHtml += part;
                    continue;
                }

                // יצירת האלמנט והמאפיינים שלו
                var element = new HtmlElement
                {
                    Name = tagName,
                    Parent = current
                };

                var attributesText = content.Substring(
                    content.IndexOf(tagName, StringComparison.OrdinalIgnoreCase)
                    + tagName.Length);

                ParseAttributes(element, attributesText);

                // הוספה לעץ
                current.Children.Add(element);

                // תגיות void או תגיות self-closing
                bool isVoid = HtmlHelper.Instance.HtmlVoidTags
                    .Contains(tagName);

                if (!hasSelfClosingSlash && !isVoid)
                {
                    stack.Push(element);
                }
            }

            return root;
        }

        private void ParseAttributes(
            HtmlElement element,
            string attributesText)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            if (string.IsNullOrWhiteSpace(attributesText))
                return;

            // תומך בערכים במרכאות כפולות, בגרשיים בודדים
            // וגם בערכים ללא מרכאות.
            var regex = new Regex(
                """
        (?<name>[\w:-]+)
        (?:\s*=\s*
            (?:
                "(?<double>[^"]*)"
                |
                '(?<single>[^']*)'
                |
                (?<unquoted>[^\s"'=<>`]+)
            )
        )?
        """,
                RegexOptions.IgnorePatternWhitespace);

            foreach (Match match in regex.Matches(attributesText))
            {
                var name = match.Groups["name"].Value.ToLowerInvariant();

                if (string.IsNullOrWhiteSpace(name))
                    continue;

                string value;

                if (match.Groups["double"].Success)
                    value = match.Groups["double"].Value;
                else if (match.Groups["single"].Success)
                    value = match.Groups["single"].Value;
                else if (match.Groups["unquoted"].Success)
                    value = match.Groups["unquoted"].Value;
                else
                    value = string.Empty; // מאפיין בוליאני, למשל disabled

                // לא דורסים מאפיין קודם אם הוא מופיע שוב.
                if (!element.Attributes.ContainsKey(name))
                    element.Attributes[name] = value;

                if (name == "id" && element.Id == null)
                    element.Id = value;

                if (name == "class" && element.Classes.Count == 0)
                {
                    element.Classes = value
                        .Split((char[])null, StringSplitOptions.RemoveEmptyEntries)
                        .ToList();
                }
            }
        }


    }
}
