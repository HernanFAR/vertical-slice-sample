namespace VSlices.CrossCutting.Pipeline.EventFiltering.MessageTemplates;

/// <summary>
/// Interface for event filtering message templates
/// </summary>
public interface IEventFilteringMessageTemplate
{
    /// <summary>
    /// Invoked if the event filter determines that the event should be executed
    /// </summary>
    string ContinueExecution { get; }

    /// <summary>
    /// Invoked if the event filter determines that the event should skipped
    /// </summary>
    string SkipExecution { get; }
}

internal sealed class SpanishEventFilteringMessageTemplate : IEventFilteringMessageTemplate
{
    public static IEventFilteringMessageTemplate Instance { get; } = new SpanishEventFilteringMessageTemplate();

    public string ContinueExecution => "UTC {0} - Filtro de eventos de {1} determinó que este evento debe ser ejecutado | Entrada: {2}";

    public string SkipExecution => "UTC {0} - Filtro de eventos de {1} determinó que este evento debe ser saltado  | Entrada: {2}";

}

internal sealed class EnglishEventFilteringMessageTemplate : IEventFilteringMessageTemplate
{
    public static IEventFilteringMessageTemplate Instance { get; } = new EnglishEventFilteringMessageTemplate();

    public string ContinueExecution => "UTC {0} - Event filtering of {1} determines that this event should be executed | Input: {2}";

    public string SkipExecution => "UTC {0} - Event filtering of {1} determines that this event should be skipped | Input: {2}";

}
