using VSlices.Base;
using VSlices.Base.Responses;

namespace VSlices.CrossCutting.ExceptionHandling;

/// <summary>
/// Base exception handling behavior
/// </summary>
/// <typeparam name="TRequest">The intercepted request to handle</typeparam>
/// <typeparam name="TResult">The expected successful result</typeparam>
public abstract class AbstractExceptionHandlingBehavior<TRequest, TResult> : AbstractPipelineBehavior<TRequest, TResult>
    where TRequest : IFeature<TResult>
{
    /// <inheritdoc />
    protected override async ValueTask<Result<TResult>> InHandleAsync(TRequest request, RequestHandlerDelegate<TResult> next, CancellationToken cancellationToken)
    {
        try
        {
            return await next();
        }
        catch (Exception ex)
        {
            await ProcessExceptionAsync(ex, request);

            return new Failure(FailureKind.UnhandledException);
        }
    }

    /// <summary>
    /// Processes the exception
    /// </summary>
    /// <remarks>You can add more specific logging, email sending, etc. here</remarks>
    /// <param name="ex">The throw exception</param>
    /// <param name="request">The related request information</param>
    /// <returns>A <see cref="ValueTask"/> representing the processing of the exception</returns>
    protected internal abstract ValueTask ProcessExceptionAsync(Exception ex, TRequest request);

}
