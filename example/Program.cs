
using jbSoft.Reusable;


public class SimpleWebServer
{
  public static int Main(string[] args)
  {
    SelfHostWebLog.WriteLine = Console.WriteLine;
    
    var server = new HttpServer(7000);

    Shutdown.AddRestartUrl = true;

    server.Start();

    return 0;
  }
}
