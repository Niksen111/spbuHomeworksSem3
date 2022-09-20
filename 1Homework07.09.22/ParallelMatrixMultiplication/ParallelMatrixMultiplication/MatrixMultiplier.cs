namespace ParallelMatrixMultiplication;


/// <summary>
/// Provides a parallel multiplication of matrices.
/// </summary>
public static class MatrixMultiplier
{
    private const int ThreadsCount = 30;
    private static Thread[] _threads = new Thread[ThreadsCount];
    private static List<List<int>> _outputMatrix;
    private static List<List<int>> _matrix1;
    private static List<List<int>> _matrix2;
    private static List<((int row,  int column) start, (int row, int column) end)> _threadsActions;
    private static Mutex _mutex;
    private static (int rows, int columns) _matrix1Size;
    private static (int rows, int columns) _matrix2Size;
    private static (int rows, int columns) _outputMatrixSize;

    
    static MatrixMultiplier()
    {
        _outputMatrix = new List<List<int>>();
        _matrix1 = new List<List<int>>();
        _matrix2 = new List<List<int>>();
        _threadsActions = new List<((int row, int column) start, (int row, int column) end)>();
        _mutex = new Mutex();

        for (int index = 0; index < ThreadsCount; ++index)
        {
            var i = index;
            _threads[i] = new Thread(() =>
            {
                while (true)
                {
                    while (_threadsActions[i].end.row < 0)
                    {
                        _mutex.WaitOne();
                    }

                    for (int j = _threadsActions[i].start.row; j <= _threadsActions[i].end.row; ++j)
                    {
                        for (int k = _threadsActions[i].start.column;
                             k < _outputMatrixSize.columns &&
                             (j < _threadsActions[i].end.row || k <= _threadsActions[i].end.column); ++k)
                        {
                            
                        }
                    }

                    _threadsActions[i] = ((-1, -1), (-1, -1));
                }
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
    public static void MultiplyMatrixParallel(string matrix1Path, string matrix2Path, string outputPath)
    {
        try
        {
            _matrix1 = GetMatrixFromFile(matrix1Path);
            _matrix2 = GetMatrixFromFile(matrix2Path);
        }
        catch (IOException e)
        {
            throw (FailedReadMatricesException) e;
        }

        _matrix1Size = (_matrix1.Count, _matrix1[0].Count);
        _matrix2Size = (_matrix2.Count, _matrix2[0].Count);
        _outputMatrixSize = (_matrix1Size.rows, _matrix2Size.columns);
        _outputMatrix = new List<List<int>>();
        
        for (int i = 0; i < _outputMatrixSize.rows; ++i)
        {
            _outputMatrix.Add(new List<int>());
            for (int j = 0; j < _outputMatrixSize.columns; ++j)
            {
                
            }
        }
    }
}