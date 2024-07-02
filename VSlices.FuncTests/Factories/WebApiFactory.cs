using Crud.CrossCutting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace VSlices.FuncTests.Factories;

public sealed class WebApiFactory
{
    private readonly WebApplicationFactory<Program> _webAppFactory = new();

    public IServiceProvider GetServiceProvider()
    {
        return _webAppFactory.Server.Services;
    }

    public HttpClient CreateClient()
    {
        return _webAppFactory.CreateClient();
    }

    public AppDbContext GetDbContext()
    {
        return _webAppFactory.Server.Services.GetRequiredService<AppDbContext>();
    }
}
