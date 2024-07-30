using LanguageExt.Sys.Traits;
using Microsoft.Extensions.DependencyInjection;
using VSlices.Core.Traits;
using Implementations = LanguageExt.Sys.Live.Implementations;

namespace VSlices.Core.Extensions;

/// <summary>
/// Extension methods to add <see cref="HandlerRuntime"/>
/// </summary>
public static class HandlerRuntimeExtensions
{
    /// <summary>
    /// Adds <see cref="HandlerRuntime"/> to the service collection
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection</returns>
    public static IServiceCollection AddHandlerRuntime(this IServiceCollection services)
    {
        return services.AddHandlerRuntime(Implementations.FileIO.Default, Implementations.DirectoryIO.Default);
    }

    /// <summary>
    /// Adds <see cref="HandlerRuntime"/> to the service collection
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="fileIo"></param>
    /// <param name="directoryIo"></param>
    /// <returns>Service collection</returns>
    public static IServiceCollection AddHandlerRuntime(this IServiceCollection services, FileIO fileIo, DirectoryIO directoryIo)
    {
        services.AddScoped(provider =>
        {
            DependencyProvider dependencyProvider = new(provider);

            return HandlerRuntime.New(dependencyProvider, fileIo, directoryIo);
        });

        return services;
    }
}
