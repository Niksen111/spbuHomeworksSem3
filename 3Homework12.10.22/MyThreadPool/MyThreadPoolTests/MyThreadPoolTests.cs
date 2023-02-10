namespace MyThreadPool.Tests;

using System;

using NUnit.Framework;

public class MyThreadPoolTests
{
    private int threadsInThreadPoolCount = 10;
    private MyThreadPool pool;

    [SetUp]
    public void SetUp()
    {
        pool = new MyThreadPool(threadsInThreadPoolCount);
    }
    
    [Test]
    public void ThreadPoolWorks()
    {
        var x = new Func<int>(() =>
        {
            return 1;
        });
        MyThreadPool pool = new(1);
        var myTask = pool.Submit(() => x());
        Assert.AreEqual(1, myTask.Result);
        pool.Shutdown();
    }

    [Test]
    public void Thread()
    {
        
    }
}