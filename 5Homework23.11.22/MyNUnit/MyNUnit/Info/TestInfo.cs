namespace MyNUnit.Info;

using System.Text.Json.Serialization;

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
    /// Gets or sets the name of the tested method.
    /// </summary>
    [JsonPropertyName("method-name")]
    public string MethodName { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the test was completed successfully.
    /// </summary>
    [JsonPropertyName("is-success")]
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Gets or sets the running time of the test.
    /// </summary>
    [JsonPropertyName("running-time")]
    public long RunningTime { get; set; }

    /// <summary>
    /// Gets or sets an exception was thrown by the test.
    /// </summary>
    [JsonIgnore]
    public Exception? Exception { get; set; }

    /// <summary>
    /// Gets or sets the reason for ignoring this test.
    /// </summary>
    [JsonPropertyName("reason-for-ignoring")]
    public string? ReasonForIgnoring { get; set; }

    /// <summary>
    /// Gets or sets comment on the running of this method.
    /// </summary>
    [JsonPropertyName("comment")]
    public string? Comment { get; set; }
}