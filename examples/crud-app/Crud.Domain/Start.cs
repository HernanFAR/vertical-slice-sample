global using LanguageExt;
global using LanguageExt.Common;

global using static LanguageExt.Prelude;
global using static VSlices.CorePrelude;

using Crud.Domain.Services;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class Start
{
    public static IServiceCollection AddDomain(this IServiceCollection services) =>
        services.AddScoped<QuestionManager>();

}