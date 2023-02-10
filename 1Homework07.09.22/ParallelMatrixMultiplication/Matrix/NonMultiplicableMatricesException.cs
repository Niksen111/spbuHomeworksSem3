namespace ParallelMatrixMultiplication;

/// <summary>
/// These matrices cannot be multiplied.
/// </summary>
[Serializable]
public class NonMultiplicableMatricesException : Exception
{
    public NonMultiplicableMatricesException() { }
    public NonMultiplicableMatricesException(string message) : base(message) { }
    public NonMultiplicableMatricesException(string message, Exception inner)
        : base(message, inner) { }
    protected NonMultiplicableMatricesException(
        System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context)
        : base(info, context) { }
}