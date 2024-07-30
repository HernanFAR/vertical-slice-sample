using LanguageExt;
using LanguageExt.Common;
using Microsoft.Extensions.Logging;
using VSlices.Base;
using VSlices.Core;
using VSlices.Core.Stream;
using VSlices.CrossCutting.StreamPipeline.Logging.MessageTemplates;
using static LanguageExt.Prelude;
using static VSlices.VSlicesPrelude;

namespace VSlices.CrossCutting.StreamPipeline.Logging;

/// <summary>
/// Logging behavior for <see cref="IStreamHandler{TRequest,TResult}"/>
/// </summary>
/// <typeparam name="TRequest">Request to handle</typeparam>
/// <typeparam name="TResult">Expected result</typeparam>
public sealed class LoggingStreamBehavior<TRequest, TResult> : AbstractStreamPipelineBehavior<TRequest, TResult>
    where TRequest : IStream<TResult>
{
    /// <inheritdoc />
    protected override Eff<VSlicesRuntime, Unit> BeforeHandle(TRequest request) =>
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
    protected override Eff<VSlicesRuntime, IAsyncEnumerable<TResult>> AfterSuccessHandling(TRequest request, IAsyncEnumerable<TResult> result) =>
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
    protected override Eff<VSlicesRuntime, IAsyncEnumerable<TResult>> AfterFailureHandling(TRequest request, Error result) =>
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
                                                  request, 
                                                  result);
                              }

                              return unit;
                          })
        from result_ in liftEff<IAsyncEnumerable<TResult>>(() => result)
        select result_;
}
