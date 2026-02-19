
using Microsoft.CSharp.RuntimeBinder;
//using System.Runtime.InteropServices;

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
        Assert.That(attribs.ToString(), Is.EqualTo(@" Id=""MyId"" Name=""My Name """));
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


    [Test]
    public void InvalidElement_NoArgs_ThrowsRuntimeBinderException()
    {
      Assert.That(() => _html.Bogus(),
                  Throws.InstanceOf<RuntimeBinderException>().With.Message.EqualTo("'jbSoft.Reusable.Html5' does not contain a definition for 'Bogus'"));
    }


    [Test]
    public void VoidElement_NoArgs_CorrectElementIsReturned()
    {
      //Act
      _html.Area();

      // Assert.
      Assert.That(_html.GetContent(), Is.EqualTo("<area>\n"));
    }


    [Test]
    public void VoidElement_NameIdArg_CorrectElementIsReturned()
    {
      //Act
      _html.Area("myArea");

      Assert.Multiple(() =>
      {
        // Assert content starts with the element name.
        Assert.That(_html.GetContent(), Does.StartWith("<area "));
        // Assert content has appropriate Id attribute.
        Assert.That(_html.GetContent(), Does.Contain("Id=\"myArea\""));
        // Assert content has appropriate Name attribute.
        Assert.That(_html.GetContent(), Does.Contain("Name=\"myArea\""));
        // Assert content ends with appropriate ending.
        Assert.That(_html.GetContent(), Does.EndWith(">\n"));
      });
    }


    [Test]
    public void VoidElement_NameIdNullAttribsArgAsString_CorrectElementIsReturned()
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
    public void VoidElement_NameIdEmptyStringAttribsArgAsDictionaryInitializer_CorrectElementIsReturned()
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
    public void VoidElement_NameIdAndAttribsArgs_CorrectElementIsReturned()
    {
      _html.Area("myArea", "shape=circle\ncoords=150,50,50");

      Assert.Multiple(() =>
      {
        // Assert content starts with the element name.
        Assert.That(_html.GetContent(), Does.StartWith("<area "));
        // Assert content has appropriate Id attribute.
        Assert.That(_html.GetContent(), Does.Contain("Id=\"myArea\""));
        // Assert content has appropriate Name attribute.
        Assert.That(_html.GetContent(), Does.Contain("Name=\"myArea\""));
        // Assert content has appropriate Shape attribute.
        Assert.That(_html.GetContent(), Does.Contain("Shape=\"circle\""));
        // Assert content has appropriate Coords attribute.
        Assert.That(_html.GetContent(), Does.Contain("Coords=\"150,50,50\""));
        // Assert content ends with appropriate ending.
        Assert.That(_html.GetContent(), Does.EndWith(">\n"));
      });
    }
  }
}


