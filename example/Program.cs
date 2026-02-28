
using jbSoft.Reusable;


public class SimpleWebServer
{
  public static async Task<int> Main(string[] args)
  {
    SelfHostWebLog.WriteLine = Console.WriteLine;
    
    var server = new HttpServer(7000);

    Shutdown.AddRestartUrl = true;

    await server.Start();

    return 0;
  }
}
