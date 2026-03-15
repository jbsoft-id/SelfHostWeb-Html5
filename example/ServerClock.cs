
using jbSoft.Reusable;


[HttpUri("/serverclock")]
public class ServerClock : AppTemplateBase
{
  public async override Task<bool> Process()
  {
    View = $@"
    <h1>Self Hosted Web Server</h1>
    <h2>Server Clock</h2>
    <br>
    <h1 id=""clockdisplay"">HH:MM:SS</h1>
    <p>This clock demonstrates a streaming operation issuing Server Sent Events (SSEs).</p>
    <p>If you stop the server, leaving the browser open, the clock will stop updating.
  Restart the server and the clock picks up again.</p>
    
    <script>
  const eventSource = new EventSource('/clockstreamer');

  eventSource.onopen = () => {{console.log('SSE connection opened')}};

  eventSource.onmessage = (event) => {{
    document.getElementById('clockdisplay').innerHTML = event.data;
  }}

  eventSource.onerror = () => {{
    if (eventSource.readyState === EventSource.CONNECTING) {{
      console.log('Reconnecting...');
    }}
  }};

</script>
";

    return await base.Process();
  }
}