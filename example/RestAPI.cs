
using System.Text.Json;

using jbSoft.Reusable;


[HttpUri("/rest.api")]
[HttpUri(@"/rest.api/item/(?<Id>\d+)")]
public class RestApi : HttpTransaction
{
  public string? Id { get; set; }


  public async override Task<bool> Process()
  {
    ContentType = "application/json";
    Content = JsonSerializer.Serialize(new
    {
      HttpMethod = HttpMethod,
      AbsolutePath = AbsolutePath,
      Id = Id,
      QueryString = DumpQueryString(),
      EntityBody = GetRequestBody(),
      status = 1
    }, new JsonSerializerOptions { WriteIndented = true });

    return true;
  }
}