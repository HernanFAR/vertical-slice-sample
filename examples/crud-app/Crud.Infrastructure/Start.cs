global using LanguageExt;
global using LanguageExt.Common;

global using static LanguageExt.Prelude;
global using static VSlices.VSlicesPrelude;

using Crud.Domain.Repositories;
using Crud.Infrastructure;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class Start
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        => services.AddScoped<IQuestionRepository, EfQuestionRepository>();
}