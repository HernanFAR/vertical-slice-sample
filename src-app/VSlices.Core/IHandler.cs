using LanguageExt;
using VSlices.Base;

namespace VSlices.Core;

/// <summary>
/// Defines a asyncronous effect for a specific <see cref="IFeature{TResult}"/>
/// </summary>
/// <remarks>If idempotency is necessary, the handler itself must ensure it</remarks>
/// <typeparam name="TRequest">The request to be handled</typeparam>
/// <typeparam name="TResult">The expected result of the handler</typeparam>
public interface IHandler<in TRequest, TResult>
    where TRequest : IFeature<TResult>
{
    /// <summary>
    /// Defines the asyncronous effect for a <see cref="IFeature{TResult}"/>
    /// </summary>
    /// <param name="request">The request to be handled</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>
    /// An <see cref="Aff{T}"/> that represents the operation in lazy evaluation, which when runned
    /// returns a <typeparamref name="TResult"/>
    /// </returns>
    Aff<TResult> Define(TRequest request, CancellationToken cancellationToken = default);
}

/// <summary>
/// Defines a handler for a <see cref="IFeature{TResult}"/>
/// </summary>
/// <typeparam name="TRequest">The request to be handled</typeparam>
public interface IHandler<in TRequest> : IHandler<TRequest, Unit>
    where TRequest : IFeature<Unit>
{ }
