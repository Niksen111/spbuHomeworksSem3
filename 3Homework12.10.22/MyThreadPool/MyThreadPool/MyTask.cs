namespace MyThreadPool;

public class MyTask<T> : IMyTask<T>
{
    private MyThreadPool pool;
    private Func<T> func;
    private readonly ManualResetEvent reset = new(false);

    public bool IsCompleted { get; } = false;

    public T Result { get; }

    public MyTask(Func<T> func, MyThreadPool threadPool)
    {
        this.func = func;
        this.pool = threadPool;
    }

    public void Start()
    {
        
    }

    public IMyTask<TNewResult> ContinueWith<TNewResult>(Func<T, TNewResult> func)
    {
        if (!this.IsCompleted)
        {
            this.reset.WaitOne();
        }

        return this.pool.Submit(new Func<TNewResult>(() => func(this.Result)));
    }
}