using LanguageExt.Traits;
using System.Runtime.Serialization;

namespace Domain.ValueObjects;

public sealed class PathString : NewType<PathString, string, PathString>, Pred<string>
{
    public PathString(string value)
        : base(value) { }

    public PathString(SerializationInfo info, StreamingContext context)
        : base(info, context) { }
    
    public static bool True(string value) => 
        Path.GetInvalidPathChars().Any(value.Contains) is false;


}
