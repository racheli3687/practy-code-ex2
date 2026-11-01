

using myProject2;

var serializer = new HtmlSerializer();

var html = await serializer.Load("https://example.com");
var dom = serializer.Serialize(html);

var selector = Selector.FromQueryString("div p");
var result = dom.FindElements(selector);

Console.WriteLine($"Found {result.Count} elements");

foreach (var element in result)
{
    Console.WriteLine($"<{element.Name}> id={element.Id} classes={string.Join(",", element.Classes)}");
}