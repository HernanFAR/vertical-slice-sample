using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using VSlices.Base.Responses;
// ReSharper disable MemberHidesStaticFromOuterClass

namespace VSlices.Core.DataAccess.EntityFramework;

public abstract partial class Repository
{
    /// <summary>
    /// Represents a repository that removes <typeparamref name="TTable"/> entities
    /// </summary>
    /// <typeparam name="TTable">The entity type to remove</typeparam>
    public abstract class ThatRemoves<TTable>
        where TTable : class
    {
        /// <summary>
        /// Represents a repository that removes <typeparamref name="TTable"/> entities using <typeparamref name="TDomain"/>
        /// classes
        /// </summary>
        /// <typeparam name="TDomain">The entity type used to remove <typeparamref name="TTable"/></typeparam>
        public abstract class UsingDomainModel<TDomain>
            where TDomain : class
        {
            /// <inheritdoc />
            public abstract class WithEntityFramework<TDbContext> : EfRemoveRepository<TDbContext, TTable, TDomain>
                where TDbContext : DbContext
            {
                /// <inheritdoc />
                protected WithEntityFramework(TDbContext context, ILogger logger) : base(context, logger)
                {
                }
            }
        }

        /// <inheritdoc />
        public abstract class WithEntityFramework<TDbContext> : EfRemoveRepository<TDbContext, TTable>
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
/// Defines a repository that can remove <typeparamref name="TTable"/> entities using EntityFramework Core
/// </summary>
/// <typeparam name="TDbContext">The <see cref="DbContext"/> related to the use case</typeparam>
/// <typeparam name="TTable">The entity to be removed</typeparam>
public abstract class EfRemoveRepository<TDbContext, TTable> : IRemoveRepository<TTable>
    where TDbContext : DbContext
    where TTable : class
{
    private readonly TDbContext _context;
    private readonly ILogger _logger;

    /// <summary>
    /// Creates a new instance with a given <typeparamref name="TDbContext"/> and <see cref="ILogger"/>
    /// </summary>
    /// <param name="context">EF Core Context of the use case</param>
    /// <param name="logger">Logger in case of errors</param>
    protected EfRemoveRepository(TDbContext context, ILogger logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Message template for concurrency errors
    /// </summary>
    protected internal virtual string ConcurrencyMessageTemplate
        => "There was a concurrency error when removing entity of type {EntityType}, with data {EntityJson}";

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
    public virtual async ValueTask<Result<TTable>> RemoveAsync(TTable entity, CancellationToken cancellationToken = default)
    {
        _context.Set<TTable>().Remove(entity);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);

            return entity;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, ConcurrencyMessageTemplate, typeof(TTable).Namespace, JsonSerializer.Serialize(entity));

            return await ProcessConcurrencyExceptionAsync(ex, cancellationToken);
        }
    }
}

/// <summary>
/// Defines a repository that can remove <typeparamref name="TDbContext"/> with <typeparamref name="TDomain"/> input, using EntityFramework Core, useful to use DDD with database-first scenarios
/// </summary>
/// <typeparam name="TDbContext">The <see cref="DbContext"/> related to the use case</typeparam>
/// <typeparam name="TDomain">The entity to input</typeparam>
/// <typeparam name="TTable">The entity to be removed</typeparam>
public abstract class EfRemoveRepository<TDbContext, TDomain, TTable> : IRemoveRepository<TDomain>
    where TDbContext : DbContext
    where TTable : class
{
    private readonly TDbContext _context;
    private readonly ILogger _logger;

    /// <summary>
    /// Creates a new instance with a given <typeparamref name="TDbContext"/> and <see cref="ILogger"/>
    /// </summary>
    /// <param name="context">EF Core Context of the use case</param>
    /// <param name="logger">Logger in case of errors</param>
    protected EfRemoveRepository(TDbContext context, ILogger logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Message template for concurrency errors
    /// </summary>
    protected internal virtual string ConcurrencyMessageTemplate
        => "There was a concurrency error when removing entity of type {EntityType}, with data {EntityJson}";

    /// <summary>
    /// Converts a <typeparamref name="TDomain"/> to a <typeparamref name="TTable"/>
    /// </summary>
    /// <param name="entity">The database entity to convert</param>
    /// <returns>A <typeparamref name="TTable"/></returns>
    protected internal abstract TTable ToDatabaseEntity(TDomain entity);

    /// <summary>
    /// Converts a <typeparamref name="TTable"/> to a <typeparamref name="TDomain"/>
    /// </summary>
    /// <param name="entity">The entity to convert</param>
    /// <returns>A <typeparamref name="TDomain"/></returns>
    protected internal abstract TDomain ToEntity(TTable entity);

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
    public virtual async ValueTask<Result<TDomain>> RemoveAsync(TDomain domain, CancellationToken cancellationToken = default)
    {
        var entity = ToDatabaseEntity(domain);

        _context.Set<TTable>().Remove(entity);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);

            return ToEntity(entity);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, ConcurrencyMessageTemplate, typeof(TTable).Namespace, JsonSerializer.Serialize(entity));

            return await ProcessConcurrencyExceptionAsync(ex, cancellationToken);
        }
    }
}
