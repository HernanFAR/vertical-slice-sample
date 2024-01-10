namespace VSlices.CrossCutting.Logging.Describers;

/// <summary>
/// Interface to describe the logging messages
/// </summary>
public interface ILoggingDescriber
{
    /// <summary>
    /// Message to log when the request is starting
    /// </summary>
    string Initial { get; }

    /// <summary>
    /// Message to log when the request is finishing with success
    /// </summary>
    string Success { get; }

    /// <summary>
    /// Message to log when the request is finishing with failure
    /// </summary>
    string Failure { get; }
}
