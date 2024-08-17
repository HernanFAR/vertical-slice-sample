using System.Runtime.Serialization;

namespace Domain.ValueObjects;

public sealed class Language : NewType<Language, string>
{
    public static Language Spanish { get; } = new("es");

    public static Language English { get; } = new("en");

    public Language(string value)
        : base(value) { }

    public Language(SerializationInfo info, StreamingContext context)
        : base(info, context) { }
}
