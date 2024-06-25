using Crud.Domain.Events;
using Crud.Domain.Repositories;
using Crud.Domain.ValueObjects;
using LanguageExt.Effects.Traits;
using VSlices.Core.Events;

namespace Crud.Domain.Services;

public sealed class QuestionManager(
    IQuestionRepository repository, 
    IEventQueueWriter eventWriter)
{
    readonly IQuestionRepository _repository = repository;
    readonly IEventQueueWriter _eventWriter = eventWriter;

    public Aff<TRuntime, Unit> Create<TRuntime>(QuestionId id, NonEmptyString text)
        where TRuntime : struct, HasCancel<TRuntime> =>
        from question in Eff(() => new Question(id, text))
        from _1 in _repository.Create<TRuntime>(question)
        from _2 in PublishEventCore<TRuntime>(question.Id, EState.Created)
        select unit;

    public Aff<TRuntime, Unit> Create<TRuntime>(NonEmptyString text)
        where TRuntime : struct, HasCancel<TRuntime> =>
        from question in Eff(() => Question.Create(text))
        from _1 in _repository.Create<TRuntime>(question)
        from _2 in PublishEventCore<TRuntime>(question.Id, EState.Created)
        select unit;

    public Aff<TRuntime, Unit> Update<TRuntime>(Question question)
        where TRuntime : struct, HasCancel<TRuntime> =>
        from _1 in _repository.Update<TRuntime>(question)
        from _2 in PublishEventCore<TRuntime>(question.Id, EState.Updated)
        select unit;

    public Aff<TRuntime, Unit> Delete<TRuntime>(Question question)
        where TRuntime : struct, HasCancel<TRuntime> =>
        from _1 in _repository.Delete<TRuntime>(question)
        from _2 in PublishEventCore<TRuntime>(question.Id, EState.Removed)
        select unit;

    Aff<TRuntime, Unit> PublishEventCore<TRuntime>(QuestionId id, EState state)
        where TRuntime : struct, HasCancel<TRuntime> =>
        from cancelToken in cancelToken<TRuntime>()
        from _ in Aff(async () =>
        {
            QuestionMutatedEvent @event = new(id, state);

            await _eventWriter.EnqueueAsync(@event, cancelToken);

            return unit;
        })
        select unit;
}
