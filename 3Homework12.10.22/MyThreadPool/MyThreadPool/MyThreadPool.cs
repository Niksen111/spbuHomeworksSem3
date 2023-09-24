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
    private bool isShutdown;
    private AutoResetEvent tasksOver;
    private Semaphore taskCount = new Semaphore(0, 10000);

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
        this.tasksOver = new AutoResetEvent(false);

        for (int i = 0; i < threadCount; ++i)
        {
            this.threads[i] = new MyThread(this.tasks, this.source.Token, this.tasksOver, this.taskCount);
        }

        this.isShutdown = false;
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
        this.tasksOver.Reset();
        this.taskCount.Release();
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
        var timeoutThread = new Thread(() =>
        {
            Thread.Sleep(20000);
            this.tasksOver.Set();
        });

        timeoutThread.Start();
        this.tasksOver.WaitOne();

        if (this.tasks.Count > 0)
        {
            throw new TimeoutException("Tasks from the queue cannot be executed.");
        }

        this.source.Cancel();

        for (int i = 0; i < this.ThreadCount; i++)
        {
            this.taskCount.Release();
        }

        var areJoined = true;
        foreach (var thread in this.threads)
        {
            thread.Join();
            if (thread.IsWorking)
            {
                areJoined = false;
            }
        }

        this.isShutdown = true;
        if (!areJoined)
        {
            throw new TimeoutException("Not all tasks were accomplished.");
        }
    }

    private class MyThread
    {
        private Thread thread;
        private BlockingCollection<Action> collection;
        private int timeout = 5000;
        private AutoResetEvent tasksOver;
        private Semaphore tasksCount;

        public MyThread(BlockingCollection<Action> collection, CancellationToken token, AutoResetEvent tasksOver, Semaphore tasksCount)
        {
            this.tasksOver = tasksOver;
            this.collection = collection;
            this.tasksCount = tasksCount;
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
                else
                {
                    this.tasksOver.Set();
                    this.tasksCount.WaitOne();
                }
            }
        }
    }

    private class MyTask<T> : IMyTask<T>
    {
        private MyThreadPool pool;
        private Func<T>? func;
        private ManualResetEvent resetEvent = new(false);
        private T? result;
        private Exception? returnedException;
        private BlockingCollection<Action> deferredTasks;

        /// <summary>
        /// Initializes a new instance of the <see cref="MyTask{T}"/> class.
        /// </summary>
        /// <param name="func">Function that will performed.</param>
        /// <param name="threadPool">Pool for the submit.</param>
        public MyTask(Func<T> func, MyThreadPool threadPool)
        {
            this.func = func;
            this.pool = threadPool;
            this.IsCompleted = false;
            this.deferredTasks = new BlockingCollection<Action>();
        }

        /// <inheritdoc/>
        public bool IsCompleted { get; private set; }

        /// <inheritdoc/>
        public T Result
        {
            get
            {
                this.resetEvent.WaitOne();
                if (this.returnedException != null)
                {
                    throw new AggregateException(this.returnedException);
                }

                return this.result!;
            }
        }

        public void Start()
        {
            try
            {
                this.result = this.func!();
            }
            catch (Exception exception)
            {
                this.returnedException = exception;
            }
            finally
            {
                this.func = null;
                this.IsCompleted = true;
                this.resetEvent.Set();
                this.deferredTasks.CompleteAdding();
                foreach (var task in this.deferredTasks)
                {
                    this.pool.Submit(() => task);
                }
            }
        }

        /// <inheritdoc/>
        public IMyTask<TNewResult> ContinueWith<TNewResult>(Func<T, TNewResult> func1)
        {
            if (this.result != null)
            {
                return this.pool.Submit(() => func1(this.Result));
            }

            MyTask<TNewResult> newTask = new MyTask<TNewResult>(() => func1(this.Result), this.pool);
            try
            {
                this.deferredTasks.Add(() => newTask.Start());
            }
            catch (InvalidOperationException)
            {
                return this.pool.Submit(() => func1(this.Result));
            }

            return newTask;
        }
    }
}