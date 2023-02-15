using MyNUnit;

if (args.Length != 1)
{
    Console.WriteLine("Too much arguments.");
    return;
}

var tokenSource = new CancellationTokenSource();
var testsRunner = new TestsRunner(tokenSource.Token);
await testsRunner.RunTests(args[0]);

foreach (var info in testsRunner.RunInfo)
{
    Console.WriteLine(info);
}