using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VSlices.Base.Responses;

// ReSharper disable MemberHidesStaticFromOuterClass
// ReSharper disable PartialTypeWithSinglePart

namespace VSlices.Core.DataAccess;

public abstract partial class Repository
{
    /// <summary>
    /// Represents a repository that updates <typeparamref name="TTable"/> entities
    /// </summary>
    /// <typeparam name="TTable">The entity type to update</typeparam>
    public abstract class ThatUpdates<TTable>
        where TTable : class
    {
        /// <summary>
        /// Represents a repository that updates <typeparamref name="TTable"/> entities using <typeparamref name="TDomain"/>
        /// classes
        /// </summary>
        /// <typeparam name="TDomain">The entity type used to update <typeparamref name="TTable"/></typeparam>
        public abstract class UsingDomainModel<TDomain>
            where TDomain : class
        {
            /// <inheritdoc />
            public abstract class WithEntityFramework<TDbContext> : EfUpdateRepository<TDbContext, TTable, TDomain>
                where TDbContext : DbContext
            {
                /// <inheritdoc />
                protected WithEntityFramework(TDbContext context, ILogger logger) : base(context, logger)
                {
                }
            }
        }

        /// <inheritdoc />
        public abstract class WithEntityFramework<TDbContext> : EfUpdateRepository<TDbContext, TTable>
            where TDbContext : DbContext
        {
            /// <inheritdoc />
            protected WithEntityFramework(TDbContext context, ILogger logger) : base(context, logger)
            {
            }
        }
    }
}

/// <summary>
/// Defines a repository that can update <typeparamref name="TEntity"/> entities using EntityFramework Core
/// </summary>
/// <typeparam name="TDbContext">The <see cref="DbContext"/> related to the use case</typeparam>
/// <typeparam name="TEntity">The entity to be removed</typeparam>
public abstract class EfUpdateRepository<TDbContext, TEntity> : IUpdateRepository<TEntity>
    where TDbContext : DbContext
    where TEntity : class
{
    private readonly TDbContext _context;
    private readonly ILogger _logger;

    /// <summary>
    /// Creates a new instance with a given <typeparamref name="TDbContext"/> and <see cref="ILogger"/>
    /// </summary>
    /// <param name="context">EF Core Context of the use case</param>
    /// <param name="logger">Logger in case of errors</param>
    protected EfUpdateRepository(TDbContext context, ILogger logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Message template for concurrency errors
    /// </summary>
    protected internal virtual string ConcurrencyMessageTemplate
        => "There was a concurrency error when updating entity of type {EntityType}, with data {EntityJson}";

    /// <summary>
    /// Processes a <see cref="DbUpdateConcurrencyException"/> and returns a <see cref="Failure"/>, usually with a concurrency error
    /// </summary>
    /// <param name="ex">The concurrency error</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A <see cref="ValueTask{T}"/> holding a <see cref="Failure"/></returns>
    protected internal virtual ValueTask<Failure> ProcessConcurrencyExceptionAsync(DbUpdateConcurrencyException ex, CancellationToken cancellationToken)
        => ValueTask.FromResult(new Failure(FailureKind.ConcurrencyError));

    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "CA2254", Justification = "Logging template can be translated to other languages in this way")]
    public virtual async ValueTask<Result<TEntity>> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        _context.Set<TEntity>().Update(entity);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);

            return entity;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, ConcurrencyMessageTemplate, typeof(TEntity).Namespace, JsonSerializer.Serialize(entity));

            return await ProcessConcurrencyExceptionAsync(ex, cancellationToken);
        }
    }
}

/// <summary>
/// Defines a repository that can update <typeparamref name="TDbContext"/> with <typeparamref name="TEntity"/> input, using EntityFramework Core, useful to use DDD with database-first scenarios
/// </summary>
/// <typeparam name="TDbContext">The <see cref="DbContext"/> related to the use case</typeparam>
/// <typeparam name="TEntity">The entity to input</typeparam>
/// <typeparam name="TDbEntity">The entity to be updated</typeparam>
public abstract class EfUpdateRepository<TDbContext, TEntity, TDbEntity> : IUpdateRepository<TEntity>
    where TDbContext : DbContext
    where TDbEntity : class
{
    private readonly TDbContext _context;
    private readonly ILogger _logger;

    /// <summary>
    /// Creates a new instance with a given <typeparamref name="TDbContext"/> and <see cref="ILogger"/>
    /// </summary>
    /// <param name="context">EF Core Context of the use case</param>
    /// <param name="logger">Logger in case of errors</param>
    protected EfUpdateRepository(TDbContext context, ILogger logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Message template for concurrency errors
    /// </summary>
    protected internal virtual string ConcurrencyMessageTemplate
        => "There was a concurrency error when updating entity of type {EntityType}, with data {EntityJson}";

    /// <summary>
    /// Converts a <typeparamref name="TEntity"/> to a <typeparamref name="TDbEntity"/>
    /// </summary>
    /// <param name="entity">The database entity to convert</param>
    /// <returns>A <typeparamref name="TDbEntity"/></returns>
    protected internal abstract TDbEntity ToDatabaseEntity(TEntity entity);

    /// <summary>
    /// Converts a <typeparamref name="TDbEntity"/> to a <typeparamref name="TEntity"/>
    /// </summary>
    /// <param name="entity">The entity to convert</param>
    /// <returns>A <typeparamref name="TEntity"/></returns>
    protected internal abstract TEntity ToEntity(TDbEntity entity);

    /// <summary>
    /// Processes a <see cref="DbUpdateConcurrencyException"/> and returns a <see cref="Failure"/>, usually with a concurrency error
    /// </summary>
    /// <param name="ex">The concurrency error</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A <see cref="ValueTask{T}"/> holding a <see cref="Failure"/></returns>
    protected internal virtual ValueTask<Failure> ProcessConcurrencyExceptionAsync(DbUpdateConcurrencyException ex, CancellationToken cancellationToken)
        => ValueTask.FromResult(new Failure(FailureKind.ConcurrencyError));

    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "CA2254", Justification = "Logging template can be translated to other languages in this way")]
    public virtual async ValueTask<Result<TEntity>> UpdateAsync(TEntity domain, CancellationToken cancellationToken = default)
    {
        var entity = ToDatabaseEntity(domain);

        _context.Set<TDbEntity>().Add(entity);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);

            return ToEntity(entity);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, ConcurrencyMessageTemplate, typeof(TDbEntity).Namespace, JsonSerializer.Serialize(entity));

            return await ProcessConcurrencyExceptionAsync(ex, cancellationToken);
        }
    }
}
