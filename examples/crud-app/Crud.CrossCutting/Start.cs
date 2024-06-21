global using LanguageExt;
global using LanguageExt.Common;
global using static LanguageExt.Prelude;
using Crud.CrossCutting;
using Microsoft.EntityFrameworkCore;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class Start
{
    public static IServiceCollection AddCrossCutting(this IServiceCollection services)
        => services.AddDbContext<AppDbContext>((b) => b.UseSqlite("Data Source=app.db"))
            .AddEndpointsApiExplorer()
            .AddSwaggerGen();
}