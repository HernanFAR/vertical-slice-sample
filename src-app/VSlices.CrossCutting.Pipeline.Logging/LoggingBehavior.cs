using LanguageExt;
using LanguageExt.Common;
using static LanguageExt.Prelude;
using LanguageExt.SysX.Live;
using Microsoft.Extensions.Logging;
using VSlices.Base;
using VSlices.Core;
using VSlices.CrossCutting.Pipeline.Logging.MessageTemplates;

namespace VSlices.CrossCutting.Pipeline.Logging;

/// <summary>
/// Logging behavior for <see cref="IHandler{TRequest, TResult}"/>
/// </summary>
/// <typeparam name="TRequest">Request to handle</typeparam>
/// <typeparam name="TResult">Expected result</typeparam>
/// <param name="messageTemplate">Message templates</param>
/// <param name="logger">Logger</param>
/// <param name="timeProvider">Time provider</param>
public sealed class LoggingBehavior<TRequest, TResult>(
    ILoggingMessageTemplate messageTemplate,
    ILogger<TRequest> logger,
    TimeProvider timeProvider)
    : AbstractPipelineBehavior<TRequest, TResult>
    where TRequest : IFeature<TResult>
{
    readonly ILoggingMessageTemplate _messageTemplate = messageTemplate;
    readonly ILogger<TRequest> _logger = logger;
    readonly TimeProvider _timeProvider = timeProvider;

    /// <inheritdoc />
    protected override Aff<Runtime, Unit> BeforeHandle(TRequest request) =>
        from _ in Eff(() =>
        {
            _logger.LogInformation(_messageTemplate.Start, 
                _timeProvider.GetUtcNow(), typeof(TRequest).FullName, request);

            return unit;
        })
        select unit;

    /// <inheritdoc />
    protected override Aff<Runtime, TResult> AfterSuccessHandling(TRequest request, TResult result) =>
        from _ in Eff(() =>
        {
            _logger.LogInformation(_messageTemplate.SuccessEnd,
                _timeProvider.GetUtcNow(), typeof(TRequest).FullName, request, result);

            return unit;
        })
        from result_ in SuccessEff(result)
        select result_;

    /// <inheritdoc />
    protected override Aff<Runtime, TResult> AfterFailureHandling(TRequest request, Error result) =>
        from _ in Eff(() =>
        {
            if (result.IsExpected)
            {
                _logger.LogWarning(_messageTemplate.FailureEnd, 
                    _timeProvider.GetUtcNow(), typeof(TRequest).FullName, request, result);
            }
            else
            {
                _logger.LogError(_messageTemplate.FailureEnd,
                    _timeProvider.GetUtcNow(), typeof(TRequest).FullName, request, result);
            }

            return unit;
        })
        from result_ in EffMaybe<TResult>(() => result)
        select result_;  
}
