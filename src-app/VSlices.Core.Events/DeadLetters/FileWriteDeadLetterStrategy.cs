 using System.Text.Json;
using VSlices.Core.Events.Configurations;
using VSlices.Domain.Interfaces;

namespace VSlices.Core.Events.DeadLetters;

/// <summary>
/// Creates a json representation of the event to a file in the system.
/// </summary>
public sealed class FileWriteDeadLetterStrategy(FileWriteDeadLetterConfiguration config) : IDeadLetterStrategy
{
    private readonly FileWriteDeadLetterConfiguration _config = config;

    /// <inheritdoc/>
    public async ValueTask PersistAsync(IEvent @event, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(_config.AbsolutePath);

        string fullFilePath = Path.Combine(_config.AbsolutePath, $"{@event.EventId}.json");

        await using StreamWriter file = File.CreateText(fullFilePath);

        await file.WriteAsync(JsonSerializer.Serialize(@event, @event.GetType(), _config.JsonOptions));
        await file.FlushAsync(cancellationToken);

        file.Close();
    }
}
