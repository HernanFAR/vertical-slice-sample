using LanguageExt;
using Microsoft.Extensions.Logging;
using static LanguageExt.Prelude;
using static VSlices.VSlicesPrelude;
using VSlices.Base;
using VSlices.CrossCutting.Pipeline.ExceptionHandling.MessageTemplates;
using VSlices.Base.Core;

namespace VSlices.CrossCutting.Pipeline.ExceptionHandling;

/// <summary>
/// Base exception handling behavior
/// </summary>
/// <typeparam name="TRequest">The intercepted request to handle</typeparam>
/// <typeparam name="TResult">The expected successful result</typeparam>
public sealed class LoggingExceptionBehavior<TRequest, TResult> : ExceptionHandlingBehavior<TRequest, TResult>
    where TRequest : IFeature<TResult>
{
    /// <inheritdoc />
    protected internal override Eff<VSlicesRuntime, TResult> Process(Exception ex, TRequest request) =>
        from logger in provide<ILogger<TRequest>>()
        from template in provide<IExceptionMessageTemplate>()
        from time in provide<TimeProvider>()
        from result in liftEff<TResult>(() =>
        {
            logger.LogError(ex, 
                            template.LogException,
                            time.GetUtcNow(), 
                            typeof(TRequest).FullName, 
                            request);

            return serverError(template.ErrorMessage);
        })
        select result;
}
