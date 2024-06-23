namespace Crud.Domain.Repositories;

public interface IQuestionRepository
{
    Aff<Unit> CreateAsync(Question question, CancellationToken cancellationToken);

    Aff<Question> ReadAsync(QuestionId requestId, CancellationToken cancellationToken);

    Aff<Unit> UpdateAsync(Question question, CancellationToken cancellationToken);

    Aff<bool> ExistsAsync(QuestionId id, CancellationToken cancellationToken);

    Aff<Unit> DeleteAsync(Question question, CancellationToken cancellationToken);
}
