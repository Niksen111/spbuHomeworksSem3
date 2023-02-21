namespace MyNUnit.Info;

/// <summary>
/// Information about running of the tests.
/// </summary>
public class SummaryInfo
{
    /// <summary>
    /// Gets AssemblyTestsInfo collection.
    /// </summary>
    public List<AssemblyTestsInfo> AssembliesInfo;

    /// <summary>
    /// Gets or sets comment on the testing of all assemblies.
    /// </summary>
    public string? Comment;

    /// <summary>
    /// Initializes a new instance of the <see cref="SummaryInfo"/> class.
    /// </summary>
    public SummaryInfo()
    {
        this.AssembliesInfo = new List<AssemblyTestsInfo>();
    }
}