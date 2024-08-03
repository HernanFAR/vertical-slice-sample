using LanguageExt.Common;
using VSlices.Base;
using VSlices.Base.Failures;
using VSlices.Base.Traits;
using static VSlices.Base.Failures.ExtensibleExpected;

// ReSharper disable CheckNamespace

namespace VSlices;

/// <summary>
/// Used to give rapid access to functions when defining monads
/// </summary>
public static class VSlicesPrelude
{
    /// <summary>
    /// Provides with an instance of <typeparamref name="T"/>
    /// </summary>
    /// <remarks>
    /// If it does not exist in the runtime, throws <see cref="InvalidOperationException"/>
    /// </remarks>
    public static Eff<VSlicesRuntime, T> provide<T>()
        where T : notnull => DependencyProviderExtensions<Eff<VSlicesRuntime>, VSlicesRuntime>.Get<T>().As();

    /// <summary>
    /// Provides with an optional instance of <typeparamref name="T"/>
    /// </summary>
    public static Eff<VSlicesRuntime, Option<T>> provideOptional<T>()
        where T : notnull =>
        DependencyProviderExtensions<Eff<VSlicesRuntime>, VSlicesRuntime>.GetOptional<T>().As();

    /// <summary>
    /// Creates an <see cref="ExtensibleExpected"/> with a bad request status (400)
    /// </summary>
    /// <param name="message"></param>
    /// <param name="extensions"></param>
    /// <returns></returns>
    public static Error badRequest(string message, Dictionary<string, object?>? extensions = null)
        => BadRequest(message, extensions ?? []);

    /// <summary>
    /// Creates an <see cref="ExtensibleExpected"/> with an unauthenticated status (401)
    /// </summary>
    public static Error unauthenticated(string message, Dictionary<string, object?>? extensions = null)
        => Unauthenticated(message, extensions ?? []);

    /// <summary>
    /// Creates an <see cref="ExtensibleExpected"/> with an unauthenticated status (403)
    /// </summary>
    public static Error forbidden(string message, Dictionary<string, object?>? extensions = null)
        => Forbidden(message, extensions ?? []);

    /// <summary>
    /// Creates an <see cref="ExtensibleExpected"/> with a not found status (404)
    /// </summary>
    public static Error notFound(string message, Dictionary<string, object?>? extensions = null)
        => NotFound(message, extensions ?? []);

    /// <summary>
    /// Creates an <see cref="ExtensibleExpected"/> with a conflict status (409)
    /// </summary>
    public static Error conflict(string message, Dictionary<string, object?>? extensions = null)
        => Conflict(message, extensions ?? []);

    /// <summary>
    /// Creates an <see cref="ExtensibleExpected"/> with a gone status (410)
    /// </summary>
    public static Error gone(string message, Dictionary<string, object?>? extensions = null)
        => Gone(message, extensions ?? []);

    /// <summary>
    /// Creates an <see cref="ExtensibleExpected"/> with a gone status (418)
    /// </summary>
    public static Error iAmTeaPot(string message, Dictionary<string, object?>? extensions = null)
        => IAmTeapot(message, extensions ?? []);

    /// <summary>
    /// Creates an <see cref="ExtensibleExpected"/> with an unprocessable status (422)
    /// </summary>
    public static Error unprocessable(string message, 
                                      IEnumerable<ValidationDetail> errors, 
                                      Dictionary<string, object?>? extensions = null) 
        => Unprocessable(message, errors, extensions ?? []);

    /// <summary>
    /// Creates an <see cref="ExtensibleExpected"/> with a locked status (423)
    /// </summary>
    public static Error locked(string message, Dictionary<string, object?>? extensions = null)
        => Locked(message, extensions ?? []);

    /// <summary>
    /// Creates an <see cref="ExtensibleExpected"/> with a locked status (424)
    /// </summary>
    public static Error failedDependency(string message, Dictionary<string, object?>? extensions = null)
        => FailedDependency(message, extensions ?? []);

    /// <summary>
    /// Creates an <see cref="ExtensibleExpected"/> with a locked status (500)
    /// </summary>
    public static Error tooEarly(string message, Dictionary<string, object?>? extensions = null)
        => TooEarly(message, extensions ?? []);

    /// <summary>
    /// Creates an <see cref="ExtensibleExpected"/> with a locked status (500)
    /// </summary>
    public static Error serverError(string message, Dictionary<string, object?>? extensions = null)
        => ServerError(message, extensions ?? []);
}
