namespace TestProject;

using MyNUnit.Attributes;

public class Class3
{
    [BeforeClass]
    public static void PassingBeforeClass()
    {
    }

    [AfterClass]
    public static void FailedAfterClass()
    {
        throw new InvalidOperationException();
    }

    [Test]
    public static void PassingTest()
    {
    }

    [Test]
    public void NonStaticPassingTest()
    {
    }
}