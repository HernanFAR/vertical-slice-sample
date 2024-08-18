using LanguageExt;
using static LanguageExt.Prelude;
using VSlices.Base;
using VSlices.Core;
using VSlices.Base.Core;
using VSlices.Base.CrossCutting;

namespace VSlices.CrossCutting.Pipeline.ExceptionHandling;

/// <summary>
/// Base exception handling behavior
/// </summary>
/// <typeparam name="TRequest">The intercepted request to handle</typeparam>
/// <typeparam name="TResult">The expected successful result</typeparam>
public abstract class ExceptionHandlingBehavior<TRequest, TResult> : AbstractPipelineBehavior<TRequest, TResult>
    where TRequest : IFeature<TResult>
{
    /// <inheritdoc />
    protected internal override Eff<VSlicesRuntime, TResult> InHandle(TRequest request, Eff<VSlicesRuntime, TResult> next) =>
        from result in next | @catch(e => e.IsExceptional, 
                                     e => Process(e.ToException(), request))
        select result;

    /// <summary>
    /// Processes the exception
    /// </summary>
    /// <remarks>You can add more specific logging, email sending, etc. here</remarks>
    /// <param name="ex">The throw exception</param>
    /// <param name="request">The related request information</param>
    /// <returns>A <see cref="ValueTask"/> representing the processing of the exception</returns>
    protected internal abstract Eff<VSlicesRuntime, TResult> Process(Exception ex, TRequest request);

}
