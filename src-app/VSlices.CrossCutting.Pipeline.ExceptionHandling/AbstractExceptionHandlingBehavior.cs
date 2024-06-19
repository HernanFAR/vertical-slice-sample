using LanguageExt;
using LanguageExt.Common;
using static LanguageExt.Prelude;
using VSlices.Base;
using VSlices.Base.Failures;

namespace VSlices.CrossCutting.Pipeline.ExceptionHandling;

/// <summary>
/// Base exception handling behavior
/// </summary>
/// <typeparam name="TRequest">The intercepted request to handle</typeparam>
/// <typeparam name="TResult">The expected successful result</typeparam>
public abstract class AbstractExceptionHandlingBehavior<TRequest, TResult> : AbstractPipelineBehavior<TRequest, TResult>
    where TRequest : IFeature<TResult>
{
    /// <inheritdoc />
    protected override Aff<TResult> InHandleAsync(TRequest request, Aff<TResult> next, CancellationToken cancellationToken) =>
        from result in next | 
                       @catch(ex => ex.IsExceptional 
                           ? ProcessExceptionAsync(ex, request) 
                           : FailAff<TResult>(ex))
        select result;  

    /// <summary>
    /// Processes the exception
    /// </summary>
    /// <remarks>You can add more specific logging, email sending, etc. here</remarks>
    /// <param name="ex">The throw exception</param>
    /// <param name="request">The related request information</param>
    /// <returns>A <see cref="ValueTask"/> representing the processing of the exception</returns>
    protected internal virtual Aff<TResult> ProcessExceptionAsync(Exception ex, TRequest request, CancellationToken cancellationToken = default)
        => FailAff<TResult>(new ServerError("Internal server error"));

}
