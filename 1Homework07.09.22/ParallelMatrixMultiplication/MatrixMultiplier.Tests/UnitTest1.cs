using ParallelMatrixMultiplication;

namespace MatrixMultiplier.Tests;

public class Tests
{

    [Test]
    public void NonRepeatableMatrices()
    {
        var matrix1 = new[] {"1 1 1", "1 1 1"};
        File.WriteAllLines("../../../TestFiles/Matrix1.txt", matrix1);
        File.WriteAllLines("../../../TestFiles/Matrix2.txt", matrix1);
        Assert.Throws<NonRepeatableMatricesException>(() => ParallelMatrixMultiplication.MatrixMultiplier.MultiplyOneThreaded("../../../TestFiles/Matrix1.txt", 
            "../../../TestFiles/Matrix2.txt", "../../../TestFiles/OutputOneThread.txt"));
        Assert.Throws<NonRepeatableMatricesException>(() => ParallelMatrixMultiplication.MatrixMultiplier.MultiplyParallel("../../../TestFiles/Matrix1.txt", 
            "../../../TestFiles/Matrix2.txt", "../../../TestFiles/OutputParallel.txt"));
        
        var matrix2 = new[] {"1 1 1", "1 1 1", "1 1"};
        File.WriteAllLines("../../../TestFiles/Matrix1.txt", matrix1);
        File.WriteAllLines("../../../TestFiles/Matrix2.txt", matrix2);
        Assert.Throws<NonRepeatableMatricesException>(() => ParallelMatrixMultiplication.MatrixMultiplier.MultiplyOneThreaded("../../../TestFiles/Matrix1.txt", 
            "../../../TestFiles/Matrix2.txt", "../../../TestFiles/OutputOneThread.txt"));
        Assert.Throws<NonRepeatableMatricesException>(() => ParallelMatrixMultiplication.MatrixMultiplier.MultiplyParallel("../../../TestFiles/Matrix1.txt", 
            "../../../TestFiles/Matrix2.txt", "../../../TestFiles/OutputParallel.txt"));
    }
    
    
}