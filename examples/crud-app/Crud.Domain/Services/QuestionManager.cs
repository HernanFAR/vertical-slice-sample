using Crud.Domain.Events;
using Crud.Domain.Repositories;
using Crud.Domain.ValueObjects;
using VSlices.Core;
using VSlices.Core.Events;

namespace Crud.Domain.Services;

public sealed class QuestionManager
{
    public Eff<HandlerRuntime, Unit> Create(QuestionId id, NonEmptyString text)  =>
        from question in liftEff(() => new Question(id, text))
        from repository in provide<IQuestionRepository>()
        from _1 in repository.Create(question)
        from _2 in PublishEventCore(question.Id, EState.Created)
        select unit;

    public Eff<HandlerRuntime, Unit> Create(NonEmptyString text) =>
        from question in liftEff(() => Question.Create(text))
        from repository in provide<IQuestionRepository>()
        from _1 in repository.Create(question)
        from _2 in PublishEventCore(question.Id, EState.Created)
        select unit;

    public Eff<HandlerRuntime, Unit> Update(Question question) =>
        from repository in provide<IQuestionRepository>()
        from _1 in repository.Update(question)
        from _2 in PublishEventCore(question.Id, EState.Updated)
        select unit;

    public Eff<HandlerRuntime, Unit> Delete(Question question) =>
        from repository in provide<IQuestionRepository>()
        from _1 in repository.Delete(question)
        from _2 in PublishEventCore(question.Id, EState.Removed)
        select unit;

    private Eff<HandlerRuntime, Unit> PublishEventCore(QuestionId id, EState state) =>
        from token in cancelToken
        from eventWriter in provide<IEventQueueWriter>()
        from _ in liftEff(async () =>
        {
            QuestionMutatedEvent @event = new(id, state);

            await eventWriter.EnqueueAsync(@event, token);

            return unit;
        })
        select unit;
}
