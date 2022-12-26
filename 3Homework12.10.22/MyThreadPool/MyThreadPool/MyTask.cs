namespace MyThreadPool;

public class MyTask<T> : IMyTask<T>
{
    public bool IsCompleted { get; } = false;

    public T Result { get; }

    public MyTask(Func<T> func, MyThreadPool threadPool)
    {
        
    }

    public void Start()
    {
        
    }

    public IMyTask<TNewResult> ContinueWith<TNewResult>(Func<T, TNewResult> func)
    {
        throw new NotImplementedException();
    }
}