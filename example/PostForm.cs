
using System.Net;
using System.Web;

using jbSoft.Reusable;


[HttpUri("/postform.html")]
public class PostForm : AppTemplateBase
{
  public override bool Process()
  {
    var entityBody = GetRequestBody() ?? "";
    var placeholderValues = new PlaceholderValues
    {
      {"HttpMethod", HttpMethod},
      {"GetRequestBody", entityBody},
      {"DecodeEntityBody", DecodeEntityBody(entityBody)},
    };

    View = FetchTemplateFromResource("PostForm.html", placeholderValues);

    return base.Process();
  }


  private string DecodeEntityBody(string entityBody)
  {
    var result = "";

    string query = HttpUtility.UrlDecode(entityBody);
    var keyVals = HttpUtility.ParseQueryString(query);

    foreach (var key in keyVals.AllKeys)
    {
      var value = keyVals[key];
      result += $"{key,5}: {value}\n";
    }

    return result;
  }
}