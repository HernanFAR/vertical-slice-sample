using Crud.Domain.Events;
using Crud.Domain.Repositories;
using Crud.Domain.ValueObjects;
using LanguageExt.SysX.Live;
using VSlices.Core.Events;

namespace Crud.Domain.Services;

public sealed class QuestionManager(
    IQuestionRepository repository, 
    IEventQueueWriter eventWriter)
{
    readonly IQuestionRepository _repository = repository;
    readonly IEventQueueWriter _eventWriter = eventWriter;

    public Aff<Runtime, Unit> Create(QuestionId id, NonEmptyString text)  =>
        from question in Eff(() => new Question(id, text))
        from _1 in _repository.Create(question)
        from _2 in PublishEventCore(question.Id, EState.Created)
        select unit;

    public Aff<Runtime, Unit> Create(NonEmptyString text) =>
        from question in Eff(() => Question.Create(text))
        from _1 in _repository.Create(question)
        from _2 in PublishEventCore(question.Id, EState.Created)
        select unit;

    public Aff<Runtime, Unit> Update(Question question) =>
        from _1 in _repository.Update(question)
        from _2 in PublishEventCore(question.Id, EState.Updated)
        select unit;

    public Aff<Runtime, Unit> Delete(Question question) =>
        from _1 in _repository.Delete(question)
        from _2 in PublishEventCore(question.Id, EState.Removed)
        select unit;

    Aff<Runtime, Unit> PublishEventCore(QuestionId id, EState state) =>
        from cancelToken in cancelToken<Runtime>()
        from _ in Aff(async () =>
        {
            QuestionMutatedEvent @event = new(id, state);

            await _eventWriter.EnqueueAsync(@event, cancelToken);

            return unit;
        })
        select unit;
}
