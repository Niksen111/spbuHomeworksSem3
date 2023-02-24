namespace MyNUnit;

using System.Diagnostics;
using System.Reflection;
using Attributes;
using Info;

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
            await writer.WriteLineAsync("    ASSEMBLY PATH:");
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
                await writer.WriteLineAsync($"Summary running time: {classInfo.RunningTime} ms");
                await writer.WriteLineAsync($"Successful tests: {classInfo.SuccessfulTestsCount}");
                await writer.WriteLineAsync($"Failed tests: {classInfo.FailedTestsCount}");

                foreach (var comment in classInfo.Comments)
                {
                    await writer.WriteLineAsync(comment);
                }

                if (classInfo.Exception != null)
                {
                    await writer.WriteLineAsync();
                    await writer.WriteLineAsync("EXCEPTION:");
                    await writer.WriteLineAsync($"{classInfo.Exception}");
                    await writer.WriteLineAsync();
                    continue;
                }

                await writer.WriteLineAsync();

                foreach (var testInfo in classInfo.TestsInfo)
                {
                    await writer.WriteLineAsync(" Test name: " + testInfo.MethodName);
                    if (testInfo.ReasonForIgnoring != null)
                    {
                        await writer.WriteLineAsync("Test ignored.");
                        await writer.WriteLineAsync(testInfo.ReasonForIgnoring);
                        await writer.WriteLineAsync();
                        continue;
                    }

                    if (testInfo.IsSuccess)
                    {
                        await writer.WriteLineAsync("Test status: Success");
                        await writer.WriteLineAsync($"Running time: {testInfo.RunningTime} ms");
                    }
                    else
                    {
                        await writer.WriteLineAsync("Test status: Failed");
                        await writer.WriteLineAsync(testInfo.Comment);
                        await writer.WriteLineAsync();
                        await writer.WriteLineAsync("EXCEPTION:");
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
                var resultOfAssembly = await Task.Run(() => AssemblyRun(assembly), token);
                summaryInfo.AssembliesInfo.Add(resultOfAssembly);
                return summaryInfo;
            }
        }

        if (!Directory.Exists(pathToAssemblies))
        {
            summaryInfo.Comment = "Directory does not exist.";
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
            if (type.IsClass && ContainsTestMethods(type))
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
            DistributeMethod(method, ref classInfo, ref before, ref after, ref beforeClass, ref afterClass, ref testMethods);
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
            var instance = testClass is { IsAbstract: true, IsSealed: true } ? null : Activator.CreateInstance(testClass);
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

    private static TestInfo TestRun(object? instance, MethodInfo methodInfo, Type? expectedException, List<MethodInfo> before, List<MethodInfo> after)
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
        var isCaught = false;
        stopwatch.Start();
        try
        {
            methodInfo.Invoke(instance, null);
        }
        catch (Exception e)
        {
            isCaught = true;
            if (expectedException == null || e.InnerException!.GetType() != expectedException)
            {
                exception = e;
                stopwatch.Stop();
                return new TestInfo(methodInfo.Name, false, 0, exception, null, "ERROR: [Test] method failed.");
            }
        }

        stopwatch.Stop();

        if (expectedException != null && !isCaught)
        {
            return new TestInfo(methodInfo.Name, false, stopwatch.ElapsedMilliseconds, null, "ERROR: The expected exception wasn't thrown out.");
        }

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

    private static bool ContainsTestMethods(Type type)
    {
        foreach (var method in type.GetMethods())
        {
            foreach (var attribute in Attribute.GetCustomAttributes(method))
            {
                if (attribute.GetType() == typeof(TestAttribute))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static void DistributeMethod(MethodInfo method, ref ClassTestsInfo classInfo, ref List<MethodInfo> before, ref List<MethodInfo> after, ref List<MethodInfo> beforeClass, ref List<MethodInfo> afterClass, ref List<(MethodInfo Method, Type? Expected)> testMethods)
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
                    classInfo.Comments.Add($"ERROR: Found a non-static class with BeforeClassAttribute: {method.Name}");
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
                    classInfo.Comments.Add($"ERROR: Found a non-static class with AfterClassAttribute: {method.Name}");
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
                if (method.IsStatic || method.GetParameters().Length > 0 || method.ReturnType != typeof(void))
                {
                    if (method.IsStatic)
                    {
                        classInfo.Comments.Add($"ERROR: Found a static Test method: {method.Name}");
                    }

                    if (method.GetParameters().Length > 0)
                    {
                        classInfo.Comments.Add($"ERROR: Found Test method containing parameters: {method.Name}");
                    }

                    if (method.ReturnType != typeof(void))
                    {
                        classInfo.Comments.Add($"ERROR: Found a method with not void output type: {method.Name}");
                    }
                }
                else if (((TestAttribute)attribute).Ignore != null)
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
}