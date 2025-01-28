global using LanguageExt;
global using LanguageExt.Common;
global using VSlices.Base;
global using static LanguageExt.Prelude;
global using static VSlices.VSlicesPrelude;

using Crud.CrossCutting;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class Start
{
    public static IServiceCollection AddCrossCutting(this IServiceCollection services)
        => services.AddDbContext<AppDbContext>(b => b.UseSqlite("Data Source=app.db")
                                                     .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking))
            .AddHangfireTaskListener(conf => conf.UseInMemoryStorage())
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