namespace MyNUnitTests;

using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using MyNUnit;
using MyNUnit.Info;
using NUnit.Framework;

public class Tests
{
    private string classesInfoPath = "../../../ClassesInfo/";

    // [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        if (File.Exists("../../../../TestProject/TestProject.dll"))
        {
            File.Delete("../../../../TestProject/TestProject.dll");
        }

        File.Copy("../../../../TestProject/bin/Debug/net6.0/TestProject.dll", "../../../../TestProject/TestProject.dll");
    }

    [Test]
    public async Task TestProject()
    {
        var info = await TestsRunner.RunTests("../../../../TestProject");
        Assert.IsNull(info.Comment);
        Assert.AreEqual(1, info.AssembliesInfo.Count);
        Assert.IsNull(info.AssembliesInfo[0].Comment);

        foreach (var classInfo in info.AssembliesInfo[0].ClassesInfo)
        {
            await Task.Run(() => this.WriteClassToJsonFile(
                classInfo,
                this.classesInfoPath + $"{classInfo.ClassName}.json"));
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
        await using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate))
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            await JsonSerializer.SerializeAsync(fs, classInfo, options);
        }
    }
}