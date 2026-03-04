using System.Diagnostics;
using System.Net;
using NUnit.Framework;

namespace jbSoft.Reusable.Tests
{
  public class TestableHttpServer : HttpServer
  {
    public bool TryWaitIsListeningState(bool target)
    {
      bool current = IsListening;

      for (int waitCount = 1; waitCount < 10; waitCount++)
      {
        SelfHostWebLog.WriteLine($"TryWaitIsListeningState {waitCount} target: {target} current: {current} {waitCount}...");
        if (current == target)
        {
          break;
        }
        Thread.Sleep(100);
        current = IsListening;
      }

      return current == target;
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
    public override Task<bool> Process()
    {
      Content = $"I heard {GetRequestBody()}";
      return Task.FromResult(true);
    }
  }


  [TestFixture]
  class HttpServerTests
  {
    private TestableHttpServer? _httpServer = null;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
      // Uncomment for additional output while debugging.
      // SelfHostWebLog.WriteLine = Console.WriteLine;
    }

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
    public void Start_ConstructedWithInvalidPort_ThrowsHttpListenerException()
    {
      // Arrange
      _httpServer = new TestableHttpServer(65536);

      // Act & Assert
      Assert.That(() => _httpServer.Start(_httpServer.CancellationTokenSource),
                  Throws.InstanceOf<HttpListenerException>());
    }


    [Test]
    public void Start_ValidPort_HandlesRequest()
    {
      // Arrange
      _httpServer = new TestableHttpServer(65535);
      var client = new HttpClient();

      // Act
      Task.Run(() => _httpServer.Start(_httpServer.CancellationTokenSource));

      // Assert
      Assert.Multiple(async () =>
      {
        Assert.That(_httpServer.TryWaitIsListeningState(true), Is.True);
        Assert.That(_httpServer.Port, Is.EqualTo(65535));
        Assert.That(_httpServer.ListenOn, Is.EqualTo("http://localhost:65535/"));

<<<<<<< HEAD
        var response = await client.PostAsync("http://localhost:65535/echo", new StringContent("Hello?"));
        var stringResponse = await response.Content.ReadAsStringAsync();
        Assert.That(stringResponse, Is.EqualTo("I heard Hello?"));
      });
=======
      var response = await client.PostAsync("http://localhost:65535/echo", new StringContent("Hello?"));
      var stringResponse = await response.Content.ReadAsStringAsync();
      Assert.That(stringResponse, Is.EqualTo("I heard Hello?"));
>>>>>>> a0a98d4 (Unit tetst)
    }


    [Test]
    public void Start_EphemeralPort_HandlesRequest()
    {
      // Arrange
      var client = new HttpClient();
      _httpServer = new TestableHttpServer();

      // Act
      Task.Run(() => _httpServer.Start(_httpServer.CancellationTokenSource));

      // Assert
      Assert.Multiple(async () =>
      {
        Assert.That(_httpServer.TryWaitIsListeningState(true), Is.True);
        Assert.That(_httpServer.Port, Is.GreaterThanOrEqualTo(1).And.LessThanOrEqualTo(65535));
        Assert.That(_httpServer.ListenOn, Is.EqualTo($"http://localhost:{_httpServer.Port}/"));

<<<<<<< HEAD
        var response = await client.PostAsync($"http://localhost:{_httpServer.Port}/echo", new StringContent("Hello?"));
        var stringResponse = await response.Content.ReadAsStringAsync();
        Assert.That(stringResponse, Is.EqualTo("I heard Hello?"));
      });
=======
      var response = await client.PostAsync($"http://localhost:{_httpServer.Port}/echo", new StringContent("Hello?"));
      var stringResponse = await response.Content.ReadAsStringAsync();
      Assert.That(stringResponse, Is.EqualTo("I heard Hello?"));
>>>>>>> a0a98d4 (Unit tetst)
    }


    [Test]
    public void Start_StartBrowserDefaulted_StartBrowserIsNotInitiated()
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
<<<<<<< HEAD
    public void CancelToken_WhileListening_StopsListening()
=======
    public async Task CancelToken_WhileListening_StopsListening()
>>>>>>> a0a98d4 (Unit tetst)
    {
      // Arrange
      var client = new HttpClient();
      _httpServer = new TestableHttpServer(7000);
<<<<<<< HEAD
      Task.Run(() => _httpServer.Start(_httpServer.CancellationTokenSource));
      Assume.That(_httpServer.TryWaitIsListeningState(true), Is.True);
=======
#pragma warning disable CS4014
      Task.Run(() => _httpServer.Start(_httpServer.CancellationTokenSource));
#pragma warning restore CS4014
      Assert.That(_httpServer.TryWaitIsListeningState(true), Is.True);
>>>>>>> a0a98d4 (Unit tetst)

      // Act
      _httpServer.CancellationTokenSource.Cancel();

      // Assert
      Assert.Multiple(() =>
      {
        Assert.That(_httpServer.TryWaitIsListeningState(false), Is.True);
        Assert.That(async () => await client.PostAsync("http://localhost:7000/echo", new StringContent("Hello?")),
<<<<<<< HEAD
                    Throws.InstanceOf<HttpRequestException>());
      });
    }


    [Test]
    public void ShutdownEndpointInvoked_WhileListening_StopsListening()
    {
      // Arrange
      var client = new HttpClient();
      _httpServer = new TestableHttpServer(7000);
      Task.Run(() => _httpServer.Start(_httpServer.CancellationTokenSource));
      Assume.That(_httpServer.TryWaitIsListeningState(true), Is.True);

      // Act
      Task.Run(async () => await client.PostAsync("http://localhost:7000/shutdown", new StringContent("")));

      // Assert
      Assert.Multiple(() =>
      {
        Assert.That(_httpServer.TryWaitIsListeningState(false), Is.True);
        Assert.That(async () => await client.PostAsync("http://localhost:7000/echo", new StringContent("Hello?")),
                    Throws.InstanceOf<HttpRequestException>());
=======
                    Throws.InstanceOf<HttpRequestException>().
                    With.Message.EqualTo("No connection could be made because the target machine actively refused it. (localhost:7000)"));
>>>>>>> a0a98d4 (Unit tetst)
      });
    }
  }
}
