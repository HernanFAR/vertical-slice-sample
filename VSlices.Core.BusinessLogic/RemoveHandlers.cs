﻿using OneOf;
using OneOf.Types;
using VSlices.Core.Abstracts.BusinessLogic;
using VSlices.Core.Abstracts.DataAccess;
using VSlices.Core.Abstracts.Responses;

namespace VSlices.Core.BusinessLogic;

public abstract class RemoveHandler<TRequest, TResponse, TEntity> : IHandler<TRequest, TResponse>
{
    private readonly IRemoveRepository<TEntity> _repository;

    protected RemoveHandler(IRemoveRepository<TEntity> repository)
    {
        _repository = repository;
    }

    public virtual async ValueTask<OneOf<TResponse, BusinessFailure>> HandleAsync(TRequest request, CancellationToken cancellationToken = default)
    {
        var useCaseValidationResult = await ValidateUseCaseRulesAsync(request, cancellationToken);

        if (useCaseValidationResult.IsT1)
        {
            return useCaseValidationResult.AsT1;
        }

        var entity = await GetAndProcessEntityAsync(request, cancellationToken);

        var dataAccessResult = await _repository.RemoveAsync(entity, cancellationToken);

        if (dataAccessResult.IsT1)
        {
            return dataAccessResult.AsT1;
        }

        return GetResponse(entity, request);
    }

    protected internal abstract ValueTask<OneOf<Success, BusinessFailure>> ValidateUseCaseRulesAsync(TRequest request,
        CancellationToken cancellationToken);

    protected internal abstract ValueTask<TEntity> GetAndProcessEntityAsync(TRequest request,
        CancellationToken cancellationToken);

    protected internal abstract TResponse GetResponse(TEntity entity, TRequest request);
}

public abstract class RequestValidatedRemoveHandler<TRequest, TResponse, TEntity> : IHandler<TRequest, TResponse>
{
    private readonly IRemoveRepository<TEntity> _repository;

    protected RequestValidatedRemoveHandler(IRemoveRepository<TEntity> repository)
    {
        _repository = repository;
    }

    public virtual async ValueTask<OneOf<TResponse, BusinessFailure>> HandleAsync(TRequest request, CancellationToken cancellationToken = default)
    {
        var requestValidationResult = await ValidateRequestAsync(request, cancellationToken);

        if (requestValidationResult.IsT1)
        {
            return requestValidationResult.AsT1;
        }

        var useCaseValidationResult = await ValidateUseCaseRulesAsync(request, cancellationToken);

        if (useCaseValidationResult.IsT1)
        {
            return useCaseValidationResult.AsT1;
        }

        var entity = await GetAndProcessEntityAsync(request, cancellationToken);

        var dataAccessResult = await _repository.RemoveAsync(entity, cancellationToken);

        if (dataAccessResult.IsT1)
        {
            return dataAccessResult.AsT1;
        }

        return GetResponse(entity, request);
    }

    protected internal abstract ValueTask<OneOf<Success, BusinessFailure>> ValidateRequestAsync(TRequest request,
        CancellationToken cancellationToken = default);

    protected internal abstract ValueTask<OneOf<Success, BusinessFailure>> ValidateUseCaseRulesAsync(TRequest request,
        CancellationToken cancellationToken);

    protected internal abstract ValueTask<TEntity> GetAndProcessEntityAsync(TRequest request,
        CancellationToken cancellationToken);

    protected internal abstract TResponse GetResponse(TEntity entity, TRequest request);
}

public abstract class EntityValidatedRemoveHandler<TRequest, TResponse, TEntity> : IHandler<TRequest, TResponse>
{
    private readonly IRemoveRepository<TEntity> _repository;

    protected EntityValidatedRemoveHandler(IRemoveRepository<TEntity> repository)
    {
        _repository = repository;
    }

    public virtual async ValueTask<OneOf<TResponse, BusinessFailure>> HandleAsync(TRequest request, CancellationToken cancellationToken = default)
    {
        var useCaseValidationResult = await ValidateUseCaseRulesAsync(request, cancellationToken);

        if (useCaseValidationResult.IsT1)
        {
            return useCaseValidationResult.AsT1;
        }

        var entity = await GetAndProcessEntityAsync(request, cancellationToken);

        var entityValidationResult = await ValidateEntityAsync(entity, cancellationToken);

        if (entityValidationResult.IsT1)
        {
            return entityValidationResult.AsT1;
        }

        var dataAccessResult = await _repository.RemoveAsync(entity, cancellationToken);

        if (dataAccessResult.IsT1)
        {
            return dataAccessResult.AsT1;
        }

        return GetResponse(entity, request);
    }

    protected internal abstract ValueTask<OneOf<Success, BusinessFailure>> ValidateUseCaseRulesAsync(TRequest request,
        CancellationToken cancellationToken);

    protected internal abstract ValueTask<TEntity> GetAndProcessEntityAsync(TRequest request,
        CancellationToken cancellationToken);

    protected internal abstract ValueTask<OneOf<Success, BusinessFailure>> ValidateEntityAsync(TEntity request,
        CancellationToken cancellationToken = default);

    protected internal abstract TResponse GetResponse(TEntity entity, TRequest request);
}

public abstract class FullyValidatedRemoveHandler<TRequest, TResponse, TEntity> : IHandler<TRequest, TResponse>
{
    private readonly IRemoveRepository<TEntity> _repository;

    protected FullyValidatedRemoveHandler(IRemoveRepository<TEntity> repository)
    {
        _repository = repository;
    }

    public virtual async ValueTask<OneOf<TResponse, BusinessFailure>> HandleAsync(TRequest request, CancellationToken cancellationToken = default)
    {
        var requestValidationResult = await ValidateRequestAsync(request, cancellationToken);

        if (requestValidationResult.IsT1)
        {
            return requestValidationResult.AsT1;
        }

        var useCaseValidationResult = await ValidateUseCaseRulesAsync(request, cancellationToken);

        if (useCaseValidationResult.IsT1)
        {
            return useCaseValidationResult.AsT1;
        }

        var entity = await GetAndProcessEntityAsync(request, cancellationToken);

        var entityValidationResult = await ValidateEntityAsync(entity, cancellationToken);

        if (entityValidationResult.IsT1)
        {
            return entityValidationResult.AsT1;
        }

        var dataAccessResult = await _repository.RemoveAsync(entity, cancellationToken);

        if (dataAccessResult.IsT1)
        {
            return dataAccessResult.AsT1;
        }

        return GetResponse(entity, request);
    }

    protected internal abstract ValueTask<OneOf<Success, BusinessFailure>> ValidateRequestAsync(TRequest request,
        CancellationToken cancellationToken = default);

    protected internal abstract ValueTask<OneOf<Success, BusinessFailure>> ValidateUseCaseRulesAsync(TRequest request,
        CancellationToken cancellationToken);

    protected internal abstract ValueTask<TEntity> GetAndProcessEntityAsync(TRequest request,
        CancellationToken cancellationToken);

    protected internal abstract ValueTask<OneOf<Success, BusinessFailure>> ValidateEntityAsync(TEntity request,
        CancellationToken cancellationToken = default);

    protected internal abstract TResponse GetResponse(TEntity entity, TRequest request);
}

public abstract class RemoveHandler<TRequest, TEntity> : RemoveHandler<TRequest, Success, TEntity>
{
    protected RemoveHandler(IRemoveRepository<TEntity> repository) : base(repository)
    { }

    protected internal override Success GetResponse(TEntity _, TRequest r) => new();
}

public abstract class RequestValidatedRemoveHandler<TRequest, TEntity> : RequestValidatedRemoveHandler<TRequest, Success, TEntity>
{
    protected RequestValidatedRemoveHandler(IRemoveRepository<TEntity> repository) : base(repository) { }
    
    protected internal override Success GetResponse(TEntity _, TRequest r) => new();
}

public abstract class EntityValidatedRemoveHandler<TRequest, TEntity> : EntityValidatedRemoveHandler<TRequest, Success, TEntity>
{
    protected EntityValidatedRemoveHandler(IRemoveRepository<TEntity> repository) : base(repository)
    { }
    
    protected internal override Success GetResponse(TEntity _, TRequest r) => new();

}

public abstract class FullyValidatedRemoveHandler<TRequest, TEntity> : FullyValidatedRemoveHandler<TRequest, Success, TEntity>
{
    protected FullyValidatedRemoveHandler(IRemoveRepository<TEntity> repository) : base(repository)
    { }
    
    protected internal override Success GetResponse(TEntity _, TRequest r) => new();
}
