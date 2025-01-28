global using LanguageExt.Common;
global using VSlices.Base;
global using LanguageExt;
global using Crud.Domain.ValueObjects;

global using static LanguageExt.Prelude;
global using static VSlices.VSlicesPrelude;

using Crud.Domain.Rules.Services;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class Start
{
    public static IServiceCollection AddDomain(this IServiceCollection services) =>
        services.AddScoped<QuestionManager>();

}