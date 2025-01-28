namespace VSlices.Base.Core;


/// <summary>
/// An asynchronous behavior associated to a given input and output
/// </summary>
/// <remarks>The behavior itself must ensure idempotency, if required</remarks>
/// <typeparam name="TIn">The required input of the behavior</typeparam>
/// <typeparam name="TOut">The expected output of the behavior</typeparam>
public interface IBehavior<in TIn, TOut>
{
    /// <summary>
    /// Defines a behavior to process a given input and output
    /// </summary>
    /// <param name="input">The input to process</param>
    /// <returns>
    /// An <see cref="LanguageExt.Eff{TRuntime, TResult}"/> that represents the operation in lazy evaluation, which when ran
    /// returns a <typeparamref name="TOut"/>
    /// </returns>
    Eff<VSlicesRuntime, TOut> Define(TIn input);
}

/// <summary>
/// Asynchronous effect for a given input and output
/// </summary>
/// <typeparam name="TIn">The request to handle</typeparam>
public interface IBehavior<in TIn> : IBehavior<TIn, Unit>;