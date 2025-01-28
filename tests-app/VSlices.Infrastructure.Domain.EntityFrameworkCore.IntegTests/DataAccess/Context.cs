using Microsoft.EntityFrameworkCore;
using VSlices.Infrastructure.Domain.EntityFrameworkCore.IntegTests.Models;

namespace VSlices.Infrastructure.Domain.EntityFrameworkCore.IntegTests.DataAccess;

public sealed class Context : DbContext
{
    public Context() { }

    public Context(DbContextOptions<Context> options) : base(options) { }

    public DbSet<TEntity> Entities => Set<TEntity>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (optionsBuilder.IsConfigured is false)
        {
            optionsBuilder.UseSqlServer();
        }
    }
}