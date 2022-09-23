namespace ParallelMatrixMultiplication;


/// <summary>
/// Provides a parallel multiplication of matrices.
/// </summary>
public static class MatrixMultiplier
{
    private const int ThreadsCount = 30;
    private static Thread[] _threads;
    private static List<List<int>> _outputMatrix;
    private static List<List<int>> _matrix1;
    private static List<List<int>> _matrix2;
    private static List<((int row,  int column) start, (int row, int column) end)> _threadsActions;
    private static (int rows, int columns) _matrix1Size;
    private static (int rows, int columns) _matrix2Size;
    private static (int rows, int columns) _outputMatrixSize;

    static MatrixMultiplier()
    {
        _threads = new Thread[ThreadsCount];
        _outputMatrix = new List<List<int>>();
        _matrix1 = new List<List<int>>();
        _matrix2 = new List<List<int>>();
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

            _threads[i] = new Thread(() =>
            {
                if (_threadsActions[i].end.row == -1)
                {
                    return;
                }

                for (int j = _threadsActions[i].start.row; j <= _threadsActions[i].end.row; ++j)
                {
                    for (int k = _threadsActions[i].start.column;
                         k < _outputMatrixSize.columns && j < _threadsActions[i].end.row 
                         || k <= _threadsActions[i].end.column && j == _threadsActions[i].end.row;
                         ++k)
                    {
                        _outputMatrix[j][k] = 0;
                        for (int l = 0; l < _matrix1Size.columns; ++l)
                        {
                            _outputMatrix[j][k] += _matrix1[j][l] * _matrix2[l][k];
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

    /// <summary>
    /// Parallel multiplies matrices and writes the answer to the file.
    /// </summary>
    /// <param name="matrix1Path">path to the first matrix</param>
    /// <param name="matrix2Path">path to the second matrix</param>
    /// <param name="outputPath">path to the output file</param>
    public static void MultiplyParallel(string matrix1Path, string matrix2Path, string outputPath)
    {
        RefreshThreads();

        _matrix1 = GetMatrixFromFile(matrix1Path);
        _matrix2 = GetMatrixFromFile(matrix2Path);
        _matrix1Size = (_matrix1.Count, _matrix1[0].Count);
        _matrix2Size = (_matrix2.Count, _matrix2[0].Count);
        _outputMatrixSize = (_matrix1Size.rows, _matrix2Size.columns);
        _outputMatrix = new List<List<int>>();
        
        for (int i = 0; i < _outputMatrixSize.rows; ++i)
        {
            _outputMatrix.Add(new List<int>());
            for (int j = 0; j < _outputMatrixSize.columns; ++j)
            {
                _outputMatrix[i].Add(0);
            }
        }

        int size = _outputMatrixSize.columns * _outputMatrixSize.rows;
        int distributedNow = -1;
        int step = (size + ThreadsCount - 1) / ThreadsCount;
        int currentThread = 0;
        
        while (distributedNow + 1 < size)
        {
            int currentStep = Math.Min(step, size - distributedNow);

            _threadsActions[currentThread] = (((distributedNow + 1) / _outputMatrixSize.columns, (distributedNow + 1) % _outputMatrixSize.columns), 
                ((distributedNow + currentStep) / _outputMatrixSize.columns, (distributedNow + currentStep) % _outputMatrixSize.columns));
            distributedNow += currentStep;
            ++currentThread;
        }

        for (int i = 0; i < ThreadsCount; ++i)
        {
            _threads[i].Start();
        }

        for (int i = 0; i < ThreadsCount; ++i)
        {
            _threads[i].Join();
        }

        var output = _outputMatrix.Select(n => string.Join(" ", n.Select(m => m.ToString()).ToArray()));
        
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
        _matrix1 = GetMatrixFromFile(matrix1Path);
        _matrix2 = GetMatrixFromFile(matrix2Path);
        _matrix1Size = (_matrix1.Count, _matrix1[0].Count);
        _matrix2Size = (_matrix2.Count, _matrix2[0].Count);
        _outputMatrixSize = (_matrix1Size.rows, _matrix2Size.columns);
        _outputMatrix = new List<List<int>>();
        
        for (int i = 0; i < _outputMatrixSize.rows; ++i)
        {
            _outputMatrix.Add(new List<int>());
            for (int j = 0; j < _outputMatrixSize.columns; ++j)
            {
                _outputMatrix[i].Add(0);
            }
        }
        
        for (int i = 0; i < _outputMatrixSize.rows; ++i)
        {
            for (int j = 0; j < _outputMatrixSize.columns; ++j)
            {
                _outputMatrix[i][j] = 0;
                for (int l = 0; l < _matrix1Size.columns; ++l)
                {
                    _outputMatrix[i][j] += _matrix1[i][l] * _matrix2[l][j];
                }
            }
        }
        
        var output = _outputMatrix.Select(n => string.Join(" ", n.Select(m => m.ToString()).ToArray()));
        
        File.WriteAllLines(outputPath, output);
    }
}