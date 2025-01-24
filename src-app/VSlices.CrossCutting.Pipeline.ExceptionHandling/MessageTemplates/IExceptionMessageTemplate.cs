namespace VSlices.CrossCutting.Interceptor.ExceptionHandling.MessageTemplates;

/// <summary>
/// Interface for logging message templates
/// </summary>
public interface IExceptionMessageTemplate
{
    /// <summary>
    /// Invoked when the pipeline catches an error
    /// </summary>
    string LogException { get; }

    /// <summary>
    /// Error message
    /// </summary>
    string ErrorMessage { get; }

}

/// <summary>
/// English logging message template
/// </summary>
internal sealed class EnglishExceptionMessageTemplate : IExceptionMessageTemplate
{
    public static IExceptionMessageTemplate Instance { get; } = new EnglishExceptionMessageTemplate();

    /// <inheritdoc />
    public string LogException => "UTC {0} - Finished handling of {1}, result: Exception raised | Input: {2}";

    public string ErrorMessage => "Internal server error. Please try again later.";
}


/// <summary>
/// Spanish logging message template
/// </summary>
internal sealed class SpanishExceptionMessageTemplate : IExceptionMessageTemplate
{
    public static IExceptionMessageTemplate Instance { get; } = new SpanishExceptionMessageTemplate();

    /// <inheritdoc />
    public string LogException => "UTC {0} - Ejecución de {1} terminada, resultado: Exception lanzada | Entrada: {2}";

    public string ErrorMessage => "Error interno. Por favor, vuelva a intentarlo.";
}