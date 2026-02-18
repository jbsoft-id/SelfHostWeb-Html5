using NUnit.Framework;

using jbSoft.Reusable;
using System.Reflection;

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
        Assert.That(attribs, Is.Empty);
        Assert.That(attribs.ToString(), Is.EqualTo(""));
      });
    }


    [Test]
    public void Constructor_ValidArg_ReturnsExpectedObject()
    {
      // Arrange
      // Act
      var attribs = new Attribs("id=MyId\n name=MyName");

      // Assert
      Assert.Multiple(() =>
      {
        Assert.That(attribs, Is.Not.Null);
        Assert.That(attribs, Is.Not.Empty);
        Assert.That(attribs.ToString(), Is.EqualTo(""));
      });
    }

  }


  [TestFixture]
  class Html5Tests
  {
    dynamic? _html;

    [SetUp]
    public void Setup()
    {
      _html = new Html5();
    }


    [TearDown]
    public void TearDown()
    {
      _html = null;
    }


    [Test]
    public void AddContent_NoPreviousContent_AddedContentPlusNewLineIsReturned()
    {
      // Arrange
      var addedContent = "AddedContent";

      //Act
      _html.AddContent(addedContent);

      // Assert
      Assert.That(_html.GetContent(), Is.EqualTo($"{addedContent}\n"));
    }
  }
}


