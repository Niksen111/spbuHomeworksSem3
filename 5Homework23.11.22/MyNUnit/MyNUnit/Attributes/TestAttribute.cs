namespace MyNUnit.Attributes;

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public class TestAttribute : Attribute
{
    public TestAttribute() : this(null, null)
    {
        
    }
    
    public TestAttribute(Type? expectedException) : this(expectedException, null)
    {
        
    }
    
    public TestAttribute(string? ignore) : this(null, ignore)
    {
        
    }
    
    public TestAttribute(Type? expectedException, string? ignore)
    {
        this.Expected = expectedException;
        this.Ignore = ignore;
    }
    
    public Type? Expected { get; set; }
    
    public string? Ignore { get; set; }
}