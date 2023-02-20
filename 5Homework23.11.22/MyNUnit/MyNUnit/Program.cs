using MyNUnit;

if (args.Length != 1)
{
    Console.WriteLine("Too much arguments.");
    return;
}

var tokenSource = new CancellationTokenSource();
var info = await TestsRunner.RunTests(args[0], tokenSource.Token);

TestsRunner.GenerateReport(info, Console.Out);
