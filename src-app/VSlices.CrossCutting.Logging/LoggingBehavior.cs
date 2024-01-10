using Microsoft.Extensions.Logging;
using VSlices.Base;
using VSlices.Base.Responses;
using VSlices.CrossCutting.Logging.Describers;

namespace VSlices.CrossCutting.Logging;

/// <summary>
/// Base logging behavior
/// </summary>
/// <remarks>Logs at start, successful end and failed end</remarks>
/// <typeparam name="TRequest">The intercepted request to log about</typeparam>
/// <typeparam name="TResponse">The expected successful response</typeparam>
public sealed class LoggingBehavior<TRequest, TResponse> : AbstractPipelineBehavior<TRequest, TResponse>
    where TRequest : IBaseRequest<TResponse>
{
    private readonly ILogger<TRequest> _logger;
    private readonly ILoggingDescriber _describer;

    /// <summary>
    /// Creates a new instance using the provided <see cref="ILogger{TRequest}"/> 
    /// </summary>
    /// <param name="logger">Logger</param>
    /// <param name="describer">Describer</param>
    public LoggingBehavior(ILogger<TRequest> logger, ILoggingDescriber describer)
    {
        _logger = logger;
        _describer = describer;
    }

    /// <inheritdoc />
    protected override ValueTask<Result<Success>> BeforeHandleAsync(TRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(_describer.Initial,
            DateTime.Now, typeof(TRequest).FullName, request);

        return base.BeforeHandleAsync(request, cancellationToken);
    }

    /// <inheritdoc />
    protected override ValueTask<Result<Success>> AfterHandleAsync(TRequest request, Result<TResponse> result, CancellationToken cancellationToken)
    {
        Action<Result<TResponse>> handler = result.IsSuccess ? SuccessHandling : FailureHandling;

        handler(result);

        return base.AfterHandleAsync(request, result, cancellationToken);
    }

    internal void SuccessHandling(Result<TResponse> response)
    {
        _logger.LogInformation(_describer.Success,
            DateTime.Now, typeof(TRequest).FullName, response.Data);
    }

    internal void FailureHandling(Result<TResponse> response)
    {
        _logger.LogWarning(_describer.Failure,
            DateTime.Now, typeof(TRequest).FullName, response.Failure);
    }
}
