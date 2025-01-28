using LanguageExt;
using Microsoft.Extensions.Logging;
using VSlices.Base;
using VSlices.Base.Core;
using VSlices.Base.CrossCutting;
using VSlices.CrossCutting.Interceptor.Filtering.MessageTemplates;
using VSlices.Domain.Interfaces;

namespace VSlices.CrossCutting.Interceptor.Filtering;

/// <summary>
/// A filtering behavior using a custom logic
/// </summary>
public sealed class FilteringBehaviorInterceptor<TRequest, TFilter, THandler> : IBehaviorInterceptor<TRequest, Unit>
    where TRequest : IEvent
    where TFilter : IEventFilter<TRequest, THandler>
    where THandler : IBehavior<TRequest>
{
    /// <inheritdoc />
    public Eff<VSlicesRuntime, Unit> Define(TRequest request, Eff<VSlicesRuntime, Unit> next) =>
        from eventFilter in VSlicesPrelude.provide<TFilter>()
        from template in VSlicesPrelude.provide<IEventFilteringMessageTemplate>()
        from logger in VSlicesPrelude.provide<ILogger<TRequest>>()
        from timeProvider in VSlicesPrelude.provide<TimeProvider>()
        from _ in eventFilter.DefineFilter(request)
                             .Bind(c =>
                             {
                                 if (c is false)
                                 {
                                     logger.LogWarning(template.SkipExecution,
                                                       timeProvider.GetUtcNow(), typeof(TRequest).FullName, request);

                                     return Prelude.unitEff;
                                 }

                                 logger.LogInformation(template.ContinueExecution,
                                                       timeProvider.GetUtcNow(), typeof(TRequest).FullName, request);

                                 return next;
                             })
        select Prelude.unit;
}