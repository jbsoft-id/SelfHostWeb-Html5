
using jbSoft.Reusable;

using HttpMultipartParser;  // https://github.com/Http-Multipart-Data-Parser/HttpMultipartParser


[HttpUri("/upload")]
public class Upload : AppTemplateBase
{
  public async override Task<bool> Process()
  {
    var placeholderValues = new PlaceholderValues
    {
      {"HttpMethod", HttpMethod}
    };

    if( HttpMethod == "POST" && HasRequestBody )
    {
      placeholderValues["DecodeEntityBody"] = DecodeBody(Request.InputStream);
    }

    View = FetchTemplateFromResource("upload.html", placeholderValues, reportMissingPlaceholders: false);

    return await base.Process();
  }


  private string DecodeBody(Stream stream)
  {
    var result = "";

    var parser = MultipartFormDataParser.Parse(stream);

    // From this point the data is parsed, we can retrieve the
    // form data using the GetParameterValue method.
    //var parameter = parser.GetParameterValue("user_input");
    //result += $"Parameter user_input is {parameter}\n";

    // Files are stored in a list:
    //var file = parser.Files.First();
    //string filename = file.FileName;
    //result += $"Filename: {filename}\n";
    //Stream data = file.Data;
    //using var outputFileStream = new FileStream(filename, FileMode.Create, FileAccess.Write);
    //data.CopyTo(outputFileStream);

    // or to handle them all.

    foreach( var param in parser.Parameters )
    {
      result += $"Parameter: {param.Name} is {param.Data}\n";
    }

    foreach( var file in parser.Files )
    {
      result += $"Filename: {file.FileName}\n";
      if( !string.IsNullOrWhiteSpace(file.FileName) )
      {
        using var outputFileStream = new FileStream(file.FileName, FileMode.Create, FileAccess.Write);
        file.Data.CopyTo(outputFileStream);
      }
    }

    return result;
  }
}