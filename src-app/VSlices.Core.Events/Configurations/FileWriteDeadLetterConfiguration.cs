using System.Text.Json;
using VSlices.Core.Events.DeadLetters;

namespace VSlices.Core.Events.Configurations;

/// <summary>
/// Configuration to use in the <see cref="FileWriteDeadLetterStrategy"/>
/// </summary>
public sealed class FileWriteDeadLetterConfiguration
{
    /// <summary>
    /// Specifies the path where the file will be written
    /// </summary>
    public string AbsolutePath { get; set; } = string.Empty;

    /// <summary>
    /// Specifies the options for the <see cref="JsonSerializer"/>
    /// </summary>
    public JsonSerializerOptions JsonOptions { get; set; } = new();

}
