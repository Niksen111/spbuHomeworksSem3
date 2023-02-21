namespace TestProject;

using MyNUnit.Attributes;

public class Class1
{
    private static int classValue;
    private int localValue;


    public Class1()
    {
        this.localValue = 0;
    }

    [BeforeClass]
    public static void AcceptableBeforeClass()
    {
        classValue = 1;
    }

    [AfterClass]
    public static void AcceptableAfterClass()
    {
        classValue = 2;
    }

    [BeforeClass]
    public void NonStaticBeforeClass()
    {
    }

    [Test("Never mind")]
    [AfterClass]
    public void NonStaticAfterClass()
    {
    }

    [Before]
    public void Before()
    {
        this.localValue = 1;
    }

    [Test]
    [After]
    public void After()
    {
        this.localValue = 2;
    }

    [After]
    public void SpecialAfter()
    {
        if (this.localValue == 3)
        {
            throw new InvalidOperationException();
        }
    }

    [Test]
    public void PassedTest()
    {
        Thread.Sleep(1000);
    }

    [Test]
    public void CheckValueTest()
    {
        Thread.Sleep(1000);
        if (this.localValue != 1 || classValue != 1)
        {
            throw new InvalidOperationException();
        }
    }

    [Test]
    public void FailedTest()
    {
        Thread.Sleep(1000);
        throw new InvalidOperationException();
    }

    [Test(typeof(InvalidOperationException))]
    public void ThrowsRightException()
    {
        Thread.Sleep(1000);
        throw new InvalidOperationException();
    }

    [Test(typeof(InvalidDataException))]
    public void TrowsWrongException()
    {
        Thread.Sleep(1000);
        throw new InvalidOperationException();
    }

    [Test]
    public void SpecialTest()
    {
        Thread.Sleep(1000);
        this.localValue = 3;
    }
}