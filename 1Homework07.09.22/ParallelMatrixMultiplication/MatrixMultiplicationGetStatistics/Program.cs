using System.Diagnostics;

var matricesSize = 3;
int iterationsNumber = 100;
var info = File.ReadAllLines("../../../GlobalInfo.txt");
int index = int.Parse(info[0]);
++index;

var line = new List<string>();
for (int i = 0; i < matricesSize; ++i)
{
    line.Add("1");
}

var line1 = String.Join(" ", line.ToArray());

File.WriteAllLines("../../../Matrices/Matrix1.txt", new []{""});
File.WriteAllLines("../../../Matrices/Matrix2.txt", new []{""});

StreamWriter file1 = new("../../../Matrices/Matrix1.txt");
StreamWriter file2 = new("../../../Matrices/Matrix2.txt");

for (int i = 0; i < matricesSize; ++i)
{
    file1.WriteLine(line1);
    file2.WriteLine(line1);
}

file1.Close();
file2.Close();

var time1 = new List<long>();
var time2 = new List<long>();
var stopwatch = new Stopwatch();

for (int i = 0; i < iterationsNumber; ++i)
{
    stopwatch.Start();
    ParallelMatrixMultiplication.MatrixMultiplier.MultiplyOneThreaded("../../../Matrices/Matrix1.txt",
        "../../../Matrices/Matrix2.txt", "../../../Matrices/OutputOneThread.txt");
    stopwatch.Stop();
    time1.Add(stopwatch.ElapsedTicks);
    stopwatch.Reset();
    
    stopwatch.Start();
    ParallelMatrixMultiplication.MatrixMultiplier.MultiplyParallel("../../../Matrices/Matrix1.txt",
        "../../../Matrices/Matrix2.txt", "../../../Matrices/OutputParallel.txt");
    stopwatch.Stop();
    time2.Add(stopwatch.ElapsedTicks);
    stopwatch.Reset();
}

var expectedValue1 = (long) time1.Average();
var expectedValue2 = (long) time2.Average();

var squaresOfDeviation1 = new List<long>();
var squaresOfDeviation2 = new List<long>();

foreach (var value in time1)
{
    squaresOfDeviation1.Add((value - expectedValue1) * (value - expectedValue1));
}

foreach (var value in time2)
{
    squaresOfDeviation2.Add((value - expectedValue2) * (value - expectedValue2));
}

var variance1 = (long) squaresOfDeviation1.Average();
var variance2 = (long) squaresOfDeviation2.Average();

var order1 = (long) Math.Pow(10, Math.Max(variance1.ToString().Length - 2, 1));
var order2 = (long) Math.Pow(10, Math.Max(variance2.ToString().Length - 2, 1));
expectedValue1 = expectedValue1 / order1 * order1;
expectedValue2 = expectedValue2 / order2 * order2;
variance1 = variance1 / order1 * order1;
variance2 = variance2 / order2 * order2;

File.AppendAllLines("../../../Statistics.txt", new []
{
    $"CONFIGURATION {index}",
    "",
    $"Sizes of matrices - {matricesSize} x {matricesSize}",
    $"Number of iterations - {iterationsNumber}",
    $"Number of threads in parallel multiplication - {ParallelMatrixMultiplication.MatrixMultiplier.ThreadsCount}",
    "",
    "One-thread multiplication:",
    $"    Expected value - {expectedValue1} stopwatch ticks",
    $"    Standard deviation - {Math.Sqrt(variance1)} stopwatch ticks",
    "",
    "Parallel multiplication:",
    $"    Expected value - {expectedValue2} stopwatch ticks",
    $"    Standard deviation - {Math.Sqrt(variance2)} stopwatch ticks",
    "",
    $"Ratio of expected values - {(float) expectedValue1 / expectedValue2}",
    ""
});

File.WriteAllLines("../../../GlobalInfo.txt", new []{index.ToString(), $"{matricesSize} {iterationsNumber} {ParallelMatrixMultiplication.MatrixMultiplier.ThreadsCount}"});