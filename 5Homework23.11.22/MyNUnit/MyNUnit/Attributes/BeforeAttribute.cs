namespace MyNUnit.Attributes;

/// <summary>
/// Identifies a method to be called immediately before each test is run.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class BeforeAttribute : Attribute
{
}