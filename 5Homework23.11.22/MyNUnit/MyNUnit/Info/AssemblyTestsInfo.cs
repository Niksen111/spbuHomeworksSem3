using System.Text.Json.Serialization;

namespace MyNUnit.Info;

/// <summary>
/// Information about the work of the class tests of this assembly.
/// </summary>
public class AssemblyTestsInfo
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AssemblyTestsInfo"/> class.
    /// </summary>
    /// <param name="assemblyPath">Path to the assembly.</param>
    public AssemblyTestsInfo(string assemblyPath)
    {
        this.AssemblyPath = assemblyPath;
        this.ClassesInfo = new List<ClassTestsInfo>();
    }

    /// <summary>
    /// Gets Assembly path.
    /// </summary>
    [JsonPropertyName("assembly-path")]
    public string AssemblyPath { get; }

    /// <summary>
    /// Gets ClassesInfo collection.
    /// </summary>
    [JsonPropertyName("classes-info")]
    public List<ClassTestsInfo> ClassesInfo { get; }

    /// <summary>
    /// Gets or sets comment on the testing of this assembly.
    /// </summary>
    [JsonPropertyName("comment")]
    public string? Comment { get; set; }
}