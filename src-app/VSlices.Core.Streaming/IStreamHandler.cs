using VSlices.Base;

namespace VSlices.Core.Stream;

/// <summary>
/// Defines an asynchronous stream effect for a specific <see cref="IFeature{TResult}"/>
/// </summary>
/// <remarks>If idempotency is necessary, the handler itself must ensure it</remarks>
/// <typeparam name="TRequest">The request to handle</typeparam>
/// <typeparam name="TResult">The expected result of the handler</typeparam>
public interface IStreamHandler<in TRequest, TResult> : IHandler<TRequest, IAsyncEnumerable<TResult>>
    where TRequest : IStream<TResult> { }
