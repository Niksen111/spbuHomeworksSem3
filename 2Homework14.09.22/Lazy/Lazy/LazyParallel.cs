namespace Lazy;

/// <summary>
/// Multi-threaded implementation of the Lazy interface.
/// </summary>
/// <typeparam name="T">Returnable type.</typeparam>
public class LazyParallel<T> : ILazy<T>
{
    private Func<T>? supplier;
    private volatile bool isCalculated;
    private Value? value;

    /// <summary>
    /// Initializes a new instance of the <see cref="LazyParallel{T}"/> class.
    /// </summary>
    /// <param name="func">Function for calculating.</param>
    public LazyParallel(Func<T> func)
    {
        this.supplier = func;
        this.isCalculated = false;
    }

    /// <inheritdoc/>
    public T? Get()
    {
        if (!this.isCalculated)
        {
            lock (this.supplier!)
            {
                if (!this.isCalculated)
                {
                    Value value1 = new Value(this.supplier());
                    Volatile.Write(ref this.value, value1);
                    this.isCalculated = true;
                    this.supplier = null;
                }
            }
        }

        return Volatile.Read(ref this.value)!.Get();
    }

    private class Value
    {
        private readonly T? value;

        public Value(T value)
        {
            this.value = value;
        }

        public T? Get()
        {
            return this.value;
        }
    }
}