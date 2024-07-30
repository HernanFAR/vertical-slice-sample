using VSlices.Base;

namespace VSlices.Core;

/// <summary>
/// Defines asynchronous effect for a specific <see cref="IFeature{TResult}"/>
/// </summary>
/// <remarks>If idempotency is necessary, the handler itself must ensure it</remarks>
/// <typeparam name="TRequest">The request to handle</typeparam>
/// <typeparam name="TResult">The expected result of the handler</typeparam>
public interface IHandler<in TRequest, TResult>
    where TRequest : IFeature<TResult>
{
    /// <summary>
    /// Defines the asynchronous effect for a <see cref="IFeature{TResult}"/>
    /// </summary>
    /// <param name="request">The request to handle</param>
    /// <returns>
    /// An <see cref="LanguageExt.Eff{TRuntime, TResult}"/> that represents the operation in lazy evaluation, which when ran
    /// returns a <typeparamref name="TResult"/>
    /// </returns>
    Eff<VSlicesRuntime, TResult> Define(TRequest request);
}

/// <summary>
/// Defines a handler for a <see cref="IFeature{TResult}"/>
/// </summary>
/// <typeparam name="TRequest">The request to handle</typeparam>
public interface IHandler<in TRequest> : IHandler<TRequest, Unit>
    where TRequest : IFeature<Unit>;
