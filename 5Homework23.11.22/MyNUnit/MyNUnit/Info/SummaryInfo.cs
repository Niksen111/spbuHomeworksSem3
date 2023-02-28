namespace MyNUnit.Info;

using System.Text.Json.Serialization;

/// <summary>
/// Information about running of the tests.
/// </summary>
public class SummaryInfo
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SummaryInfo"/> class.
    /// </summary>
    public SummaryInfo()
    {
        this.AssembliesInfo = new List<AssemblyTestsInfo>();
    }

    /// <summary>
    /// Gets or sets AssemblyTestsInfo collection.
    /// </summary>
    [JsonPropertyName("assemblies-info")]
    public List<AssemblyTestsInfo> AssembliesInfo { get; set; }

    /// <summary>
    /// Gets or sets comment on the testing of all assemblies.
    /// </summary>
    [JsonPropertyName("comment")]
    public string? Comment { get; set; }
}