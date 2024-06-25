global using LanguageExt;
global using LanguageExt.Common;
global using static LanguageExt.Prelude;
using Crud.Domain.Services;
using LanguageExt.SysX.Live;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class Start
{
    public static IServiceCollection AddDomain(this IServiceCollection services) =>
        services.AddScoped<QuestionManager>();

}