using System.Diagnostics;

var matricesSize = 150;
int iterationsNumber = 100;
var info = File.ReadAllLines("../../../GlobalInfo.txt");
int index = int.Parse(info[0]);
++index;

var line = new List<string>();
for (int i = 0; i < matricesSize; ++i)
{
    line.Add("100");
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

var time1 = new Dictionary<long, int>();
var time2 = new Dictionary<long, int>();
var stopwatch = new Stopwatch();

for (int i = 0; i < iterationsNumber; ++i)
{
    stopwatch.Start();
    ParallelMatrixMultiplication.MatrixMultiplier.MultiplyOneThreaded("../../../Matrices/Matrix1.txt",
        "../../../Matrices/Matrix2.txt", "../../../Matrices/OutputOneThread.txt");
    stopwatch.Stop();
    if (!time1.ContainsKey(stopwatch.ElapsedTicks))
    {
        time1.Add(stopwatch.ElapsedTicks, 0);
    }
    ++time1[stopwatch.ElapsedTicks];
    stopwatch.Reset();
    
    stopwatch.Start();
    ParallelMatrixMultiplication.MatrixMultiplier.MultiplyParallel("../../../Matrices/Matrix1.txt",
        "../../../Matrices/Matrix2.txt", "../../../Matrices/OutputParallel.txt");
    stopwatch.Stop();
    if (!time2.ContainsKey(stopwatch.ElapsedTicks))
    {
        time2.Add(stopwatch.ElapsedTicks, 0);
    }
    ++time2[stopwatch.ElapsedTicks];
    stopwatch.Reset();
}

long expectedValue1 = 0;
long expectedValue2 = 0;

foreach (var value in time1)
{
    expectedValue1 += value.Key / iterationsNumber * value.Value;
}

foreach (var value in time2)
{
    expectedValue2 += value.Key / iterationsNumber * value.Value;
}

var squaresOfDeviation1 = new Dictionary<long, int>();
var squaresOfDeviation2 = new Dictionary<long, int>();

foreach (var value in time1)
{
    var square = (value.Key - expectedValue1) * (value.Key - expectedValue1);
    if (!squaresOfDeviation1.ContainsKey(square))
    {
        squaresOfDeviation1.Add(square, 0);
    }
    ++squaresOfDeviation1[square];
}

foreach (var value in time2)
{
    var square = (value.Key - expectedValue2) * (value.Key - expectedValue2);
    if (!squaresOfDeviation2.ContainsKey(square))
    {
        squaresOfDeviation2.Add(square, 0);
    }
    ++squaresOfDeviation2[square];
}

long variance1 = 0;
long variance2 = 0;

foreach (var value in squaresOfDeviation1)
{
    variance1 +=  value.Key / iterationsNumber * value.Value;
}

foreach (var value in squaresOfDeviation2)
{
    variance2 += value.Key / iterationsNumber * value.Value;
}

File.AppendAllLines("../../../Statistics.txt", new []
{
    $"CONFIGURATION {index}",
    "",
    $"Sizes of matrices - {matricesSize} x {matricesSize}",
    $"Number of iterations - {iterationsNumber}",
    $"Number of threads in parallel multiplication - {ParallelMatrixMultiplication.MatrixMultiplier.ThreadsCount}",
    "",
    "One-thread multiplication:",
    $"    Expected value - {expectedValue1}",
    $"    Standard deviation - {Math.Sqrt(variance1)}",
    "",
    "Parallel multiplication:",
    $"    Expected value - {expectedValue2}",
    $"    Standard deviation - {Math.Sqrt(variance2)}",
    "",
    $"Ratio of expected values - {(float) expectedValue1 / expectedValue2}",
    ""
});

File.WriteAllLines("../../../GlobalInfo.txt", new []{index.ToString(), $"{matricesSize} {iterationsNumber} {ParallelMatrixMultiplication.MatrixMultiplier.ThreadsCount}"});