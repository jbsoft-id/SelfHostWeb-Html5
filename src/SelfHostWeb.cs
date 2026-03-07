/**********************************************************************************************************************
** Copyright (c) 2025 jbSoft
**
The SelfHostWeb software contained herein implements a flexible WEB server that can be easily embedded within a C# 
application and is simple to use.  The intent is to provide a platform independant means to add a GUI to an 
application.  Being aquainted with WinForms and WPF for Windows application development, I was reluctant to invest 
the time and effort to learn yet another GUI framework that may or may not be well supported, free, and most 
importantly truely cross-platform.  Already being somewhat proficient with HTML5/Javascript and knowing their 
ability to create cross-platform GUIs, all that was needed is the ability to serve these technologies from a C# 
application.  Thus, SelfHostWeb was born.

Before getting into the details, I want to acknowledge that I have broken one of the biggest "rules" of "good" 
software design being the 'One class per file' rule.  And depending on your persuasion, probably others.  We are 
all intitled to our opinions.  If you choose not to use this software because of this or any other reason, you 
won't offend me.  If you choose to completely reformat/restyle/rewrite this software, you won't offend me.  Just 
don't critisize me for my opinion.  Its mine.  (And even if you do, you won't offend me.  I just don't care. :o )

USAGE
Note that each class below has been documented.  So in an effort not to duplicate that documentation, only an 
architectural overview will be given here.

To get a basic system up and running you will need to create an instance of the HttpServer class and call its 
Start() method.  That's it.

Okay, a little more information might be helpful.  There is an optional parameter for the HttpServer constuctor
that defaults to zero, which means that the server should determine the port to use.  This also will cause the 
server to open the default browser to the base URL for the server (e.g. http://localhost:nnnnn/) when Start() is 
called.  This is helpful since the user won't know the port number unless it is somehow conveyed to them.

If a port number is passed to the constructor and it is available, the browser won't be automatically started
when the Start method is invoked unless the optional startBrowser parameter is passed in and set to true. 
However, since the port number is known, the URL is also known to the user.

Another option is to override the StartBrowser() method to launch a client application other than the default 
browser or to pass special start parameters to the browser.

It should also be noted that the HttpServer runs on the thread that invokes Start() and thus will block that 
thread until the server is shutdown, which can occur in two ways: 1) The Start() method has a CancelationTokenSource
parameter which can be used to stop the HttpServer as part of standard process termination, and 2) the client
can cause the process to terminate by invoking the /shutdown URL as noted below.

Two URLs are supported by out of the box.
  * /favicon.ico -  Which is commonly requested by modern browsers will work *if* a favicon.ico file is included in 
                    the project as an embedded resource.  Include this in the .csproj file:

                    <ItemGroup>
                      <EmbeddedResource Include="Resources/favicon.ico" />
                    </ItemGroup>

  * /shutdown -     This URL will cause the web server to shutdown and return execution to the part of the application
                    that called the Start() method.

In addition, it will be necessary to declare additional subclasses of HttpTransaction in order for the server instance 
to do anything useful.  See the class documentation for HttpTransaction for details.
**********************************************************************************************************************/

using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace jbSoft.Reusable
{
  /// <summary>
  /// Provides a method for being able to redirect logged information for the SelfHostWeb code.
  /// By default log entries are disregarded, but by setting the WriteLine property, log entries can 
  /// be written to alternate locations.
  /// </summary>
  public static class SelfHostWebLog
  {
    /// <summary>
    /// Gets or sets an action that accepts a single string parameter that is the log entry to 
    /// record.  Defauls to doing nothing.  For initial development setting this to Console.WriteLine can be helpful.
    /// </summary>
    public static Action<string> WriteLine { get; set; } = (msg) => { };
  }


  /// <summary>
  /// A container for holding placeholder names and their associated values.
  /// Note, the placeholder names are case-insensitive.
  /// </summary>
  public class PlaceholderValues : Dictionary<string, string>
  {
    public PlaceholderValues() : base(StringComparer.OrdinalIgnoreCase)
    { }

    /// <summary>
    /// Converts the value of this instance to a string. 
    /// </summary>
    /// <returns>A string whose value is the same as this instance.</returns>
    public override string ToString()
    {
      var result = new StringBuilder("{");
      var cnt = 0;

      foreach (var item in this)
      {
        if (cnt++ > 0)
        {
          result.Append(", ");
        }

        result.Append($"{item.Key}=>'{item.Value}'");
      }

      return result.Append('}').ToString();
    }
  }


  /// <summary>
  /// Represents the self-hosted HttpServer that will listen on http://localhost:{port} where port is 
  /// provided upon construction.  Then calling Start() will cause the server to start listening for 
  /// client requests.
  /// </summary>
  public class HttpServer
  {
    private List<(string uri, Type httpTrans)> _httpTransactions = [];


    /// <summary>
    /// Gets an indication of whether the HttpServer is listening for requests.
    /// </summary>
    public bool IsListening { get; private set; } = false;

    /// <summary>
    /// Gets the port on which the HttpServer is listening.
    /// </summary>
    public int Port { get; private set; } = 0;

    /// <summary>
    /// Gets the URL on which the HttpServer is listening.
    /// </summary>
    public string ListenOn { get; private set; } = string.Empty;


    /// <summary>
    /// Constructs an instance and discovers subclasses of HttpTransaction and the HttpUris they are 
    /// decorated with in order to know what URL resources are supported.
    /// </summary>
    /// <param name="port">[Optional] The TCP port to listen on.  The default value causes the server to find an
    /// available port.</param>
    public HttpServer(int port = 0)
    {
      Port = port;

      _httpTransactions = GetClassesOf(typeof(HttpTransaction));
    }


    private List<(string, Type)> GetClassesOf(Type targetType)
    {
      List<(string, Type)> classes = [];

      // Get all types from the current assembly
      Assembly currentAssembly = Assembly.GetExecutingAssembly();

      // Find all classes that inherit from base class HttpTransaction.
      var derivedClasses = currentAssembly.GetTypes().Where(type => !type.IsAbstract &&
                                                            type.IsSubclassOf(targetType)
                                                            // The following line will exclude all but the default HttpTransaction types.
                                                            //&& type.FullName.StartsWith("jbSoft.Reusable.")
                                                            );

      SelfHostWebLog.WriteLine($"Classes inheriting from {targetType}:");
      foreach (Type type in derivedClasses)
      {
        var first = true;

        foreach (var attr in type.GetCustomAttributes<HttpUriAttribute>())
        {
          if (first)
          {
            SelfHostWebLog.WriteLine($"- {type.Name} - {type.FullName}");
            first = false;
          }

          SelfHostWebLog.WriteLine($"  > {attr.Uri}");

          classes.Add((attr.Uri, type));
        }
      }

      return classes;
    }


    /// <summary>
    /// Start running the Http Server.
    /// </summary>
    /// <param name="cancellationTokenSource">Provides a means of stopping the HttpServer as part of standard
    /// process termination.
    /// </param>
    /// <param name="startBrowser">[Optional] Indicates whether or not to force a start of the default browser at the URL 
    /// that this server is listening.  Note, that if the port number (optional constructor parameter) is left at zero the
    /// browser will be started regardless of this parameter value.  The protected StartBrowser() method can be overridden
    /// to launch a client application other than the default browser or to pass special start parameters to the browser.
    public async Task Start(CancellationTokenSource cancellationTokenSource, bool startBrowser = false)
    {
      using (var listener = new HttpListener())
      {
        cancellationTokenSource.Token.Register(() =>
        {
          StopListening(listener);
          SelfHostWebLog.WriteLine("Listener stopped by CancellationTokenSource");
        });

        var strtBrwsr = startBrowser;

        if (Port == 0)
        {
          Port = GetAvailableTcpPort();

          // Add a close message to the shutdown response, since we are using an ephemeral port that won't be known
          // to the user and the browser will be started automatically.
          Shutdown.AddCloseMsg = true;
          strtBrwsr = true;
        }

        ListenOn = $"http://localhost:{Port}/";

        try
        {
          listener.Prefixes.Add(ListenOn);
          listener.Start();
          IsListening = true;
          SelfHostWebLog.WriteLine($"Listening on {ListenOn}");
        }
        catch (Exception ex)
        {
          SelfHostWebLog.WriteLine($"Failed to start listener: {ex}");
          throw;
        }

        if (strtBrwsr)
        {
          StartBrowser(ListenOn);
        }

        while (IsListening)
        {
          IAsyncResult result = listener.BeginGetContext(new AsyncCallback(ListenerCallback), listener);
          result.AsyncWaitHandle.WaitOne();
        }

        listener.Close();
        SelfHostWebLog.WriteLine($"Listener closed");
      }
    }


    private async void ListenerCallback(IAsyncResult result)
    {
      Debug.Assert(result.AsyncState != null);

      HttpListenerContext? context = null;
      bool clientRequestedShutdown = false;
      string? absPath = string.Empty;
      HttpListener listener = (HttpListener)result.AsyncState;

      try
      {
        // Call EndGetContext to complete the asynchronous operation.
        context = listener.EndGetContext(result);
      }
      catch (Exception e) when (e is ObjectDisposedException || e is HttpListenerException)
      {
        // explain...
      }

      if (IsListening && context != null)
      {

        HttpListenerRequest request = context.Request;
        absPath = request.Url?.AbsolutePath;

        SelfHostWebLog.WriteLine($"REQUEST: {request.HttpMethod} - {absPath}");

        HttpListenerResponse response = context.Response;

        if (!string.IsNullOrWhiteSpace(absPath))
        {
          var httpTrans = FetchHttpTransaction(absPath, context);

          if (httpTrans != null)
          {
            SelfHostWebLog.WriteLine($"Found HttpTransaction: {httpTrans.GetType().Name}");

            try
            {
              clientRequestedShutdown = !await httpTrans.Process();
              response.StatusCode = httpTrans.StatusCode;
              response.ContentType = httpTrans.ContentType;
              response.ContentLength64 = httpTrans.OutputBuffer.Length;
              response.OutputStream.Write(httpTrans.OutputBuffer, 0, httpTrans.OutputBuffer.Length);
            }
            catch (Exception ex)
            {
              string responseString = $@"<html>
                <body>
                  <h1>500 Internal Server Error</h1>
                  <p>Processing the requested URL {absPath} was not successful.</p>
                  <pre>{ex}</pre>
                </body>
              </html>";
              byte[] buffer = Encoding.UTF8.GetBytes(responseString);

              response.StatusCode = 500;
              response.ContentType = "text/html";
              response.ContentLength64 = buffer.Length;
              response.OutputStream.Write(buffer, 0, buffer.Length);
            }
          }
          // Report URL not found.
          else
          {
            var additionalInfo = absPath != "/" ? "" : @"<p>You will need to create a subclass of HttpTransaction and 
              decorate it will one or more HttpUri attributes in order to be able to handle web requests.</p>";
            var responseString = $"<html><body><h1>404 Error</h1><p>The requested URL {absPath} was not found!</p>{additionalInfo}</body></html>";
            byte[] buffer = Encoding.UTF8.GetBytes(responseString);

            response.StatusCode = 404;
            response.ContentType = "text/html";
            response.ContentLength64 = buffer.Length;
            response.OutputStream.Write(buffer, 0, buffer.Length);
          }
        }
        response.Close();
      }

      if (clientRequestedShutdown)
      {
        StopListening(listener);
        SelfHostWebLog.WriteLine($"Listener stopped by client invoking {absPath}");
      }
    }


    /// <summary>
    /// Creates an instance of the HttpTransaction sub-class that supports the given URI, populates its
    /// properties if appropriate, and returns it.
    /// </summary>
    /// <param name="uri">The URI that is being requested.</param>
    /// <param name="context">Contextual information provided by the HttpServer regarding the client request as well 
    /// as provides a means for returning a response.</param>
    /// <returns>
    /// An instance of the HttpTransaction sub-class that supports the given URI if one exists; null otherwise.
    /// </returns>
    /// <exception cref="MissingMemberException">
    /// Thrown if the URI has parameters specified for which there is no matching property.
    /// </exception>
    private HttpTransaction? FetchHttpTransaction(string uri, HttpListenerContext context)
    {
      HttpTransaction? transaction = null;

      foreach (var item in _httpTransactions)
      {
        var regex = new Regex($"^{item.uri}$");
        var match = regex.Match(uri);

        if (match.Success)
        {
          transaction = Activator.CreateInstance(item.httpTrans) as HttpTransaction;

          if (transaction != null)
          {
            transaction.Context = context;

            foreach (var grpName in regex.GetGroupNames().Skip(1))
            {
              SelfHostWebLog.WriteLine($"Group[{grpName}] = {match.Groups[grpName]}");

              var propertyInfo = item.httpTrans.GetProperty(grpName);

              if (propertyInfo != null)
              {
                propertyInfo.SetValue(transaction, match.Groups[grpName].Value, null);
              }
              else
              {
                throw new MissingMemberException(item.httpTrans.Name, grpName);
              }
            }
          }

          break;
        }
      }

      return transaction;
    }


    private static int GetAvailableTcpPort()
    {
      Socket tmpSocket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
      try
      {
        tmpSocket.Bind(new IPEndPoint(IPAddress.Loopback, 0));

        if (tmpSocket.LocalEndPoint is IPEndPoint endPoint)
        {
          return endPoint.Port;
        }

        throw new Exception();
      }
      finally
      {
        // Ensure the socket is closed to free the port
        tmpSocket.Close();
      }
    }


    protected virtual void StartBrowser(string listenOn)
    {
      try
      {
        Process.Start(new ProcessStartInfo { FileName = listenOn, UseShellExecute = true });
      }
      catch (Exception ex)
      {
        SelfHostWebLog.WriteLine($"Failed to start browser: {ex.Message}");
      }
    }


    private void StopListening(HttpListener listener)
    {
      listener.Stop();
      IsListening = false;
      Port = 0;
      ListenOn = string.Empty;
    }
  }



  /// <summary>
  /// Specifies the URI that the class will respond to.  The URI string may contain a Regex with named groups.
  /// If so the class must contain a public string property whose value will be set to that part of the URL.
  /// </summary>
  /// <remarks>
  /// The URI should start with a '/'.  If not, one will be added for you.
  /// </remarks>
  /// <example>
  ///   [HttpUri("/rest.api")]
  ///   [HttpUri(@"/rest.api/item/(?<Id>\d+)")]
  /// </example>
  [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
  public class HttpUriAttribute : Attribute
  {
    /// <summary>
    /// Gets the URI for this attribute.
    /// </summary>
    public string Uri { get; private set; }


    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="uri">The URI for this attribute.</param>
    public HttpUriAttribute(string uri)
    {
      uri = uri.Trim();

      if (string.IsNullOrEmpty(uri))
      {
        uri = "/";
      }
      else if (uri[0] != '/')
      {
        uri = "/" + uri;
      }

      Uri = uri;
    }
  }


  /// <summary>
  /// A base class for objects that handle http requests and generating responses.
  /// </summary>
  /// <remarks>
  /// The sub-class must be decorated with one or more HttpUri attributes that specify which 
  /// URLs will invoke it.
  /// </remarks>
  public abstract class HttpTransaction
  {
    private byte[] _outputBuffer = [];
    public string[]? QueryStringKeys { get; private set; } = null;

    protected HttpListenerRequest Request { get { return Context.Request; } }

    /// <summary>
    /// Gets a value indicating if the request has a query string.
    /// </summary>
    protected bool HasQueryString { get { return (Request.QueryString?.Count ?? 0) > 0; } }

    /// <summary>
    /// Gets a value indicating if the request has body data.
    /// </summary>
    protected bool HasRequestBody { get { return Request.HasEntityBody; } }

    /// <summary>
    /// Gets or sets the HTTP Listener Context that was created by the Server when the client request was made.
    /// </summary>
    public required HttpListenerContext Context
    {
      get { return _Context; }
      set
      {
        _Context = value;

        if (HasQueryString)
        {
          var a = Request.QueryString.GetValues(null) ?? [];
          var b = Request.QueryString.AllKeys.Where(k => !string.IsNullOrWhiteSpace(k)) ?? [];

#pragma warning disable CS8601 // Possible null reference assignment.
          QueryStringKeys = [.. a, .. b];
#pragma warning restore CS8601 // Possible null reference assignment.
        }
      }
    }
    public required HttpListenerContext _Context;

    /// <summary>
    /// The HTTP Method used by the client to make the request.
    /// </summary>
    public string HttpMethod { get { return Request.HttpMethod; } }

    /// <summary>
    /// Gets or sets the status code of the response.  Defaults to 200.  (aka. Success)
    /// </summary>
    public int StatusCode { get; set; } = 200;

    /// <summary>
    /// Gets or sets the content type of the response.  Defaults to "text/html"
    /// </summary>
    /// <remarks>
    /// Other common types are application/json, image/x-icon, and image/png.  But use what ever is appropriate.
    /// </remarks>
    public string ContentType { get; set; } = "text/html";

    /// <summary>
    /// Gets the absolute path of url that initiated this transaction.
    /// </summary>
    public string AbsolutePath { get { return Request.Url?.AbsolutePath ?? ""; } }

    /// <summary>
    /// Gets or sets the content of the response.  This is useful when the response is a string such as HTML or Json data.
    /// </summary>
    /// <remarks>
    /// Note, this property is not compatible with LoadContentFromResource().  The last invokation will overwrite 
    /// any preceeding value.
    /// </remarks>
    public string Content
    {
      get { return _Content; }
      set
      {
        _Content = value;
        _outputBuffer = Encoding.UTF8.GetBytes(_Content);
      }
    }
    private string _Content = "";

    /// <summary>
    /// Gets the response content as a byte array, which is needed by the server when sending the response.
    /// </summary>
    public byte[] OutputBuffer { get { return _outputBuffer; } }


    /// <summary>
    /// Process any request data and generate a response.
    /// </summary>
    /// <returns>
    /// True indicating the server should continue running; false indicating the server should shutdown after this transaction.
    /// </returns>
    public async virtual Task<bool> Process()
    {
      return true;
    }


    /// <summary>
    /// Loads binary content from an embedded resource and sets an appropriate ContentType.
    /// </summary>
    /// <param name="name">The resource name.</param>
    /// <remarks>
    /// Note, this method is not compatible with the Content propery.  The last invokation will overwrite 
    /// any preceeding value.
    /// </remarks>
    protected void LoadContentFromResource(string name)
    {
      switch (Path.GetExtension(name))
      {
        case ".png":
          ContentType = "image/png";
          _outputBuffer = ReadResource(name) ?? [];
          break;

        case ".ico":
          ContentType = "image/x-icon";
          _outputBuffer = ReadResource(name) ?? [];
          break;

        default:
          throw new Exception($"Unsupported resource type: {name}");
      }
    }


    /// <summary>
    /// Loads an imbedded template resource document and then replaces the template's
    /// Mustache placeholders ( eg. {{VAR}} ) with appropriate values from the given PlaceHolders collection.
    /// Note: The Mustache placeholder can optionally be inclosed in C-style comments ( eg. /*{{VAR}}*/ ) to avoid 
    /// some editors considering them syntax errors in some HTML sections, like <style>.
    /// </summary>
    /// <param name="name">The name of the embedded resource to load.</param>
    /// <param name="placeholders">A dictionary where the keys are placeholder names and the values are the replacement text.</param>
    /// <param name="reportMissingPlaceholders">If true, placeholders in the template document without matching keys in the dictionary
    /// will be logged and a message prepended to the document.  If false, missing placeholders are just left in the document.</param>
    /// <returns>
    /// The template document with placeholders replaced where possible.
    /// </returns>
    protected static string FetchTemplateFromResource(string name, PlaceholderValues? placeholders = null, bool reportMissingPlaceholders = true)
    {
      var template = string.Empty;

      var resource = ReadResource(name);

      if (resource != null)
      {
        template = Encoding.UTF8.GetString(resource);

        if (placeholders != null)
        {
          var matches = Regex.Matches(template, @"(?:/\*)?{{(.+)}}(?:\*/)?");

          if (matches.Count > 0)
          {
            foreach (Match match in matches)
            {
              SelfHostWebLog.WriteLine($"Match: '{match.Value}' at index {match.Index}, length {match.Length}, name {match.Groups[1].Value}");

              if (placeholders.ContainsKey(match.Groups[1].Value))
              {
                template = template.Replace(match.Value, placeholders[match.Groups[1].Value]);
              }
              else if (reportMissingPlaceholders)
              {
                SelfHostWebLog.WriteLine($"No value provided for templace place holder {match.Groups[1].Value}");
                template = $"<pre>No value provided for templace placeholder name {match.Groups[1].Value}</pre>{template}";
              }
              // else
              // {
              //   template = template.Replace(match.Value, string.Empty);
              // }
            }
          }
        }
      }

      return template;
    }


    /// <summary>
    /// Reads an embedded resource.
    /// </summary>
    /// <param name="name">The resource name.</param>
    /// <returns>
    /// The resource as a byte array.
    /// </returns>
    /// <exception cref="Exception">
    /// Thrown if resource cannot be found.
    /// </exception>
    private static byte[] ReadResource(string name)
    {
      var resource = string.Empty;
      var buffer = new byte[0];

      // Determine path
      var assembly = Assembly.GetExecutingAssembly();

      var resourcePath = assembly.GetManifestResourceNames().SingleOrDefault(str => str.EndsWith(name));

      if (resourcePath != null)
      {
        using (var stream = assembly.GetManifestResourceStream(resourcePath))
        {
          if (stream != null)
          {
            buffer = new byte[stream.Length];
            stream.ReadExactly(buffer);
          }
        }
      }
      else
      {
        throw new Exception($"Resource '{name}' cannot be found.");
      }

      return buffer;
    }


    /// <summary>
    /// Dump the query string, if it exists, to a formatted string suitable for displaying.
    /// </summary>
    /// <returns>
    /// A string containing the dumped query string if one exists.
    /// </returns>
    protected string DumpQueryString()
    {
      var queryString = new StringBuilder();

      if (HasQueryString)
      {
        queryString.AppendLine("Query String:");
        var keysWithoutValues = Request.QueryString.GetValues(null);

        if (keysWithoutValues != null)
        {
          foreach (var key in keysWithoutValues)
          {
            queryString.AppendLine($"  Key: {key}");
          }
        }

        foreach (var key in Request.QueryString.AllKeys)
        {
          if (key != null)
          {
            queryString.Append($"  Key/Value(s): {key} = ");

            var values = Request.QueryString.GetValues(key.ToString());
            if (values != null)
            {
              queryString.AppendLine(string.Join(", ", values));
            }
            else
            {
              queryString.AppendLine("");
            }
          }
        }
      }
      else
      {
        queryString.AppendLine("No Query String");
      }

      return queryString.ToString();
    }


    /// <summary>
    /// Gets query string values for the given key name.
    /// </summary>
    /// <param name="key">The name of the query string key whose value is requested.</param>
    /// <returns>
    /// If the key exists, the value or values will be returned in a string array.
    /// If the key exists without a value (aka. a flag) an empty array is returned.
    /// If the key does not exist, a null is returned.
    /// </returns>
    protected string[]? GetQueryStringValue(string key)
    {
      string[]? values = null;

      if (HasQueryString && !string.IsNullOrWhiteSpace(key))
      {
        values = Request.QueryString.GetValues(key);

        if (values == null && QueryStringKeys != null && QueryStringKeys.Contains(key))
        {
          values = [];
        }
      }

      return values;
    }


    /// <summary>
    /// Gets the request body if one exists.  This method can only be called once as the body stream is consumed in the first call.
    /// </summary>
    /// <returns>
    /// The request body as a string if one exists; null otherwise.
    /// </returns>
    protected string? GetRequestBody()
    {
      string? body = null;

      if (HasRequestBody)
      {
        // Access the input stream containing the request body
        using (var bodyStream = Request.InputStream)
        {
          // Determine the content encoding from the request, or use UTF8 as a fallback
          var encoding = Request.ContentEncoding ?? Encoding.UTF8;

          // Use a StreamReader to read the content from the stream
          using (var reader = new StreamReader(bodyStream, encoding))
          {
            // Read the entire content of the stream into a string
            body = reader.ReadToEnd();
          }
        }
      }

      return body;
    }
  }



  /// <summary>
  /// Handles a favicon request.
  /// </summary>
  [HttpUri("/favicon.ico")]
  public class Favicon : HttpTransaction
  {
    public async override Task<bool> Process()
    {
      LoadContentFromResource("favicon.ico");

      return true;
    }
  }


  /// <summary>
  /// Handles a shutdown request and terminates the Server.
  /// </summary>
  [HttpUri("/shutdown")]
  public class Shutdown : HttpTransaction
  {
    /// <summary>
    /// Gets or sets an indication of whether or not a Restart link should be added to the response page.
    /// If the restart link is added, it will link to the referer, making it very easy to return to the 
    /// previous URL location at the time the shutdown was requested.  This is very handy during development.
    /// </summary>
    public static bool AddRestartUrl { get; set; } = false;

    /// <summary>
    /// Gets or sets an indication of where a "Please close this window/tab." message is added to the response.
    /// This is set automatically when using a non-fixed port for the HttpServer.
    /// Note that setting this to true will override AddRestartUrl, since restarted likely won't be possible when
    /// using a non-fixed port.
    /// </summary>
    internal static bool AddCloseMsg { get; set; } = false;


    public async override Task<bool> Process()
    {
      var href = string.IsNullOrWhiteSpace(Request.Headers["Referer"]) ? "/" : Request.Headers["Referer"];
      var extraBodyPart = AddRestartUrl ? $"<a href='{href}'>Restart</a>" : "";

      if (AddCloseMsg)
      {
        // Doesn't make sense to have a close message and a restart, so replace any restart if adding the close message.
        extraBodyPart = "<p>Please close this window/tab.</p>";
      }

      Content = $"<html><body><h1>Shutting Down</h1><p>Server shutdown was requested.</p>{extraBodyPart}</body></html>";

      return false;
    }
  }
}