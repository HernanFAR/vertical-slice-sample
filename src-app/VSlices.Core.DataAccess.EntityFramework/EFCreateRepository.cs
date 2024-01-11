using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using VSlices.Base.Responses;
// ReSharper disable MemberHidesStaticFromOuterClass

namespace VSlices.Core.DataAccess.EntityFramework;

/// <summary>
/// Start point of a FluentAPI that helps with the definition of <see cref="ICreateRepository{TEntity}"/> implementations
/// </summary>
public abstract partial class Repository
{
    /// <summary>
    /// Represents a repository that creates <typeparamref name="TTable"/> entities
    /// </summary>
    /// <typeparam name="TTable">The entity type to create</typeparam>
    public abstract  class ThatCreates<TTable>
        where TTable : class
    {
        /// <summary>
        /// Represents a repository that creates <typeparamref name="TTable"/> entities using <typeparamref name="TDomain"/>
        /// classes
        /// </summary>
        /// <typeparam name="TDomain">The entity type used to create <typeparamref name="TTable"/></typeparam>
        public abstract partial class UsingDomainModel<TDomain>
            where TDomain : class
        {
            /// <inheritdoc />
            public abstract class WithEntityFramework<TDbContext> : EfCreateRepository<TDbContext, TDomain, TTable>
                where TDbContext : DbContext
            {
                /// <inheritdoc />
                protected WithEntityFramework(TDbContext context, ILogger logger) : base(context, logger)
                {
                }
            }
        }

        /// <inheritdoc />
        public abstract class WithEntityFramework<TDbContext> : EfCreateRepository<TDbContext, TTable>
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
/// Defines a repository that can create <typeparamref name="TEntity"/> entities using EntityFramework Core
/// </summary>
/// <typeparam name="TDbContext">The <see cref="DbContext"/> related to the use case</typeparam>
/// <typeparam name="TEntity">The entity to be created</typeparam>
public abstract class EfCreateRepository<TDbContext, TEntity> : ICreateRepository<TEntity>
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
    protected EfCreateRepository(TDbContext context, ILogger logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Message template for concurrency errors
    /// </summary>
    protected internal virtual string ConcurrencyMessageTemplate
        => "There was a concurrency error when creating entity of type {EntityType}, with data {EntityJson}";

    /// <summary>
    /// Processes a <see cref="DbUpdateConcurrencyException"/> and returns a <see cref="Failure"/>, usually with a concurrency error
    /// </summary>
    /// <param name="ex">The concurrency error</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A <see cref="ValueTask{T}"/> holding a <see cref="Failure"/></returns>
    protected internal virtual ValueTask<Failure> ProcessConcurrencyExceptionAsync(DbUpdateConcurrencyException ex, CancellationToken cancellationToken)
        => ValueTask.FromResult(new Failure(FailureKind.ConcurrencyError));

    /// <inheritdoc/>
    public virtual async ValueTask<Result<TEntity>> CreateAsync(TEntity entity, CancellationToken cancellationToken)
    {
        _context.Set<TEntity>().Add(entity);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);

            return entity;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, ConcurrencyMessageTemplate, typeof(TEntity).Namespace, entity);

            return await ProcessConcurrencyExceptionAsync(ex, cancellationToken);
        }
    }
}

/// <summary>
/// Defines a repository that can create <typeparamref name="TDbContext"/> with <typeparamref name="TEntity"/> input,
/// using EntityFramework Core
/// </summary>
/// <remarks>Useful to use Domain Driven Design</remarks>
/// <typeparam name="TDbContext">The <see cref="DbContext"/> related to the use case</typeparam>
/// <typeparam name="TEntity">The entity to input</typeparam>
/// <typeparam name="TDbEntity">The entity to be created</typeparam>
public abstract class EfCreateRepository<TDbContext, TEntity, TDbEntity> : ICreateRepository<TEntity>
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
    protected EfCreateRepository(TDbContext context, ILogger logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Message template for concurrency errors
    /// </summary>
    protected internal virtual string ConcurrencyMessageTemplate
        => "There was a concurrency error when creating entity of type {EntityType}, with data {EntityJson}";

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
    public virtual async ValueTask<Result<TEntity>> CreateAsync(TEntity domain, CancellationToken cancellationToken)
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

