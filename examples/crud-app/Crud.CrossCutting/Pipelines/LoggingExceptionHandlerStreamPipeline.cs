using LanguageExt.SysX.Live;
using Microsoft.Extensions.Logging;
using VSlices.Base.Failures;
using VSlices.Core.Stream;
using VSlices.CrossCutting.StreamPipeline.ExceptionHandling;

namespace Crud.CrossCutting.Pipelines;

public sealed class LoggingExceptionHandlerStreamPipeline<TRequest, TResult>(ILogger<TResult> logger)
    : AbstractExceptionHandlingStreamBehavior<TRequest, TResult>
    where TRequest : IStream<TResult>
{
    readonly ILogger<TResult> _logger = logger;

    protected override Aff<Runtime, IAsyncEnumerable<TResult>> Process(Exception ex, TRequest request) =>
        from result in EffMaybe<IAsyncEnumerable<TResult>>(() =>
        {
            _logger.LogError(ex, "Hubo una excepción al momento de manejar {Request}.", request);

            return new ServerError("Error de servidor interno").AsError();
        })
        select result;
}
