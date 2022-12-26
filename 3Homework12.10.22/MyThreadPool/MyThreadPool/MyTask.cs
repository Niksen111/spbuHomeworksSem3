namespace MyThreadPool;

public class MyTask<T> : IMyTask<T>
{
    private MyThreadPool pool;
    private Func<T> func;
    private ManualResetEvent reset = new(false);
    private T result;
    private Exception? retrunedException;

    /// <inheritdoc/>
    public bool IsCompleted { get; private set; } = false;

    public MyTask(Func<T> func, MyThreadPool threadPool)
    {
        this.func = func;
        this.pool = threadPool;
    }

    /// <inheritdoc/>
    public T Result
    {
        get
        {
            this.reset.WaitOne();
            if (this.retrunedException != null)
            {
                throw new AggregateException(this.retrunedException);
            }

            return this.result;
        }
    }

    public void Start()
    {
        try
        {
            this.pool.Submit(new Func<T>(() => this.func()));
        }
        catch (Exception exception)
        {
            this.retrunedException = exception;
        }
        finally
        {
            this.reset.Set();
            this.IsCompleted = true;
        }
    }

    /// <inheritdoc/>
    public IMyTask<TNewResult> ContinueWith<TNewResult>(Func<T, TNewResult> func1)
    {
        if (!this.IsCompleted)
        {
            this.reset.WaitOne();
        }

        return this.pool.Submit(new Func<TNewResult>(() => func1(this.Result)));
    }
}