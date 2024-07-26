using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSlices.Core.Traits;
using VSlices.Core;
// ReSharper disable CheckNamespace

namespace VSlices;

/// <summary>
/// Used to give rapid access to functions when defining monads
/// </summary>
public static class CorePrelude
{
    public static Eff<HandlerRuntime, T> provide<T>()
        where T : notnull =>
        DependencyProviderExtensions<Eff<HandlerRuntime>, HandlerRuntime>.Get<T>()
                                                     .As();

    public static Eff<HandlerRuntime, Option<T>> provideOptional<T>()
        where T : notnull =>
        DependencyProviderExtensions<Eff<HandlerRuntime>, HandlerRuntime>.GetOptional<T>()
                                                     .As();

}
