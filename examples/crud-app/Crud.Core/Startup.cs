global using VSlices.Base.Failures;
global using VSlices.Core;
global using VSlices.Core.Builder;
global using VSlices.Core.Presentation;
global using VSlices.Core.UseCases;
global using LanguageExt;
global using LanguageExt.Common;
global using static LanguageExt.Prelude;
global using Microsoft.AspNetCore.Builder;
global using Microsoft.AspNetCore.Http;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.AspNetCore.Routing;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class Startup
{
    public static IServiceCollection AddCore(this IServiceCollection services)
        => services.AddFeatureDependenciesFromAssemblyContaining<Anchor>()
            .AddReflectionRequestRunner();
}

internal sealed class Anchor;