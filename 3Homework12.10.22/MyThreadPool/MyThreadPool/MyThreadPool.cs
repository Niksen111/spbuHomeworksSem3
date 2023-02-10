﻿namespace MyThreadPool;

using System.Collections.Concurrent;

/// <summary>
/// The ThreadPool abstraction.
/// </summary>
public class MyThreadPool
{
    private BlockingCollection<Action> tasks;
    private MyThread[] threads;
    private CancellationTokenSource source = new();
    private bool isShutdown = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="MyThreadPool"/> class.
    /// </summary>
    /// <param name="threadCount">Number of this ThreadPool threads.</param>
    public MyThreadPool(int threadCount = 10)
    {
        if (threadCount <= 0)
        {
            throw new InvalidDataException();
        }

        this.ThreadCount = threadCount;
        this.tasks = new();
        this.threads = new MyThread[threadCount];

        for (int i = 0; i < threadCount; ++i)
        {
            this.threads[i] = new MyThread(this.tasks, this.source.Token);
        }
    }

    /// <summary>
    /// Gets number of existing threads.
    /// </summary>
    public int ThreadCount { get; }

    /// <summary>
    /// Submits new task to the ThreadPool.
    /// </summary>
    /// <param name="func">A calculation to perform.</param>
    /// <typeparam name="TResult">Value type.</typeparam>
    /// <returns>Task.</returns>
    public IMyTask<TResult> Submit<TResult>(Func<TResult> func)
    {
        var task = new MyTask<TResult>(func, this);
        this.tasks.Add(() => task.Start());
        return task;
    }

    /// <summary>
    /// Completes the threads.
    /// </summary>
    public void Shutdown()
    {
        if (this.isShutdown)
        {
            return;
        }

        this.tasks.CompleteAdding();

        while (this.tasks.Count > 0)
        {
        }

        this.source.Cancel();
        foreach (var thread in this.threads)
        {
            thread.Join();
            if (thread.IsWorking)
            {
                thread.Interrupt();
            }
        }
    }

    private class MyThread
    {
        private Thread thread;
        private BlockingCollection<Action> collection;
        private int timeout = 5000;

        public MyThread(BlockingCollection<Action> collection, CancellationToken token)
        {
            this.collection = collection;
            this.thread = new Thread(() => this.Start(token));
            this.IsWorking = false;
            this.thread.Start();
        }

        public bool IsWorking { get; private set; }

        public void Join()
        {
            if (this.thread.IsAlive)
            {
                this.thread.Join(this.timeout);
            }
        }

        public void Interrupt()
        {
            this.thread.Interrupt();
        }

        private void Start(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                if (this.collection.TryTake(out var action))
                {
                    this.IsWorking = true;
                    action();
                    this.IsWorking = false;
                }
            }
        }
    }
}