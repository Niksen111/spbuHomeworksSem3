namespace ParallelMatrixMultiplication;

/// <summary>
/// Matrix abstraction.
/// </summary>
public class Matrix
{
    /// <summary>
    /// Gives the number of rows in the matrix.
    /// </summary>
    public int RowsCount => matrix.Count;
    
    /// <summary>
    /// Gives the number of columns in the matrix.
    /// </summary>
    public int ColumnsCount => matrix[0].Count;
    private List<List<int>> matrix;

    public Matrix(List<List<int>> matrix)
    {
        this.matrix = matrix;
    }

    public Matrix(string path)
    {
        matrix = GetMatrixFromFile(path);
    }

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
            throw new NonMultipleMatricesException();
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
        
        (int rows, int columns) outputMatrixSize = (matrixA.RowsCount, matrixB.ColumnsCount);
        var result = new int[outputMatrixSize.rows][];

        var threadsNumber = Math.Min(Environment.ProcessorCount, outputMatrixSize.rows);
        var threads = new Thread[threadsNumber];
        var remainingCells = outputMatrixSize.rows;
        var matrix1 = matrixA.ToTwoDimensionalArray();
        var matrix2 = matrixB.ToTwoDimensionalArray();
        
        for (int i = 0; i < threadsNumber; ++i)
        {
            int begin = outputMatrixSize.rows - remainingCells;
            int end = begin + (int) Math.Ceiling((float) remainingCells / (threadsNumber - i));
            remainingCells -= end - begin;
            threads[i] = new Thread(() =>
            {
                for (int l = begin; l < end; ++l)
                {
                    var array = new int[outputMatrixSize.columns];
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
        
        (int rows, int columns) outputMatrixSize = (matrixA.RowsCount, matrixB.ColumnsCount);
        var outputMatrix = new List<List<int>>();

        for (int i = 0; i < outputMatrixSize.rows; ++i)
        {
            outputMatrix.Add(new List<int>());
            for (int j = 0; j < outputMatrixSize.columns; ++j)
            {
                outputMatrix[i].Add(0);
            }
        }
        
        var matrix1 = matrixA.ToTwoDimensionalList();
        var matrix2 = matrixB.ToTwoDimensionalList();
        for (int i = 0; i < outputMatrixSize.rows; ++i)
        {
            for (int j = 0; j < outputMatrixSize.columns; ++j)
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
    public List<List<int>> ToTwoDimensionalList()
    {
        return matrix;
    }
    
    /// <summary>
    /// Converts matrix to Two-Dimensional array.
    /// </summary>
    /// <returns>Two-Dimensional array.</returns>
    public int[][] ToTwoDimensionalArray()
    {
        var result = new int[RowsCount][];
        for (int i = 0; i < RowsCount; ++i)
        {
            result[i] = matrix[i].ToArray();
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
            for (int j = 0; j < matrixA.ColumnsCount; ++i)
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

        for (int i = 0; i < ColumnsCount; ++i)
        {
            newMatrix.Add(new List<int>());
            for (int j = 0; j < RowsCount; ++j)
            {
                newMatrix[i].Add(matrix[j][i]);
            }
        }

        matrix = newMatrix;
    }

    /// <summary>
    /// Writes the matrix to a file.
    /// </summary>
    /// <param name="path">path to file.</param>
    public void WriteToFile(string path)
    {
        File.WriteAllLines(path, matrix.Select(n => string.Join(" ", n.Select(m => m.ToString()).ToArray())));
    }
}