namespace TestProject;

using MyNUnit.Attributes;

public class Class4
{
    private static int classValue;

    [BeforeClass]
    public static void ChangeLocalValue()
    {
        classValue = 100;
    }

    [Before]
    public void FailedBefore()
    {
        if (classValue == 100)
        {
            throw new InvalidOperationException();
        }
    }

    [Test]
    public void EmptyTest()
    {
    }
}