namespace VSlices.Base.Responses;

/// <summary>
/// Represents a result from a process.
/// </summary>
/// <typeparam name="T">The expected result in success case</typeparam>
public readonly struct Result<T>
{
    private readonly Failure? _failure;
    private readonly T? _data;

    /// <summary>
    /// Indicates if the process has been successful
    /// </summary>
    public bool IsSuccess => _failure == null;

    /// <summary>
    /// Indicates if the process has failed
    /// </summary>
    public bool IsFailure => _failure != null;

    /// <summary>
    /// The success result of the process, throws <see cref="InvalidOperationException"/> if accessed on failure
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    public T Data => _data ?? throw new InvalidOperationException(nameof(_data));

    /// <summary>
    /// The failure result of the process, throws <see cref="InvalidOperationException"/> if accessed on success
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    public Failure Failure => _failure ?? throw new InvalidOperationException(nameof(_failure));

    /// <summary>
    /// Creates a new instance of <see cref="Result{T}"/> with a success value
    /// </summary>
    /// <param name="data">The success value of the process</param>
    public Result(T data)
    {
        _data = data;
    }

    /// <summary>
    /// Creates a new instance of <see cref="Result{T}"/> with a failure value
    /// </summary>
    /// <param name="failure">A struct with the failure detail</param>
    public Result(Failure failure)
    {
        _failure = failure;
    }

    /// <summary>
    /// Implicitly converts a <see cref="Responses.Failure"/> to a <see cref="Result{T}"/>
    /// </summary>
    /// <param name="failure">The failure to implicitly convert</param>
    /// <returns>A failure result</returns>
    public static implicit operator Result<T>(Failure failure) => new(failure);

    /// <summary>
    /// Implicitly converts a <typeparamref name="T"/> to a <see cref="Result{T}"/>
    /// </summary>
    /// <param name="data">The data to implicitly convert</param>
    /// <returns>A success <see cref="Result{T}" /></returns>
    public static implicit operator Result<T>(T data) => new(data);

}
