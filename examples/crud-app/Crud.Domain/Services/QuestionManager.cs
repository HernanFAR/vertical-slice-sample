﻿using Crud.Domain.DataAccess;
using Crud.Domain.Events;
using Crud.Domain.ValueObjects;
using VSlices.Base;
using VSlices.Core;
using VSlices.Core.Events;

namespace Crud.Domain.Services;

public sealed class QuestionManager
{
    public Eff<VSlicesRuntime, Unit> Create(QuestionId id, NonEmptyString text)  =>
        from question in liftEff(() => Question.Create(id, text))
        from unitOfWork in provide<IAppUnitOfWork>()
        from _1 in unitOfWork.Questions.Add(question)
        from _2 in unitOfWork.SaveChanges()
        from _3 in PublishEventCore(question.Id, EState.Created)
        select unit;

    public Eff<VSlicesRuntime, Unit> Create(NonEmptyString text) =>
        from question in liftEff(() => Question.Create(text))
        from unitOfWork in provide<IAppUnitOfWork>()
        from _1 in unitOfWork.Questions.Add(question)
        from _2 in unitOfWork.SaveChanges()
        from _3 in PublishEventCore(question.Id, EState.Created)
        select unit;

    public Eff<VSlicesRuntime, Unit> Update(Question question) =>
        from unitOfWork in provide<IAppUnitOfWork>()
        from _1 in unitOfWork.Questions.Update(question)
        from _2 in unitOfWork.SaveChanges()
        from _3 in PublishEventCore(question.Id, EState.Updated)
        select unit;

    public Eff<VSlicesRuntime, Unit> Delete(Question question) =>
        from unitOfWork in provide<IAppUnitOfWork>()
        from _1 in unitOfWork.Questions.Delete(question)
        from _2 in unitOfWork.SaveChanges()
        from _3 in PublishEventCore(question.Id, EState.Removed)
        select unit;

    private Eff<VSlicesRuntime, Unit> PublishEventCore(QuestionId id, EState state) =>
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
