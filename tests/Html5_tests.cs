using NUnit.Framework;

using jbSoft.Reusable;

namespace jbSoft.Reusable.Tests
{
  [TestFixture]
  class Html5Tests
  {
    [SetUp]
    public void Setup()
    {
    }


    [TearDown]
    public void TearDown()
    {
    }


    [Test]
    public void AddContent_NoPreviousContent_AddedContentIsReturned()
    {
      // Arrange
      var addedContent = "AddedContent";
      dynamic html = new Html5();

      //Act
      html.AddContent(addedContent);

      // Assert
      Assert.That(html.GetContent(), Is.EqualTo($"{addedContent}\n"));
    }
  }
}


