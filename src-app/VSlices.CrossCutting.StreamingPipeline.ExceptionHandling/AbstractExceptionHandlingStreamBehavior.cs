using LanguageExt;
using VSlices.Base;
using VSlices.Core;
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
    protected override Eff<VSlicesRuntime, IAsyncEnumerable<TResult>> InHandle(
        TRequest request, Eff<VSlicesRuntime, IAsyncEnumerable<TResult>> next) =>
        from result in next | catchM(e => e.IsExceptional,
                                     e => Process(e.ToException(), request))
        select result;

    /// <summary>
    /// Processes the exception
    /// </summary>
    /// <remarks>You can add more specific logging, email sending, etc. here</remarks>
    /// <param name="ex">The throw exception</param>
    /// <param name="request">The related request information</param>
    /// <returns>A <see cref="ValueTask"/> representing the processing of the exception</returns>
    protected internal abstract Eff<VSlicesRuntime, IAsyncEnumerable<TResult>> Process(Exception ex, TRequest request);
}
