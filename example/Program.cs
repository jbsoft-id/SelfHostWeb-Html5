
using jbSoft.Reusable;


public class SimpleWebServer
{
  public static async Task<int> Main(string[] args)
  {
    SelfHostWebLog.WriteLine = Console.WriteLine;
    CancellationTokenSource cancellationTokenSource = new();

    AppDomain.CurrentDomain.ProcessExit += (sender, eventArgs) =>
    {
      SelfHostWebLog.WriteLine("ProcessExit signal received. Initiating shutdown...");
      cancellationTokenSource.Cancel();
    };

    Console.CancelKeyPress += (sender, eventArgs) =>
    {
      SelfHostWebLog.WriteLine("Ctrl+C signal received. Initiating shutdown...");
      cancellationTokenSource.Cancel();
    };

    var server = new HttpServer(7000);

    Shutdown.AddRestartUrl = true;

    await server.Start(cancellationTokenSource);

    return 0;
  }
}