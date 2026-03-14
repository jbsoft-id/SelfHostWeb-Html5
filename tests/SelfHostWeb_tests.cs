using System.Diagnostics;
using System.Net;
using System.Net.ServerSentEvents;
using System.Text;
using System.Text.Json;
using FluentAssertions;
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
  public class EchoApi : HttpOperation
  {
    public override Task<bool> Process()
    {
      Content = $"I heard {GetRequestBody()}";
      return Task.FromResult(true);
    }
  }


  [HttpUri("/brokentransaction")]
  public class MissingResource : HttpOperation
  {
    public override Task<bool> Process()
    {
      throw new Exception("Fatal error.");
    }
  }


  [HttpUri("/brokenstreamer")]
  public class BrokenStreamer : HttpOperation, IHttpStream
  {
    public override Task<bool> Process()
    {
      throw new Exception("The clock struck 13.");
    }
  }


  [HttpUri("/streamer")]
  public class Streamer : HttpOperation, IHttpStream
  {
    public override Task<bool> Process()
    {
      var response = Context.Response;
      List<string> alphabet = ["Alfa", "Bravo", "Charlie", "Delta"];

      alphabet.ForEach(async letter =>
      {
        byte[] buffer = Encoding.UTF8.GetBytes(
          string.Format("data: {0}\n\n", JsonSerializer.Serialize(letter)));
        response.OutputStream.Write(buffer, 0, buffer.Length);
        response.OutputStream.Flush();
        await Task.Delay(1000);
      });

      return Task.FromResult(true);


      //int i = 1;
      //       while (true)
      //       {
      //         byte[] buffer = Encoding.UTF8.GetBytes(
      //           string.Format("data: {0}\n\n", JsonSerializer.Serialize(DateTime.Now.ToString("HH:mm:ss"))));

      //         response.OutputStream.Write(buffer, 0, buffer.Length);
      //         response.OutputStream.Flush();

      // //Debug.WriteLine($"{i++}");
      //         await Task.Delay(5000);
      //       }
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

        var response = await client.PostAsync("http://localhost:65535/echo", new StringContent("Hello?"));
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var stringResponse = await response.Content.ReadAsStringAsync();
        Assert.That(stringResponse, Is.EqualTo("I heard Hello?"));
      });
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

        var response = await client.PostAsync($"http://localhost:{_httpServer.Port}/echo", new StringContent("Hello?"));
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var stringResponse = await response.Content.ReadAsStringAsync();
        Assert.That(stringResponse, Is.EqualTo("I heard Hello?"));
      });
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
    public void CancelToken_WhileListening_StopsListening()
    {
      // Arrange
      var client = new HttpClient();
      _httpServer = new TestableHttpServer(7000);
      Task.Run(() => _httpServer.Start(_httpServer.CancellationTokenSource));
      Assume.That(_httpServer.TryWaitIsListeningState(true), Is.True);

      // Act
      _httpServer.CancellationTokenSource.Cancel();

      // Assert
      Assert.Multiple(() =>
      {
        Assert.That(_httpServer.TryWaitIsListeningState(false), Is.True);
        Assert.That(async () => await client.PostAsync("http://localhost:7000/echo", new StringContent("Hello?")),
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
      });
    }


    [Test]
    public void NonexistentEndpointInvoked_WhileListening_Returns404()
    {
      // Arrange
      _httpServer = new TestableHttpServer(7000);
      var client = new HttpClient();

      // Act
      Task.Run(() => _httpServer.Start(_httpServer.CancellationTokenSource));

      // Assert
      Assert.Multiple(async () =>
      {
        Assert.That(_httpServer.TryWaitIsListeningState(true), Is.True);

        var response = await client.PostAsync($"http://localhost:7000/nonexistent", content: null);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        var stringResponse = await response.Content.ReadAsStringAsync();
        Assert.That(stringResponse, Is.EqualTo(
          "<html><body><h1>404 Error</h1><p>The requested URL /nonexistent was not found!</p></body></html>"));
      });
    }


    [Test]
    public void TransactionThrowsException_WhileListening_Returns500()
    {
      // Arrange
      _httpServer = new TestableHttpServer(7000);
      var client = new HttpClient();

      // Act
      Task.Run(() => _httpServer.Start(_httpServer.CancellationTokenSource));

      // Assert
      Assert.Multiple(async () =>
      {
        Assert.That(_httpServer.TryWaitIsListeningState(true), Is.True);

        var response = await client.PostAsync($"http://localhost:7000/brokentransaction", content: null);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
        var stringResponse = await response.Content.ReadAsStringAsync();
        Assert.That(stringResponse, Contains.Substring("Fatal error."));
      });
    }




    // good stream


    [Test]
    public void StreamEndpointInvoked_WhileListening_StreamReceived()
    {
      // Arrange
      List<string> expected = ["\"Alfa\"", "\"Bravo\"", "\"Charlie\"", "\"Delta\""];
      _httpServer = new TestableHttpServer(7000);
      var client = new HttpClient();

      // Act
      Task.Run(() => _httpServer.Start(_httpServer.CancellationTokenSource));

      // Assert
      Assert.Multiple(async () =>
      {
        Assert.That(_httpServer.TryWaitIsListeningState(true), Is.True);

        //using client??

        List<string> actual = new();
        using var client = new HttpClient();
        using var stream = await client.GetStreamAsync("http://localhost:7000/streamer");
        await foreach (SseItem<string> item in SseParser.Create(stream).EnumerateAsync())
        {
          Debug.WriteLine($"Event: {item.EventId}, Data: {item.Data}, Id: {item.EventType}, Retry: {item.ReconnectionInterval}");
          actual.Add(item.Data);
        }

        actual.Should().BeEquivalentTo(expected);





        // Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
        // Assert.That(stringResponse, Is.Empty);
      });
    }


    [Test]
    public void StreamThrowsException_WhileListening_Returns500()
    {
      // Arrange
      _httpServer = new TestableHttpServer(7000);
      var client = new HttpClient();

      // Act
      Task.Run(() => _httpServer.Start(_httpServer.CancellationTokenSource));

      // Assert
      Assert.Multiple(async () =>
      {
        Assert.That(_httpServer.TryWaitIsListeningState(true), Is.True);



        //post?

        var response = await client.PostAsync($"http://localhost:7000/brokenstreamer", new StringContent(""));
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
        var stringResponse = await response.Content.ReadAsStringAsync();
        Assert.That(stringResponse, Is.Empty);
      });
    }
  }
}
