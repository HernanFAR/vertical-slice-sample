using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VSlices.Base;
using VSlices.CrossCutting.Pipeline;
using VSlices.CrossCutting.Pipeline.Logging;
using VSlices.CrossCutting.Pipeline.Logging.MessageTemplates;

// ReSharper disable once CheckNamespace
namespace VSlices.Core.Builder;

/// <summary>
/// <see cref="FeatureBuilder"/> extensions for <see cref="LoggingBehavior{TRequest,TResult}"/>
/// </summary>
public static class LoggingBehaviorExtensions
{
    /// <summary>
    /// Adds an open generic pipeline behavior to the service collection
    /// </summary>
    /// <param name="featureBuilder">Service Collection</param>
    /// <returns>Service Collection</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static LoggingBehaviorBuilder AddLoggingFor<T>(this FeatureBuilder featureBuilder)
        where T : IFeature
        => featureBuilder.AddLoggingFor(typeof(T));
    
    /// <summary>
    /// Adds an open generic pipeline behavior to the service collection
    /// </summary>
    /// <param name="featureBuilder">Service Collection</param>
    /// <param name="feature">Behavior</param>
    /// <returns>Service Collection</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static LoggingBehaviorBuilder AddLoggingFor(this FeatureBuilder featureBuilder, Type feature)
    {
        Type featureInterface = feature
            .GetInterfaces()
            .Where(o => o.IsGenericType)
            .SingleOrDefault(o => o.GetGenericTypeDefinition() == typeof(IFeature<>))
            ?? throw new InvalidOperationException(
                $"The type {feature.FullName} does not implement {typeof(IFeature<>).FullName}");

        Type featureResultType = featureInterface.GetGenericArguments()[0];

        Type loggingBehaviorType = typeof(LoggingBehavior<,>).MakeGenericType(feature, featureResultType);
        Type pipelineBehaviorType = typeof(IPipelineBehavior<,>).MakeGenericType(feature, featureResultType);

        featureBuilder.Services
            .AddTransient(pipelineBehaviorType, loggingBehaviorType)
            .TryAddSingleton(TimeProvider.System);

        return new LoggingBehaviorBuilder(featureBuilder);

    }
}

/// <summary>
/// Builder for <see cref="LoggingBehavior{TRequest,TResult}"/>
/// </summary>
/// <param name="builder"></param>
public sealed class LoggingBehaviorBuilder(FeatureBuilder builder)
{
    readonly FeatureBuilder _builder = builder;

    /// <summary>
    /// Add a custom <see cref="ILoggingMessageTemplate"/>
    /// </summary>
    public FeatureBuilder UsingTemplate<TMessageTemplate>()
        where TMessageTemplate : class, ILoggingMessageTemplate
    {
        _builder.Services.AddSingleton<ILoggingMessageTemplate, TMessageTemplate>();

        return _builder;
    }

    /// <summary>
    /// Add a english <see cref="ILoggingMessageTemplate"/>
    /// </summary>
    public FeatureBuilder UsingEnglishTemplate()
    {
        _builder.Services.AddSingleton<ILoggingMessageTemplate, EnglishLoggingMessageTemplate>();

        return _builder;
    }

    /// <summary>
    /// Add a spanish <see cref="ILoggingMessageTemplate"/>
    /// </summary>
    public FeatureBuilder UsingSpanishTemplate()
    {
        _builder.Services.AddSingleton<ILoggingMessageTemplate, SpanishLoggingMessageTemplate>();

        return _builder;
    }
}
