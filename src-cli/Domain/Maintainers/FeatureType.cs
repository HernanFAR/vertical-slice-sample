using System.Runtime.Serialization;
using Domain.ValueObjects;

namespace Domain.Maintainers;

public sealed class FeatureType : NewType<FeatureType, string>
{
    public static FeatureType Query { get; } = new("Query");

    public static FeatureType Command { get; } = new("Command");

    public static FeatureType Event { get; } = new("Event");

    public FeatureType(string value)
        : base(value)
    {
    }

    public FeatureType(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
