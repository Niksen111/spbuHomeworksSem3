namespace MyThreadPool;

/// <inheritdoc />
public class MyTask<T> : IMyTask<T>
{
    private MyThreadPool pool;
    private Func<T> func;
    private ManualResetEvent reset = new(false);
    private T result;
    private Exception? returnedException;

    /// <inheritdoc/>
    public bool IsCompleted { get; private set; } = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="MyTask{T}"/> class.
    /// </summary>
    /// <param name="func">Function that will performed.</param>
    /// <param name="threadPool"></param>
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
            if (this.returnedException != null)
            {
                throw new AggregateException(this.returnedException);
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
            this.returnedException = exception;
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