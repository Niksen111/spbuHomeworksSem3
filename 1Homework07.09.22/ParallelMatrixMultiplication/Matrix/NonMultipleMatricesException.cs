namespace ParallelMatrixMultiplication;

/// <summary>
/// These matrices cannot be multiplied.
/// </summary>
[Serializable]
public class NonMultipleMatricesException : Exception
{
    public NonMultipleMatricesException() { }
    public NonMultipleMatricesException(string message) : base(message) { }
    public NonMultipleMatricesException(string message, Exception inner)
        : base(message, inner) { }
    protected NonMultipleMatricesException(
        System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context)
        : base(info, context) { }
}