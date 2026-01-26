
using jbSoft.Reusable;


[HttpUri("/resthtml5")]
public class RestHtml5 : AppTemplateBase
{
  private string _description = @"
On this example page we will explore using Async requests to a REST 'like' backend API that uses the Html5 class
to generate parts of the page that will be bundled in a JSON response object and returned.  
<br><br>
The async request is made by calling SendData() which is defined in the AppTemplate.html file, which in turn 
invokes the built in Javascript fetch() method.
<br><br>
The REST 'like' api is implemented in UpdateTable.cs.  It receives JSON data that contains user input from the 
form input fields on this page.  It then generates response data and returns it as JSON data.
<br><br>
Lastly, this page uses a local UpdateFunction() function that is called when the number input is changed or by
clicking the Update button.  It gathers the input data and calls the SendData() method and then handles the 
response by updating the DOM.
";

  private string _css = @"
Label {
  width: 90px;
  display: inline-block;
  text-align: right;
}
Label::after { content: "":""; }
/*
** Table styling.
*/
DIV.tableFixHead {
  overflow-y: auto;
  max-height: 75vh;
  margin-bottom: 5px;
  border: 1px solid #000000;
}
.AsyncListTable {
  width: 100%;
  border-spacing: 0px 0px;
  border: 1px solid #000000;
  font-size: 14px;
}
.AsyncListTable thead {
  position: sticky;
  top: 0;
}
.AsyncListTable thead th,
.AsyncListTable thead td {
  color: rgba(37, 32, 32, 0.932);
  background-color: #95d097;
  font-family: Arial, Helvetica, sans-serif;
  font-size: 12px;
  font-weight: bold;
}
.AsyncListTable tfoot th {
  position: sticky;
  bottom: 0;
  color: rgba(37, 32, 32, 0.932);
  background-color: #95d097;
  font-family: Arial, Helvetica, sans-serif;
  font-size: 12px;
  font-weight: bold;
}
.AsyncListTable td,
.AsyncListTable th {
  padding-left: 3px;
  padding-right: 2px;
  border-top: 2px solid #000000;
  border-bottom: 0px solid #000000;
  border-left: 0px solid #000000;
  border-right: 1px solid #000000;
}
.AsyncListTable .ClickableTd {
  cursor: pointer;
}
.AsyncListTable BUTTON,
.AsyncListTable INPUT[type=button],
.AsyncListTable INPUT[type=submit],
.AsyncListTable INPUT[type=reset] {
  font-family: Arial, Helvetica, sans-serif;
	font-size: 14px;
  vertical-align: middle;
  padding: 1px 6px 1px 6px;
  min-width: auto;
}

TD.left {
  text-align: left;
}
TD.center {
  text-align: center;
}
TD.right {
  text-align: right;
}

.AsyncListTable TR:nth-child(odd) {
  color: #DDDDDD;
  background-color: #435161;
}
.AsyncListTable TR:nth-child(even) {
  color: #DDDDDD;
  background-color: #384250;
}
.AsyncListTable TR:hover {
  background-color:rgba(19, 135, 83, 0.437);
}  ";

  private string _script = @"
function UpdateTable()
{
  data = 
  {
    Rows: document.getElementById('rows').value,
    Value: document.getElementById('val').value,
    Select: document.getElementById('select').value,
  };

  SendData(data, 'updatetable', 'PUT')
  .then((respJson) =>
  {
    console.log('respJson: ' + JSON.stringify(respJson, null, 2));
    document.getElementById('tbody').innerHTML = respJson.TBody;
    document.getElementById('select').innerHTML = respJson.SelOpts;
    document.getElementById('footer').innerHTML = respJson.Footer;
  })
  .catch((ex) => { console.log('UpdateTable promise Failed: ' + ex); });
}";


  public override bool Process()
  {
    dynamic html = new Html5();

    Style = _css;
    Script = _script;

    View = html
    .H1("Async page updates with REST and Html5")
    .Br()
    .P(_description)
    .Br()
    .BeginFieldset()
      .Legend("Inputs")
      .BeginForm()
        .Label("Rows").eInput("number", "rows", "0", "onChange=UpdateTable()").Span("This input updates the table on change.", null, "style=font-size: 80%;").Br()
        .Label("Value").eInput("text", "val", "Default").Br()
        .Label("Select").eSelect("select", new Dictionary<string, string> { { "0", "None" } }, null, null, null).Br()
        .Button("Update", "update", "onClick=UpdateTable(); return false;").Br()
      .EndForm()
    .EndFieldset()
    .Br()
    .BeginDiv(null, "class=tableFixHead")
      .BeginTable("jobs", "class=AsyncListTable")
        .BeginTHead()
          .BeginTr()
            .Th("Id").Th("Name").Th("Value")
          .EndTr()
        .EndTHead()
        .TBody(null, "tbody")
        .BeginTFoot()
          .BeginTr()
            .Th("&nbsp;", "footer", "colspan=99\n class=center")
          .EndTr()
        .EndTFoot()
      .EndTable()
    .EndDiv()
    .GetContent();

    return base.Process();
  }
}