namespace TestProject;

using MyNUnit.Attributes;

public class Class2
{
    [BeforeClass]
    public static void NotWorkingBeforeClass()
    {
        throw new InvalidOperationException();
    }

    [Test]
    public void PassingTest()
    {
    }
}