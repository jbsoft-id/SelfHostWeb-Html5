
using jbSoft.Reusable;


[HttpUri("/logo.png")]
public class Logo : HttpTransaction
{
  public async override Task<bool> Process()
  {
    LoadContentFromResource("logo.png");

    return true;
  }
}