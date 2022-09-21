namespace MatrixMultiplier.Tests;

public class Tests
{

    [Test]
    public void Test1()
    {
        ParallelMatrixMultiplication.MatrixMultiplier.Multiply("../../../TestFiles/Matrix1.txt", 
            "../../../TestFiles/Matrix2.txt", "../../../TestFiles/Output.txt");
    }
}