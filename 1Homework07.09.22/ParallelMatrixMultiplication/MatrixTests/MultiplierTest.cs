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
        Assert.Throws<FileNotFoundException>(() => Matrix.MultiplyOneThreaded(
            new Matrix("../../../TestFiles/Lol.txt"),
            new Matrix("../../../TestFiles/Matrix2.txt")));
        Assert.Throws<FileNotFoundException>(() => Matrix.Multiply(
            new Matrix("../../../TestFiles/Lol.txt"),
            new Matrix("../../../TestFiles/Matrix2.txt")));
    }

    [Test]
    public void NonRepeatableMatrices()
    {
        var matrix1 = new[]
        {
            "1 1 1",
            "1 1 1",
        };
        File.WriteAllLines("../../../TestFiles/Matrix1.txt", matrix1);
        File.WriteAllLines("../../../TestFiles/Matrix2.txt", matrix1);
        Assert.Throws<NonMultipleMatricesException>(() => Matrix.MultiplyOneThreaded(
            new Matrix("../../../TestFiles/Matrix1.txt"),
            new Matrix("../../../TestFiles/Matrix2.txt")).WriteToFile("../../../TestFiles/OutputOneThread.txt"));
        Assert.Throws<NonMultipleMatricesException>(() => Matrix.Multiply(
            new Matrix("../../../TestFiles/Matrix1.txt"),
            new Matrix("../../../TestFiles/Matrix2.txt")).WriteToFile("../../../TestFiles/OutputParallel.txt"));

        var matrix2 = new[] { "1 1 1", "1 1 1", "1 1" };
        File.WriteAllLines("../../../TestFiles/Matrix1.txt", matrix1);
        File.WriteAllLines("../../../TestFiles/Matrix2.txt", matrix2);
        Assert.Throws<InvalidDataException>(() => Matrix.MultiplyOneThreaded(
            new Matrix("../../../TestFiles/Matrix1.txt"),
            new Matrix("../../../TestFiles/Matrix2.txt")).WriteToFile("../../../TestFiles/OutputOneThread.txt"));
        Assert.Throws<InvalidDataException>(() => Matrix.Multiply(
            new Matrix("../../../TestFiles/Matrix1.txt"),
            new Matrix("../../../TestFiles/Matrix2.txt")).WriteToFile("../../../TestFiles/OutputParallel.txt"));
    }

    [Test]
    public void BigMatrices()
    {
        var line = new List<string>();
        for (int i = 0; i < 100; ++i)
        {
            line.Add("100");
        }

        var line1 = string.Join(" ", line.ToArray());

        File.WriteAllLines("../../../TestFiles/Matrix1.txt", new[] { string.Empty });
        File.WriteAllLines("../../../TestFiles/Matrix2.txt", new[] { string.Empty });

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

        Matrix.MultiplyOneThreaded(
            new Matrix("../../../TestFiles/Matrix1.txt"),
            new Matrix("../../../TestFiles/Matrix2.txt")).WriteToFile("../../../TestFiles/OutputOneThread.txt");
        Matrix.Multiply(
            new Matrix("../../../TestFiles/Matrix1.txt"),
            new Matrix("../../../TestFiles/Matrix2.txt")).WriteToFile("../../../TestFiles/OutputParallel.txt");

        Assert.IsTrue(this.AreMatricesIdentical("../../../TestFiles/OutputOneThread.txt", "../../../TestFiles/Result.txt"));
        Assert.IsTrue(this.AreMatricesIdentical("../../../TestFiles/OutputParallel.txt", "../../../TestFiles/Result.txt"));
    }

    [Test]
    public void AddUpWorks()
    {
        var matrix = new Matrix(new int[][] { new int[] { 1, 1, 1 } });
        var answer = new Matrix(new int[][] { new int[] { 2, 2, 2 } });
        Assert.That(Matrix.AddUp(matrix, matrix).ToTwoDimensionalList(), Is.EqualTo(answer.ToTwoDimensionalList()));
    }

    [Test]
    public void TransposeWorks()
    {
        var matrix = new Matrix(new int[][] { new int[] { 1, 1, 1 } });
        matrix.Transpose();
        var answer = new Matrix(new[] { new[] { 1 },  new[] { 1 }, new[] { 1 }, });
        Assert.That(matrix.ToTwoDimensionalArray(), Is.EqualTo(answer.ToTwoDimensionalList()));
    }
}