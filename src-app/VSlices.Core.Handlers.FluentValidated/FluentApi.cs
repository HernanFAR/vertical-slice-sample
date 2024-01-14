using VSlices.Base;
using VSlices.Base.Responses;
using VSlices.Core.DataAccess;

// ReSharper disable once CheckNamespace
namespace VSlices.Core.Handlers.FluentApi;

/// <summary>
/// Start point of a FluentAPI that helps with the definition of fluent validated <see cref="IHandler{TRequest,TResult}"/>
/// implementations
/// </summary>
public abstract class FluentValidatedHandler
{
    /// <summary>
    /// Represents a handler for <see cref="IFeature{TResult}"/> that returns <see cref="Success"/>
    /// </summary>
    public abstract partial class WithoutResult
    {
        /// <summary>
        /// Represents a handler for <typeparamref name="TFeature"/>
        /// </summary>
        public abstract partial class ForFeature<TFeature>
            where TFeature : IFeature<Success>
        {
            /// <summary>
            /// Represents a handler for <typeparamref name="TFeature"/>, that creates <typeparamref name="TEntity"/>
            /// </summary>
            public abstract partial class ToCreate<TEntity> : EntityValidatedCreateHandler<TFeature, TEntity>
            {
                /// <inheritdoc />
                protected ToCreate(ICreateRepository<TEntity> repository) : base(repository) { }

            }

            /// <summary>
            /// Represents a handler for <typeparamref name="TFeature"/>, that updates <typeparamref name="TEntity"/>
            /// </summary>
            public abstract partial class ToUpdate<TEntity> : EntityValidatedUpdateHandler<TFeature, TEntity>
            {
                /// <inheritdoc />
                protected ToUpdate(IUpdateRepository<TEntity> repository) : base(repository) { }

            }

            /// <summary>
            /// Represents a handler for <typeparamref name="TFeature"/>, that removes <typeparamref name="TEntity"/>
            /// </summary>
            public abstract class ToDelete<TEntity> : EntityValidatedRemoveHandler<TFeature, TEntity>
            {
                /// <inheritdoc />
                protected ToDelete(IRemoveRepository<TEntity> repository) : base(repository) { }

            }
        }
    }

    /// <summary>
    /// Represents a handler for <see cref="IFeature{TResult}"/> that returns <typeparamref name="TResult"/>
    /// </summary>
    public abstract partial class WithResult<TResult>
    {
        /// <summary>
        /// Represents a handler for <typeparamref name="TFeature"/>
        /// </summary>
        public abstract partial class ForFeature<TFeature>
            where TFeature : IFeature<TResult>
        {
            /// <summary>
            /// Represents a handler for <typeparamref name="TFeature"/>, that creates <typeparamref name="TEntity"/>
            /// </summary>
            public abstract partial class ToCreate<TEntity> : EntityValidatedCreateHandler<TFeature, TResult, TEntity>
            {
                /// <inheritdoc />
                protected ToCreate(ICreateRepository<TEntity> repository) : base(repository) { }

            }

            /// <summary>
            /// Represents a handler for <typeparamref name="TFeature"/> that returns <typeparamref name="TResult"/>
            /// </summary>
            public abstract partial class ToRead : BasicReadHandler<TFeature, TResult>
            {
                /// <inheritdoc />
                protected ToRead(IReadRepository<TResult> repository) : base(repository) { }

                /// <summary>
                /// Represents a handler for <typeparamref name="TFeature"/> that returns <typeparamref name="TResult"/>
                /// using <typeparamref name="TFeature"/> to complete the read process
                /// </summary>
                public abstract partial class WithOptions : ReadHandler<TFeature, TResult>
                {
                    /// <inheritdoc />
                    protected WithOptions(IReadRepository<TResult, TFeature> repository) : base(repository) { }
                }

                /// <summary>
                /// Represents a handler for <typeparamref name="TFeature"/> that returns <typeparamref name="TResult"/>
                /// using <typeparamref name="TSearchOptions"/> to complete the read process
                /// </summary>
                public abstract partial class WithOptions<TSearchOptions> : ReadHandler<TFeature, TSearchOptions, TResult>
                {
                    /// <inheritdoc />
                    protected WithOptions(IReadRepository<TResult, TSearchOptions> repository) : base(repository) { }
                }
            }

            /// <summary>
            /// Represents a handler for <typeparamref name="TFeature"/>, that updates <typeparamref name="TEntity"/>
            /// </summary>
            public abstract class ToUpdate<TEntity> : EntityValidatedUpdateHandler<TFeature, TResult, TEntity>
            {
                /// <inheritdoc />
                protected ToUpdate(IUpdateRepository<TEntity> repository) : base(repository) { }

            }

            /// <summary>
            /// Represents a handler for <typeparamref name="TFeature"/>, that removes <typeparamref name="TEntity"/>
            /// </summary>
            public abstract class ToDelete<TEntity> : EntityValidatedRemoveHandler<TFeature, TResult, TEntity>
            {
                /// <inheritdoc />
                protected ToDelete(IRemoveRepository<TEntity> repository) : base(repository) { }

            }
        }
    }
}
