using System.Diagnostics;
using ParallelMatrixMultiplication;

var matricesSize = 100;
int iterationsNumber = 100;
var info = File.ReadAllLines("../../../GlobalInfo.txt");
int index = int.Parse(info[0]);
++index;

var line = new List<string>();
for (int i = 0; i < matricesSize; ++i)
{
    line.Add("111");
}

var line1 = string.Join(" ", line.ToArray());

File.WriteAllLines("../../../Matrices/Matrix1.txt", new[] { string.Empty });
File.WriteAllLines("../../../Matrices/Matrix2.txt", new[] { string.Empty });

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
    Matrix.MultiplyOneThreaded(
        new Matrix("../../../Matrices/Matrix1.txt"),
        new Matrix("../../../Matrices/Matrix2.txt")).WriteToFile("../../../Matrices/OutputOneThread.txt");
    stopwatch.Stop();
    time1.Add(stopwatch.ElapsedTicks);
    stopwatch.Reset();

    stopwatch.Start();
    Matrix.Multiply(
        new Matrix("../../../Matrices/Matrix1.txt"),
        new Matrix("../../../Matrices/Matrix2.txt")).WriteToFile("../../../Matrices/OutputParallel.txt");
    stopwatch.Stop();
    time2.Add(stopwatch.ElapsedTicks);
    stopwatch.Reset();
}

var expectedValue1 = (long)time1.Average();
var expectedValue2 = (long)time2.Average();

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

var sqrtVariance1 = (long)Math.Sqrt(squaresOfDeviation1.Average());
var sqrtVariance2 = (long)Math.Sqrt(squaresOfDeviation2.Average());
var accuracy1 = Math.Max(sqrtVariance1.ToString().Length - 2, 1);
var accuracy2 = Math.Max(sqrtVariance2.ToString().Length - 2, 1);

var order1 = (long)Math.Pow(10, accuracy1);
var order2 = (long)Math.Pow(10, accuracy2);
var value1 = expectedValue1 / order1 * order1;
var value2 = expectedValue2 / order2 * order2;
sqrtVariance1 = sqrtVariance1 / order1 * order1;
sqrtVariance2 = sqrtVariance2 / order2 * order2;

File.AppendAllLines("../../../Statistics.txt", new[]
{
    $"CONFIGURATION {index}",
    string.Empty,
    $"Sizes of matrices - {matricesSize} x {matricesSize}",
    $"Number of iterations - {iterationsNumber}",
    string.Empty,
    "One-thread multiplication:",
    $"    Expected value - {value1} stopwatch ticks",
    $"    Standard deviation - {sqrtVariance1} stopwatch ticks",
    $"Values with an accuracy of 10^{accuracy1}",
    string.Empty,
    "Parallel multiplication:",
    $"    Expected value - {value2} stopwatch ticks",
    $"    Standard deviation - {sqrtVariance2} stopwatch ticks",
    $"Values with an accuracy of 10^{accuracy2}",
    string.Empty,
    $"Ratio of expected values - {(float)expectedValue1 / expectedValue2}",
    string.Empty,
});

File.WriteAllLines("../../../GlobalInfo.txt", new[] {index.ToString(), $"{matricesSize} {iterationsNumber}" });