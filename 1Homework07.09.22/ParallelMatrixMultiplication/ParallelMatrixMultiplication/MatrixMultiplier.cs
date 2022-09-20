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

    static MatrixMultiplier()
    {
        _outputMatrix = new List<List<int>>();
        _matrix1 = new List<List<int>>();
        _matrix2 = new List<List<int>>();

        for (int i = 0; i < ThreadsCount; ++i)
        {
            
        }
    }

    private static void Multiply(int rowStart, int columnStart, int numberElements)
    {
        
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
        
        
    }
}