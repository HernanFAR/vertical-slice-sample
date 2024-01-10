namespace VSlices.Base.Responses;

/// <summary>
/// A simple and light-weight struct to indicate success in a operation
/// </summary>
public readonly struct Success
{
    /// <summary>
    /// A static instance of <see cref="Success"/>
    /// </summary>
    public static readonly Success Value = new();

    /// <summary>
    /// A static instance of <see cref="ValueTask"/> of <see cref="Success"/>
    /// </summary>
    public static ValueTask<Success> TaskValue => ValueTask.FromResult(Value);
}
