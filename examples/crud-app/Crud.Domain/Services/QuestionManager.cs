using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crud.Domain.Events;
using Crud.Domain.Repositories;
using VSlices.Core.Events;

namespace Crud.Domain.Services;

public sealed class QuestionManager(IQuestionRepository repository, IEventQueueWriter eventWriter)
{
    readonly IQuestionRepository _repository = repository;
    readonly IEventQueueWriter _eventWriter = eventWriter;

    public Aff<Unit> CreateAsync(string text, CancellationToken cancellationToken) =>
        from question in Eff(() => Question.Create(text))
        from _1 in _repository.CreateAsync(question, cancellationToken)
        from _2 in PublishEventCore(question.Id, EState.Created, cancellationToken)
        select unit;

    public Aff<Unit> UpdateAsync(Question question, CancellationToken cancellationToken) =>
        from _1 in _repository.UpdateAsync(question, cancellationToken)
        from _2 in PublishEventCore(question.Id, EState.Updated, cancellationToken)
        select unit;

    public Aff<Unit> DeleteAsync(Question question, CancellationToken cancellationToken) =>
        from _1 in _repository.DeleteAsync(question, cancellationToken)
        from _2 in PublishEventCore(question.Id, EState.Removed, cancellationToken)
        select unit;

    private Aff<Unit> PublishEventCore(QuestionId id, EState state, CancellationToken cancellationToken) =>
        from _ in Aff(async () =>
        {
            var @event = new QuestionMutatedEvent(id, state);

            await _eventWriter.EnqueueAsync(@event, cancellationToken);

            return unit;
        })
        select unit;

}
