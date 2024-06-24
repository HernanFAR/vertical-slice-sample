using LanguageExt.SysX.Live;
using Microsoft.Extensions.Logging;
using VSlices.Base;
using VSlices.Base.Failures;
using VSlices.CrossCutting.Pipeline.ExceptionHandling;

namespace Crud.CrossCutting.Pipelines;

public sealed class LoggingExceptionHandlerPipeline<TRequest, TResult>(ILogger<TResult> logger)
    : AbstractExceptionHandlingBehavior<TRequest, TResult>
    where TRequest : IFeature<TResult>
{
    readonly ILogger<TResult> _logger = logger;

    protected override Aff<Runtime, TResult> Process(Exception ex, TRequest request) =>
        from result in EffMaybe<TResult>(() =>
        {
            _logger.LogError(ex, "Hubo una excepción al momento de manejar {Request}.", request);

            return new ServerError("Error de servidor interno").AsError();
        })
        select result;
}
