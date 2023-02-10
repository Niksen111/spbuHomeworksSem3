namespace ParallelMatrixMultiplication;

/// <summary>
/// Matrix abstraction.
/// </summary>
public class Matrix
{
    /// <summary>
    /// Gets gives the number of rows in the matrix.
    /// </summary>
    public int RowsCount => matrix.Count;

    /// <summary>
    /// Gets gives the number of columns in the matrix.
    /// </summary>
    public int ColumnsCount => this.matrix[0].Count;

    private List<List<int>> matrix;

    /// <summary>
    /// Initializes a new instance of the <see cref="Matrix"/> class.
    /// </summary>
    /// <param name="matrix">The base for creating a matrix.</param>
    public Matrix(List<List<int>> matrix)
    {
        this.matrix = matrix;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Matrix"/> class.
    /// </summary>
    /// <param name="path">The base for creating a matrix.</param>
    public Matrix(string path)
    {
        this.matrix = this.GetMatrixFromFile(path);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Matrix"/> class.
    /// </summary>
    /// <param name="matrix">The base for creating a matrix.</param>
    public Matrix(int[][] matrix)
    {
        this.matrix = new List<List<int>>();
        for (int i = 0; i < matrix.Length; ++i)
        {
            this.matrix.Add(new List<int>(matrix[i]));
        }
    }

    private List<List<int>> GetMatrixFromFile(string path)
    {
        var lines = File.ReadAllLines(path);
        var newMatrix = new List<List<int>>();
        for (int i = 0; i < lines.Length; ++i)
        {
            newMatrix.Add(new List<int>());
            newMatrix[i].AddRange(lines[i].Split().Select(n => int.Parse(n)).ToList());
            if (i != 0 && newMatrix[i].Count != newMatrix[0].Count)
            {
                throw new InvalidDataException();
            }
        }

        return newMatrix;
    }

    private static void CheckMatrices(Matrix matrixA, Matrix matrixB)
    {
        if (matrixA.ColumnsCount != matrixB.RowsCount)
        {
            throw new NonMultiplicableMatricesException();
        }
    }

    /// <summary>
    /// Parallel multiplies matrices.
    /// </summary>
    /// <param name="matrixA">the first matrix.</param>
    /// <param name="matrixB">the second matrix.</param>
    /// <returns>the result of multiplication.</returns>
    public static Matrix Multiply(Matrix matrixA, Matrix matrixB)
    {
        CheckMatrices(matrixA, matrixB);

        (int Rows, int Columns) outputMatrixSize = (matrixA.RowsCount, matrixB.ColumnsCount);
        var result = new int[outputMatrixSize.Rows][];

        var threadsNumber = Math.Min(Environment.ProcessorCount, outputMatrixSize.Rows);
        var threads = new Thread[threadsNumber];
        var remainingCells = outputMatrixSize.Rows;
        var matrix1 = matrixA.ToTwoDimensionalArray();
        var matrix2 = matrixB.ToTwoDimensionalArray();

        for (int i = 0; i < threadsNumber; ++i)
        {
            int begin = outputMatrixSize.Rows - remainingCells;
            int end = begin + (int)Math.Ceiling((float)remainingCells / (threadsNumber - i));
            remainingCells -= end - begin;
            threads[i] = new Thread(() =>
            {
                for (int l = begin; l < end; ++l)
                {
                    var array = new int[outputMatrixSize.Columns];
                    for (int n = 0; n < matrixB.ColumnsCount; ++n)
                    {
                        var sum = 0;
                        for (int m = 0; m < matrixA.ColumnsCount; ++m)
                        {
                            sum += matrix1[l][m] * matrix2[m][n];
                        }

                        array[n] = sum;
                    }

                    result[l] = array;
                }
            });
        }

        foreach (var thread in threads)
        {
            thread.Start();
        }

        foreach (var thread in threads)
        {
            thread.Join();
        }

        return new Matrix(result);
    }

    /// <summary>
    /// Multiplies matrices using a single thread.
    /// </summary>
    /// <param name="matrixA">the first matrix.</param>
    /// <param name="matrixB">the second matrix.</param>
    /// <returns>the result of multiplication.</returns>>
    public static Matrix MultiplyOneThreaded(Matrix matrixA, Matrix matrixB)
    {
        CheckMatrices(matrixA, matrixB);

        (int Rows, int Columns) outputMatrixSize = (matrixA.RowsCount, matrixB.ColumnsCount);
        var outputMatrix = new List<List<int>>();

        for (int i = 0; i < outputMatrixSize.Rows; ++i)
        {
            outputMatrix.Add(new List<int>());
            for (int j = 0; j < outputMatrixSize.Columns; ++j)
            {
                outputMatrix[i].Add(0);
            }
        }

        var matrix1 = matrixA.ToTwoDimensionalList();
        var matrix2 = matrixB.ToTwoDimensionalList();
        for (int i = 0; i < outputMatrixSize.Rows; ++i)
        {
            for (int j = 0; j < outputMatrixSize.Columns; ++j)
            {
                outputMatrix[i][j] = 0;
                for (int l = 0; l < matrixA.ColumnsCount; ++l)
                {
                    outputMatrix[i][j] += matrix1[i][l] * matrix2[l][j];
                }
            }
        }

        return new Matrix(outputMatrix);
    }

    /// <summary>
    /// Converts matrix to Two-Dimensional list.
    /// </summary>
    /// <returns>Two-Dimensional list.</returns>
    public List<List<int>> ToTwoDimensionalList() => this.matrix;
    

    /// <summary>
    /// Converts matrix to Two-Dimensional array.
    /// </summary>
    /// <returns>Two-Dimensional array.</returns>
    public int[][] ToTwoDimensionalArray()
    {
        var result = new int[this.RowsCount][];
        for (int i = 0; i < this.RowsCount; ++i)
        {
            result[i] = this.matrix[i].ToArray();
        }

        return result;
    }

    /// <summary>
    /// Adds 2 matrices and returns the result.
    /// </summary>
    /// <param name="matrixA">the first matrix.</param>
    /// <param name="matrixB">the second matrix.</param>
    /// <returns>Result of addition.</returns>
    public static Matrix AddUp(Matrix matrixA, Matrix matrixB)
    {
        if (matrixA.RowsCount != matrixB.RowsCount || matrixA.ColumnsCount != matrixB.ColumnsCount)
        {
            throw new NonSummableMatrices();
        }

        var matrix1 = matrixA.ToTwoDimensionalList();
        var matrix2 = matrixB.ToTwoDimensionalList();
        var result = new List<List<int>>();
        for (int i = 0; i < matrixA.RowsCount; ++i)
        {
            result.Add(new List<int>());
            for (int j = 0; j < matrixA.ColumnsCount; ++j)
            {
                result[i].Add(matrix1[i][j] + matrix2[i][j]);
            }
        }

        return new Matrix(result);
    }

    /// <summary>
    /// Transposes this matrix.
    /// </summary>
    public void Transpose()
    {
        var newMatrix = new List<List<int>>();

        for (int i = 0; i < this.ColumnsCount; ++i)
        {
            newMatrix.Add(new List<int>());
            for (int j = 0; j < this.RowsCount; ++j)
            {
                newMatrix[i].Add(this.matrix[j][i]);
            }
        }

        this.matrix = newMatrix;
    }

    /// <summary>
    /// Writes the matrix to a file.
    /// </summary>
    /// <param name="path">path to file.</param>
    public void WriteToFile(string path) 
        => File.WriteAllLines(path, this.matrix.Select(n => string.Join(" ", n.Select(m => m.ToString()).ToArray())));
}