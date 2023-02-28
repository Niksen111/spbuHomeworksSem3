namespace Lazy;

/// <summary>
/// One-threaded implementation of the Lazy interface.
/// </summary>
/// <typeparam name="T">Returnable type.</typeparam>
public class LazyOneThread<T> : ILazy<T>
{
    private Func<T>? supplier;
    private bool isCalculated;
    private T? value;

    /// <summary>
    /// Initializes a new instance of the <see cref="LazyOneThread{T}"/> class.
    /// </summary>
    /// <param name="func">Function for calculating.</param>
    public LazyOneThread(Func<T> func)
    {
        this.supplier = func;
        this.isCalculated = false;
    }

    /// <inheritdoc/>
    public T? Get()
    {
        if (!this.isCalculated)
        {
            this.value = this.supplier!();
            this.isCalculated = true;
            this.supplier = null;
        }

        return this.value;
    }
}