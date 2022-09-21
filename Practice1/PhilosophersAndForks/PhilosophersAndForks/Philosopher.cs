namespace PhilosophersAndForks;

public class Philosopher
{
    private readonly int _handsNumber = 2;
    public int HandsNumber => _handsNumber;
    private bool _isAlive;
    private Object[] _forks;
    private Thread _life;

    public Philosopher(Object leftFork, Object rightFork)
    {
        _forks = new object[_handsNumber];
        _isAlive = false;
        _forks[0] = leftFork;
        _forks[1] = rightFork;
        var rand = new Random();
        int firstFork = rand.Next(0, 1);
        
        _life = new Thread(() =>
        {
            lock (_forks[firstFork])
            {
                lock (_forks[firstFork == 0 ? 1 : 0])
                {
                    int start = 1000;
                    while (start > 0)
                    {
                        start -= 7;
                    }
                    
                }
            }
        });
    }

    public void StartEating()
    {
        if (!_isAlive)
        {
            _life.Start();
        }
    }

    public void StopEating()
    {
        _life.Join();
        _isAlive = false;
    }
    
    
}