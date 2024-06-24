using LanguageExt;
using LanguageExt.SysX.Live;
using VSlices.Core.Stream;

namespace VSlices.CrossCutting.StreamPipeline;

/// <summary>
/// A middleware behavior for a <see cref="IStreamHandler{TRequest,TResult}"/> <see cref="IStream{TResult}" />
/// </summary>
/// <typeparam name="TRequest">The request to intercept</typeparam>
/// <typeparam name="TResult">The expected result</typeparam>
public interface IStreamPipelineBehavior<in TRequest, TResult>
    where TRequest : IStream<TResult>
{
    /// <summary>
    /// A method that intercepts the pipeline
    /// </summary>
    /// <param name="request">The intercepted request</param>
    /// <param name="next">The next action in the pipeline</param>
    /// <returns>
    /// A <see cref="Aff{T}"/> that represents the operation in lazy evaluation, which returns a <typeparamref name="TResult"/>
    /// </returns>
    Aff<Runtime, IAsyncEnumerable<TResult>> Define(TRequest request, Aff<Runtime, IAsyncEnumerable<TResult>> next);
}
