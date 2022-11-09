using System.IO;
using System.Text;
using NUnit.Framework;

namespace md5.Tests;

public class Tests
{
    [Test]
    public void SimpleDirectoryTest()
    {
        var x1 = CheckSum.GetCheckSum("../../");
        var x2 = CheckSum.GetCheckSumParallel("../../").Result;
        var result = File.ReadAllBytes("../../../sum1.txt");
        Assert.AreEqual(x1, x2);
        Assert.AreEqual(result, x1);
    }
    
    [Test]
    public void SimpleFileTest()
    {
        var x1 = CheckSum.GetCheckSum("../../../sum1.txt");
        var x2 = CheckSum.GetCheckSumParallel("../../../sum1.txt").Result;
        Assert.AreEqual(x1, x2);
    }
}