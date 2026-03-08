
using jbSoft.Reusable;


[HttpUri("/resttests")]
public class RestTests : AppTemplateBase
{
  public async override Task<bool> Process()
  {
    View = FetchTemplateFromResource("RestTests.html");

    return await base.Process();
  }
}