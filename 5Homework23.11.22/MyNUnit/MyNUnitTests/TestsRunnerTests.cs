using System;

namespace MyNUnitTests;

using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using MyNUnit;
using MyNUnit.Info;
using NUnit.Framework;

public class Tests
{
    private string classesInfoPath = $"../../../ClassesInfo{Path.DirectorySeparatorChar}";

    [Test]
    public async Task TestProject()
    {
        var info = await TestsRunner.RunTests("../../../../TestProject");
        Assert.IsNull(info.Comment);
        Assert.AreEqual(1, info.AssembliesInfo.Count);
        Assert.IsNull(info.AssembliesInfo[0].Comment);

        foreach (var classInfo in info.AssembliesInfo[0].ClassesInfo)
        {
            var realClassInfo = await this.GetClassInfoFromJsonFile(this.classesInfoPath + $"{classInfo.ClassName}.json");
            if (realClassInfo == null)
            {
                Assert.Fail();
                continue;
            }

            this.CompareClassTestsInfo(realClassInfo, classInfo);
        }
    }

    [Test]
    public async Task EmptyProject()
    {
        var info = await TestsRunner.RunTests("../../../NotProject");
        Assert.NotNull(info.Comment);
        Assert.AreEqual("No assemblies found.", info.Comment);

        info = await TestsRunner.RunTests("IncorrectPath");
        Assert.NotNull(info.Comment);
        Assert.AreEqual("Directory is not exist.", info.Comment);
    }

    private async Task WriteClassToJsonFile(ClassTestsInfo classInfo, string path)
    {
        File.Delete(path);
        await using (var fs = File.Open(path, FileMode.OpenOrCreate))
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            await JsonSerializer.SerializeAsync(fs, classInfo, options);
        }
    }

    private async Task<ClassTestsInfo?> GetClassInfoFromJsonFile(string path)
    {
        await using var fs = File.Open(path, FileMode.Open);
        return await JsonSerializer.DeserializeAsync<ClassTestsInfo>(fs);
    }

    private void CompareClassTestsInfo(ClassTestsInfo expected, ClassTestsInfo classInfo)
    {
        Assert.AreEqual(expected.ClassName, classInfo.ClassName);
        Assert.AreEqual(expected.TestsInfo.Count, classInfo.TestsInfo.Count);
        expected.TestsInfo = expected.TestsInfo.OrderBy(x => x.MethodName).ToList();
        classInfo.TestsInfo = classInfo.TestsInfo.OrderBy(x => x.MethodName).ToList();
        for (int i = 0; i < expected.TestsInfo.Count; ++i)
        {
            this.CompareTestInfo(expected.TestsInfo[i], classInfo.TestsInfo[i]);
        }
    }

    private void CompareTestInfo(TestInfo expected, TestInfo info)
    {
        Assert.AreEqual(expected.MethodName, info.MethodName);
        Assert.AreEqual(expected.Comment, info.Comment);
        Assert.AreEqual(expected.IsSuccess, info.IsSuccess);
        Assert.AreEqual(expected.ReasonForIgnoring, info.ReasonForIgnoring);
    }
}