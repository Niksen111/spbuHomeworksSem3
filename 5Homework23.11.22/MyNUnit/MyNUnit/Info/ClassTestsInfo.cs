using System.Text.Json.Serialization;

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
    /// Gets or sets the name of the tested class.
    /// </summary>
    [JsonPropertyName("class-name")]
    public string? ClassName { get; set; }

    /// <summary>
    /// Gets or sets a collection of TestInfo of this class.
    /// </summary>
    [JsonPropertyName("tests-info")]
    public List<TestInfo> TestsInfo { get; set; }

    /// <summary>
    /// Gets or sets comments on the testing of this class.
    /// </summary>
    [JsonPropertyName("comments")]
    public List<string> Comments { get; set; }

    /// <summary>
    /// Gets or sets an exception was thrown by the before/after class methods.
    /// </summary>
    [JsonIgnore]
    public Exception? Exception { get; set; }

    /// <summary>
    /// Gets the cumulative running time of all the tests passed in that class.
    /// </summary>
    [JsonIgnore]
    public long RunningTime
    {
        get
        {
            long counter = 0;
            foreach (var test in this.TestsInfo)
            {
                counter += test.RunningTime;
            }

            return counter;
        }
    }

    /// <summary>
    /// Gets successful tests count.
    /// </summary>
    [JsonIgnore]
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
    [JsonIgnore]
    public int FailedTestsCount => this.TestsInfo.Count - this.SuccessfulTestsCount;
}