
using System.Net;

using jbSoft.Reusable;


[HttpUri("/missingresource")]
public class MissingResource : AppTemplateBase
{
  public async override Task<bool> Process()
  {
    LoadContentFromResource("missingresource.ico");

    return await base.Process();
  }
}