using LanguageExt;

namespace VSlices.Core.UseCases;

/// <summary>
/// Represents an asynchronous runner for <see cref="IRequest{T}"/>
/// </summary>
public interface IRequestRunner
{
    /// <summary>
    /// Asynchronously runs the <see cref="IHandler{TRequest,TResult}"/> effect associated to
    /// <see cref="IRequest{T}" />, using a specified runtime.
    /// </summary>
    /// <typeparam name="TResult">Expected response type</typeparam>
    /// <param name="request">Request to handle</param>
    /// <param name="runtime">Runtime of the handler</param>
    /// <returns>
    /// A <see cref="Fin{T}"/> that represents an asynchronous lazy operation, which returns a
    /// <typeparamref name="TResult"/>.
    /// </returns>
    Fin<TResult> Run<TResult>(IRequest<TResult> request, HandlerRuntime runtime);

    /// <summary>
    /// Asynchronously runs the <see cref="IHandler{TRequest,TResult}"/> effect associated to
    /// <see cref="IRequest{T}" />, using generated runtime with the specified cancellation token.
    /// </summary>
    /// <typeparam name="TResult">Expected response type</typeparam>
    /// <param name="request">Request to handle</param>
    /// <param name="cancellationToken">Cancellation token to create a runtime of the handler</param>
    /// <returns>
    /// A <see cref="Fin{T}"/> that represents an asynchronous lazy operation, which returns a
    /// <typeparamref name="TResult"/>.
    /// </returns>
    Fin<TResult> Run<TResult>(IRequest<TResult> request, CancellationToken cancellationToken = default);

}
