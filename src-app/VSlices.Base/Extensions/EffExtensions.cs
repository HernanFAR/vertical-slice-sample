// ReSharper disable once CheckNamespace
namespace LanguageExt;

/// <summary>
/// <see cref="Eff{TRuntime, TResult}"/> extensions
/// </summary>
public static class EffExtensions
{
    /// <summary>
    /// Executes the effect, creating a <see cref="EnvIO"/> using the specified <see cref="CancellationToken"/>
    /// </summary>
    /// <typeparam name="TRuntime"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="eff"></param>
    /// <param name="runtime"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static Fin<TResult> Run<TRuntime, TResult>(this Eff<TRuntime, TResult> eff, 
                                                      TRuntime runtime, 
                                                      CancellationToken cancellationToken)
    {
        using var envIo = EnvIO.New(token: cancellationToken);

        return eff.Run(runtime, envIo);
    }
}
