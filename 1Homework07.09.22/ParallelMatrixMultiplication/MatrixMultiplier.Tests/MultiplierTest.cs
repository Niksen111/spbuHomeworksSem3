namespace MatrixMultiplier.Tests;

using ParallelMatrixMultiplication;

public class Tests
{
    private bool AreMatricesIdentical(string path1, string path2)
    {
        StreamReader file1 = new(path1);
        StreamReader file2 = new(path2);

        while (true)
        {
            var line1 = file1.ReadLine();
            var line2 = file2.ReadLine();
            if (line1 == null && line2 == null)
            {
                file1.Close();
                file2.Close();
                return true;
            }

            if (line1 == null ^ line2 == null)
            {
                file1.Close();
                file2.Close();
                return false;
            }

            if (String.Compare(line1, line2) != 0)
            {
                file1.Close();
                file2.Close();
                return false;
            }
        }
    }

    [Test]
    public void WrongPath()
    {
        Assert.Throws<FileNotFoundException>(() => ParallelMatrixMultiplication.MatrixMultiplier.MultiplyOneThreaded(
            "../../../TestFiles/Lol.txt",
            "../../../TestFiles/Matrix2.txt", "../../../TestFiles/OutputOneThread.txt"));
        Assert.Throws<FileNotFoundException>(() => ParallelMatrixMultiplication.MatrixMultiplier.MultiplyParallel(
            "../../../TestFiles/Lol.txt",
            "../../../TestFiles/Matrix2.txt", "../../../TestFiles/OutputParallel.txt"));
    }

    [Test]
    public void NonRepeatableMatrices()
    {
        var matrix1 = new[] {
            "1 1 1", 
            "1 1 1"};
        File.WriteAllLines("../../../TestFiles/Matrix1.txt", matrix1);
        File.WriteAllLines("../../../TestFiles/Matrix2.txt", matrix1);
        Assert.Throws<NonMultipleMatricesException>(() => ParallelMatrixMultiplication.MatrixMultiplier.MultiplyOneThreaded("../../../TestFiles/Matrix1.txt", 
            "../../../TestFiles/Matrix2.txt", "../../../TestFiles/OutputOneThread.txt"));
        Assert.Throws<NonMultipleMatricesException>(() => ParallelMatrixMultiplication.MatrixMultiplier.MultiplyParallel("../../../TestFiles/Matrix1.txt", 
            "../../../TestFiles/Matrix2.txt", "../../../TestFiles/OutputParallel.txt"));
        
        var matrix2 = new[] {"1 1 1", "1 1 1", "1 1"};
        File.WriteAllLines("../../../TestFiles/Matrix1.txt", matrix1);
        File.WriteAllLines("../../../TestFiles/Matrix2.txt", matrix2);
        Assert.Throws<NonMultipleMatricesException>(() => ParallelMatrixMultiplication.MatrixMultiplier.MultiplyOneThreaded("../../../TestFiles/Matrix1.txt", 
            "../../../TestFiles/Matrix2.txt", "../../../TestFiles/OutputOneThread.txt"));
        Assert.Throws<NonMultipleMatricesException>(() => ParallelMatrixMultiplication.MatrixMultiplier.MultiplyParallel("../../../TestFiles/Matrix1.txt", 
            "../../../TestFiles/Matrix2.txt", "../../../TestFiles/OutputParallel.txt"));
    }

    [Test]
    public void BigMatrices()
    {
        var line = new List<string>();
        for (int i = 0; i < 100; ++i)
        {
            line.Add("100");
        }

        var line1 = String.Join(" ", line.ToArray());

        File.WriteAllLines("../../../TestFiles/Matrix1.txt", new []{""});
        File.WriteAllLines("../../../TestFiles/Matrix2.txt", new []{""});
        
        StreamWriter file1 = new("../../../TestFiles/Matrix1.txt");
        StreamWriter file2 = new("../../../TestFiles/Matrix2.txt");
        
        for (int i = 0; i < 100; ++i)
        {
            if (line1 == null)
            {
                Assert.Fail();
            }
            file1.WriteLine(line1);
            file2.WriteLine(line1);
        }
        
        file1.Close();
        file2.Close();

        ParallelMatrixMultiplication.MatrixMultiplier.MultiplyOneThreaded("../../../TestFiles/Matrix1.txt",
            "../../../TestFiles/Matrix2.txt", "../../../TestFiles/OutputOneThread.txt");
        ParallelMatrixMultiplication.MatrixMultiplier.MultiplyParallel("../../../TestFiles/Matrix1.txt",
            "../../../TestFiles/Matrix2.txt", "../../../TestFiles/OutputParallel.txt");
        
        Assert.IsTrue(AreMatricesIdentical("../../../TestFiles/OutputOneThread.txt", "../../../TestFiles/Result.txt"));
        Assert.IsTrue(AreMatricesIdentical("../../../TestFiles/OutputParallel.txt", "../../../TestFiles/Result.txt"));
    }
}