
using jbSoft.Reusable;


[HttpUri("/logo.png")]
public class Logo : HttpTransaction
{
  public override Task<bool> Process()
  {
    LoadContentFromResource("logo.png");

    return Task.FromResult(true);
  }
} 