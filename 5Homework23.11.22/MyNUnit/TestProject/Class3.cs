namespace TestProject;

using MyNUnit.Attributes;

public static class Class3
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
}