
using System.Net;

using jbSoft.Reusable;


[HttpUri("/missingresource")]
public class MissingResource : AppTemplateBase
{
  public override bool Process()
  {
    LoadContentFromResource("missingresource.ico");

    return base.Process();
  }
}