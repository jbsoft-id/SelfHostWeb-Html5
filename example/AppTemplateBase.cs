
using jbSoft.Reusable;


public class AppTemplateBase : HttpTransaction
{
  public string Style { get; set; } = "";
  public string View { get; set; } = "";
  public string Script { get; set; } = "";

  private PlaceholderValues placeholders = [];


  public AppTemplateBase()
  {
    placeholders["Title"] = "Self Hosted Web Server";

    placeholders["Menu"] = @"
    <p>Web Examples</p>
      <a href=""index.html?KeyWithMultiVals=val1&KeyWithNoVal&KeyWithMultiVals=val2&KeyWithMultiVals=val3&test=1234&bool"">GET with Query String</a>
      <a href=""postform.html"">Post Form</a>
      <a href=""testhtml5"">Test Html5</a>
    <p>Async / REST Examples</p>
      <a href=""resttests"">REST Tests</a>
      <a href=""resthtml5"">Async-REST w/ Html5</a>
    <p>Error Tests</p>
      <a href=""missingresource"">Missing Resource</a>
      <a href=""nonexistanturl"">Non-Existant URL</a>
    <p>Internal Ops</p>
      <a href=""testtemplate"">GetTemplateFromResource()</a>
      <a href=""shutdown"">Shutdown</a>";

    Style = @"
    h1, h2 { display: inline-block; margin-top: 1px; margin-bottom: 10px; }
    h2::before { content: "" \2013 ""; }
    PRE.run {
      background-color: aquamarine;
      color: black;
      border: 3px solid black;
      border-radius: 5px;
      padding: 6px;
    }";
  }


  public override Task<bool> Process()
  {
    placeholders["Style"] = Style;
    placeholders["View"] = View;
    placeholders["JavaScript"] = Script;

    Content = FetchTemplateFromResource("AppTemplate.html", placeholders, reportMissingPlaceholders: false);

    return Task.FromResult(true);
  }
}