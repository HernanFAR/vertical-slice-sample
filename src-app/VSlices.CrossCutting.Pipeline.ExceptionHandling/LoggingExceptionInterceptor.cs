using LanguageExt;
using Microsoft.Extensions.Logging;
using VSlices.Base;
using VSlices.CrossCutting.Interceptor.ExceptionHandling.MessageTemplates;
using static VSlices.VSlicesPrelude;
using static LanguageExt.Prelude;

namespace VSlices.CrossCutting.Interceptor.ExceptionHandling;

/// <summary>
/// Base exception handling behavior
/// </summary>
/// <typeparam name="TIn">The intercepted input</typeparam>
/// <typeparam name="TOut">The expected result</typeparam>
public sealed class LoggingExceptionInterceptor<TIn, TOut> : ExceptionHandlingInterceptor<TIn, TOut>
{
    /// <inheritdoc />
    protected internal override Eff<VSlicesRuntime, TOut> Process(Exception ex, TIn request) =>
        from logger in provide<ILogger<TIn>>()
        from template in provide<IExceptionMessageTemplate>()
        from time in provide<TimeProvider>()
        from result in liftEff<TOut>(() =>
        {
            logger.LogError(ex, 
                            template.LogException,
                            time.GetUtcNow(), 
                            typeof(TIn).FullName, 
                            request);

            return serverError(template.ErrorMessage);
        })
        select result;
}
