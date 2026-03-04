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


    protected override void StartBrowser(string listenOn)
    {
      HasStartBrowserInitiated = true;
    }
  }

  [HttpUri("/echo")]
  public class EchoApi : HttpTransaction
  {
    public string? Id { get; set; }

    public async override Task<bool> Process()
    {
      Content = $"I heard {GetRequestBody()}";
      return true;
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
      _httpServer = new TestableHttpServer(65536);

      // Act & Assert
      Assert.That(() => _httpServer.Start(_httpServer.CancellationTokenSource),
                  Throws.InstanceOf<HttpListenerException>().With.Message.EqualTo("The parameter is incorrect."));

    }


    [Test]
    public async Task Start_ValidPort_HandlesRequest()
    {
      // Arrange
      _httpServer = new TestableHttpServer(65535);
      var client = new HttpClient();

      // Act
#pragma warning disable CS4014
      Task.Run(() => _httpServer.Start(_httpServer.CancellationTokenSource));
#pragma warning restore CS4014

      // Assert
      Assert.That(_httpServer.TryWaitIsListeningState(true), Is.True);
      Assert.That(_httpServer.Port, Is.EqualTo(65535));
      Assert.That(_httpServer.ListenOn, Is.EqualTo("http://localhost:65535/"));

      var response = await client.PostAsync("http://localhost:65535/echo", new StringContent("Hello?"));
      var stringResponse = await response.Content.ReadAsStringAsync();
      Assert.That(stringResponse, Is.EqualTo("I heard Hello?"));
    }


    [Test]
    public async Task Start_EphemeralPort_HandlesRequest()
    {
      // Arrange
      var client = new HttpClient();
      _httpServer = new TestableHttpServer();

      // Act
#pragma warning disable CS4014
      Task.Run(() => _httpServer.Start(_httpServer.CancellationTokenSource));
#pragma warning restore CS4014

      // Assert
      Assert.That(_httpServer.TryWaitIsListeningState(true), Is.True);
      Assert.That(_httpServer.Port, Is.GreaterThanOrEqualTo(1).And.LessThanOrEqualTo(65535));
      Assert.That(_httpServer.ListenOn, Is.EqualTo($"http://localhost:{_httpServer.Port}/"));

      var response = await client.PostAsync($"http://localhost:{_httpServer.Port}/echo", new StringContent("Hello?"));
      var stringResponse = await response.Content.ReadAsStringAsync();
      Assert.That(stringResponse, Is.EqualTo("I heard Hello?"));
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
        Assert.That(_httpServer.Port, Is.EqualTo(7000));
        Assert.That(_httpServer.ListenOn, Is.EqualTo("http://localhost:7000/"));
        Assert.That(_httpServer.HasStartBrowserInitiated, Is.False);
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
        Assert.That(_httpServer.Port, Is.EqualTo(7000));
        Assert.That(_httpServer.ListenOn, Is.EqualTo("http://localhost:7000/"));
        Assert.That(_httpServer.HasStartBrowserInitiated, Is.False);
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
        Assert.That(_httpServer.Port, Is.EqualTo(7000));
        Assert.That(_httpServer.ListenOn, Is.EqualTo("http://localhost:7000/"));
        Assert.That(_httpServer.HasStartBrowserInitiated, Is.True);
      });
    }


    [Test]
    public async Task CancelToken_WhileListening_StopsListening()
    {
      // Arrange
      var client = new HttpClient();
      _httpServer = new TestableHttpServer(7000);
#pragma warning disable CS4014
      Task.Run(() => _httpServer.Start(_httpServer.CancellationTokenSource));
#pragma warning restore CS4014
      Assert.That(_httpServer.TryWaitIsListeningState(true), Is.True);

      // Act
      _httpServer.CancellationTokenSource.Cancel();

      // Assert
      Assert.Multiple(() =>
      {
        Assert.That(_httpServer.TryWaitIsListeningState(false), Is.True);
        Assert.That(async () => await client.PostAsync("http://localhost:7000/echo", new StringContent("Hello?")),
                    Throws.InstanceOf<HttpRequestException>().
                    With.Message.EqualTo("No connection could be made because the target machine actively refused it. (localhost:7000)"));
      });
    }
  }
}
