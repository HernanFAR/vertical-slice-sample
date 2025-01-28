using LanguageExt;
using VSlices.Base;
using VSlices.Base.CrossCutting;
using static LanguageExt.Prelude;

namespace VSlices.CrossCutting.Interceptor.ExceptionHandling;

/// <summary>
/// Base exception handling behavior
/// </summary>
/// <typeparam name="TIn">The intercepted input</typeparam>
/// <typeparam name="TOut">The expected result</typeparam>
public abstract class ExceptionHandlingInterceptor<TIn, TOut> : AbstractBehaviorInterceptor<TIn, TOut>
{
    /// <inheritdoc />
    protected internal override Eff<VSlicesRuntime, TOut> InHandle(TIn request, Eff<VSlicesRuntime, TOut> next) =>
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
    protected internal abstract Eff<VSlicesRuntime, TOut> Process(Exception ex, TIn request);

}
