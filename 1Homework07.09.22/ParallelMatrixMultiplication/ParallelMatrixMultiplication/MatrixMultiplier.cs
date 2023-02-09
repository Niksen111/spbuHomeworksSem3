namespace ParallelMatrixMultiplication;

/// <summary>
/// Provides a parallel multiplication of matrices.
/// </summary>
public static class MatrixMultiplier
{
    public static int ThreadsCount => Environment.ProcessorCount;
    public static int ActiveTasks => activeTasks;
    private static int activeTasks;
    private static bool isStoped = false;
    private static Thread[] threads;
    private static List<List<int>> outputMatrix;
    private static List<List<int>> matrix1;
    private static List<List<int>> matrix2;
    private static readonly List<((int row,  int column) start, (int row, int column) end)> ThreadsActions;
    private static (int rows, int columns) matrix1Size;
    private static (int rows, int columns) matrix2Size;
    private static (int rows, int columns) outputMatrixSize;
    

    static MatrixMultiplier()
    {
        threads = new Thread[ThreadsCount];
        outputMatrix = new List<List<int>>();
        matrix1 = new List<List<int>>();
        matrix2 = new List<List<int>>();
        ThreadsActions = new List<((int row, int column) start, (int row, int column) end)>();
        for (int index = 0; index < ThreadsCount; ++index)
        {
            var i = index;
            if (ThreadsActions.Count < index + 1)
            {
                ThreadsActions.Add(((-1, -1), (-1, -1)));
            }

            threads[i] = new Thread(() =>
            {
                while (true)
                {
                    if (isStoped)
                    {
                        return;
                    }
                    while (ThreadsActions[i].end.row == -1)
                    {
                        // Waiting for any changes
                    }

                    for (int j = ThreadsActions[i].start.row; j <= ThreadsActions[i].end.row; ++j)
                    {
                        for (int k = j == ThreadsActions[i].start.row ? ThreadsActions[i].start.column : 0;
                             (k < outputMatrixSize.columns && j < ThreadsActions[i].end.row)
                             || (k <= ThreadsActions[i].end.column && j == ThreadsActions[i].end.row);
                             ++k)
                        {
                            outputMatrix[j][k] = 0;
                            for (int l = 0; l < matrix1Size.columns; ++l)
                            {
                                outputMatrix[j][k] += matrix1[j][l] * matrix2[l][k];
                            }
                        }
                    }

                    ThreadsActions[i] = ((-1, -1), (-1, -1));
                }
            });
            threads[i].IsBackground = true;
        }
        
        for (int i = 0; i < ThreadsCount; ++i)
        {
            threads[i].Start();
        }
    }

    private static List<List<int>> GetMatrixFromFile(string path)
    {
        var lines = File.ReadAllLines(path);
        var matrix = new List<List<int>>();
        for (int i = 0; i < lines.Length; ++i)
        {
            matrix.Add(new List<int>());
            matrix[i].AddRange(lines[i].Split().Select(n => int.Parse(n)).ToList());
        }

        return matrix;
    }

    private static void CheckMatrices(List<List<int>> matrixA, List<List<int>> matrixB)
    {
        (int rows, int columns) matrixASize = (matrixA.Count, MatrixMultiplier.matrix1[0].Count);
        (int rows, int columns) matrixBSize = (matrixB.Count, MatrixMultiplier.matrix2[0].Count);

        if (matrixASize.columns != matrixBSize.rows)
        {
            throw new NonMultipleMatricesException();
        }

        for (int i = 0; i < matrixASize.rows; ++i)
        {
            if (matrixA[i].Count != matrixASize.columns)
            {
                throw new NonMultipleMatricesException();
            }
        }
        for (int i = 0; i < matrixBSize.rows; ++i)
        {
            if (matrixB[i].Count != matrixBSize.columns)
            {
                throw new NonMultipleMatricesException();
            }
        }
    }

    /// <summary>
    /// Parallel multiplies matrices and writes the answer to the file.
    /// </summary>
    /// <param name="matrix1Path">path to the first matrix.</param>
    /// <param name="matrix2Path">path to the second matrix.</param>
    /// <param name="outputPath">path to the output file.</param>
    public static void MultiplyParallel(string matrix1Path, string matrix2Path, string outputPath)
    {
        matrix1 = GetMatrixFromFile(matrix1Path);
        matrix2 = GetMatrixFromFile(matrix2Path);
        var output = MultiplyParallel(matrix1, matrix2);
        File.WriteAllLines(outputPath, output.Select(n => string.Join(" ", n.Select(m => m.ToString()).ToArray())));
    }

    /// <summary>
    /// Parallel multiplies matrices.
    /// </summary>
    /// <param name="matrixA">the first matrix.</param>
    /// <param name="matrixB">the second matrix.</param>
    /// <returns>the result of multiplication.</returns>
    public static List<List<int>> MultiplyParallel(List<List<int>> matrixA, List<List<int>> matrixB)
    {
        Interlocked.Increment(ref activeTasks);
        try
        {
            CheckMatrices(matrixA, matrixB);

            matrix1Size = (matrixA.Count, matrixA[0].Count);
            matrix2Size = (matrixB.Count, matrixB[0].Count);
            outputMatrixSize = (matrix1Size.rows, matrix2Size.columns);
            outputMatrix = new List<List<int>>();

            for (int i = 0; i < outputMatrixSize.rows; ++i)
            {
                outputMatrix.Add(new List<int>());
                for (int j = 0; j < outputMatrixSize.columns; ++j)
                {
                    outputMatrix[i].Add(0);
                }
            }

            int size = outputMatrixSize.columns * outputMatrixSize.rows;
            int distributedNow = -1;
            int step = (size + ThreadsCount - 1) / ThreadsCount;
            int currentThread = 0;

            while (distributedNow + 1 < size)
            {
                int currentStep = Math.Min(step, size - distributedNow);

                ThreadsActions[currentThread] = (
                    ((distributedNow + 1) / outputMatrixSize.columns, (distributedNow + 1) % outputMatrixSize.columns),
                    ((distributedNow + currentStep) / outputMatrixSize.columns,
                        (distributedNow + currentStep) % outputMatrixSize.columns));

                distributedNow += currentStep;
                ++currentThread;
            }

            --currentThread;
            ThreadsActions[currentThread] = (ThreadsActions[currentThread].start,
                (ThreadsActions[currentThread].end.row - 1, outputMatrixSize.columns - 1));

            

            return outputMatrix;
        }
        finally
        {
            Interlocked.Decrement(ref activeTasks);
        }
    }

    /// <summary>
    /// Multiplies matrices using a single thread.
    /// </summary>
    /// <param name="matrix1Path">path to the first matrix.</param>
    /// <param name="matrix2Path">path to the second matrix.</param>
    /// <param name="outputPath">path to the output file.</param>
    public static void MultiplyOneThreaded(string matrix1Path, string matrix2Path, string outputPath)
    {
        var matrixA = GetMatrixFromFile(matrix1Path);
        var matrixB = GetMatrixFromFile(matrix2Path);
        var output = MultiplyOneThreaded(matrixA, matrixB);
        File.WriteAllLines(outputPath, output.Select(n => string.Join(" ", n.Select(m => m.ToString()).ToArray())));
    }

    /// <summary>
    /// Multiplies matrices using a single thread.
    /// </summary>
    /// <param name="matrixA">the first matrix.</param>
    /// <param name="matrixB">the second matrix.</param>
    /// <returns>the result of multiplication.</returns>>
    public static List<List<int>> MultiplyOneThreaded(List<List<int>> matrixA, List<List<int>> matrixB)
    {
        Interlocked.Increment(ref activeTasks);
        try
        {
            matrix1 = matrixA;
            matrix2 = matrixB;

            CheckMatrices(matrix1, matrix2);

            matrix1Size = (matrix1.Count, matrix1[0].Count);
            matrix2Size = (matrix2.Count, matrix2[0].Count);
            outputMatrixSize = (matrix1Size.rows, matrix2Size.columns);
            outputMatrix = new List<List<int>>();

            for (int i = 0; i < outputMatrixSize.rows; ++i)
            {
                outputMatrix.Add(new List<int>());
                for (int j = 0; j < outputMatrixSize.columns; ++j)
                {
                    outputMatrix[i].Add(0);
                }
            }

            for (int i = 0; i < outputMatrixSize.rows; ++i)
            {
                for (int j = 0; j < outputMatrixSize.columns; ++j)
                {
                    outputMatrix[i][j] = 0;
                    for (int l = 0; l < matrix1Size.columns; ++l)
                    {
                        outputMatrix[i][j] += matrix1[i][l] * matrix2[l][j];
                    }
                }
            }

            return outputMatrix;
        }
        finally
        {
            Interlocked.Decrement(ref activeTasks);
        }
    }
}