namespace Crud.Domain.Repositories;

public interface IQuestionRepository
{
    ValueTask<Fin<Unit>> CreateAsync(Question question, CancellationToken cancellationToken);

    ValueTask<Fin<Question>> ReadAsync(QuestionId requestId, CancellationToken cancellationToken);

    ValueTask<Fin<Unit>> UpdateAsync(Question question, CancellationToken cancellationToken);

    ValueTask<Fin<bool>> ExistsAsync(QuestionId id, CancellationToken cancellationToken);

    ValueTask<Fin<Unit>> DeleteAsync(Question question, CancellationToken cancellationToken);
}
