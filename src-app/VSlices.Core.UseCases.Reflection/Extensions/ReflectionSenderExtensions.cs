using VSlices.Core.UseCases;
using VSlices.Core.UseCases.Reflection;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// <see cref="IServiceCollection" /> extensions for <see cref="ISender"/>
/// </summary>
public static class ReflectionSenderExtensions
{
    /// <summary>
    /// Add a reflection <see cref="ISender"/> implementation to the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">Service Collection</param>
    /// <returns>Service Collection</returns>
    public static IServiceCollection AddReflectionSender(this IServiceCollection services)
    {
        return services.AddSender<ReflectionSender>();
    }
}
