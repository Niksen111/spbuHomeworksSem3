using MyNUnit;

if (args.Length != 1)
{
    Console.WriteLine("Too much arguments.");
    return;
}

var tokenSource = new CancellationTokenSource();
var testsRunner = new TestsRunner();
var info = await testsRunner.RunTests(args[0], tokenSource.Token);
if (info == null)
{
    Console.WriteLine("This directory is not contains a tests.");
    return;
}

Console.WriteLine(info.ToString());
