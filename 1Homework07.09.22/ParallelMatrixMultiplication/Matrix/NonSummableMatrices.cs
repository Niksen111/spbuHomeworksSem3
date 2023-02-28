namespace ParallelMatrixMultiplication;

/// <summary>
/// These matrices cannot be summed.
/// </summary>
[Serializable]
public class NonSummableMatrices : Exception
{
    public NonSummableMatrices() { }
    public NonSummableMatrices(string message) : base(message) { }
    public NonSummableMatrices(string message, Exception inner)
        : base(message, inner) { }
    protected NonSummableMatrices(
        System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context)
        : base(info, context) { }
}