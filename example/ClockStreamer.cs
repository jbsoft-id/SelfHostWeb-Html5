
using System.Text;
using System.Text.Json;
using jbSoft.Reusable;

[HttpUri("/clockstreamer")]
public class ClockStreamer : HttpOperation, IHttpStream
{
  public string? Id { get; set; }


  public override async Task<bool> Process()
  {
    var response = Context.Response;

    while (true)
    {
      byte[] buffer = Encoding.UTF8.GetBytes(
        string.Format("data: {0}\n\n", JsonSerializer.Serialize(DateTime.Now.ToString("HH:mm:ss"))));

      response.OutputStream.Write(buffer, 0, buffer.Length);
      response.OutputStream.Flush();

      await Task.Delay(1000);
    }
  }
}
