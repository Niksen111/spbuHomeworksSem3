namespace Lazy;

/// <summary>
/// Provides lazy calculations.
/// </summary>
/// <typeparam name="T">Type of the returnable value.</typeparam>
public interface ILazy<T>
{
    /// <summary>
    /// The first call calls supplier and returns the result
    /// Repeated calls return the same object as the first call, without re-counting.
    /// </summary>
    /// <returns>Result of the calculation.</returns>
    T? Get();
}