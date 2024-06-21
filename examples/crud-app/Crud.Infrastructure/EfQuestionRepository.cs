using Crud.CrossCutting;
using Crud.Domain;
using Crud.Domain.Repositories;
using LanguageExt.SomeHelp;
using Microsoft.EntityFrameworkCore;
using VSlices.Base.Failures;

namespace Crud.Infrastructure;

public sealed class EfQuestionRepository(AppDbContext context) : IQuestionRepository
{
    readonly AppDbContext _context = context;

    public async ValueTask<Fin<Unit>> CreateAsync(Question question, CancellationToken cancellationToken)
    {
        Aff<Unit> effect = from question_ in SuccessAff(new TQuestion
                           {
                               Id = question.Id.Value,
                               Text = question.Text
                           })
                           from _ in Aff(async () =>
                           {
                               question_.Text = question.Text;

                               _context.Questions.Add(question_);
                               await _context.SaveChangesAsync(cancellationToken);

                               return unit;
                           })
                           select unit;

        return await effect.Run();
    }


    public async ValueTask<Fin<Question>> ReadAsync(QuestionId requestId, CancellationToken cancellationToken)
    {
        Aff<Question> effect = from question_ in ReadModel(requestId.Value, cancellationToken)
            select new Question(new QuestionId(question_.Id), question_.Text);

        return await effect.Run();
    }

    public async ValueTask<Fin<Unit>> UpdateAsync(Question question, CancellationToken cancellationToken)
    {
        Aff<Unit> effect = from question_ in ReadModel(question.Id.Value, cancellationToken)
                           from _ in Aff(async () =>
                           {
                               question_.Text = question.Text;

                               _context.Questions.Update(question_);
                               await _context.SaveChangesAsync(cancellationToken);

                               return unit;
                           })
                           select unit;

        return await effect.Run();
    }

    public async ValueTask<Fin<bool>> ExistsAsync(QuestionId id, CancellationToken cancellationToken)
    {
        bool exists = await _context.Questions
            .AnyAsync(x => x.Id == id.Value, cancellationToken);

        return FinSucc(exists);
    }

    public async ValueTask<Fin<Unit>> DeleteAsync(Question question, CancellationToken cancellationToken)
    {
        Aff<Unit> effect = from question_ in ReadModel(question.Id.Value, cancellationToken)
                           from _ in Aff(async () =>
                           {
                               _context.Questions.Remove(question_);

                               await _context.SaveChangesAsync(cancellationToken);

                               return unit;
                           })
                           select unit;

        return await effect.Run();
    }

    Aff<TQuestion> ReadModel(Guid id, CancellationToken cancellationToken) =>
        from question in AffMaybe<TQuestion>(async () =>
        {
            TQuestion? question = await _context.Questions.Where(x => x.Id == id).FirstOrDefaultAsync(cancellationToken);

            return question is not null 
                ? question
                : new NotFound("La pregunta no se ha encontrado");
        })
        select question;
}
