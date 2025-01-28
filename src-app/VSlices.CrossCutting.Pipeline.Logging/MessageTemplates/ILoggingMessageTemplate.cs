namespace VSlices.CrossCutting.Interceptor.Logging.MessageTemplates;

/// <summary>
/// Interface for logging message templates
/// </summary>
public interface ILoggingMessageTemplate
{
    /// <summary>
    /// Invoked when the pipeline starts
    /// </summary>
    string Start { get; }

    /// <summary>
    /// Invoked when the handler executes with failure
    /// </summary>
    string FailureEnd { get; }

    /// <summary>
    /// Invoked when the handler executes correctly
    /// </summary>
    string SuccessEnd { get; }
}

/// <summary>
/// English logging message template
/// </summary>
internal sealed class EnglishLoggingMessageTemplate : ILoggingMessageTemplate
{
    public static ILoggingMessageTemplate Instance { get; } = new EnglishLoggingMessageTemplate();

    /// <inheritdoc />
    public string Start => "UTC {0} - Starting handling of {1} | Input: {2}";

    /// <inheritdoc />
    public string FailureEnd => "UTC {0} - Finished handling of {1}, result: Failure | Input: {2} | Output: {3}";

    /// <inheritdoc />
    public string SuccessEnd => "UTC {0} - Finished handling of {1}, result: Success | Input: {2} | Output: {3}";
}


/// <summary>
/// Spanish logging message template
/// </summary>
internal sealed class SpanishLoggingMessageTemplate : ILoggingMessageTemplate
{
    public static ILoggingMessageTemplate Instance { get; } = new SpanishLoggingMessageTemplate();

    /// <inheritdoc />
    public string Start => "UTC {0} - Ejecución de {1} iniciada | Entrada: {2}";

    /// <inheritdoc />
    public string FailureEnd => "UTC {0} - Ejecución de {1} terminada, resultado: Fallo | Entrada: {2} | Salida: {3}";

    /// <inheritdoc />
    public string SuccessEnd => "UTC {0} - Ejecución de {1} terminada, result: Exito | Entrada: {2} | Salida: {3}";
}