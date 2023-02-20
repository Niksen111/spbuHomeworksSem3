namespace MyNUnit.Info;


/// <summary>
/// Information about the work of the test.
/// </summary>
public class TestInfo
{
    public TestInfo(string methodName, bool isSuccess, long runningTime, Exception? exception = null, string? reasonForIgnoring = null, string? comment = null)
    {
        this.MethodName = methodName;
        this.IsSuccess = isSuccess;
        this.RunningTime = runningTime;
        this.Exception = exception;
        this.ReasonForIgnoring = reasonForIgnoring;
        this.Comment = comment;
    }

    /// <summary>
    /// Gets the name of the tested method.
    /// </summary>
    public string MethodName { get; }

    /// <summary>
    /// Gets a value indicating whether the test was completed successfully.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets the running time of the test.
    /// </summary>
    public long RunningTime { get; }

    /// <summary>
    /// Gets an exception was thrown by the test.
    /// </summary>
    public Exception? Exception { get; }

    /// <summary>
    /// Gets the reason for ignoring this test.
    /// </summary>
    public string? ReasonForIgnoring { get; }

    /// <summary>
    /// Gets comment on the running of this method.
    /// </summary>
    public string? Comment { get; }
}