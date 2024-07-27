global using LanguageExt;
global using LanguageExt.Common;
global using static LanguageExt.Prelude;
global using static VSlices.CorePrelude;

using Crud.CrossCutting;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class Start
{
    public static IServiceCollection AddCrossCutting(this IServiceCollection services)
        => services.AddDbContext<AppDbContext>((b) => b.UseSqlite("Data Source=app.db"))
            .AddHostedTaskListener()
            .AddEndpointsApiExplorer()
            .AddSwaggerGen(e =>
            {
                e.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "BCSPN",
                    Version = "v1"
                });
            });
}