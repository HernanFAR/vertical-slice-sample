using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crud.CrossCutting;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<TQuestion> Questions => Set<TQuestion>();

    public DbSet<TCategory> Categories => Set<TCategory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly); 
    }
}

public sealed class TQuestion
{
    public const string TableName = "Questions";
    public const int TextMaxLength = 512;

    public Guid Id { get; set; }

    public string Text { get; set; } = string.Empty;

}

public sealed class TCategory
{
    public const string TableName = "Categories";
    public const int TextMaxLength = 128;

    public Guid Id { get; set; }

    public string Text { get; set; } = string.Empty;

}

internal sealed class QuestionEntityTypeConfiguration : IEntityTypeConfiguration<TQuestion>
{
    public void Configure(EntityTypeBuilder<TQuestion> builder)
    {
        builder.ToTable(TQuestion.TableName);

        builder.Property(x => x.Text).HasMaxLength(TQuestion.TextMaxLength);

    }
}

internal sealed class CategoryEntityTypeConfiguration : IEntityTypeConfiguration<TCategory>
{
    public void Configure(EntityTypeBuilder<TCategory> builder)
    {
        builder.ToTable(TCategory.TableName);

        builder.Property(x => x.Text).HasMaxLength(TCategory.TextMaxLength);

    }
}
