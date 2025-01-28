using LanguageExt;
using VSlices.Base.Core;

namespace VSlices.Core.UseCases;

/// <summary>
/// Represents an asynchronous runner for <see cref="IInput{TOut}"/>
/// </summary>
public interface IRequestRunner
{
    /// <summary>
    /// Asynchronously runs the <see cref="IBehavior{TIn, TOut}"/> effect associated to
    /// <see cref="IInput{TOut}" />, using generated runtime with the specified cancellation token.
    /// </summary>
    /// <typeparam name="TOut">Expected response type</typeparam>
    /// <param name="input">Input to handle</param>
    /// <param name="cancellationToken">Cancellation token to create a runtime of the handler</param>
    /// <returns>
    /// A <see cref="Fin{T}"/> that represents an asynchronous lazy operation, which returns a
    /// <typeparamref name="TOut"/>.
    /// </returns>
    Fin<TOut> Run<TOut>(IInput<TOut> input, CancellationToken cancellationToken = default);

}
