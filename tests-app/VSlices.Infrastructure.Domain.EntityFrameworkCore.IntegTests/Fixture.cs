using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;
using VSlices.Infrastructure.Domain.EntityFrameworkCore.IntegTests.DataAccess;

namespace VSlices.Infrastructure.Domain.EntityFrameworkCore.IntegTests;

public sealed class Fixture : IAsyncLifetime
{
    private MsSqlContainer _container;
    private Context _context;
    private IServiceProvider _provider;

    public Context Context => _context;

    public IServiceProvider Provider => _provider;

    public async Task InitializeAsync()
    {
        _container = new MsSqlBuilder()
                     .WithPassword("H4lv.2024IIXD")
                     .Build();

        await _container.StartAsync();

        DbContextOptions<Context> opts = new DbContextOptionsBuilder<Context>()
                                         .UseSqlServer(_container.GetConnectionString())
                                         .Options;

        _context = new Context(opts);

        await _context.Database.MigrateAsync();

        _provider = new ServiceCollection()
                    .AddDbContext<Context>(options => options.UseSqlServer(_container.GetConnectionString()))
                    .BuildServiceProvider();


    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
        await _context.DisposeAsync();
    }
}
