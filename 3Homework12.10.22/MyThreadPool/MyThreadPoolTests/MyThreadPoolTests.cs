namespace MyThreadPool.Tests;

using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

using System;

using NUnit.Framework;

public class MyThreadPoolTests
{
    private int threadsInThreadPoolCount = 10;
    
    [Test]
    public void AtLeastNThreadsInThePool()
    {
        MyThreadPool pool = new(threadsInThreadPoolCount);
        var func = new Func<int>(() =>
        {
            Thread.Sleep(5000);
            return 1;
        });

        var stopwatch = new Stopwatch();
        var tasks = new List<IMyTask<int>>();
        stopwatch.Start();
        for (int i = 0; i < threadsInThreadPoolCount; ++i)
        {
            tasks.Add(pool.Submit(() => func()));
        }

        for (int i = 0; i < threadsInThreadPoolCount; ++i)
        {
            Assert.AreEqual(1, tasks[i].Result);
        }
        
        stopwatch.Stop();
        Assert.Less(stopwatch.ElapsedMilliseconds, 10000);

        pool.Shutdown();
    }

    [Test]
    public void ManyTasksWorks()
    {
        MyThreadPool pool = new(threadsInThreadPoolCount);
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
        MyThreadPool pool = new(threadsInThreadPoolCount);
        var func1 = new Func<int>(() =>
        {
            Thread.Sleep(500);
            return 1;
        });

        var func2 = new Func<int, int>((x) =>
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
        
        var func2 = new Func<int, int>( x => 2 * x );

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
}