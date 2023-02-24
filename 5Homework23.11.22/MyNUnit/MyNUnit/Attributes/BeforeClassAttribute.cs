namespace MyNUnit.Attributes;

/// <summary>
/// Specifies the method that will be called once before running all tests in the class.
/// The method must be static.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class BeforeClassAttribute : Attribute
{
}