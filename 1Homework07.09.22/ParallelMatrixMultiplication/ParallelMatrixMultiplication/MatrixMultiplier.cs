namespace ParallelMatrixMultiplication;


/// <summary>
/// 
/// </summary>
public class MatrixMultiplier
{
    private List<List<int>> GetMatrixFromFile(string path)
    {
        
        return null;
    }
    
    /// <summary>
    /// Parallel multiplies matrices and writes the answer to the file.
    /// </summary>
    /// <param name="matrix1Path">path to the first matrix</param>
    /// <param name="matrix2Path">path to the second matrix</param>
    /// <param name="outputPath">path to the output file</param>
    public void MultiplyMatrixParallel(string matrix1Path, string matrix2Path, string outputPath)
    {
        List<List<int>> matrix1;
        List<List<int>> matrix2;
        try
        {
            matrix1 = GetMatrixFromFile(matrix1Path);
            matrix2 = GetMatrixFromFile(matrix1Path);
        }
        catch (Exception)
        {
            
        }
    }
}