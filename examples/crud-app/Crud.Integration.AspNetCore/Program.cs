WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddInfrastructure()
    .AddCrossCutting()
    .AddCore()
    .AddDomain()
    .Orchestrate(composer => composer.AllFeaturesFromAssemblyContaining<Crud.Core.Anchor>());

WebApplication app = builder.Build();

// Configure the HTTP input pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseEndpointDefinitions();

app.Run();

internal sealed partial class Program;