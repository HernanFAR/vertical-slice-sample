using LanguageExt;
using LanguageExt.Common;
using Microsoft.Extensions.Logging;
using VSlices.Base;
using VSlices.Core;
using VSlices.CrossCutting.Pipeline.Logging.MessageTemplates;
using static LanguageExt.Prelude;
using static VSlices.CorePrelude;

namespace VSlices.CrossCutting.Pipeline.Logging;

/// <summary>
/// Logging behavior for <see cref="IHandler{TRequest, TResult}"/>
/// </summary>
/// <typeparam name="TRequest">Request to handle</typeparam>
/// <typeparam name="TResult">Expected result</typeparam>
public sealed class LoggingBehavior<TRequest, TResult> : AbstractPipelineBehavior<TRequest, TResult>
    where TRequest : IFeature<TResult>
{
    /// <inheritdoc />
    protected override Eff<HandlerRuntime, Unit> BeforeHandle(TRequest request) =>
        from logger in provide<ILogger<TRequest>>()
        from template in provide<ILoggingMessageTemplate>()
        from time in provide<TimeProvider>()
        from _ in liftEff(() =>
        {
            logger.LogInformation(template.Start, 
                                  time.GetUtcNow(), 
                                  typeof(TRequest).FullName, 
                                  request);

            return unit;
        })
        select unit;

    /// <inheritdoc />
    protected override Eff<HandlerRuntime, TResult> AfterSuccessHandling(TRequest request, TResult result) =>
        from logger in provide<ILogger<TRequest>>()
        from template in provide<ILoggingMessageTemplate>()
        from time in provide<TimeProvider>()
        from _ in liftEff(() =>
                          {
                              logger.LogInformation(template.SuccessEnd,
                                                    time.GetUtcNow(), 
                                                    typeof(TRequest).FullName, 
                                                    request, result);

                              return unit;
                          })
        from result_ in SuccessEff(result)
        select result_;

    /// <inheritdoc />
    protected override Eff<HandlerRuntime, TResult> AfterFailureHandling(TRequest request, Error result) =>
        from logger in provide<ILogger<TRequest>>()
        from template in provide<ILoggingMessageTemplate>()
        from time in provide<TimeProvider>()
        from _ in liftEff(() =>
                          {
                              if (result.IsExpected)
                              {
                                  logger.LogWarning(template.FailureEnd,
                                                    time.GetUtcNow(),
                                                    typeof(TRequest).FullName, 
                                                    request, result);
                              }
                              else
                              {
                                  logger.LogError(template.FailureEnd,
                                                  time.GetUtcNow(),
                                                  typeof(TRequest).FullName,
                                                  request, result);
                              }

                              return unit;
                          })
        from result_ in liftEff<TResult>(() => result)
        select result_;
}
