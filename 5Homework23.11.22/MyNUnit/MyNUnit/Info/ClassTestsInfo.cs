namespace MyNUnit.Info;

/// <summary>
/// Information about the work of the tests of this class.
/// </summary>
public class ClassTestsInfo
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ClassTestsInfo"/> class.
    /// </summary>
    /// <param name="className">Name of the tested class.</param>
    public ClassTestsInfo(string? className)
    {
        this.ClassName = className;
        this.TestsInfo = new List<TestInfo>();
        this.Comments = new List<string>();
    }

    /// <summary>
    /// Gets the name of the tested class.
    /// </summary>
    public string? ClassName { get; }

    /// <summary>
    /// Gets a collection of TestInfo of this class.
    /// </summary>
    public List<TestInfo> TestsInfo { get; }

    /// <summary>
    /// Gets or sets comments on the testing of this class.
    /// </summary>
    public List<string> Comments { get; }

    /// <summary>
    /// Gets an exception was thrown by the before/after class methods.
    /// </summary>
    public Exception? Exception;

    /// <summary>
    /// Gets successful tests count.
    /// </summary>
    public int SuccessfulTestsCount
    {
        get
        {
            var cnt = 0;
            foreach (var test in this.TestsInfo)
            {
                if (test.IsSuccess)
                {
                    ++cnt;
                }
            }

            return cnt;
        }
    }

    /// <summary>
    /// Gets failed tests count.
    /// </summary>
    public int FailedTestsCount => this.TestsInfo.Count - this.SuccessfulTestsCount;
}