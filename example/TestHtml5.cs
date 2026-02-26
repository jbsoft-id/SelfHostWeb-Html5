
using jbSoft.Reusable;


[HttpUri("/testhtml5")]
public class TestHtml5 : AppTemplateBase
{
  public override bool Process()
  {
    dynamic html = new Html5();


    View = html
    .H1("Header One", null, new Attribs { { "style", "color: chartreuse;" } })
    .H2("Header 2", "Id2", "style=background-color: gray;")
    .Script("", null, new Attribs { ["type"] = "text/javascript", ["src"] = "../jslibs/TableEditButtons.js" })
    .Script("", null, "type=text/javascript\n disabled\n src=../jslibs/TableEditButtons.js\n style=vertical-align:middle; color: chartreuse;")
    .H4("Header 4", "Id4", "style=background-color: lime;")
    .H5("Header 5", "Id5", new Attribs { { "style", "color: chartreuse;" } })
    .Span("This is a span element.")
    .BeginP("p_tag", "style=color: lightblue;")
    .AddContent("This is content for the P tag.")
    .EndP()
    .eInput("email", "email", "john@mynetzone.net")
    .Br()
    .eRadioButton("One", "TestGrp", "rb1", 1)
    .eRadioButton("Two", "TestGrp", "rb2", 2)
    .Br()
    .eCheckbox("chkbox1", true, "Test ChkBox")
    .eCheckbox("chkbox2")
    .Br()
    .eDataList("datalist1", new List<string> { "Item 1", "Item 2", "Item 3", "Last Item" })
    .Br()
    .eSelect("select1", new Dictionary<string, string> { { "1", "Item 1" }, { "2", "Item 2" }, { "3", "Item 3" } }, new List<string> { "3" })
    .Br()
    .eTextarea("textarea1", "Some text for the area.", 4, 40)
    .Br()
    .eSubmit("esubmit", "eSubmit")
    .input("submit1", "type = submit\n value = Submit")
    .Br()
    .eInput("radio", "fav_language", "HTML", "id=html")
    .Label("HTML", null, "for=html")
    .eInput("radio", "fav_language", "CSS", "id=css")
    .Label("CSS", null, "for=css")
    .GetContent();

    return base.Process();
  }
}