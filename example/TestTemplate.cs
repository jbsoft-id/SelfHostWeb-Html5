
using jbSoft.Reusable;


[HttpUri("/testtemplate")]
public class TemplateTest : AppTemplateBase
{
  public override bool Process()
  {
    var pv1 = new PlaceholderValues { {"NAME", "Bob"}, {"DATE", "30-Nov-2025"}, {"SIGNATURE", "Staff"} };
    var pv2 = new PlaceholderValues { {"NAME", "Bob"}, {"WHEN", "30-Nov-2025"}, {"SIGNATURE", "Staff"} };
    var pv3 = new PlaceholderValues { {"nAME", "Bob"}, {"date", "30-Nov-2025"}, {"sigNATure", "Staff"} };

    View = $@"
<style>
  H3, H5 {{
    margin-bottom: 0px;
  }}
  H5 {{
    margin-top: 4px;
  }}
  P {{
    margin-top: 0px;
  }}
  P.note {{
    margin-bottom: 0px;
    font-size: 85%;
  }}
  UL {{
    margin-top: 0px;
    margin-bottom: 30px;
  }}
  LI {{
    font-size: 85%;
  }}
</style>

<h1>GetTemplateFromResource() method demo</h1>

<H3>The template:</H3>
<pre class=""run"">{FetchTemplateFromResource("TestTemplate.txt")}</pre>
<p class=""note"">Items of Note:</p>
<ul>
  <li>The original, un-populated, template can be obtained by calling <code>FetchTemplateFromResource()</code>
      with only the template resource name.</li>
  <li>Leave the other parameters defaulted.</li>
</ul>

<H3>Various Test Runs</H3>

<H5>Run 1 - placeholderValues: {pv1}, reportMissingPlaceholders: true</H5>
<pre class=""run"">{FetchTemplateFromResource("TestTemplate.txt", pv1)}</pre>
<p class=""note"">Items of Note:</p>
<ul>
  <li>Since all placeholder names are being passed in, <code>reportMissingPlaceholders</code> will have no effect.</li>
  <li>The placeholder names are case insensitive.</li>
</ul>

<H5>Run 2 - placeholderValues: {pv2}, reportMissingPlaceholders: true</H5>
<pre class=""run"">{FetchTemplateFromResource("TestTemplate.txt", pv2)}</pre>
<p class=""note"">Items of Note:</p>
<ul>
  <li>Since the DATE placeholder is not provided and <code>reportMissingPlaceholders</code> is true, notice that a
      message has been prepended to the result.</li>
  <li>The {{{{DATE}}}} place holder remains in the template.</li>
  <li>Extra PlaceholderValues are ignored.</li>
</ul>

<H5>Run 3 - placeholderValues: {pv2}, reportMissingPlaceholders: false</H5>
<pre class=""run"">{FetchTemplateFromResource("TestTemplate.txt", pv2, false)}</pre>
<p class=""note"">Items of Note:</p>
<ul>
  <li>Since <code>reportMissingPlaceholders</code> is false this time, there is no message indicating that the DATE
      placeholder is missing.</li>
  <li>The {{{{DATE}}}} place holder remains in the template.</li>
  <li>Extra PlaceholderValues are ignored.</li>
</ul>

<H5>Run 4 - placeholderValues: {pv3}, reportMissingPlaceholders: false</H5>
<pre class=""run"">{FetchTemplateFromResource("TestTemplate.txt", pv3, false)}</pre>
<p class=""note"">Items of Note:</p>
<ul>
  <li>Further examples of PlaceholderValue name case mis-matching not mattering.</li>
</ul>";

    return base.Process();
  }
}
