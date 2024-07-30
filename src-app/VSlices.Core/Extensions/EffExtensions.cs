// ReSharper disable once CheckNamespace
namespace LanguageExt;

public static class EffExtensions
{
    public static Fin<TResult> Run<TRuntime, TResult>(this Eff<TRuntime, TResult> eff, 
                                                      TRuntime runtime, 
                                                      CancellationToken cancellationToken)
    {
        using var envIo = EnvIO.New(token: cancellationToken);

        return eff.Run(runtime, envIo);
    }
}
