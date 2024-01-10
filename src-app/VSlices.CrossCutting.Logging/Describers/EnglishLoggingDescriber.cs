namespace VSlices.CrossCutting.Logging.Describers;

/// <summary>
/// Describer for logging messages in english
/// </summary>
public sealed class EnglishLoggingDescriber : ILoggingDescriber
{
    /// <inheritdoc/>
    public string Initial => "Log hour: {0} | Starting handling of {1}, with the following properties: {2}.";

    /// <inheritdoc/>
    public string Success => "Log hour: {0} | Finishing handling of {1}, response obtained correctly: {2}.";

    /// <inheritdoc/>
    public string Failure => "Log hour: {0} | Finishing handling of {1}, response obtained with errors: {2}.";

}
