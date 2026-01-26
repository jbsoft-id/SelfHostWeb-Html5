
using jbSoft.Reusable;


[HttpUri("/resttests")]
public class RestTests : AppTemplateBase
{
  public override bool Process()
  {
    View = FetchTemplateFromResource("RestTests.html");

    return base.Process();
  }
}