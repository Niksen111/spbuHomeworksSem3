using System;
using System.Threading.Tasks;

namespace Lazy.Tests;

using NUnit.Framework;

public class Tests
{
    private Func<object> hugeFunction = () =>
    {
        return 0;
    };

    [SetUp]
    public void SetUp()
    {
        this.hugeFunction = () =>
        {
            int number = 0;
            for (int i = 0; i < 300000000; ++i)
            {
                number += i;
            }

            return number;
        };
    }

    [Test]
    public void OneThreadIsWorking()
    {
        LazyOneThread<object> lazy = new LazyOneThread<object>(this.hugeFunction);
        var object1 = lazy.Get();
        var object2 = lazy.Get();
        var object3 = lazy.Get();
        if (object1 == null || object2 == null || object3 == null)
        {
            Assert.Fail();
            return;
        }

        var object4 = new LazyOneThread<object>(this.hugeFunction).Get();

        Assert.IsTrue(ReferenceEquals(object1, object2));
        Assert.IsTrue(ReferenceEquals(object1, object3));
        Assert.IsTrue(ReferenceEquals(object2, object3));
        Assert.IsFalse(ReferenceEquals(object1, object4));
    }

    [Test]
    public void ParallelLazyIsWorking()
    {
        LazyParallel<object> lazy = new LazyParallel<object>(this.hugeFunction);
        var task1 = Task.Run(() => lazy.Get());
        var task2 = Task.Run(() => lazy.Get());
        var task3 = Task.Run(() => lazy.Get());
        var object1 = task1.Result;
        var object2 = task2.Result;
        var object3 = task3.Result;
        if (object1 == null || object2 == null || object3 == null)
        {
            Assert.Fail();
            return;
        }

        var object4 = new LazyParallel<object>(this.hugeFunction).Get();

        Assert.IsTrue(ReferenceEquals(object1, object2));
        Assert.IsTrue(ReferenceEquals(object1, object3));
        Assert.IsTrue(ReferenceEquals(object2, object3));
        Assert.IsFalse(ReferenceEquals(object1, object4));
    }
}