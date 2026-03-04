using System.Diagnostics;
using System.Net;
using NUnit.Framework;

namespace jbSoft.Reusable.Tests
{
  public class TestableHttpServer : HttpServer
  {
    public bool TryWaitIsListeningState(bool target)
    {
      bool actual = IsListening;

      for (int waitCount = 1; waitCount < 10; waitCount++)
      {
        Console.WriteLine($"TryWaitIsListeningState {waitCount} {target} {actual} {waitCount}...");
        if (actual == target)
        {
          break;
        }
        Thread.Sleep(100);
        actual = IsListening;
      }

      return actual == target;
    }

    public TestableHttpServer(int port = 0) : base(port) { }

    public CancellationTokenSource CancellationTokenSource { get; private set; } = new();

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
      if (_httpServer != null && _httpServer.IsListening)
      {
        _httpServer.CancellationTokenSource.Cancel();
        var stoppedListening = _httpServer.TryWaitIsListeningState(false);
        _httpServer = null;
        Debug.Assert(stoppedListening);
      }
    }

    [Test]
    public void Start_ConstructedWithInvalidPort_ThrowsXXXXX()
    {
      // Arrange
      _httpServer = new TestableHttpServer(700000000);

      // Act & Assert
      Assert.That(() => _httpServer.Start(_httpServer.CancellationTokenSource),
                  Throws.InstanceOf<HttpListenerException>().With.Message.EqualTo("The parameter is incorrect."));

    }

    [Test]
    public void Start_StartBrowserNull_StartBrowserIsNotInitiated()
    {
      // Arrange
      _httpServer = new TestableHttpServer(7000);

      // Act
      Task.Run(() => _httpServer.Start(_httpServer.CancellationTokenSource));

      // Assert
      Assert.Multiple(() =>
      {
        Assert.That(_httpServer.TryWaitIsListeningState(true), Is.True);
        Assert.That(_httpServer.HasStartBrowserInitiated, Is.False);
        Assert.That(_httpServer.ListenOn, Is.Empty);
      });
    }

    [Test]
    public void Start_StartBrowserFalse_StartBrowserIsNotInitiated()
    {
      // Arrange
      _httpServer = new TestableHttpServer(7000);

      // Act
      Task.Run(() => _httpServer.Start(_httpServer.CancellationTokenSource, false));

      // Assert
      Assert.Multiple(() =>
      {
        Assert.That(_httpServer.TryWaitIsListeningState(true), Is.True);
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
      Task.Run(() => _httpServer.Start(_httpServer.CancellationTokenSource, true));

      // Assert
      Assert.Multiple(() =>
      {
        Assert.That(_httpServer.TryWaitIsListeningState(true), Is.True);
        Assert.That(_httpServer.HasStartBrowserInitiated, Is.True);
        Assert.That(_httpServer.ListenOn, Is.EqualTo("http://localhost:7000/"));
      });
    }
  }
}
