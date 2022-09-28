namespace Lazy;

public class LazyParallel<T> : ILazy<T>
{
    private Func<T>? _supplier;
    private bool _isCalculated;
    private T? _value;

    public LazyParallel(Func<T> func)
    {
        _supplier = func;
        _isCalculated = false;
    }

    public T? Get()
    {
        if (!_isCalculated)
        {
            lock (_supplier!)
            {
                _value = _supplier!();
            }
            _isCalculated = true;
            _supplier = null;
        }

        return _value;
    }
}