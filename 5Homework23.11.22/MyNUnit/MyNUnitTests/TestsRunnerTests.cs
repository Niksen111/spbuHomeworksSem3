using System;
using System.IO;

namespace MyNUnitTests;

using System.Threading.Tasks;
using MyNUnit;
using NUnit.Framework;

public class Tests
{
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
}