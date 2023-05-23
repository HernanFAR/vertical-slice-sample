﻿using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OneOf;
using OneOf.Types;
using VSlices.Core.Abstracts.DataAccess;
using VSlices.Core.Abstracts.Responses;

namespace VSlices.Core.DataAccess.EntityFramework;

public abstract class EFUpdateRepository<TDbContext, TEntity> : IUpdateRepository<TEntity>
    where TDbContext : DbContext
    where TEntity : class
{
    private readonly TDbContext _context;
    private readonly ILogger _logger;

    protected EFUpdateRepository(TDbContext context, ILogger logger)
    {
        _context = context;
        _logger = logger;
    }

    protected internal virtual string ConcurrencyMessageTemplate
        => "There was a concurrency error when updating entity of type {EntityType}, with data {EntityJson}";

    protected internal virtual ValueTask<BusinessFailure> ProcessConcurrencyExceptionAsync(DbUpdateConcurrencyException ex, CancellationToken cancellationToken = default)
        => ValueTask.FromResult(BusinessFailure.Of.ConcurrencyError(Array.Empty<string>()));

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "CA2254", Justification = "Logging template can be translated to other languages in this way")]
    public virtual async ValueTask<OneOf<TEntity, BusinessFailure>> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
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

public abstract class EFUpdateRepository<TDbContext, TEntity, TDbEntity> : IUpdateRepository<TEntity>
    where TDbContext : DbContext
    where TDbEntity : class
{
    private readonly TDbContext _context;
    private readonly ILogger _logger;

    protected EFUpdateRepository(TDbContext context, ILogger logger)
    {
        _context = context;
        _logger = logger;
    }

    protected internal virtual string ConcurrencyMessageTemplate
        => "There was a concurrency error when updating entity of type {EntityType}, with data {EntityJson}";
    
    protected internal abstract TDbEntity ToDatabaseEntity(TEntity domain);
    
    protected internal abstract TEntity ToEntity(TDbEntity entity);

    protected internal virtual ValueTask<BusinessFailure> ProcessConcurrencyExceptionAsync(DbUpdateConcurrencyException ex, CancellationToken cancellationToken = default)
        => ValueTask.FromResult(BusinessFailure.Of.ConcurrencyError(Array.Empty<string>()));

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "CA2254", Justification = "Logging template can be translated to other languages in this way")]
    public virtual async ValueTask<OneOf<TEntity, BusinessFailure>> UpdateAsync(TEntity domain, CancellationToken cancellationToken = default)
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
