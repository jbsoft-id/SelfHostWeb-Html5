
using System.Text.Json;
using System.Text.Json.Nodes;

using jbSoft.Reusable;


[HttpUri("/updatetable")]
public class UpdateTable : HttpTransaction
{
  public async override Task<bool> Process()
  {
    var result = false;

    var jsonString = GetRequestBody();

    if(jsonString != null)
    {
      var jsonNode = JsonNode.Parse(jsonString);

      if( jsonNode != null )
      {
        dynamic html = new Html5();

        var rows = int.Parse((string?)jsonNode["Rows"] ?? "0");
        var value = (string?)jsonNode["Value"] ?? "";
        var selected = int.Parse((string?)jsonNode["Select"] ?? "0");

        for(int i = 1; i<=rows; i++)
        {
          html
          .BeginTr(null, $"style={(selected == i ? "font-weight: bolder; color: peru;" : "")}")
            .Td(i.ToString())
            .Td($"Row {i}")
            .Td(value)
          .EndTr();
        }

        var tbody = html.GetContent(true);

        html.Option("None", null, $"value={0}");

        for(int i = 1; i<=rows; i++)
        {
          html
          .Option($"Row {i}", null, $"value={i}{(selected == i ? "\nselected=true" : "")}");
        }

        ContentType = "application/json";
        Content = JsonSerializer.Serialize(new
        {
          TBody = tbody,
          SelOpts = html.GetContent(),
          Footer = $"Showing {rows} rows.",
        });

        result = true;
      }
    }

    return result;
  }
}