using Microsoft.Extensions.Logging;
using VSlices.Base.Core;
using VSlices.Core.UseCases;
using VSlices.CrossCutting.Interceptor.ExceptionHandling;

namespace Crud.CrossCutting.Pipelines;

public sealed class LoggingExceptionHandlerPipeline<TIn, TOut> : ExceptionHandlingInterceptor<TIn, TOut>
{
    protected override Eff<VSlicesRuntime, TOut> Process(Exception ex, TIn request) =>
        from logger in provide<ILogger<TIn>>()
        from result in liftEff<TOut>(() =>
        {
            logger.LogError(ex, "Hubo una excepción al momento de manejar {Request}.", request);

            return serverError("Error de servidor interno");
        })
        select result;
}
