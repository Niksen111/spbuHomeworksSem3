namespace MyThreadPool;

/// <summary>
/// An operation performed on MyThreadPool that returns a value.
/// </summary>
/// <typeparam name="TResult">Type of the returnable value.</typeparam>
public interface IMyTask<TResult>
{
    /// <summary>
    /// Gets a value indicating whether get true if the task is completed.
    /// </summary>
    bool IsCompleted { get; }

    /// <summary>
    /// Gets the task result.
    /// </summary>
    TResult Result { get; }

    /// <summary>
    /// Creates a new task based on this task.
    /// </summary>
    /// <param name="func">The function to perform.</param>
    /// <typeparam name="TNewResult">Type of the new returnable value.</typeparam>
    /// <returns>Task with new returnable value.</returns>
    IMyTask<TNewResult> ContinueWith<TNewResult>(Func<TResult, TNewResult> func);
}