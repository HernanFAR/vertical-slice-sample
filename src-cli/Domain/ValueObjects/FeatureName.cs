using System.Runtime.Serialization;

namespace Domain.ValueObjects;

public sealed class FeatureName : NewType<FeatureName, string>
{
    public FeatureName(string value)
        : base(value) { }

    public FeatureName(SerializationInfo info, StreamingContext context)
        : base(info, context) { }
}