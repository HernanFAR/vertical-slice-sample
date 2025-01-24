namespace VSlices.CrossCutting.Interceptor.Filtering.MessageTemplates;

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