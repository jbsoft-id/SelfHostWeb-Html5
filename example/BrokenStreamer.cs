
using System.Text.Json;

using jbSoft.Reusable;


[HttpUri("/brokenstreamer")]
public class BrokenStreamer : HttpOperation, IHttpStream
{
  public string? Id { get; set; }


  public override Task<bool> Process()
  {
    throw new Exception("The clock struck 13.");
  }
}