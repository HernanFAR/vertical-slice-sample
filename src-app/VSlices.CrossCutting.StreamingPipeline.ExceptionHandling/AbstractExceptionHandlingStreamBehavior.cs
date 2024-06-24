using LanguageExt;
using VSlices.Base.Failures;
using VSlices.Core.Stream;
using static LanguageExt.Prelude;

namespace VSlices.CrossCutting.StreamPipeline.ExceptionHandling;

/// <summary>
/// Base exception handling behavior
/// </summary>
/// <typeparam name="TRequest">The intercepted request to handle</typeparam>
/// <typeparam name="TResult">The expected successful result</typeparam>
public abstract class AbstractExceptionHandlingStreamBehavior<TRequest, TResult> : AbstractStreamPipelineBehavior<TRequest, TResult>
    where TRequest : IStream<TResult>
{
    /// <inheritdoc />
    protected override Aff<IAsyncEnumerable<TResult>> InHandleAsync(TRequest request, Aff<IAsyncEnumerable<TResult>> next, CancellationToken cancellationToken)
    {
        return from result in next
                       | @catch(ex => ex.IsExceptional
                           ? ProcessExceptionAsync(ex, request)
                           : FailAff<IAsyncEnumerable<TResult>>(ex))
               select result;
    }

    /// <summary>
    /// Processes the exception
    /// </summary>
    /// <remarks>You can add more specific logging, email sending, etc. here</remarks>
    /// <param name="ex">The throw exception</param>
    /// <param name="request">The related request information</param>
    /// <param name="cancellationToken"></param>
    /// <returns>A <see cref="ValueTask"/> representing the processing of the exception</returns>
    protected internal virtual Aff<IAsyncEnumerable<TResult>> ProcessExceptionAsync(Exception ex, TRequest request, CancellationToken cancellationToken = default)
    {
        return FailAff<IAsyncEnumerable<TResult>>(new ServerError("Internal server error"));
    }
}
