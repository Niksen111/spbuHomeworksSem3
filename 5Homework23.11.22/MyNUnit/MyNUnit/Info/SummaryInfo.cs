namespace MyNUnit.Info;

/// <summary>
/// 
/// </summary>
public class SummaryInfo
{
    public SummaryInfo()
    {
        this.AssembliesInfo = new List<AssemblyTestsInfo>();
    }

    /// <summary>
    /// Gets AssemblyTestsInfo collection.
    /// </summary>
    public List<AssemblyTestsInfo> AssembliesInfo;

    /// <summary>
    /// Gets or sets comment on the testing of all assemblies.
    /// </summary>
    public string? Comment;
}