
using jbSoft.Reusable;


[HttpUri("/logo.png")]
public class Logo : HttpTransaction
{
  public override bool Process()
  {
    LoadContentFromResource("logo.png");

    return true;
  }
}