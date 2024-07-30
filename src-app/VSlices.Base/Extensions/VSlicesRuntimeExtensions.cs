using VSlices.Base;
using VSlices.Base.Traits;
using Implementations = LanguageExt.Sys.Live.Implementations;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods to add <see cref="VSlicesRuntime"/>
/// </summary>
public static class VSlicesRuntimeExtensions
{
    /// <summary>
    /// Adds <see cref="VSlicesRuntime"/> to the service collection
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection</returns>
    public static IServiceCollection AddVSlicesRuntime(this IServiceCollection services)
    {
        return services.AddVSlicesRuntime(Implementations.FileIO.Default, Implementations.DirectoryIO.Default);
    }

    /// <summary>
    /// Adds <see cref="VSlicesRuntime"/> to the service collection
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="fileIo"></param>
    /// <param name="directoryIo"></param>
    /// <returns>Service collection</returns>
    public static IServiceCollection AddVSlicesRuntime(this IServiceCollection services, FileIO fileIo, DirectoryIO directoryIo)
    {
        services.AddScoped(provider =>
        {
            DependencyProvider dependencyProvider = new(provider);

            return VSlicesRuntime.New(dependencyProvider, fileIo, directoryIo);
        });

        return services;
    }
}
