using Microsoft.Extensions.DependencyInjection;
using VSlices.Base;
using VSlices.Base.Builder;
using VSlices.Core.Events;
using VSlices.Domain.Interfaces;

// ReSharper disable once CheckNamespace
namespace VSlices.Core.Builder;

/// <summary>
/// <see cref="FeatureDefinition{TFeature,TResult}" /> extensions to simplify <see cref="IEvent" />'s 
/// <see cref="IEventHandler{TEvent}" /> definition
/// </summary>
public static class EventHandlerExtensions
{
    /// <summary>
    /// Adds <typeparamref name="T"/> as <see cref="IEventHandler{TRequest}"/> to the service collection.
    /// </summary>
    public static FeatureDefinition<,> AddEventHandler<T>(this FeatureDefinition<,> featureDefinition)
        => featureDefinition.AddEventHandler(typeof(T));

    /// <summary>
    /// Adds a the specified <see cref="Type"/> as <see cref="IEventHandler{TRequest}"/> to the service collection.
    /// </summary>
    public static FeatureDefinition<,> AddEventHandler(this FeatureDefinition<,> featureDefinition, Type handlerType)
    {
        var handlerInterface = handlerType.GetInterfaces()
            .Where(o => o.IsGenericType)
            .SingleOrDefault(o => o.GetGenericTypeDefinition() == typeof(IEventHandler<>))
            ?? throw new InvalidOperationException(
                $"The type {handlerType.FullName} does not implement {typeof(IEventHandler<>).FullName}");

        featureDefinition.Services.AddTransient(handlerInterface, handlerType);

        return featureDefinition;

    }
}

/// <summary>
/// Wrapper for <see cref="FeatureDefinition{TFeature,TResult}"/> that manages pipeline execution of <see cref="IEventHandler{TRequest}"/>
/// </summary>
/// <param name="featureDefinitionDefinition"></param>
public sealed class EventHandlerBuilder(FeatureDefinition<,> featureDefinitionDefinition)
{
    private readonly FeatureDefinition<,> _featureDefinitionDefinition = featureDefinitionDefinition;

    public FeatureDefinition<,> FeatureDefinition => _featureDefinitionDefinition;
}
