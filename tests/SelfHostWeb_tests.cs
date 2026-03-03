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
    public async Task TearDown()
    {
      if (_httpServer != null)
      {
        await _httpServer.Stop();
        _httpServer = null;
      }
    }


    [Test]
    public void Start_StartBrowserNull_StartBrowserIsNotInitiated()
    {
      // Arrange
      _httpServer = new TestableHttpServer(7000);

      // Act
      _httpServer.Start();

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
      _httpServer.Start(startBrowser: false);

      // Assert
      Assert.Multiple(() =>
      {
        Assert.That(_httpServer.IsListening, Is.True);
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
      _httpServer.Start(startBrowser: true);

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
