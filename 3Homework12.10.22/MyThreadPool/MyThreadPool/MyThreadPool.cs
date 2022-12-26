using System.Collections.Concurrent;

namespace MyThreadPool;

/// <summary>
/// Pool of tasks
/// </summary>
public class MyThreadPool
{
    private BlockingCollection<Action> _tasks;
    private MyThread[] _myThreads;
    private CancellationTokenSource _source = new ();
    private bool _isShutdown = false;

    public int ThreadCount  { get; }

    public MyThreadPool(int threadCount = 10)
    {
        if (threadCount <= 0)
        {
            throw new BadThreadsNumberException();
        }
        
        ThreadCount = threadCount;
        _tasks = new ();
        _myThreads = new MyThread[threadCount];
        
        for (int i = 0; i < threadCount; ++i)
        {
            _myThreads[i] = new MyThread(_tasks, _source.Token);
        }
    }

    public IMyTask<TResult> Submit<TResult>(Func<TResult> func)
    {
        var task = new MyTask<TResult>(func);
        return null;
    }

    public void Shutdown()
    {
        if (_isShutdown)
        {
            return;
        }
        _tasks.CompleteAdding();
        _source.Cancel();
    }

    private class MyThread
    {
        private Thread _thread;
        private BlockingCollection<Action> _collection;

        public MyThread(BlockingCollection<Action> collection, CancellationToken token)
        {
            _collection = collection;
        }

        public bool IsWaiting { get; private set; }

        public void Join() => _thread.Join();
    }
}
