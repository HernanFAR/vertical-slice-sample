global using LanguageExt;
global using LanguageExt.Common;
global using VSlices.Base;
global using VSlices.Base.Failures;
global using VSlices.Core;
global using VSlices.Core.Builder;
global using VSlices.Core.Presentation;
global using VSlices.Core.UseCases;
global using Microsoft.AspNetCore.Builder;
global using Microsoft.AspNetCore.Http;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.AspNetCore.Routing;

global using static VSlices.VSlicesPrelude;
global using static LanguageExt.Prelude;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class Startup
    {
        public static IServiceCollection AddCore(this IServiceCollection services) =>
            services.AddVSlicesRuntime()
                    // Recurring jobs
                    .AddRecurringJobListener()
                    // Input
                    .AddReflectionRequestRunner()
                    // Events
                    .AddReflectionEventRunner()
                    .AddInMemoryEventQueue()
                    .AddEventListener()
                    .WithFileWriteInDeadLetterCase(config =>
                    {
                        config.AbsolutePath = "C:\\DeadLetters";
                        config.JsonOptions.IncludeFields = true;
                    });
    }
}

namespace Crud.Core
{
    public sealed class Anchor;
}
