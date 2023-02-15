using System.Reflection;

namespace MyNUnit;

/// <summary>
/// Class for running and checking tests.
/// </summary>
public class TestsRunner
{
    public TestsRunner(CancellationToken token)
    {
        this.token = token;
    }

    private CancellationToken token;

    /// <summary>
    /// Information about the last tests run.
    /// </summary>
    public List<string> RunInfo { get; private set; }

    /// <summary>
    /// Runs all tests in the loaded assembly.
    /// </summary>
    public async Task RunTests(string pathToAssemblies)
    {
        if (File.Exists(pathToAssemblies))
        {
            if (string.CompareOrdinal(Path.GetExtension(pathToAssemblies), ".dll") == 0)
            {
                var assembly = Assembly.LoadFile(pathToAssemblies);
                await this.AssemblyRun(assembly);
                return;
            }
        }
        
        var testsAssemblies = Directory.EnumerateFiles(pathToAssemblies, "*.dll");
        
    }

    private async Task AssemblyRun(Assembly assembly)
    {
        
    }

    private async Task ClassRun()
    {
        
    }
}