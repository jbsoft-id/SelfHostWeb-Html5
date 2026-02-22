
using NUnit.Framework;

namespace jbSoft.Reusable.Tests
{
  [TestFixture]
  class AttribsTests
  {
    [Test]
    public void Constructor_NoArg_ReturnsExpectedObject()
    {
      // Arrange
      // Act
      var attribs = new Attribs();

      // Assert
      Assert.Multiple(() =>
      {
        Assert.That(attribs, Is.Not.Null);
        Assert.That(attribs, Has.Count.EqualTo(0));
        Assert.That(attribs.ToString(), Is.EqualTo(""));
      });
    }


    [Test]
    public void Constructor_InvalidStringArg_ThrowsException()
    {
      Assert.That(() => new Attribs("id MyId\n name MyName"),
                  Throws.InstanceOf<Exception>().With.Message.EqualTo("Error parsing attribute string."));
    }


    [Test]
    public void Constructor_StringArg_ReturnsExpectedObject()
    {
      // Arrange
      // Act
      var attribs = new Attribs("id=MyId\n name =  My Name \n");

      // Assert
      Assert.Multiple(() =>
      {
        Assert.That(attribs, Is.Not.Null);
        // Assert that the attrib string can be parsed into two name/value pairs.
        Assert.That(attribs, Has.Count.EqualTo(2));
        // Assert that a correctly formatted HTML attribute string is returned from ToString().
        Assert.That(attribs.ToString(), Is.EqualTo(@" Name=""My Name "" Id=""MyId"""));
        // Assert that attribute key names are case-insensitive.
        Assert.That(attribs["id"], Is.EqualTo("MyId"));
        Assert.That(attribs["Id"], Is.EqualTo("MyId"));
        Assert.That(attribs["iD"], Is.EqualTo("MyId"));
        Assert.That(attribs["ID"], Is.EqualTo("MyId"));
        // Asert that space around the equal sign separating attribute names and values doesn't matter.
        Assert.That(attribs["Name"], Is.EqualTo("My Name "));
      });
    }
  }


  [TestFixture]
  class Html5Tests
  {
    // The = 0 bit avoids a "Non-nullable field '_html' must contain a non-null value when exiting constructor" warning.
    private dynamic _html = 0;

    [SetUp]
    public void Setup()
    {
      _html = new Html5();
    }

    #region AddContent and GetContent tests
    [Test]
    public void AddContentAndGetContent_NoPreviousContent_AddedContentPlusNewLineIsReturned()
    {
      // Arrange
      var addedContent = "AddedContent";

      //Act
      _html.AddContent(addedContent);

      Assert.Multiple(() =>
      {
        // Assert that content is returned.
        Assert.That(_html.GetContent(), Is.EqualTo($"{addedContent}\n"));
        // Assert that previous GetContent didn't clear the contents and the same content is returned.
        Assert.That(_html.GetContent(clear: true), Is.EqualTo($"{addedContent}\n"));
        // Assert that GetContent with a true parameter is now empty.
        Assert.That(_html.GetContent(), Is.Empty);
      });
    }


    [Test]
    public void AddContent_MultipleCalls_ContentIsAccumulated()
    {
      //Act
      _html.AddContent("Line 1");
      _html.AddContent("Line 2");
      _html.AddContent("Last Line");

      // Assert that content is returned.
      Assert.That(_html.GetContent(), Is.EqualTo("Line 1\nLine 2\nLast Line\n"));
    }
    #endregion AddContent and GetContent tests

    #region Invalid element or arguments tests
    [Test]
    public void TryInvokeMember_InvalidElement_ThrowsHtml5Exception()
    {
      Assert.That(() => _html.Bogus(),
                  Throws.InstanceOf<Html5Exception>().With.Message.EqualTo("Unrecognized 'bogus' element."));
    }


    [Test]
    public void TryInvokeMember_ElementWithIncorrectArguments_ThrowsHtml5ExceptionWithMessage()
    {
      Assert.Multiple(() =>
      {
        // Br is a void element and take only two args.
        Assert.That(() => _html.Br("Content", "myId", "at=myName"),
                    Throws.InstanceOf<Html5Exception>().With.Message.EqualTo("Too many argument values for br element."));
        // The nameId arg must be a string.
        Assert.That(() => _html.Br(123, "at=myName"),
                    Throws.InstanceOf<Html5Exception>().With.Message.EqualTo("Invalid value for br's nameId argument."));
        // The invalid attributes argurment value.
        Assert.That(() => _html.Br("myId", 123),
                    Throws.InstanceOf<Html5Exception>().With.Message.EqualTo("Invalid value for br's attributes argument."));

        // Span is a content element and take only three args.
        Assert.That(() => _html.Span(null, null, null, "test"),
                    Throws.InstanceOf<Html5Exception>().With.Message.EqualTo("Too many argument values for span element."));
        // The invalid content argurment value.
        Assert.That(() => _html.Span(123),
                    Throws.InstanceOf<Html5Exception>().With.Message.EqualTo("Invalid value for span's content argument."));
        // The invalid nameId argurment value.
        Assert.That(() => _html.Span(null, 123),
                    Throws.InstanceOf<Html5Exception>().With.Message.EqualTo("Invalid value for span's nameId argument."));
        // The invalid attributes argurment value.
        Assert.That(() => _html.Span(null, null, 123),
                    Throws.InstanceOf<Html5Exception>().With.Message.EqualTo("Invalid value for span's attributes argument."));
      });
    }
    #endregion Invalid element or arguments tests

    #region Fluent vs Nonfluent calls (No contest)
    [Test]
    public void TryInvokeMember_FluentVsNonfluent_HaveSameContent()
    {
      var id = "myId";
      var attrib = "style=font-weight: bold;";
      var content = "My content.";

      Assert.Multiple(() =>
      {
        // Void elements
        Assert.That(_html.Br().GetContent(clear: true), Is.EqualTo(_html.Br_()));
        Assert.That(_html.Area(id, attrib).GetContent(clear: true), Is.EqualTo(_html.Area_(id, attrib)));

        // Content elements
        Assert.That(_html.Span().GetContent(clear: true), Is.EqualTo(_html.Span_()));
        Assert.That(_html.Span(content).GetContent(clear: true), Is.EqualTo(_html.Span_(content)));
        Assert.That(_html.Span(content, id).GetContent(clear: true), Is.EqualTo(_html.Span_(content, id)));
        Assert.That(_html.Span(content, id, attrib).GetContent(clear: true), Is.EqualTo(_html.Span_(content, id, attrib)));
      });
    }
    #endregion Fluent vs Nonfluent calls (No contest)

    #region Void element tests
    [Test]
    public void TryInvokeMember_VoidElementNoArgsFluent_CorrectElementIsReturned()
    {
      //Act
      _html.Area();

      // Assert.
      Assert.That(_html.GetContent(), Is.EqualTo("<area>\n"));
    }


    [Test]
    public void TryInvokeMember_VoidElementWithNameIdArgNonfluent_CorrectElementIsReturned()
    {
      //Act
      var actual = _html.Area_("myArea");

      Assert.Multiple(() =>
      {
        // Assert content starts with the element name.
        Assert.That(actual, Does.StartWith("<area "));
        // Assert content has appropriate Id attribute.
        Assert.That(actual, Does.Contain("Id=\"myArea\""));
        // Assert content has appropriate Name attribute.
        Assert.That(actual, Does.Contain("Name=\"myArea\""));
        // Assert content ends with appropriate ending.
        Assert.That(actual, Does.EndWith(">\n"));
      });
    }


    [Test]
    public void TryInvokeMember_VoidElementWithNameIdNullAttribsArgAsStringFluent_CorrectElementIsReturned()
    {
      //Act
      _html.Area(null, "shape=circle\n coords=150,50,50");

      Assert.Multiple(() =>
      {
        // Assert content starts with the element name.
        Assert.That(_html.GetContent(), Does.StartWith("<area "));
        // Assert content has appropriate Shape attribute.
        Assert.That(_html.GetContent(), Does.Contain("Shape=\"circle\""));
        // Assert content has appropriate Coords attribute.
        Assert.That(_html.GetContent(), Does.Contain("Coords=\"150,50,50\""));
        // Assert content ends with appropriate ending.
        Assert.That(_html.GetContent(), Does.EndWith(">\n"));
        // Assert content doesn't have Id attribute.
        Assert.That(_html.GetContent(), Does.Not.Contain("Id="));
        // Assert content doesn't have Name attribute.
        Assert.That(_html.GetContent(), Does.Not.Contain("Name="));
      });
    }


    [Test]
    public void TryInvokeMember_VoidElementWithEmptyStringNameIdAttribsArgAsInitializerFluent_CorrectElementIsReturned()
    {
      //Act
      _html.Area("", new Attribs { { "Shape", "circle" }, { "Coords", "150,50,50" } });

      Assert.Multiple(() =>
      {
        // Assert content starts with the element name.
        Assert.That(_html.GetContent(), Does.StartWith("<area "));
        // Assert content has appropriate Shape attribute.
        Assert.That(_html.GetContent(), Does.Contain("Shape=\"circle\""));
        // Assert content has appropriate Coords attribute.
        Assert.That(_html.GetContent(), Does.Contain("Coords=\"150,50,50\""));
        // Assert content ends with appropriate ending.
        Assert.That(_html.GetContent(), Does.EndWith(">\n"));
        // Assert content doesn't have Id attribute.
        Assert.That(_html.GetContent(), Does.Not.Contain("Id="));
        // Assert content doesn't have Name attribute.
        Assert.That(_html.GetContent(), Does.Not.Contain("Name="));
      });
    }


    [Test]
    public void TryInvokeMember_VoidElementWithNameIdAndAttribsArgsAsStringNonfluent_CorrectElementIsReturned()
    {
      var actual = _html.Area_("myArea", "shape=circle\ncoords=150,50,50");

      Assert.Multiple(() =>
      {
        // Assert content starts with the element name.
        Assert.That(actual, Does.StartWith("<area "));
        // Assert content has appropriate Id attribute.
        Assert.That(actual, Does.Contain("Id=\"myArea\""));
        // Assert content has appropriate Name attribute.
        Assert.That(actual, Does.Contain("Name=\"myArea\""));
        // Assert content has appropriate Shape attribute.
        Assert.That(actual, Does.Contain("Shape=\"circle\""));
        // Assert content has appropriate Coords attribute.
        Assert.That(actual, Does.Contain("Coords=\"150,50,50\""));
        // Assert content ends with appropriate ending.
        Assert.That(actual, Does.EndWith(">\n"));
      });
    }
    #endregion  Void element tests

    #region Content element tests
    [Test]
    public void TryInvokeMember_ContentElementNoArgsFluent_CorrectElementIsReturned()
    {
      //Act
      _html.Span();

      // Assert.
      Assert.That(_html.GetContent(), Is.EqualTo("<span></span>\n"));
    }


    [Test]
    public void TryInvokeMember_ContentElementWithContentNameIdArgNonfluent_CorrectElementIsReturned()
    {
      //Act
      var actual = _html.Span_("Content", "myId");

      Assert.Multiple(() =>
      {
        // Assert content starts with the element name.
        Assert.That(actual, Does.StartWith("<span "));
        // Assert content has appropriate Id attribute.
        Assert.That(actual, Does.Contain("Id=\"myId\""));
        // Assert content has appropriate Name attribute.
        Assert.That(actual, Does.Contain("Name=\"myId\""));
        // Assert content ends with appropriate ending.
        Assert.That(actual, Does.EndWith(">Content</span>\n"));
      });
    }


    [Test]
    public void TryInvokeMember_ContentElementWithContentNameIdNullAttribsArgAsStringFluent_CorrectElementIsReturned()
    {
      //Act
      _html.Span("Content", null, "style=color:blue;");

      Assert.Multiple(() =>
      {
        // Assert content starts with the element name.
        Assert.That(_html.GetContent(), Does.StartWith("<span "));
        // Assert content has appropriate style attribute.
        Assert.That(_html.GetContent(), Does.Contain("Style=\"color:blue;\""));
        // Assert content ends with appropriate ending.
        Assert.That(_html.GetContent(), Does.EndWith(">Content</span>\n"));
        // Assert content doesn't have Id attribute.
        Assert.That(_html.GetContent(), Does.Not.Contain("Id="));
        // Assert content doesn't have Name attribute.
        Assert.That(_html.GetContent(), Does.Not.Contain("Name="));
      });
    }


    [Test]
    public void TryInvokeMember_ContentElementContentWithEmptyStringNameIdAttribsArgAsInitializerFluent_CorrectElementIsReturned()
    {
      //Act
      _html.Span("", "", new Attribs { { "Style", "color:blue;" } });

      Assert.Multiple(() =>
      {
        // Assert content starts with the element name.
        Assert.That(_html.GetContent(), Does.StartWith("<span "));
        // Assert content has appropriate style attribute.
        Assert.That(_html.GetContent(), Does.Contain("Style=\"color:blue;\""));
        // Assert content ends with appropriate ending.
        Assert.That(_html.GetContent(), Does.EndWith("></span>\n"));
        // Assert content doesn't have Id attribute.
        Assert.That(_html.GetContent(), Does.Not.Contain("Id="));
        // Assert content doesn't have Name attribute.
        Assert.That(_html.GetContent(), Does.Not.Contain("Name="));
      });
    }


    [Test]
    public void TryInvokeMember_ContentElementContentWithNameIdAndAttribsArgsAsStringNonfluent_CorrectElementIsReturned()
    {
      var actual = _html.Span_("Test 123", "myId", "style=color:blue;");

      Assert.Multiple(() =>
      {
        // Assert content starts with the element name.
        Assert.That(actual, Does.StartWith("<span "));
        // Assert content has appropriate Id attribute.
        Assert.That(actual, Does.Contain("Id=\"myId\""));
        // Assert content has appropriate Name attribute.
        Assert.That(actual, Does.Contain("Name=\"myId\""));
        // Assert content has appropriate style attribute.
        Assert.That(actual, Does.Contain("Style=\"color:blue;\""));
        // Assert content ends with appropriate ending.
        Assert.That(actual, Does.EndWith(">Test 123</span>\n"));
      });
    }
    #endregion Content element tests

    #region Begin and End tests
    [Test]
    public void TryInvokeMember_ValidBeginCall_ReturnsExpectedContent()
    {
      Assert.Multiple(() =>
      {
        // Being called with no args.
        Assert.That(_html.BeginSpan_(), Is.EqualTo("<span>\n"));

        // Being called with nameId and attribute args.
        Assert.That(_html.BeginSpan_("nameId", "style = color:blue;"), Does.StartWith("<span")
                                                                       .And.Contain("Id=\"nameId\"")
                                                                       .And.Contain("Name=\"nameId\"")
                                                                       .And.Contain("Style=\"color:blue;\"")
                                                                       .And.EndsWith(">\n"));
      });
    }


    [Test]
    public void TryInvokeMember_ValidBeginEndCall_ReturnsExpectedContent()
    {
      Assert.That(_html.BeginSpan("nameId")
                       .AddContent("Test abz.")
                       .EndSpan()
                       .GetContent(),
                  Does.StartWith("<span")
                  .And.Contain("Id=\"nameId\"")
                  .And.Contain("Name=\"nameId\"")
                  .And.EndsWith(">\nTest abz.\n</span>\n"));
    }


    [Test]
    public void TryInvokeMember_InvalidEndCalls_ThrowsHtml5Exception()
    {
      Assert.Multiple(() =>
      {
        // Br is a void element and cannot be used with Begin/End.
        Assert.That(() => _html.EndBr(),
                    Throws.InstanceOf<Html5Exception>().With.Message.EqualTo("End cannot be used with 'br' element.").IgnoreCase);
        // End called without any previous Begin.
        Assert.That(() => _html.EndSpan(),
                    Throws.InstanceOf<Html5Exception>().With.Message.EqualTo("EndSpan called without a matching Begin(). No previous Begin call.").IgnoreCase);
        // End called without corresponding Begin.
        Assert.That(() => _html.BeginDiv().EndSpan(),
                    Throws.InstanceOf<Html5Exception>().With.Message.EqualTo("EndSpan called without a matching Begin(). Expecting EndDiv.").IgnoreCase);
        // End called without immediately preceeding Begin.
        Assert.That(() => _html.BeginSpan().BeginDiv().EndSpan(),
                    Throws.InstanceOf<Html5Exception>().With.Message.EqualTo("EndSpan called without a matching Begin(). Expecting EndDiv.").IgnoreCase);
      });
    }
    #endregion Begin and End tests

    #region Enhanced Elements tests
    [Test]
    public void TryInvokeMember_EnhancedElements_ReturnExpectedContent()
    {
      var id = "myId";
      var attrib = "style=color: green;";
      var text = "My textual content.";
      var options = new Dictionary<string, string> { {"1", "One"}, {"2", "Two"}, {"9", "End"} };
      var defVals = new List<string> { "1" };

      var eCheckbox = "<label For=\"myId\">Label:</label>\n<input Name=\"myId\" Id=\"myId\" Type=\"checkbox\" Checked Style=\"color: green;\">\n";
      var eDataList = "<datalist Name=\"myId\" Id=\"myId\" Style=\"color: green;\">\n<option Value=\"One\">One</option>\n<option Value=\"Two\">Two</option>\n<option Value=\"End\">End</option>\n</datalist>\n";
      var eInput = "<input Name=\"myId\" Id=\"myId\" Style=\"color: green;\" Type=\"Date\" Value=\"1-Jan-2000\">\n";
      var eRadioButton = "<input Name=\"Group\" Id=\"myId\" Style=\"color: green;\" Type=\"radio\" Value=\"2000\">\n<label For=\"myId\">Label</label>\n";
      var eSelect = "<select Name=\"myId\" Id=\"myId\" Style=\"color: green;\">\n<option Style=\"display: none;\" Value>Select</option>\n<option Value=\"1\" Selected>One</option>\n<option Value=\"2\">Two</option>\n<option Value=\"9\">End</option>\n</select>\n";
      var eSubmit = "<input Name=\"myId\" Id=\"myId\" Style=\"color: green;\" Type=\"submit\" Value=\"Caption\">\n";
      var eTextArea = "<textarea Name=\"myId\" Id=\"myId\" Style=\"color: green;\" Rows=\"5\" Cols=\"40\">My textual content.</textarea>\n";

      Assert.Multiple(() =>
      {
        Assert.That(_html.eCheckbox_(id, true, "Label: ", attrib), Is.EqualTo(eCheckbox));
        Assert.That(_html.eDataList_(id, new List<string> { "One", "Two", "End" }, attrib), Is.EqualTo(eDataList));
        Assert.That(_html.eInput_("Date", id, "1-Jan-2000", attrib), Is.EqualTo(eInput));
        Assert.That(_html.eRadioButton_("Label", "Group", id, 2000, attrib), Is.EqualTo(eRadioButton));
        Assert.That(_html.eSelect_(id, options, defVals, "Select",  attrib), Is.EqualTo(eSelect));
        Assert.That(_html.eSubmit_(id, "Caption", attrib), Is.EqualTo(eSubmit));
        Assert.That(_html.eTextArea_(id, text, 5, 40, attrib), Is.EqualTo(eTextArea));
      });
    }
    #endregion Enhanced Elements tests
  }
}
