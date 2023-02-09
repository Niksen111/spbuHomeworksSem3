namespace ParallelMatrixMultiplication;

/// <summary>
/// Provides a parallel multiplication of matrices.
/// </summary>
public static class MatrixMultiplier
{
    public static int ThreadsCount => 15;
    private static Thread[] threads;
    private static List<List<int>> outputMatrix;
    private static List<List<int>> matrix1;
    private static List<List<int>> matrix2;
    private static readonly List<((int row,  int column) start, (int row, int column) end)> _threadsActions;
    private static (int rows, int columns) matrix1Size;
    private static (int rows, int columns) matrix2Size;
    private static (int rows, int columns) outputMatrixSize;

    static MatrixMultiplier()
    {
        threads = new Thread[ThreadsCount];
        outputMatrix = new List<List<int>>();
        matrix1 = new List<List<int>>();
        matrix2 = new List<List<int>>();
        _threadsActions = new List<((int row, int column) start, (int row, int column) end)>();
        
        RefreshThreads();
    }

    private static void RefreshThreads()
    {
        for (int index = 0; index < ThreadsCount; ++index)
        {
            var i = index;
            if (_threadsActions.Count < index + 1)
            {
                _threadsActions.Add(((-1, -1), (-1, -1)));
            }

            threads[i] = new Thread(() =>
            {
                if (_threadsActions[i].end.row == -1)
                {
                    return;
                }

                for (int j = _threadsActions[i].start.row; j <= _threadsActions[i].end.row; ++j)
                {
                    for (int k = j == _threadsActions[i].start.row ? _threadsActions[i].start.column : 0;
                         k < outputMatrixSize.columns && j < _threadsActions[i].end.row 
                         || k <= _threadsActions[i].end.column && j == _threadsActions[i].end.row;
                         ++k)
                    {
                        outputMatrix[j][k] = 0;
                        for (int l = 0; l < matrix1Size.columns; ++l)
                        {
                            outputMatrix[j][k] += matrix1[j][l] * matrix2[l][k];
                        }
                    }
                }

                _threadsActions[i] = ((-1, -1), (-1, -1));
            });
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

    private static void CheckMatrices(List<List<int>> matrix1, List<List<int>> matrix2)
    {
        (int rows, int columns) matrix1Size = (matrix1.Count, MatrixMultiplier.matrix1[0].Count);
        (int rows, int columns) matrix2Size = (matrix2.Count, MatrixMultiplier.matrix2[0].Count);

        if (matrix1Size.columns != matrix2Size.rows)
        {
            throw new NonMultipleMatricesException();
        }

        for (int i = 0; i < matrix1Size.rows; ++i)
        {
            if (matrix1[i].Count != matrix1Size.columns)
            {
                throw new NonMultipleMatricesException();
            }
        }
        
        for (int i = 0; i < matrix2Size.rows; ++i)
        {
            if (matrix2[i].Count != matrix2Size.columns)
            {
                throw new NonMultipleMatricesException();
            }
        }
    }

    /// <summary>
    /// Parallel multiplies matrices and writes the answer to the file.
    /// </summary>
    /// <param name="matrix1Path">path to the first matrix</param>
    /// <param name="matrix2Path">path to the second matrix</param>
    /// <param name="outputPath">path to the output file</param>
    public static void MultiplyParallel(string matrix1Path, string matrix2Path, string outputPath)
    {
        RefreshThreads();

        matrix1 = GetMatrixFromFile(matrix1Path);
        matrix2 = GetMatrixFromFile(matrix2Path);
        
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

        int size = outputMatrixSize.columns * outputMatrixSize.rows;
        int distributedNow = -1;
        int step = (size + ThreadsCount - 1) / ThreadsCount;
        int currentThread = 0;
        
        while (distributedNow + 1 < size)
        {
            int currentStep = Math.Min(step, size - distributedNow);
            
            _threadsActions[currentThread] = (((distributedNow + 1) / outputMatrixSize.columns, (distributedNow + 1) % outputMatrixSize.columns), 
                ((distributedNow + currentStep) / outputMatrixSize.columns, (distributedNow + currentStep) % outputMatrixSize.columns)); 
            
            distributedNow += currentStep;
            ++currentThread;
        }

        --currentThread;
        _threadsActions[currentThread] = ((_threadsActions[currentThread].start), (_threadsActions[currentThread].end.row - 1, outputMatrixSize.columns - 1));

        for (int i = 0; i < ThreadsCount; ++i)
        {
            threads[i].Start();
        }

        for (int i = 0; i < ThreadsCount; ++i)
        {
            threads[i].Join();
        }

        var output = outputMatrix.Select(n => string.Join(" ", n.Select(m => m.ToString()).ToArray()));
        
        File.WriteAllLines(outputPath, output);
    }

    /// <summary>
    /// Multiplies matrices using a single thread 
    /// </summary>
    /// <param name="matrix1Path">path to the first matrix</param>
    /// <param name="matrix2Path">path to the second matrix</param>
    /// <param name="outputPath">path to the output file</param>
    public static void MultiplyOneThreaded(string matrix1Path, string matrix2Path, string outputPath)
    {
        matrix1 = GetMatrixFromFile(matrix1Path);
        matrix2 = GetMatrixFromFile(matrix2Path);
        
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
        
        var output = outputMatrix.Select(n => string.Join(" ", n.Select(m => m.ToString()).ToArray()));
        
        File.WriteAllLines(outputPath, output);
    }
}