
using System.Net;
using jbSoft.Reusable;

[HttpUri("/")]
[HttpUri("/index.html")]
public class Index : AppTemplateBase
{
  public override bool Process()
  {
    View = $@"
    <h1>Self Hosted Web Server</h1>
    <h2>Index</h2>
    <p>
    This example demonstrates working with Query Strings.  Further testing can be done by altering the Query String in the
    browser address bar.
    </p>
    <h3>HttpTransaction Method/Property Values</h3>
    <div>
      <pre class=""run"">
Property HttpTransaction.HttpMethod: {HttpMethod}

Property HttpTransaction.QueryStringKeys: {string.Join(", ", QueryStringKeys ?? [])}

Method HttpTransaction.DumpQueryString():
{DumpQueryString()}
    
Method HttpTransaction.GetQueryStringValue(""NonExistentKey"") = {DumpGetQueryStringValue("NonExistentKey")}
Method HttpTransaction.GetQueryStringValue(""KeyWithNoVal"") = {DumpGetQueryStringValue("KeyWithNoVal")}
Method HttpTransaction.GetQueryStringValue(""KeyWithMultiVals"") = {DumpGetQueryStringValue("KeyWithMultiVals")}
      </pre>
    </div>";

    return base.Process();
  }


  private string DumpGetQueryStringValue(string key)
  {
    var result = "*NULL*";

    var vals = GetQueryStringValue(key);
    if (vals != null)
    {
      result = "'" + string.Join("', '", vals) + "'";
    }

    return result;
  }
}