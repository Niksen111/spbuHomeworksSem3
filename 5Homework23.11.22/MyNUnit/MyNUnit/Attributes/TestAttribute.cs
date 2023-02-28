namespace MyNUnit.Attributes;

/// <summary>
/// Marks the method as callable from the MyNUnit TestsRunner.
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public class TestAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TestAttribute"/> class.
    /// </summary>
    public TestAttribute()
        : this(null, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TestAttribute"/> class.
    /// </summary>
    /// <param name="expectedException">The exception that the method must throw.</param>
    public TestAttribute(Type? expectedException)
        : this(expectedException, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TestAttribute"/> class.
    /// </summary>
    /// <param name="ignore">The reason for ignoring the method.</param>
    public TestAttribute(string? ignore)
        : this(null, ignore)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TestAttribute"/> class.
    /// </summary>
    /// <param name="expectedException">The exception that the method must throw.</param>
    /// <param name="ignore">The reason for ignoring the method.</param>
    public TestAttribute(Type? expectedException, string? ignore)
    {
        this.Expected = expectedException;
        this.Ignore = ignore;
    }

    /// <summary>
    /// Gets or sets the exception that the method must throw.
    /// </summary>
    public Type? Expected { get; set; }

    /// <summary>
    /// Gets or sets the reason for ignoring the method.
    /// </summary>
    public string? Ignore { get; set; }
}