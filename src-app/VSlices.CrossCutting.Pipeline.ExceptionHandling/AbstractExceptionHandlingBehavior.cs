using LanguageExt;
using LanguageExt.Common;
using LanguageExt.SysX.Live;
using static LanguageExt.Prelude;
using VSlices.Base;
using VSlices.Base.Failures;

namespace VSlices.CrossCutting.Pipeline.ExceptionHandling;

/// <summary>
/// Base exception handling behavior
/// </summary>
/// <typeparam name="TRequest">The intercepted request to handle</typeparam>
/// <typeparam name="TResult">The expected successful result</typeparam>
public abstract class AbstractExceptionHandlingBehavior<TRequest, TResult> : AbstractPipelineBehavior<TRequest, TResult>
    where TRequest : IFeature<TResult>
{
    /// <inheritdoc />
    protected override Aff<Runtime, TResult> InHandle(TRequest request, Aff<Runtime, TResult> next) =>
        from result in next
                       | @catch(ex => ex.IsExceptional
                           ? Process(ex, request)
                           : FailAff<TResult>(ex))
        select result;

    /// <summary>
    /// Processes the exception
    /// </summary>
    /// <remarks>You can add more specific logging, email sending, etc. here</remarks>
    /// <param name="ex">The throw exception</param>
    /// <param name="request">The related request information</param>
    /// <returns>A <see cref="ValueTask"/> representing the processing of the exception</returns>
    protected internal abstract Aff<Runtime, TResult> Process(Exception ex, TRequest request);

}
