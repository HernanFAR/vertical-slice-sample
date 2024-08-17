global using LanguageExt;
global using static LanguageExt.Prelude;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class Startup
{
    public static IServiceCollection AddDomainDependencies(this IServiceCollection services) => 
        services;
}
