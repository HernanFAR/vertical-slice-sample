using VSlices.Base.Failures;

namespace VSlices.Base.Extensions;

/// <summary>
/// Extensions for <see cref="Option{T}"/>
/// </summary>
public static class OptionExtensions
{
    /// <summary>
    /// Creates a <see cref="LanguageExt.Eff{RT, A}"/> to get the value from an <see cref="Option{T}"/> or an <see cref="ExtensibleExpected"/>
    /// with not found (code 404)
    /// </summary>
    public static Eff<VSlicesRuntime, T> GetOrNotFoundEff<T>(this Option<T> option, 
                                                             string message, 
                                                             Dictionary<string, object?>? extensions = null) => 
        option.ToEff(ExtensibleExpected.NotFound(message, extensions ?? []));
}
