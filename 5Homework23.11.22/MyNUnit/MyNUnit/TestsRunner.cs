namespace MyNUnit;

using System.Diagnostics;
using System.Reflection;
using MyNUnit.Attributes;
using MyNUnit.Info;

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
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public static async Task GenerateReport(SummaryInfo info, TextWriter writer)
    {
        if (info.Comment != null)
        {
            writer.WriteLine(info.Comment);
            return;
        }

        foreach (var assemblyInfo in info.AssembliesInfo)
        {
            await writer.WriteAsync("    ");
            await writer.WriteLineAsync(assemblyInfo.AssemblyPath);
            if (assemblyInfo.Comment != null)
            {
                await writer.WriteLineAsync(assemblyInfo.Comment);
                await writer.WriteLineAsync();
                continue;
            }

            await writer.WriteLineAsync();
            foreach (var classInfo in assemblyInfo.ClassesInfo)
            {
                await writer.WriteLineAsync($"  Class: {classInfo.ClassName}");
                foreach (var comment in classInfo.Comments)
                {
                    await writer.WriteLineAsync(comment);
                }

                if (classInfo.Exception != null)
                {
                    await writer.WriteLineAsync($"{classInfo.Exception}");
                    continue;
                }

                await writer.WriteLineAsync($"Successful tests: {classInfo.SuccessfulTestsCount}");
                await writer.WriteLineAsync($"Failed tests: {classInfo.FailedTestsCount}");
                await writer.WriteLineAsync();

                foreach (var testInfo in classInfo.TestsInfo)
                {
                    await writer.WriteLineAsync(testInfo.MethodName);
                    if (testInfo.ReasonForIgnoring != null)
                    {
                        await writer.WriteLineAsync("Test ignored.");
                        await writer.WriteLineAsync(testInfo.ReasonForIgnoring);
                        continue;
                    }

                    if (testInfo.IsSuccess)
                    {
                        await writer.WriteLineAsync("Test status: Success");
                        await writer.WriteLineAsync($"Running time: {testInfo.RunningTime}");
                    }
                    else
                    {
                        await writer.WriteLineAsync("Test status: Failed");
                        await writer.WriteLineAsync(testInfo.Comment);
                        await writer.WriteLineAsync($"{testInfo.Exception}");
                    }

                    await writer.WriteLineAsync();
                }
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
    public static async Task<SummaryInfo> RunTests(string pathToAssemblies, CancellationToken token = default(CancellationToken))
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

        if (!Directory.Exists(pathToAssemblies))
        {
            summaryInfo.Comment = "Directory is not exist.";
            return summaryInfo;
        }

        var testsAssemblies = Directory.EnumerateFiles(pathToAssemblies, "*.dll");
        var assemblies = new List<Task<AssemblyTestsInfo>>();
        foreach (var file in testsAssemblies)
        {
            assemblies.Add(Task.Run(() => AssemblyRun(Assembly.LoadFrom(file)), token));
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
                        var reasonForIgnoring = ((TestAttribute)attribute).Ignore;
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

        if (testMethods.Count == 0)
        {
            return null;
        }

        foreach (var method in beforeClass)
        {
            try
            {
                method.Invoke(null, null);
            }
            catch (Exception e)
            {
                classInfo.Exception = e;
                classInfo.Comments.Add($"ERROR: Running the [BeforeClass] method {method.Name} failed.");
                return classInfo;
            }
        }

        var stopwatch = new Stopwatch();
        var tests = new List<Task<TestInfo>>();
        stopwatch.Start();
        foreach (var testMethod in testMethods)
        {
            var instance = testClass.IsAbstract && testClass.IsSealed ? null : Activator.CreateInstance(testClass);
            tests.Add(Task.Run(() => TestRun(instance, testMethod.Method, testMethod.Expected, before, after)));
        }

        foreach (var test in tests)
        {
            classInfo.TestsInfo.Add(await test);
        }

        stopwatch.Stop();
        classInfo.RunningTime = stopwatch.ElapsedMilliseconds;

        foreach (var method in afterClass)
        {
            try
            {
                method.Invoke(null, null);
            }
            catch (Exception e)
            {
                classInfo.Exception = e;
                classInfo.Comments.Add($"ERROR: Running the [AfterClass] method {method.Name} failed.");
                return classInfo;
            }
        }

        return classInfo;
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
                return new TestInfo(methodInfo.Name, false, 0, exception, null, $"ERROR: Running the[Before] method {method.Name} failed.");
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
            if (exceptedException == null || e.InnerException!.GetType() != exceptedException)
            {
                exception = e;
                stopwatch.Stop();
                return new TestInfo(methodInfo.Name, false, 0, exception, null, "ERROR: [Test] method failed.");
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
                return new TestInfo(methodInfo.Name, false, stopwatch.ElapsedMilliseconds, e, null, $"ERROR: Running the [After] method {method.Name} failed.");
            }
        }

        return new TestInfo(methodInfo.Name, true, stopwatch.ElapsedMilliseconds);
    }
}