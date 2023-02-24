namespace TestProject;

using MyNUnit.Attributes;

public class Class5
{
    [Test]
    public void PassingTest()
    {
    }

    [Test]
    public static void StaticTest()
    {
    }

    [Test]
    public static int StaticReturnsInt()
    {
        return 1;
    }

    [Test]
    public void HasParameters(int p1, string p2)
    {
    }
}