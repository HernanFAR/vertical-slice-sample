global using static LanguageExt.Prelude;
global using static VSlices.VSlicesPrelude;
global using static Core.Functional.EffExtensions;
global using LanguageExt;


// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class CoreStartup
{
    public static IServiceCollection AddCoreDependencies(this IServiceCollection services) => 
        services.AddFeatureDependenciesFromAssemblyContaining<Anchor>()
                .AddVSlicesRuntime()
                .AddReflectionRequestRunner();
}

internal sealed class Anchor;
