namespace MyThreadPool;

public class MyTask<T> : IMyTask<T>
{
    private bool _isComplited = false;
    public bool IsCompleted => _isComplited;

    public T Result { get; }

    public MyTask(Func<T> func)
    {
        
    }

    public IMyTask<TNewResult> ContinueWith<TNewResult>(Func<T, TNewResult> func)
    {
        throw new NotImplementedException();
    }
}