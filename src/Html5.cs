/**********************************************************************************************************************
** Copyright (c) 2025 jbSoft
**
This is a very simple HTML5 generator.  There are no frills and very few rules.  It is possible to create full HTML  
documents or just parts like forms or tables only.  It is possible to generate documents that don't conform to any 
rules and maybe don't even render correctly.  If you want something that guides you to proper document structure and
forces you to follow rules, this class is not for you.  If you want something that gives you the freedom to generate
the HTML you want, this maybe for you.

Usage documentation.

The first step is to create an instance of the class.  The important thing to note here is that the instance variable
MUST be typed as dynamic.  This is necessary since this class doesn't actually implement individual methods for all
100+ HTML elements.  Rather, calls to methods with element names (herein call element methods) are intercepted and 
handled by a generic method.

dynamic html = new Html5();

By default calling element methods or the AddContent method (all explained below) will accumulate the generated 
output in the Html5 instance (for later retrieval) and returns the instance itself.  This makes for a compact 
fluent-like style.

  Example                                               Results

  var htmlContent = html
  .H1("Header One")                                     <h1>Header One</h1>
  .Span("This is a span element.")                      <span>This is a span element.</span>
  .AddContent("This is raw content.")                   This is raw content.
  .GetContent();

However, sometimes it is necessary to create html that becomes the content of another.  Since by default each 
method's results are accumulated in the Html5 object, another approach is required.  The approach taken herein
is to add an underscore suffix to the method name which alters the element method's behaviour.  Rather than 
accumulate the HTML and return a self reference, the HTML content is returned and nothing is accumulated.  This
is called the nonfluent style.

  Exanple (input element inside table data element)     Results

  var htmlContent = html                                
  .Td(html.Input_("id1"))                               <td><input Id="id1" Name="id1"></td>
  .GetContent();

Instance methods are divided into three groups as shown below.  Note that with the dynamic nature of these methods 
that their names are case insensitive.
- Void elements are those that don't have any content. Their method signature is shown below them.  
- Content elements have content and a signature that includes such a parameter.
- Enhanced elements are those that were determined to be common enough to deserve specific signatures to simplify 
  that address their most common attributes.  Their names are preceeded with an 'e' and their signatures listed 
  along side their names.

1. Void Elements
  area, base, br, col, embed, hr, img, input, link, meta, param, source, track, wbr
    (string? nameId = null, Attribs? attributes = null)

2. Content Elements
  a, abbr, address, article, aside, audio, b, bdi, bdo, blockquote, body, button, canvas, caption, cite, code, 
  colgroup, data, datalist, dd, del, details, dfn, div, dl, dt, em, fieldset, figcaption, figure, footer, form, 
  h1, h2, h3, h4, h5, h6, head, header, html, i, iframe, ins, kbd, label, legend, li, main, map, mark, meter, 
  nav, noscript, object, ol, optgroup, option, output, p, picture, pre, progress, q, rp, rt, ruby, s, samp,
  script, section, select, small, span, strong, style, sub, summary, sup, table, tbody, td, template, textarea, 
  tfoot, th, thead, time, title, tr, u, ul, var, video
    (string content, string? nameId = null, Attribs? attributes = null)

3. Enhanced Elements
  eCheckbox(string nameId, bool checked=false, string? label=null, Attribs? attributes=null)
  eDataList(string nameId, List<string> options, Attribs? attributes = null)
  eInput(string type, string nameId=null, value=null, Attribs? attributes=null)
  eRadioButton(string label, string group, string id, object value, Attribs? attributes=null)
  eSelect(string nameId, Dictionary<string, string> options, List<string>? defaultValues = null, string? prompt = null, Attribs? attributes = null)
  eSubmit(string nameId, string caption, Attribs? attributes = null)
  eTextArea(string nameId, string text, int rows, int cols, Attribs? attributes = null)

The last parameter to all element methods is an optional attributes parameter of type Attribs.  Attribs is a derivative 
of a Dictionary where both the key and value are strings.  The keys are case-insensitive.  This serves as the collection
of attributes associated with the HTML element.  There are two methods to pass them in.  The first is any of the 
standard C# methods for dictionary initialization.  The second is a a simplified string format.  The following examples
all set the same attributes.

  new Attribs { { "style", "color: chartreuse;" }, { "value", "Testing 123" } }
  new Attribs { ["style"] = "color: chartreuse;", ["value"] = "Testing 123" }
  "style=color: chartreuse;\n value = Testing 123"

All the non-enhanced element methods take an option nameId parameter.  If left at the default null value, no Name or Id
attributes will be added to the element.  If a value is passed in, Name and Id attributes will be added to the element
with the same value.  In most cases this is fine and even desirable.  However there are cases where unique values are 
desired.  For example with radio buttons inputs as in the following.

  <input type="radio" id="html" name="fav_language" value="HTML">
  <label for="html">HTML</label>
  <input type="radio" id="css"  name="fav_language" value="CSS">
  <label for="css">CSS</label>

To achieve this either a name or id attribute can be pass to the Attribs parameter and the corresponding value will be 
overridden.  Or in the case that the nameId parameter is null, only the Name and/or Id attribute provided in the Attribs 
parameter will be added.  For example to recreate the above html using the Enhanced Element as well as the Content Element:

  var htmlContent = html                                    var htmlContent = html
  .eInput("radio", "fav_language", "HTML", "id=html")       .Input("fav_language", "type=radio\n id=html\n value=HTML")
  .Label("HTML", null, "for=html")                          .Label("HTML", null, "for=html")
  .eInput("radio", "fav_language", "CSS", "id=css")         .Input("fav_language", "type=radio\n id=css\n value=CSS")
  .Label("CSS", null, "for=css")                            .Label("CSS", null, "for=css")
  .GetContent();                                            .GetContent();

Of course the easiest way to create radio buttons is to use the eRadioButton enhanced element method.

For some content elements it is not convenient or even possible to pass in all the content to the method at once.  To 
make this easier it is possible to prefix these element methods with Begin and End.  This will begin the element and 
allow other elements to be added before ending the element.  The only rule enforced in this class is that there must be 
a corresponding End*** call for each Begin*** call and they must be correctly nested.  (i.e. BeginX().BeginY().EndX().EndY()
is invalid)

The Begin*** calls have this signature: (string? nameId = null, Attribs? attributes = null)
The End*** calls have this signature: ()

  Example

   var htmlContent = html                                 
  .BeginForm("entry_form", "method=POST")                 <form Id="entry_form" Name="entry_form" Method="POST">
  .Input("name", "type=text\n size=40")                   <input Id="name" Name="name" Type="text" Size="40">
  .Input("email", "type=email")                           <input Id="email" Name="email" Type="email">
  .EndForm()                                              </form>
  .GetContent();

The AddContent(string content) method can be used to add raw content to the Html5 instance.

To retrieve the accumulated HTML content call the GetContent(bool clear = false) method.  By default the content is 
returned and remains held withing the Html5 instance.  This is fine as it normally the case that the instance is 
only to be used once.  But if that isn't the case and reusing the instance multiple times is intended, passing in true 
to the method will clear the accumulated content after returning its present value.

A final comment.

I want to acknowledge that I have broken one of the biggest "rules" of "good" software design being the 'One class per 
file' rule.  And depending on your persuasion, probably others.  We are all intitled to our opinions.  If you choose 
not to use this software because of this or any other reason, you won't offend me.  If you choose to completely 
reformat/restyle/rewrite this software, you won't offend me.  Just don't critisize me for my opinion.  Its mine.  (And 
even if you do, you won't offend me.  I just don't care. :o )
**********************************************************************************************************************/

using System.Dynamic;
using System.Text;
using System.Text.RegularExpressions;

namespace jbSoft.Reusable
{
  /// <summary>
  /// A container for attributes for HTML5 elements. 
  /// </summary>
  public class Attribs : Dictionary<string, string>
  {
    private static Regex _attribPattern = new Regex(@"(?:^|\s*)(?<attrib>[a-zA-Z]+)(?: *= *(?<value>.*?))?(?:\n|$)");

    public const string ID = "Id";
    public const string NAME = "Name";

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <remarks>
    /// Sets the base class key comparison to ignore case.
    /// </remarks>
    public Attribs() : base(StringComparer.OrdinalIgnoreCase)
    { }


    /// <summary>
    /// Constructor.  Parses a simple string representation of the attribute key/value pairs.
    /// </summary>
    /// <param name="attribs">Simple string representation of the attribute key/value pairs.</param>
    /// <exception cref="Html5Exception">
    /// Thrown if there are parse errors.
    /// </exception>
    public Attribs(string? attribs = null) : this()
    {
      if (attribs != null)
      {
        var matches = _attribPattern.Matches(attribs);

        int currentPosition = 0;
        foreach (Match match in matches)
        {
          Add(match.Groups["attrib"].Value, match.Groups["value"].Value);

          // Check if the current match starts exactly where the last one ended
          if (match.Index != currentPosition)
          {
            throw new Html5Exception("Error parsing attribute string.");
          }
          currentPosition += match.Length;
        }

        // Check if the final position reached the very end of the input string
        if (currentPosition != attribs.Length)
        {
          throw new Html5Exception("Error parsing attribute string.");
        }
      }
    }


    /// <summary>
    /// Returns a string representation of the attributes in an appropriate HTML5 format.
    /// Note that the Name and Id attributes are always placed at the beginning of the string.
    /// </summary>
    /// <returns>
    /// A string representation of the attributes in an appropriate HTML5 format.
    /// </returns>
    public override string ToString()
    {
      var attribs = new StringBuilder();
      string? val;

      // Force the name and id attributes to be first.
      if( TryGetValue(NAME, out val) )
      {
        attribs.Append(MakeAttribute(NAME, val));
      }

      if( TryGetValue(ID, out val) )
      {
        attribs.Append(MakeAttribute(ID, val));
      }

      foreach (var kvp in this)
      {
        if( kvp.Key.Equals(NAME, StringComparison.InvariantCultureIgnoreCase) ||
            kvp.Key.Equals(ID, StringComparison.InvariantCultureIgnoreCase) )
        {
          continue;
        }

        attribs.Append(MakeAttribute(kvp.Key, kvp.Value));
      }

      return attribs.ToString();
    }
    

    private static string MakeAttribute(string name, string? value)
    {
      var result = "";

      if (!string.IsNullOrWhiteSpace(name))
      {
        name = char.ToUpper(name[0]) + name.Substring(1).ToLower();

        if (value == null)
        {
          result = "";
        }
        else if (string.IsNullOrWhiteSpace(value))
        {
          result = $" {name}";
        }
        else
        {
          result = $" {name}=\"{System.Net.WebUtility.HtmlEncode(value)}\"";
        }
      }
      else
      {
        throw new Html5Exception("Cannot make and attribute without a name.");
      }

      return result;
    }
  }


  /// <summary>
  /// A very basic HTML5 code generator.
  /// </summary>
  public class Html5 : DynamicObject
  {
    private StringBuilder _content = new();

    private static readonly HashSet<string> VoidElements = [
      "area", "base", "br", "col", "embed", "hr", "img", "input", "link", "meta", "param", "source", "track", "wbr"];

    private static readonly HashSet<string> Elements = [
      "a", "abbr", "address", "article", "aside", "audio", "b", "bdi", "bdo", "blockquote", "body", "button", "canvas",
      "caption", "cite", "code", "colgroup", "data", "datalist", "dd", "del", "details", "dfn", "div", "dl", "dt", "em",
      "fieldset", "figcaption", "figure", "footer", "form", "h1", "h2", "h3", "h4", "h5", "h6", "head", "header", "html",
      "i", "iframe", "ins", "kbd", "label", "legend", "li", "main", "map", "mark", "meter", "nav", "noscript", "object",
      "ol", "optgroup", "option", "output", "p", "picture", "pre", "progress", "q", "rp", "rt", "ruby", "s", "samp",
      "script", "section", "select", "small", "span", "strong", "style", "sub", "summary", "sup", "table", "tbody", "td",
      "template", "textarea", "tfoot", "th", "thead", "time", "title", "tr", "u", "ul", "var", "video"];

    private static readonly Dictionary<string, Func<object?[]?, string>> EnhancedElements = new Dictionary<string, Func<object?[]?, string>> (StringComparer.InvariantCultureIgnoreCase)
    {
      ["eCheckbox"] = EnhancedCheckbox,
      ["eDatalist"] = EnhancedDataList,
      ["eInput"] = EnhancedInput,
      ["eRadiobutton"] = EnhancedRadioButton,
      ["eSelect"] = EnhancedSelect,
      ["eSubmit"] = EnhancedSubmit,
      ["eTextarea"] = EnhancedTextArea,
    };

    private readonly Stack<string> _beginEndStack = [];


    public Html5 AddContent(string content)
    {
      if (!string.IsNullOrWhiteSpace(content)) _content.Append($"{content}\n");

      return this;
    }


    // Override TryInvokeMember to specify how operations that invoke a member are performed.
    public override bool TryInvokeMember(InvokeMemberBinder binder, object?[]? args, out object? result)
    {
      var element = binder.Name.ToLowerInvariant();
      result = this;
      string? nameId;
      Attribs? attribs;
      string html = "";
      var fluentMode = true;

      if (element.EndsWith('_'))
      {
        fluentMode = false;
        element = element[..^1];
      }

      if (EnhancedElements.TryGetValue(element, out var Method))
      {
        html = Method(args);
      }
      else if (VoidElements.Contains(element))
      {
        ParseArgsIntoNameIdAttributes(element, args, out nameId, out attribs);
        html = VoidElement(element, nameId, attribs);
      }
      else if (Elements.Contains(element))
      {
        ParseArgsIntoContentNameIdAttributes(element, args, out string content, out nameId, out attribs);
        html = Element(element, content, nameId, attribs);
      }
      else if (element.StartsWith("begin"))
      {
        element = element[5..];

        if (Elements.Contains(element))
        {
          ParseArgsIntoNameIdAttributes(element, args, out nameId, out attribs);
          html = Begin(element, nameId, attribs);
        }
        else
        {
          throw new Html5Exception($"Begin cannot be used with '{element}' element.");
        }
      }
      else if (element.StartsWith("end"))
      {
        element = element[3..];

        if (Elements.Contains(element))
        {
          html = End((string)element);
        }
        else
        {
          throw new Html5Exception($"End cannot be used with '{element}' element.");
        }
      }
      else
      {
        throw new Html5Exception($"Unrecognized '{element}' element.");
      }

      if (fluentMode)
      {
        _content.Append(html);
      }
      else
      {
        result = html;
      }

      // Always return true to avoid RuntimeBinderException, since errors will be handled otherwise.
      return true;
    }


    /// <summary>
    /// Gets the accumulated HTML content.
    /// </summary>
    /// <param name="clear">[Optional]If true clears the conetents after retrieval. Defaults to false.</param>
    /// <returns>
    /// The accumulated HTML content.
    /// </returns>
    /// <exception cref="Html5Exception"></exception>
    public string GetContent(bool clear = false)
    {
      var result = _content.ToString();

      if (_beginEndStack.Count > 0)
      {
        throw new Html5Exception($"Missing End*() call(s).  Un-ended begins: {string.Join(", ", _beginEndStack)}");
      }

      if (clear)
      {
        _content.Clear();
      }

      return result;
    }


    private static string VoidElement(string tag, string? nameId = null, Attribs? attributes = null)
    {
      if (attributes == null)
      {
        attributes = new Attribs();
      }

      tag = tag.ToLower();

      var element = $"<{tag}";

      if (!string.IsNullOrEmpty(nameId))
      {
        if (!attributes.ContainsKey(Attribs.ID))
        {
          attributes[Attribs.ID] = nameId;
        }

        if (!attributes.ContainsKey(Attribs.NAME))
        {
          attributes[Attribs.NAME] = nameId;
        }
      }

      if (attributes.Any())
      {
        element += attributes.ToString();
      }

      element += ">\n";

      string? result = element;
      return result;
    }


    private static string Element(string tag, string content = "", string? nameId = null, Attribs? attributes = null)
    {
      tag = tag.ToLower();

      string result = VoidElement(tag, nameId, attributes);

      if (!string.IsNullOrEmpty(result))
      {
        result = result.Trim();

        if (string.IsNullOrEmpty(content))
        {
          result += $"</{tag}>\n";
        }
        else
        {
          content = content.Trim();
          result += $"{content}</{tag}>\n";
        }
      }

      return result;
    }


    private static void ParseArgsIntoNameIdAttributes(string element, object?[]? args, out string? nameId, out Attribs? attributes)
    {
      nameId = null;
      attributes = null;

      if (args != null && args.Length > 2)
      {
        // There should be at most two args.
        throw new Html5Exception($"Too many argument values for {element} element.");
      }
      else if (args != null && args.Length >= 1)
      {
        if (args[0] == null || args[0] is string)
        {
          nameId = args[0] as string;
        }
        else
        {
          throw new Html5Exception($"Invalid value for {element}'s nameId argument.");
        }

        if (args.Length >= 2)
        {
          if (args[1] is string)
          {
            attributes = new Attribs(args[1] as string);
          }
          else if (args[1] is Attribs)
          {
            attributes = args[1] as Attribs;
          }
          else
          {
            throw new Html5Exception($"Invalid value for {element}'s attributes argument.");
          }
        }
      }
    }


    private static void ParseArgsIntoContentNameIdAttributes(string element, object?[]? args, out string content, out string? nameId, out Attribs? attributes)
    {
      nameId = null;
      attributes = null;
      content = "";

      if (args != null && args.Length > 3)
      {
        // There should be at most three args.
        throw new Html5Exception($"Too many argument values for {element} element.");
      }
      else if (args != null && args.Length >= 1)
      {
        if (args[0] == null || args[0] is string)
        {
          content = args[0] as string ?? "";
        }
        else
        {
          throw new Html5Exception($"Invalid value for {element}'s content argument.");
        }

        if (args.Length >= 2)
        {
          if(args[1] == null || args[1] is string)
          {
            nameId = args[1] as string;
          }
          else
          {
            throw new Html5Exception($"Invalid value for {element}'s nameId argument.");
          }

          if (args.Length >= 3)
          {
            if (args[2] is string)
            {
              attributes = new Attribs(args[2] as string);
            }
            else if (args[2] is Attribs)
            {
              attributes = args[2] as Attribs;
            }
            else
            {
              throw new Html5Exception($"Invalid value for {element}'s attributes argument.");
            }
          }
        }
      }
    }
    

    /// <summary>
    /// Begins an html element with the given tag name.
    /// </summary>
    /// <param name="tag">The name of the element to begin.</param>
    /// <param name="nameId">Optional. Name and Id attributes.</param>
    /// <param name="attributes">Optional. A dictionary of attributes and their respective values.</param>
    /// <returns>Returns null on failure or this instance on success.</returns>
    private string Begin(string tag, string? nameId = null, Attribs? attributes = null)
    {
      tag = tag.ToLowerInvariant();

      var voidElementResult = VoidElement(tag, nameId, attributes);

      if (string.IsNullOrEmpty(voidElementResult))
      {
        throw new Html5Exception("Failed to generate begin element.");
      }

      _beginEndStack.Push(tag);

      return voidElementResult;
    }


    private string End(string endElement)
    {
      string element = string.Empty;

      endElement = endElement.ToLowerInvariant();

      if(_beginEndStack.Count > 0)
      {
        element = _beginEndStack.Pop();

        if (endElement != element)
        {
          throw new Html5Exception($"End{endElement} called without a matching Begin(). Expecting End{element}.");
        }
      }
      else
      {
        throw new Html5Exception($"End{endElement} called without a matching Begin(). No previous Begin call.");
      }

      return $"</{element}>\n";
    }


    private static string EnhancedCheckbox(object?[]? args)
    {
      bool checkedVal = false;
      string? label = null;
      Attribs? attributes = null;

      if (args != null && args.Length >= 1 && args[0] is string nameId)
      {
        if (args.Length >= 2 && args[1] is bool v)
        {
          checkedVal = v;
        }

        if (args.Length >= 3 && args[2] is string w)
        {
          label = w;
        }

        if (args.Length >= 4)
        {
          if (args[3] is string)
          {
            attributes = new Attribs(args[3] as string);
          }
          else if (args[3] is Attribs)
          {
            attributes = args[3] as Attribs;
          }
        }
      }
      else
      {
        throw new Html5Exception("Missing required nameId parameter(s).");
      }

      var labelAttribs = new Attribs();
      if (!string.IsNullOrEmpty(nameId))
      {
        labelAttribs["for"] = nameId;
      }

      var inputAttribs = new Attribs { ["type"] = "checkbox" };

      if (attributes != null && attributes.ContainsKey("title"))
      {
        labelAttribs["title"] = attributes["title"];
      }

      if (checkedVal)
      {
        inputAttribs["checked"] = "";
      }

      if (attributes != null)
      {
        foreach (var kvp in attributes)
        {
          if (!inputAttribs.ContainsKey(kvp.Key))
          {
            inputAttribs[kvp.Key] = kvp.Value;
          }
        }
      }

      string content = "";
      if (!string.IsNullOrWhiteSpace(label))
      {
        content = Element("label", label, null, labelAttribs);
      }

      return content + VoidElement("input", nameId, inputAttribs);
    }


    private static string EnhancedDataList(object?[]? args)
    {
      Attribs? attributes = null;

      if (args != null && args.Length >= 2 &&
          args[0] is string nameId &&
          args[1] is List<string> options)
      {
        if (args.Length >= 3)
        {
          if (args[2] is string)
          {
            attributes = new Attribs(args[2] as string);
          }
          else if (args[2] is Attribs)
          {
            attributes = args[2] as Attribs;
          }
        }
      }
      else
      {
        throw new Html5Exception("Missing required nameId and/or options parameter(s).");
      }

      var html = VoidElement("datalist", nameId, attributes);

      foreach (var value in options)
      {
        html += Element("option", value, null, new Attribs { { "value", value } });
      }
      html += "</datalist>\n";

      return html;
    }


    private static string EnhancedInput(object?[]? args)
    {
      object? value = null;
      Attribs attributes = [];

      if (args != null && args.Length >= 2 && args[0] is string type && args[1] is string nameId)
      {
        if (args.Length >= 3)
        {
          value = args[2];
        }

        if (args.Length >= 4)
        {
          if (args[3] is string)
          {
            attributes = new Attribs(args[3] as string);
          }
          else if (args[3] is Attribs)
          {
            attributes = args[3] as Attribs ?? [];
          }
        }
      }
      else
      {
        throw new Html5Exception("Missing required type and/or nameId parameter(s).");
      }

      attributes["type"] = type;
      attributes["value"] = value?.ToString() ?? "";

      return VoidElement("input", nameId, attributes);
    }


    private static string EnhancedRadioButton(object?[]? args)
    {
      Attribs attributes = [];

      if (args != null && args.Length >= 4 &&
          args[0] is string label &&
          args[1] is string group &&
          args[2] is string id &&
          args[3] is object value)
      {
        if (args.Length >= 5)
        {
          if (args[4] is string)
          {
            attributes = new Attribs(args[4] as string);
          }
          else if (args[4] is Attribs)
          {
            attributes = args[4] as Attribs ?? [];
          }
        }
      }
      else
      {
        throw new Html5Exception("Missing required label, group, id and/or value parameter(s).");
      }

      attributes["type"] = "radio";
      attributes["value"] = value?.ToString() ?? "";
      attributes["id"] = id;

      return VoidElement("input", group, attributes) + Element("label", label, null, new Attribs { ["for"] = id });
    }


    private static string EnhancedSelect(object?[]? args)
    {
      List<string>? defaultValues = null;
      string? prompt = null;
      Attribs? attributes = null;

      if (args != null && args.Length >= 2 &&
          args[0] is string nameId &&
          args[1] is Dictionary<string, string> options)
      {
        if (args.Length >= 3)
        {
          defaultValues = args[2] as List<string>;
        }

        if (args.Length >= 4)
        {
          prompt = args[3] as string;
        }

        if (args.Length >= 5)
        {
          if (args[4] is string)
          {
            attributes = new Attribs(args[4] as string);
          }
          else if (args[4] is Attribs)
          {
            attributes = args[4] as Attribs;
          }
        }
      }
      else
      {
        throw new Html5Exception("Missing required nameId and/or options parameter(s).");
      }

      if (attributes != null && attributes.ContainsKey("Multiple") && !string.IsNullOrEmpty(nameId))
      {
        attributes["Name"] = $"{nameId}[]";
      }

      var result = VoidElement("select", nameId, attributes);

      if (!string.IsNullOrEmpty(prompt))
      {
        result += Element("option", prompt, null, new Attribs { { "style", "display: none;" }, { "Value", "" } });
      }

      foreach (var kvp in options)
      {
        var attribs = new Attribs { { "value", kvp.Key } };
        if (defaultValues != null && defaultValues.Contains(kvp.Key))
        {
          attribs["selected"] = "";
        }

        result += Element("option", kvp.Value, null, attribs);
      }

      result += "</select>\n";

      return result;
    }


    private static string EnhancedSubmit(object?[]? args)
    {
      Attribs attributes = [];

      if (args != null && args.Length >= 2 &&
          args[0] is string nameId &&
          args[1] is string caption)
      {
        if (args.Length >= 3)
        {
          if (args[2] is string)
          {
            attributes = new Attribs(args[2] as string);
          }
          else if (args[2] is Attribs)
          {
            attributes = args[2] as Attribs ?? [];
          }
        }
      }
      else
      {
        throw new Html5Exception("Missing required nameId and/or caption parameter(s).");
      }

      attributes["Type"] = "submit";
      attributes["Value"] = caption;

      return VoidElement("input", nameId, attributes);
    }


    private static string EnhancedTextArea(object?[]? args)
    {
      Attribs attributes = [];

      if (args != null && args.Length >= 4 &&
          args[0] is string nameId &&
          args[1] is string text &&
          args[2] is int rows &&
          args[3] is int cols)
      {
        if (args.Length >= 5)
        {
          if (args[4] is string)
          {
            attributes = new Attribs(args[4] as string);
          }
          else if (args[4] is Attribs)
          {
            attributes = args[4] as Attribs ?? [];
          }
        }
      }
      else
      {
        throw new Html5Exception("Missing required nameId, text, rows and/or cols parameter(s).");
      }

      attributes["Rows"] = rows.ToString();
      attributes["Cols"] = cols.ToString();

      return Element("textarea", text, nameId, attributes);
    }
  }


  /// <summary>
  /// Exception thrown by the Html5 class.
  /// </summary>
  public class Html5Exception : Exception
  {
    public Html5Exception(string message) : base(message)
    {}
  }
}