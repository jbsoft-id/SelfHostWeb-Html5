using FluentAssertions;

[assembly: FluentAssertions.Extensibility.AssertionEngineInitializer(
    typeof(AssertionEngineInitializer),
    nameof(AssertionEngineInitializer.AcknowledgeSoftWarning))]

// https://fluentassertions.com/introduction:
// Since Fluent Assertions 8 doesn’t need any license key, there’s a soft warning that is displayed
// for every test run. This is to remind consumers that you need a paid license for commercial use.
// To suppress this warning, there’s a static property called License.Accepted that can be set to
// true. You can add the following code to your test project to automatically toggle this flag.
public static class AssertionEngineInitializer
{
  public static void AcknowledgeSoftWarning()
  {
    License.Accepted = true;
  }
}