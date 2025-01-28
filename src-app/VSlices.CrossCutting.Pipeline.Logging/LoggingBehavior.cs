using LanguageExt;
using LanguageExt.Common;
using Microsoft.Extensions.Logging;
using VSlices.Base;
using VSlices.Base.CrossCutting;
using VSlices.CrossCutting.Interceptor.Logging.MessageTemplates;
using static LanguageExt.Prelude;
using static VSlices.VSlicesPrelude;

namespace VSlices.CrossCutting.Interceptor.Logging;

/// <summary>
/// Logging behavior for <see cref="IBehaviorInterceptor{TInput,TOutput}"/>
/// </summary>
/// <typeparam name="TIn">The intercepted input</typeparam>
/// <typeparam name="TOut">The expected result</typeparam>
public sealed class LoggingInterceptor<TIn, TOut> : AbstractBehaviorInterceptor<TIn, TOut>
{
    /// <inheritdoc />
    protected internal override Eff<VSlicesRuntime, Unit> BeforeHandle(TIn request) =>
        from logger in provide<ILogger<TIn>>()
        from template in provide<ILoggingMessageTemplate>()
        from time in provide<TimeProvider>()
        from _ in liftEff(() =>
        {
            logger.LogInformation(template.Start, 
                                  time.GetUtcNow(), 
                                  typeof(TIn).FullName, 
                                  request);

            return unit;
        })
        select unit;

    /// <inheritdoc />
    protected internal override Eff<VSlicesRuntime, TOut> AfterSuccessHandling(TIn request, TOut result) =>
        from logger in provide<ILogger<TIn>>()
        from template in provide<ILoggingMessageTemplate>()
        from time in provide<TimeProvider>()
        from _ in liftEff(() =>
        {
            logger.LogInformation(template.SuccessEnd,
                                  time.GetUtcNow(),
                                  typeof(TIn).FullName,
                                  request,
                                  result);

            return unit;
        })
        from result_ in SuccessEff(result)
        select result_;

    /// <inheritdoc />
    protected internal override Eff<VSlicesRuntime, TOut> AfterFailureHandling(TIn request, Error result) =>
        from logger in provide<ILogger<TIn>>()
        from template in provide<ILoggingMessageTemplate>()
        from time in provide<TimeProvider>()
        from _ in liftEff(() =>
                          {
                              if (result.IsExpected)
                              {
                                  logger.LogWarning(template.FailureEnd,
                                                    time.GetUtcNow(),
                                                    typeof(TIn).FullName, 
                                                    request, result);
                              }
                              else
                              {
                                  logger.LogError(template.FailureEnd,
                                                  time.GetUtcNow(),
                                                  typeof(TIn).FullName,
                                                  request, result);
                              }

                              return unit;
                          })
        from result_ in liftEff<TOut>(() => result)
        select result_;
}
