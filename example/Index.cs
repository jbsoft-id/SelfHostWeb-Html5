
using jbSoft.Reusable;

[HttpUri("/")]
[HttpUri("/index.html")]
public class Index : AppTemplateBase
{
  public async override Task<bool> Process()
  {
    View = $@"
    <h1>Self Hosted Web Server</h1>
    <h2>Index</h2>
    <br>
    <h1 id=""clockdisplay"">HH:MM:SS</h1>
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
    </div>
    
    <script>
  const eventSource = new EventSource('/clockstreamer');

  eventSource.onopen = () => {{console.log('SSE connection opened')}};

  eventSource.onmessage = (event) => {{
    const payload = JSON.parse(event.data);
    document.getElementById('clockdisplay').innerHTML = payload;
  }}

  eventSource.onerror = () => {{
    if (eventSource.readyState === EventSource.CONNECTING) {{
      console.log('Reconnecting...');
    }}
  }};

</script>
";

    return await base.Process();
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