using NUnit.Framework;

namespace jbSoft.Reusable.Tests
{
  public class TestableHttpServer : HttpServer
  {
    public TestableHttpServer(int port = 0) : base(port) { }

    private CancellationTokenSource _cancellationTokenSource = new();

    public void TestableStart(bool? startBrowser = null)
    {
      if (startBrowser == null)
      {
        Task.Run(() => { Start(_cancellationTokenSource); });
      }
      else
      {
        Task.Run(() => { Start(_cancellationTokenSource, (bool)startBrowser); });
      }

      for (int i = 1; i < 10; i++)
      {
        Console.WriteLine($"Act {i}");
        if (IsListening)
        {
          break;
        }
        Thread.Sleep(100);
      }

    }

    public void Stop()
    {
      _cancellationTokenSource.Cancel();
      
      for (int i = 1; i < 10; i++)
      {
        Console.WriteLine($"Teardown {i}");
        if (!IsListening)
        {
          break;
        }

        Thread.Sleep(100);
      }
    }

    public bool HasStartBrowserInitiated { get; private set; } = false;

    public string ListenOn { get; private set; } = string.Empty;

    protected override void StartBrowser(string listenOn)
    {
      HasStartBrowserInitiated = true;
      ListenOn = listenOn;
    }
  }


  [TestFixture]
  class HttpServerTests
  {
    private TestableHttpServer? _httpServer = null;

    [TearDown]
    public void TearDown()
    {
      _httpServer?.Stop();
      _httpServer = null;
    }


    [Test]
    public void Start_StartBrowserNull_StartBrowserIsNotInitiated()
    {
      // Arrange
      _httpServer = new TestableHttpServer(7000);

      // Act
      _httpServer.TestableStart();

      // Assert
      Assert.Multiple(() =>
      {
        Assert.That(_httpServer.IsListening, Is.True);
        Assert.That(_httpServer.HasStartBrowserInitiated, Is.False);
        Assert.That(_httpServer.ListenOn, Is.Empty);
      });
    }

    [Test]
    public async Task Start_StartBrowserFalse_StartBrowserIsNotInitiated()
    {
      // Arrange
      _httpServer = new TestableHttpServer(7000);

      // Act
      _httpServer.TestableStart(startBrowser: false);

      // Assert
      Assert.Multiple(() =>
      {
        Assert.That(_httpServer.IsListening, Is.True);
        Assert.That(_httpServer.HasStartBrowserInitiated, Is.False);
        Assert.That(_httpServer.ListenOn, Is.Empty);
      });
    }

    [Test]
    public void Start_StartBrowserTrue_StartBrowserIsInitiated()
    {
      // Arrange
      _httpServer = new TestableHttpServer(7000);

      // Act
      _httpServer.TestableStart(startBrowser: true);

      // Assert
      Assert.Multiple(() =>
      {
        Assert.That(_httpServer.IsListening, Is.True);
        Assert.That(_httpServer.HasStartBrowserInitiated, Is.True);
        Assert.That(_httpServer.ListenOn, Is.EqualTo("http://localhost:7000/"));
      });
    }
  }
}
