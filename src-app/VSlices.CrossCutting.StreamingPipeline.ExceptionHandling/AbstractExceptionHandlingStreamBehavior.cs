using LanguageExt;
using LanguageExt.SysX.Live;
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
    protected override Aff<Runtime, IAsyncEnumerable<TResult>> InHandle(TRequest request, Aff<Runtime, IAsyncEnumerable<TResult>> next)
    {
        return from result in next
                       | @catch(ex => ex.IsExceptional
                           ? Process(ex, request)
                           : FailAff<Runtime, IAsyncEnumerable<TResult>>(ex))
               select result;
    }

    /// <summary>
    /// Processes the exception
    /// </summary>
    /// <remarks>You can add more specific logging, email sending, etc. here</remarks>
    /// <param name="ex">The throw exception</param>
    /// <param name="request">The related request information</param>
    /// <returns>A <see cref="ValueTask"/> representing the processing of the exception</returns>
    protected internal abstract Aff<Runtime, IAsyncEnumerable<TResult>> Process(Exception ex, TRequest request);
}
