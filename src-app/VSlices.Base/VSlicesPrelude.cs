using VSlices.Base;
using VSlices.Base.Traits;

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
}
