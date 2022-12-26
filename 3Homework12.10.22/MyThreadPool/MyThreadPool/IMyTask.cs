namespace MyThreadPool;

/// <summary>
/// An operation performed on MyThreadPool that returns a value.
/// </summary>
/// <typeparam name="TResult">Return value type of task function.</typeparam>
public interface IMyTask<TResult>
{
    /// <summary>
    /// Gets a value indicating whether the task result is ready.
    /// </summary>
    bool IsCompleted { get; }

    /// <summary>
    /// Gets the task result.
    /// </summary>
    TResult Result { get; }

    /// <summary>
    /// Creates a new task based on this task.
    /// </summary>
    /// <param name="func">A calculation to make.</param>
    /// <typeparam name="TNewResult">Type of the resulting value.</typeparam>
    /// <returns>Task with new resulting value.</returns>
    IMyTask<TNewResult> ContinueWith<TNewResult>(Func<TResult, TNewResult> func);
}