using System.Diagnostics;

namespace MyNUnit;

using System.Collections.Concurrent;
using System.Reflection;

/// <summary>
/// Class for running and checking tests.
/// </summary>
public class TestsRunner
{
    /// <summary>
    /// Runs all tests in the specified directory or assembly.
    /// </summary>
    /// <param name="pathToAssemblies">The path to the assemblies.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task<BlockingCollection<AssemblyTestsInfo>?> RunTests(string pathToAssemblies, CancellationToken token)
    {
        if (File.Exists(pathToAssemblies))
        {
            if (string.CompareOrdinal(Path.GetExtension(pathToAssemblies), ".dll") == 0)
            {
                var assembly = Assembly.LoadFile(pathToAssemblies);
                var assemblyInfo = new BlockingCollection<AssemblyTestsInfo>();
                var resultOfAssembly = await this.AssemblyRun(assembly);
                if (resultOfAssembly == null)
                {
                    return null;
                }

                assemblyInfo.Add(resultOfAssembly);
                assemblyInfo.CompleteAdding();
                return assemblyInfo;
            }
        }

        var runInfo = new BlockingCollection<AssemblyTestsInfo>();
        var testsAssemblies = Directory.EnumerateFiles(pathToAssemblies, "*.dll");
        var assemblies = new List<Task<AssemblyTestsInfo?>>();
        foreach (var assembly in testsAssemblies)
        {
            assemblies.Add(Task.Run(() => this.AssemblyRun(Assembly.Load(assembly))));
        }

        foreach (var assembly in assemblies)
        {
            var resultOfAssembly = await assembly;
            if (resultOfAssembly != null)
            {
                runInfo.Add(resultOfAssembly);
            }
        }

        runInfo.CompleteAdding();
        return runInfo;
    }

    private async Task<AssemblyTestsInfo?> AssemblyRun(Assembly assembly)
    {
        var assemblyInfo = new AssemblyTestsInfo(assembly.Location);
        var types = assembly.ExportedTypes;
        var classesResults = new List<Task<ClassTestsInfo?>>();

        foreach (var type in types)
        {
            if (type.IsClass)
            {
                classesResults.Add(Task.Run(() => this.ClassRun(type)));
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

        assemblyInfo.ClassesInfo.CompleteAdding();
        return assemblyInfo;
    }

    private async Task<ClassTestsInfo?> ClassRun(Type testClass)
    {
        var classInfo = new ClassTestsInfo(testClass.FullName);
        var methods = testClass.GetMethods();
        var beforeClass = new List<MethodInfo>();
        var afterClass = new List<MethodInfo>();
        var before = new List<MethodInfo>();
        var after = new List<MethodInfo>();
        var testMethods = new List<MethodInfo>();
        var constructors = new List<MethodInfo>();
        foreach (var method in methods)
        {
            foreach (var attribute in Attribute.GetCustomAttributes(method))
            {
                if (attribute.GetType() == typeof(Attributes.BeforeClassAttribute))
                {
                    if (method.IsStatic)
                    {
                        beforeClass.Add(method);
                    }
                    else
                    {
                        if (classInfo.Comments == null)
                        {
                            classInfo.Comments = string.Empty;
                        }

                        classInfo.Comments += "ERROR: Founded non-static class with BeforeClassAttribute.\n";
                    }
                }
                else if (attribute.GetType() == typeof(Attributes.AfterClassAttribute))
                {
                    if (method.IsStatic)
                    {
                        afterClass.Add(method);
                    }
                    else
                    {
                        if (classInfo.Comments == null)
                        {
                            classInfo.Comments = string.Empty;
                        }

                        classInfo.Comments += "ERROR: Founded non-static class with AfterClassAttribute.\n";
                    }
                }
                else if (attribute.GetType() == typeof(Attributes.BeforeAttribute))
                {
                    before.Add(method);
                }
                else if (attribute.GetType() == typeof(Attributes.AfterAttribute))
                {
                    after.Add(method);
                }
                else if (attribute.GetType() == typeof(Attributes.TestAttribute))
                {
                    if (((Attributes.TestAttribute)attribute).Ignore != null)
                    {
                        var reasonForIgnoring = ((Attributes.TestAttribute)attribute).Ignore;
                        var localInfo = new TestInfo(method.Name, false, 0, null, reasonForIgnoring);
                        classInfo.TestsInfo.Add(localInfo);
                    }
                    else
                    {
                        testMethods.Add(method);
                    }
                }
                else if (method.IsConstructor)
                {
                    constructors.Add(method);
                }
            }
        }

        foreach (var method in beforeClass)
        {
            method.Invoke(null, null);
        }

        var testInvoking = new Func<object?, MethodInfo, TestInfo>((instance, m) =>
        {
            foreach (var method in before)
            {
                method.Invoke(instance, null);
            }

            var stopwatch = new Stopwatch();
            Exception? exception = null;
            stopwatch.Start();
            try
            {
                m.Invoke(instance, null);
            }
            catch (Exception e)
            {
                exception = e;
            }

            stopwatch.Stop();

            foreach (var method in after)
            {
                method.Invoke(instance, null);
            }

            return new TestInfo(m.Name, exception == null, stopwatch.ElapsedMilliseconds, exception);
        });

        var tests = new List<Task<TestInfo>>();
        foreach (var testMethod in testMethods)
        {
            var instance = Activator.CreateInstance(testClass);
            tests.Add(Task.Run(() => testInvoking(instance, testMethod)));
        }

        foreach (var test in tests)
        {
            classInfo.TestsInfo.Add(await test);
        }

        foreach (var method in afterClass)
        {
            method.Invoke(null, null);
        }

        return classInfo;
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

        /// <summary>
        /// Gets or sets comments on the testing of this assembly.
        /// </summary>
        public string? Comments { get; set; }
    }

    /// <summary>
    /// Information about the work of the tests of this class.
    /// </summary>
    public class ClassTestsInfo
    {
        public ClassTestsInfo(string? className)
        {
            this.ClassName = className;
            this.TestsInfo = new BlockingCollection<TestInfo>();
        }

        /// <summary>
        /// Gets the name of the tested class.
        /// </summary>
        public string? ClassName { get; }

        /// <summary>
        /// Gets a collection of TestInfo of this class.
        /// </summary>
        public BlockingCollection<TestInfo> TestsInfo { get; }

        /// <summary>
        /// Gets or sets comments on the testing of this class.
        /// </summary>
        public string? Comments { get; set; }
    }

    /// <summary>
    /// Information about the work of the test.
    /// </summary>
    public class TestInfo
    {
        public TestInfo(string methodName, bool isSuccess, long runningTime, Exception? exception = null, string? reasonForIgnoring = null)
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
        public long RunningTime { get; }

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