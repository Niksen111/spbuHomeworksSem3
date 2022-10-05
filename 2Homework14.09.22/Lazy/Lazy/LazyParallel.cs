namespace Lazy;

/// <summary>
/// 
/// </summary>
/// <typeparam name="T"></typeparam>
public class LazyParallel<T> : ILazy<T>
{
    private Func<T>? _supplier;
    private bool _isCalculated;
    private Value? _value;

    public LazyParallel(Func<T> func)
    {
        _supplier = func;
        _isCalculated = false;
    }

    private class Value 
    {
        private readonly T? _value;
        
        public Value(T value)
        {
            _value = value;
        }

        public T? Get()
        {
            return _value;
        }
    }

    public T? Get()
    {
        if (!_isCalculated)
        {
            lock (_supplier!)
            {
                if (!_isCalculated)
                {
                    Value value = new Value(_supplier());
                    Volatile.Write( ref _value, value);
                    _isCalculated = true;
                    _supplier = null;
                }
            }
        }
        
        return Volatile.Read(ref _value)!.Get();
    }
}