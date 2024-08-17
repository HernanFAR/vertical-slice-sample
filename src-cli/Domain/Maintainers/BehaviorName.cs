using System.Runtime.Serialization;

namespace Domain.Maintainers;

public sealed class BehaviorName : NewType<BehaviorName, string>
{
    public static BehaviorName FluentValidation { get; } = new("FluentValidation");

    public static BehaviorName Logging { get; } = new("Logging");

    public static BehaviorName ExceptionLogging { get; } = new("ExceptionLogging");   

    public BehaviorName(string value)
        : base(value) { }

    public BehaviorName(SerializationInfo info, StreamingContext context)
        : base(info, context) { }
}
