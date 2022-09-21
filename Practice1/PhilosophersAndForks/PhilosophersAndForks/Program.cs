namespace PhilosophersAndForks;

public class Program
{
    public static void Main(string[] args)
    {
        int philosophersNumber = 11;
        var philosophers = new Philosopher[philosophersNumber];
        var forks = new Object[philosophersNumber + 1];

        for (int i = 0; i < philosophersNumber; ++i)
        {
            philosophers[i] = new Philosopher(forks[i], forks[(i + 1) % philosophersNumber]);
        }
        
        for (int i = 0; i < philosophersNumber; ++i)
        {
            philosophers[i].StartEating();
        }
        
        for (int i = 0; i < philosophersNumber; ++i)
        {
            philosophers[i].StopEating();
        }
    }
}