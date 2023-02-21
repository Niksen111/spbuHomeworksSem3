namespace TestProject;

using MyNUnit.Attributes;

public class Class4
{
    private static int localValue;

    public Class4()
    {
        localValue = 0;
    }

    [BeforeClass]
    public static void ChangeLocalValue()
    {
        localValue = 1;
    }

    [Before]
    public void FailedBefore()
    {
        if (localValue == 1)
        {
            throw new InvalidOperationException();
        }
    }

    [Test]
    public void EmptyTest()
    {
    }
}