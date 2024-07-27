using Microsoft.Extensions.Logging;
using VSlices.Base.Failures;
using VSlices.Core;
using VSlices.Core.Stream;
using VSlices.CrossCutting.StreamPipeline.ExceptionHandling;

namespace Crud.CrossCutting.Pipelines;

public sealed class LoggingExceptionHandlerStreamPipeline<TRequest, TResult>
    : AbstractExceptionHandlingStreamBehavior<TRequest, TResult>
    where TRequest : IStream<TResult>
{
    protected override Eff<HandlerRuntime, IAsyncEnumerable<TResult>> Process(Exception ex, TRequest request) =>
        from logger in provide<ILogger<TRequest>>()
        from result in liftEff<IAsyncEnumerable<TResult>>(() =>
        {
            logger.LogError(ex, "Hubo una excepción al momento de manejar {Request}.", request);

            return new ServerError("Error de servidor interno").AsError();
        })
        select result;
}
