namespace MyThreadPool.Tests;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

public class MyThreadPoolTests
{
    private int threadsInThreadPoolCount = 10;

    [Test]
    public void AtLeastNThreadsInThePool()
    {
        MyThreadPool pool = new(this.threadsInThreadPoolCount);
        var func = new Func<int>(() =>
        {
            Thread.Sleep(5000);
            return 1;
        });

        var stopwatch = new Stopwatch();
        var tasks = new List<IMyTask<int>>();
        stopwatch.Start();
        for (int i = 0; i < this.threadsInThreadPoolCount; ++i)
        {
            tasks.Add(pool.Submit(() => func()));
        }

        for (int i = 0; i < this.threadsInThreadPoolCount; ++i)
        {
            Assert.AreEqual(1, tasks[i].Result);
        }

        stopwatch.Stop();
        Assert.Less(stopwatch.ElapsedMilliseconds, 10000);
        Assert.AreEqual(10, pool.ThreadCount);

        pool.Shutdown();
    }

    [Test]
    public void ManyTasksWorks()
    {
        MyThreadPool pool = new(this.threadsInThreadPoolCount);
        var func = new Func<int>(() =>
        {
            int x = 0;
            for (int i = 1; i < 1000; ++i)
            {
                x += i;
            }

            return x;
        });

        var tasks = new List<IMyTask<int>>();
        for (int i = 0; i < 500; ++i)
        {
            tasks.Add(pool.Submit(() => func()));
        }

        for (int i = 0; i < 500; ++i)
        {
            Assert.AreEqual(499500, tasks[i].Result);
        }

        pool.Shutdown();
    }

    [Test]
    public void ShutdownWorksWithEndlessProcess()
    {
        MyThreadPool pool = new(3);
        var func = new Func<int>(() =>
        {
            Thread.Sleep(20000);
            return 1;
        });

        var tasks = new List<IMyTask<int>>();
        for (int i = 0; i < 2; ++i)
        {
            tasks.Add(pool.Submit(() => func()));
        }

        for (int i = 0; i < 10; ++i)
        {
            tasks.Add(pool.Submit(() => 2 * 2));
        }

        for (int i = 9; i < 12; ++i)
        {
            Assert.AreEqual(4, tasks[i].Result);
        }

        Assert.Catch<TimeoutException>(pool.Shutdown);
    }

    [Test]
    public void SeveralContinueWithWorks()
    {
        MyThreadPool pool = new(this.threadsInThreadPoolCount);
        var func1 = new Func<int>(() =>
        {
            Thread.Sleep(500);
            return 1;
        });

        var func2 = new Func<int, int>(x =>
        {
            Thread.Sleep(500);
            return 2 * x;
        });

        var tasks = new List<IMyTask<int>>();
        tasks.Add(pool.Submit(() => func1()));
        for (int i = 1; i < 10; ++i)
        {
            tasks.Add(tasks[i - 1].ContinueWith(func2));
        }

        int x = 1;
        for (int i = 0; i < 1; ++i)
        {
            Assert.AreEqual(x, tasks[i].Result);
            x *= 2;
        }

        pool.Shutdown();
}

    [Test]
    public void ContinueWithDoesNotBlockThread()
    {
        MyThreadPool pool = new(3);
        var func1 = new Func<int>(() =>
        {
            Thread.Sleep(5000);
            return 1;
        });

        var func2 = new Func<int, int>(x => 2 * x);

        var stopwatch = new Stopwatch();
        var tasks = new List<IMyTask<int>>();
        stopwatch.Start();
        tasks.Add(pool.Submit(() => func1()));
        tasks.Add(tasks[0].ContinueWith(func2));
        tasks.Add(pool.Submit(() => func1()));

        Assert.AreEqual(1, tasks[0].Result);
        Assert.AreEqual(2, tasks[1].Result);
        Assert.AreEqual(1, tasks[2].Result);
        stopwatch.Stop();
        Assert.Less(stopwatch.ElapsedMilliseconds, 10000);

        pool.Shutdown();
    }

    [Test]
    public async Task ParallelSubmitsWorks()
    {
        MyThreadPool pool = new();
        var func1 = new Func<int>(() =>
        {
            Thread.Sleep(50);
            return 1;
        });

        var tasks = new Task<int>[100];
        for (int i = 0; i < 100; ++i)
        {
            tasks[i] = Task.Run(() => pool.Submit(() => func1()).Result);
        }

        for (int i = 0; i < 100; ++i)
        {
            Assert.AreEqual(1, await tasks[i]);
        }

        pool.Shutdown();
    }

    [Test]
    public async Task ParallelSubmitAdnShutDownWorks()
    {
        MyThreadPool pool = new();
        var func1 = new Func<int>(() =>
        {
            Thread.Sleep(50);
            return 1;
        });

        var tasks = new Task<int>[100];
        Task? task1 = null;
        for (int i = 0; i < 100; ++i)
        {
            if (i == 50)
            {
                task1 = Task.Run(() => pool.Shutdown());
            }

            tasks[i] = Task.Run(() => pool.Submit(() => func1()).Result);
        }

        for (int i = 0; i < 100; ++i)
        {
            int result;
            var invalidOperation = new InvalidOperationException().GetType();
            try
            {
                result = tasks[i].Result;
                Assert.AreEqual(1, result);
            }
            catch (AggregateException e)
            {
                Assert.AreEqual(1, e.InnerExceptions.Count);
                Assert.AreEqual(invalidOperation, e.InnerException!.GetType());
            }
        }

        Assert.NotNull(task1);
        if (task1 == null)
        {
            return;
        }

        await task1;
        Assert.IsNull(task1.Exception);

        pool.Shutdown();
    }

    [Test]
    public Task ParallelSubmitsContinueWithAndShutDownWorks()
    {
        MyThreadPool pool = new(1);
        var func1 = new Func<int>(() =>
        {
            Thread.Sleep(50);
            return 1;
        });
        var func2 = new Func<int, int>(x =>
        {
            Thread.Sleep(50);
            return 2 * x;
        });

        var taskSubmit = Task.Run(() => pool.Submit(() => func1()));
        Assert.AreEqual(1, taskSubmit.Result.Result);
        var taskShutDown = Task.Run(() => pool.Shutdown());
        var taskContinueWith = Task.Run(() => taskSubmit.Result.ContinueWith(func2));

        int result;
        var invalidOperation = new InvalidOperationException().GetType();
        try
        {
            result = taskContinueWith.Result.Result;
            Assert.AreEqual(2, result);
        }
        catch (AggregateException e)
        {
            Assert.AreEqual(1, e.InnerExceptions.Count);
            Assert.AreEqual(invalidOperation, e.InnerException!.GetType());
        }

        Assert.IsNull(taskShutDown.Exception);
        pool.Shutdown();
        return Task.CompletedTask;
    }
}