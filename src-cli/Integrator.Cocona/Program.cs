using Cocona;
using Cocona.Builder;
using Microsoft.Extensions.DependencyInjection;

CoconaAppBuilder builder = CoconaApp.CreateBuilder();

builder.Services
       .AddCoreDependencies()
       .AddCrossCuttingDependencies()
       .AddDomainDependencies();

CoconaApp app = builder.Build();

app.UseCoconaIntegrators();

app.Run();