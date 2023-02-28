namespace MyNUnit.Attributes;

/// <summary>
/// Specifies the method that will be called immediately after each test runs.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class AfterAttribute : Attribute
{
}