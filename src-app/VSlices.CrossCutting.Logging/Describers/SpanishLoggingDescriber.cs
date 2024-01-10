namespace VSlices.CrossCutting.Logging.Describers;

/// <summary>
/// Describer for logging messages in spanish
/// </summary>
public sealed class SpanishLoggingDescriber : ILoggingDescriber
{
    /// <inheritdoc/>
    public string Initial => "Hora de registro: {0} | Iniciando manejo de {1}, con valores: {2}.";

    /// <inheritdoc/>
    public string Success => "Hora de registro: {0} | Terminado manejo de {1}, respuesta obtenida correctamente: {2}.";

    /// <inheritdoc/>
    public string Failure => "Hora de registro: {0} | Terminado manejo de {1}, respuesta obtenida con errores: {2}.";

}
