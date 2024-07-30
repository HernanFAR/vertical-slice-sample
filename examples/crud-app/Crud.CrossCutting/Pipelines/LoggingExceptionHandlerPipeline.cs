using Microsoft.Extensions.Logging;
using VSlices.Base;
using VSlices.Base.Failures;
using VSlices.Core;
using VSlices.CrossCutting.Pipeline.ExceptionHandling;

namespace Crud.CrossCutting.Pipelines;

public sealed class LoggingExceptionHandlerPipeline<TRequest, TResult> : AbstractExceptionHandlingBehavior<TRequest, TResult>
    where TRequest : IFeature<TResult>
{
    protected override Eff<VSlicesRuntime, TResult> Process(Exception ex, TRequest request) =>
        from logger in provide<ILogger<TRequest>>()
        from result in liftEff<TResult>(() =>
        {
            logger.LogError(ex, "Hubo una excepción al momento de manejar {Request}.", request);

            return new ServerError("Error de servidor interno").AsError();
        })
        select result;
}
