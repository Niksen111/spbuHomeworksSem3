namespace Lazy;

public class LazyOneThread<T> : ILazy<T>
{
    private Func<T>? _supplier;
    private bool _isCalculated;
    private T? _value;

    public LazyOneThread(Func<T> func)
    {
        _supplier = func;
        _isCalculated = false;
    }

    public T? Get()
    {
        if (!_isCalculated)
        {
            _value = _supplier!();
            _isCalculated = true;
            _supplier = null;
        }

        return _value;
    }
}