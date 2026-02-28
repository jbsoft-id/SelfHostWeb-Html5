using NUnit.Framework;

namespace jbSoft.Reusable.Tests
{
  public class TestableHttpServer : HttpServer
  {
    public TestableHttpServer(int port = 0) : base(port) { }

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
      _httpServer = null;
    }


    [Test]
    public async Task Start_StartBrowserNull_StartBrowserIsNotInitiated()
    {
      // Arrange
      _httpServer = new TestableHttpServer(7000);

      // Act
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
      Task.Run(() => _httpServer.Start());
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
      await Task.Delay(1000);
      _httpServer.CancelEvent.Set();

      // Assert
      Assert.Multiple(() =>
      {
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
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
      Task.Run(() => _httpServer.Start(startBrowser: false));
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
      await Task.Delay(1000);
      _httpServer.CancelEvent.Set();

      // Assert
      Assert.Multiple(() =>
      {
        Assert.That(_httpServer.HasStartBrowserInitiated, Is.False);
        Assert.That(_httpServer.ListenOn, Is.Empty);
      });
    }

    [Test]
    public async Task Start_StartBrowserTrue_StartBrowserIsInitiated()
    {
      // Arrange
      _httpServer = new TestableHttpServer(7000);

      // Act
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
      Task.Run(() => _httpServer.Start(startBrowser: true));
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
      await Task.Delay(1000);
      _httpServer.CancelEvent.Set();

      // Assert
      Assert.Multiple(() =>
      {
        Assert.That(_httpServer.HasStartBrowserInitiated, Is.True);
        Assert.That(_httpServer.ListenOn, Is.EqualTo("http://localhost:7000/"));
      });
    }
  }
}
