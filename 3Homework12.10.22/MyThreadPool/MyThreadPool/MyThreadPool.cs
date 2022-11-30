using System.Collections.Concurrent;

namespace MyThreadPool;

/// <summary>
/// Pool of tasks
/// </summary>
public class MyThreadPool
{
    private ConcurrentQueue<IMyTask<Object>> _tasks;

    public int ThreadsNumber  { get; }

    public MyThreadPool(int threadsNumber)
    {
        if (threadsNumber <= 0)
        {
            throw new BadThreadsNumberException();
        }
        
        ThreadsNumber = threadsNumber;
        _tasks = new ConcurrentQueue<IMyTask<Object>>();
    }
    
    
}
