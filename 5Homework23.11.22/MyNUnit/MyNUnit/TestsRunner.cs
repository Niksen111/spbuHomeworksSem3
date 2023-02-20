namespace MyNUnit;

using System.Diagnostics;
using System.Reflection;
using MyNUnit.Attributes;

/// <summary>
/// Class for running and checking tests.
/// </summary>
public static class TestsRunner
{
    /// <summary>
    /// Generates a report on the running of the tests.
    /// </summary>
    /// <param name="info">The result from the TestsRunner tests running.</param>
    /// <param name="writer">Report output.</param>
    public static void GenerateReport(SummaryInfo info, TextWriter writer)
    {
        if (info.Comment != null)
        {
            writer.WriteLine(info.Comment);
            return;
        }

        foreach (var assemblyInfo in info.AssembliesInfo)
        {
            writer.WriteLine(assemblyInfo.AssemblyPath);
            if (assemblyInfo.Comment != null)
            {
                writer.WriteLine(assemblyInfo.Comment);
                continue;
            }

            foreach (var classInfo in assemblyInfo.ClassesInfo)
            {
                
            }
        }
    }

    /// <summary>
    /// Runs all tests in the specified directory or assembly.
    /// </summary>
    /// <param name="pathToAssemblies">The path to the assemblies.</param>
    /// <param name="token">Token that sends a notification that operations should be canceled.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.
    /// If there are no test methods along this path, it returns null.</returns>
    public static async Task<SummaryInfo> RunTests(string pathToAssemblies, CancellationToken token)
    {
        var summaryInfo = new SummaryInfo();

        if (File.Exists(pathToAssemblies))
        {
            if (string.CompareOrdinal(Path.GetExtension(pathToAssemblies), ".dll") == 0)
            {
                var assembly = Assembly.LoadFile(pathToAssemblies);
                var resultOfAssembly = await AssemblyRun(assembly);
                summaryInfo.AssembliesInfo.Add(resultOfAssembly);
                return summaryInfo;
            }
        }

        var testsAssemblies = Directory.EnumerateFiles(pathToAssemblies, "*.dll");
        var assemblies = new List<Task<AssemblyTestsInfo>>();
        foreach (var assembly in testsAssemblies)
        {
            assemblies.Add(Task.Run(() => AssemblyRun(Assembly.Load(assembly)), token));
        }

        foreach (var assembly in assemblies)
        {
            var resultOfAssembly = await assembly;
            summaryInfo.AssembliesInfo.Add(resultOfAssembly);
        }

        if (summaryInfo.AssembliesInfo.Count == 0)
        {
            summaryInfo.Comment = "No assemblies found.";
        }

        return summaryInfo;
    }

    private static async Task<AssemblyTestsInfo> AssemblyRun(Assembly assembly)
    {
        var assemblyInfo = new AssemblyTestsInfo(assembly.Location);
        var types = assembly.ExportedTypes;
        var classesResults = new List<Task<ClassTestsInfo?>>();

        foreach (var type in types)
        {
            if (type.IsClass)
            {
                classesResults.Add(Task.Run(() => ClassRun(type)));
            }
        }

        foreach (var result in classesResults)
        {
            var realResult = await result;
            if (realResult != null)
            {
                assemblyInfo.ClassesInfo.Add(realResult);
            }
        }

        if (classesResults.Count == 0)
        {
            assemblyInfo.Comment = "No classes found.";
        }
        else if (assemblyInfo.ClassesInfo.Count == 0)
        {
            assemblyInfo.Comment = "No tests found.";
        }

        return assemblyInfo;
    }

    private static async Task<ClassTestsInfo?> ClassRun(Type testClass)
    {
        var classInfo = new ClassTestsInfo(testClass.FullName);
        var methods = testClass.GetMethods();
        var beforeClass = new List<MethodInfo>();
        var afterClass = new List<MethodInfo>();
        var before = new List<MethodInfo>();
        var after = new List<MethodInfo>();
        var testMethods = new List<(MethodInfo Method, Type? Expected)>();
        foreach (var method in methods)
        {
            foreach (var attribute in Attribute.GetCustomAttributes(method))
            {
                if (attribute.GetType() == typeof(BeforeClassAttribute))
                {
                    if (method.IsStatic)
                    {
                        beforeClass.Add(method);
                    }
                    else
                    {
                        classInfo.Comments.Add($"ERROR: Founded non-static class with BeforeClassAttribute: {method.Name}");
                    }
                }
                else if (attribute.GetType() == typeof(AfterClassAttribute))
                {
                    if (method.IsStatic)
                    {
                        afterClass.Add(method);
                    }
                    else
                    {
                        classInfo.Comments.Add($"ERROR: Founded non-static class with AfterClassAttribute: {method.Name}");
                    }
                }
                else if (attribute.GetType() == typeof(BeforeAttribute))
                {
                    before.Add(method);
                }
                else if (attribute.GetType() == typeof(AfterAttribute))
                {
                    after.Add(method);
                }
                else if (attribute.GetType() == typeof(TestAttribute))
                {
                    if (((TestAttribute)attribute).Ignore != null)
                    {
                        var reasonForIgnoring = ((Attributes.TestAttribute)attribute).Ignore;
                        var localInfo = new TestInfo(method.Name, false, 0, null, reasonForIgnoring);
                        classInfo.TestsInfo.Add(localInfo);
                    }
                    else
                    {
                        testMethods.Add((method, ((TestAttribute)attribute!).Expected));
                    }
                }
            }
        }

        foreach (var method in beforeClass)
        {
            method.Invoke(null, null);
        }

        var tests = new List<Task<TestInfo>>();
        foreach (var testMethod in testMethods)
        {
            var instance = Activator.CreateInstance(testClass);
            tests.Add(Task.Run(() => TestRun(instance, testMethod.Method, testMethod.Expected, before, after)));
        }

        foreach (var test in tests)
        {
            classInfo.TestsInfo.Add(await test);
        }

        foreach (var method in afterClass)
        {
            method.Invoke(null, null);
        }

        return classInfo.TestsInfo.Count == 0 ? null : classInfo;
    }

    private static TestInfo TestRun(object? instance, MethodInfo methodInfo, Type? exceptedException, List<MethodInfo> before, List<MethodInfo> after)
    {
        Exception exception;

        foreach (var method in before)
        {
            try
            {
                method.Invoke(instance, null);
            }
            catch (Exception e)
            {
                exception = e;
                return new TestInfo(methodInfo.Name, false, 0, exception, null, $"ERROR: Running the 'before' method {method.Name} failed.");
            }
        }

        var stopwatch = new Stopwatch();
        stopwatch.Start();
        try
        {
            methodInfo.Invoke(instance, null);
        }
        catch (Exception e)
        {
            if (exceptedException == null || !e.GetType().IsAssignableTo(exceptedException))
            {
                exception = e;
                stopwatch.Stop();
                return new TestInfo(methodInfo.Name, false, 0, exception, null, "ERROR: Running of the method failed.");
            }
        }

        stopwatch.Stop();

        foreach (var method in after)
        {
            try
            {
                method.Invoke(instance, null);
            }
            catch (Exception e)
            {
                return new TestInfo(methodInfo.Name, false, stopwatch.ElapsedMilliseconds, e, null, $"ERROR: Running the 'after' method {method.Name} failed.");
            }
        }

        return new TestInfo(methodInfo.Name, true, stopwatch.ElapsedMilliseconds);
    }

    public class SummaryInfo
    {
        public SummaryInfo()
        {
            this.AssembliesInfo = new List<AssemblyTestsInfo>();
        }

        /// <summary>
        /// Gets AssemblyTestsInfo collection.
        /// </summary>
        public List<AssemblyTestsInfo> AssembliesInfo;

        /// <summary>
        /// Gets or sets comment on the testing of all assemblies.
        /// </summary>
        public string? Comment;
    }

    /// <summary>
    /// Information about the work of the class tests of this assembly.
    /// </summary>
    public class AssemblyTestsInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyTestsInfo"/> class.
        /// </summary>
        /// <param name="assemblyPath">Path to the assembly.</param>
        public AssemblyTestsInfo(string assemblyPath)
        {
            this.AssemblyPath = assemblyPath;
            this.ClassesInfo = new List<ClassTestsInfo>();
        }

        /// <summary>
        /// Gets Assembly path.
        /// </summary>
        public string AssemblyPath { get; }

        /// <summary>
        /// Gets ClassesInfo collection.
        /// </summary>
        public List<ClassTestsInfo> ClassesInfo { get; }

        /// <summary>
        /// Gets or sets comment on the testing of this assembly.
        /// </summary>
        public string? Comment { get; set; }
    }

    /// <summary>
    /// Information about the work of the tests of this class.
    /// </summary>
    public class ClassTestsInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClassTestsInfo"/> class.
        /// </summary>
        /// <param name="className">Name of the tested class.</param>
        public ClassTestsInfo(string? className)
        {
            this.ClassName = className;
            this.TestsInfo = new List<TestInfo>();
            this.Comments = new List<string>();
        }

        /// <summary>
        /// Gets the name of the tested class.
        /// </summary>
        public string? ClassName { get; }

        /// <summary>
        /// Gets a collection of TestInfo of this class.
        /// </summary>
        public List<TestInfo> TestsInfo { get; }

        /// <summary>
        /// Gets or sets comments on the testing of this class.
        /// </summary>
        public List<string> Comments { get; }

        /// <summary>
        /// Gets successful tests count.
        /// </summary>
        public int SuccessfulTestsCount
        {
            get
            {
                var cnt = 0;
                foreach (var test in this.TestsInfo)
                {
                    if (test.IsSuccess)
                    {
                        ++cnt;
                    }
                }

                return cnt;
            }
        }

        /// <summary>
        /// Gets failed tests count.
        /// </summary>
        public int FailedTestsCount => this.TestsInfo.Count - this.SuccessfulTestsCount;
    }

    /// <summary>
    /// Information about the work of the test.
    /// </summary>
    public class TestInfo
    {
        public TestInfo(string methodName, bool isSuccess, long runningTime, Exception? exception = null, string? reasonForIgnoring = null, string? comment = null)
        {
            this.MethodName = methodName;
            this.IsSuccess = isSuccess;
            this.RunningTime = runningTime;
            this.Exception = exception;
            this.ReasonForIgnoring = reasonForIgnoring;
            this.Comment = comment;
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
        public long RunningTime { get; }

        /// <summary>
        /// Gets an exception was thrown by the test.
        /// </summary>
        public Exception? Exception { get; }

        /// <summary>
        /// Gets the reason for ignoring this test.
        /// </summary>
        public string? ReasonForIgnoring { get; }

        /// <summary>
        /// Gets comment on the running of this method.
        /// </summary>
        public string? Comment { get; }
    }
}