using System.Collections.Concurrent;
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

    private async Task<AssemblyTestsInfo?> AssemblyRun(Assembly assembly)
    {
        var assemblyInfo = new AssemblyTestsInfo(assembly.Location);

        return assemblyInfo;
    }

    private async Task<ClassTestsInfo?> ClassRun()
    {
        var classesInfo = new ClassTestsInfo("kek");

        return classesInfo;
    }

    /// <summary>
    /// Information about the work of the class tests of this assembly.
    /// </summary>
    public class AssemblyTestsInfo
    {
        public AssemblyTestsInfo(string assemblyPath)
        {
            this.AssemblyPath = assemblyPath;
            this.ClassesInfo = new BlockingCollection<ClassTestsInfo>();
        }

        /// <summary>
        /// Gets Assembly path.
        /// </summary>
        public string AssemblyPath { get; }

        /// <summary>
        /// Gets ClassesInfo collection.
        /// </summary>
        public BlockingCollection<ClassTestsInfo> ClassesInfo { get; }

        public Exception? Exception { get; }

        private void Add(ClassTestsInfo info)
        {
            this.ClassesInfo.Add(info);
        }

        private void CompleteAdding()
        {
            this.ClassesInfo.CompleteAdding();
        }
    }

    /// <summary>
    /// Information about the work of the tests of this class.
    /// </summary>
    public class ClassTestsInfo
    {
        public ClassTestsInfo(string className)
        {
            this.ClassName = className;
            this.TestsInfo = new BlockingCollection<TestInfo>();
        }

        /// <summary>
        /// Gets the name of the tested class.
        /// </summary>
        public string ClassName { get; }

        /// <summary>
        /// Gets a collection of TestInfo of this class.
        /// </summary>
        public BlockingCollection<TestInfo> TestsInfo { get; }

        private void Add(TestInfo info)
        {
            this.TestsInfo.Add(info);
        }

        private void CompleteAdding()
        {
            this.TestsInfo.CompleteAdding();
        }
    }

    /// <summary>
    /// Information about the work of the test.
    /// </summary>
    public class TestInfo
    {
        public TestInfo(string methodName, bool isSuccess, int runningTime, Exception? exception = null, string? reasonForIgnoring = null)
        {
            this.MethodName = methodName;
            this.IsSuccess = isSuccess;
            this.RunningTime = runningTime;
            this.Exception = exception;
            this.ReasonForIgnoring = reasonForIgnoring;
        }

        /// <summary>
        /// Gets the name of the tested method.
        /// </summary>
        public string MethodName { get; }

        /// <summary>
        /// Gets a value indicating whether the test was completed successfully.
        /// </summary>
        public bool IsSuccess { get; }

        /// <summary>
        /// Gets the running time of the test.
        /// </summary>
        public int RunningTime { get; }

        /// <summary>
        /// Gets an exception was thrown by the test.
        /// </summary>
        public Exception? Exception { get; }

        /// <summary>
        /// Gets the reason for ignoring this test.
        /// </summary>
        public string? ReasonForIgnoring { get; }
    }
}