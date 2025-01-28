using LanguageExt;
using VSlices.Base;
using VSlices.Domain.Interfaces;

namespace VSlices.Domain.Repository;

/// <summary>
/// Defines a contract for a repository for <see cref="IAggregateRoot{TKey}"/>
/// </summary>
public interface IRepository<TRoot, in TKey>
    where TRoot : class, IAggregateRoot<TKey> 
    where TKey : class, IEquatable<TKey>
{
    /// <summary>
    /// Get an <see cref="IAggregateRoot{TKey}"/>
    /// </summary>
    Eff<VSlicesRuntime, TRoot> Get(TKey id);

    /// <summary>
    /// Get an <see cref="Option{A}"/> of <see cref="IAggregateRoot{TKey}"/>
    /// </summary>
    Eff<VSlicesRuntime, Option<TRoot>> GetOrOption(TKey id);

    /// <summary>
    /// Adds a <see cref="IAggregateRoot{TKey}"/> to the repository
    /// </summary>
    /// <remarks>
    /// Calling this method won't persist the <see cref="IAggregateRoot{TKey}"/>
    /// until you call <see cref="IUnitOfWork.SaveChanges"/>
    /// </remarks>
    Eff<VSlicesRuntime, TRoot> Add(TRoot root);

    /// <summary>
    /// Adds a <see cref="IAggregateRoot{TKey}"/> to the repository
    /// </summary>
    /// <remarks>
    /// Calling this method won't persist the <see cref="IAggregateRoot{TKey}"/>
    /// until you call <see cref="IUnitOfWork.SaveChanges"/>
    /// </remarks>
    Eff<VSlicesRuntime, TRoot> Update(TRoot root);

    /// <summary>
    /// Adds a <see cref="IAggregateRoot{TKey}"/> to the repository
    /// </summary>
    /// <remarks>
    /// Calling this method won't persist the <see cref="IAggregateRoot{TKey}"/>
    /// until you call <see cref="IUnitOfWork.SaveChanges"/>
    /// </remarks>
    Eff<VSlicesRuntime, TRoot> Delete(TRoot root);

    /// <summary>
    /// Verifies if an <see cref="IAggregateRoot{TKey}"/> exists using the id
    /// </summary>
    Eff<VSlicesRuntime, bool> Exists(TKey id);

}
