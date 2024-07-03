using LanguageExt;
using LanguageExt.Common;
using LanguageExt.SysX.Live;
using Microsoft.Extensions.Logging;
using VSlices.Core.Stream;
using VSlices.CrossCutting.StreamPipeline.Logging.MessageTemplates;
using static LanguageExt.Prelude;

namespace VSlices.CrossCutting.StreamPipeline.Logging;

/// <summary>
/// Logging behavior for <see cref="IStreamHandler{TRequest,TResult}"/>
/// </summary>
/// <typeparam name="TRequest">Request to handle</typeparam>
/// <typeparam name="TResult">Expected result</typeparam>
/// <param name="messageTemplate">Message templates</param>
/// <param name="logger">Logger</param>
/// <param name="timeProvider">Time provider</param>
public sealed class LoggingStreamBehavior<TRequest, TResult>(
    ILoggingMessageTemplate messageTemplate,
    ILogger<TRequest> logger,
    TimeProvider timeProvider)
    : AbstractStreamPipelineBehavior<TRequest, TResult>
    where TRequest : IStream<TResult>
{
    private readonly ILoggingMessageTemplate _messageTemplate = messageTemplate;
    private readonly ILogger<TRequest> _logger = logger;
    private readonly TimeProvider _timeProvider = timeProvider;

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
    protected override Aff<Runtime, IAsyncEnumerable<TResult>> AfterSuccessHandling(TRequest request, IAsyncEnumerable<TResult> result) =>
        from _ in Eff(() =>
        {
            _logger.LogInformation(_messageTemplate.SuccessEnd,
                _timeProvider.GetUtcNow(), typeof(TRequest).FullName, request, result);

            return unit;
        })
        from result_ in SuccessEff(result)
        select result_;

    /// <inheritdoc />
    protected override Aff<Runtime, IAsyncEnumerable<TResult>> AfterFailureHandling(TRequest request, Error result) =>
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
        from result_ in EffMaybe<IAsyncEnumerable<TResult>>(() => result)
        select result_;
}
