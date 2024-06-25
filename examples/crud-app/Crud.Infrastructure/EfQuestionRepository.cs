using Crud.CrossCutting;
using Crud.Domain;
using Crud.Domain.Repositories;
using Crud.Domain.ValueObjects;
using LanguageExt.Effects.Traits;
using Microsoft.EntityFrameworkCore;
using VSlices.Base.Failures;

namespace Crud.Infrastructure;

public sealed class EfQuestionRepository(AppDbContext context) : IQuestionRepository
{
    readonly AppDbContext _context = context;

    public Aff<TRuntime, Unit> Create<TRuntime>(Question question)
        where TRuntime : struct, HasCancel<TRuntime> =>
        from cancelToken in cancelToken<TRuntime>()
        from question_ in SuccessAff(new TQuestion
        {
            Id = question.Id.Value,
            Text = question.Text.Value
        })
        from _ in Aff(async () =>
        {
            question_.Text = question.Text.Value;

            _context.Questions.Add(question_);
            await _context.SaveChangesAsync(cancelToken);

            return unit;
        })
        select unit;


    public Aff<TRuntime, Question> Read<TRuntime>(QuestionId questionId)
        where TRuntime : struct, HasCancel<TRuntime> =>
        from question_ in ReadModel<TRuntime>(questionId.Value)
        select new Question(new QuestionId(question_.Id), new NonEmptyString(question_.Text));

    public Aff<TRuntime, Unit> Update<TRuntime>(Question question)
        where TRuntime : struct, HasCancel<TRuntime> =>
        from cancelToken in cancelToken<TRuntime>()
        from question_ in ReadModel<TRuntime>(question.Id.Value)
        from _ in Aff(async () =>
        {
            question_.Text = question.Text.Value;

            _context.Questions.Update(question_);
            await _context.SaveChangesAsync(cancelToken);

            return unit;
        })
        select unit;

    public Aff<TRuntime, bool> Exists<TRuntime>(QuestionId id)
        where TRuntime : struct, HasCancel<TRuntime> =>
        from cancelToken in cancelToken<TRuntime>()
        from exist in Aff(async () => await _context
            .Questions
            .AnyAsync(x => x.Id == id.Value, cancelToken))
        select exist;

    public Aff<TRuntime, bool> Exists<TRuntime>(NonEmptyString text)
        where TRuntime : struct, HasCancel<TRuntime> =>
        from cancelToken in cancelToken<TRuntime>()
        from exist in Aff(async () => await _context
            .Questions
            .AnyAsync(x => x.Text == text.Value, cancelToken))
        select exist;

    public Aff<TRuntime, Unit> Delete<TRuntime>(Question question)
        where TRuntime : struct, HasCancel<TRuntime> =>
        from cancelToken in cancelToken<TRuntime>()
        from question_ in ReadModel<TRuntime>(question.Id.Value)
        from _ in Aff(async () =>
        {
            _context.Questions.Remove(question_);

            await _context.SaveChangesAsync(cancelToken);

            return unit;
        })
        select unit;

    Aff<TRuntime, TQuestion> ReadModel<TRuntime>(Guid id)
        where TRuntime : struct, HasCancel<TRuntime> =>
        from cancelToken in cancelToken<TRuntime>()
        from question in AffMaybe<TQuestion>(async () =>
        {
            TQuestion? question = await _context.Questions
                .Where(x => x.Id == id)
                .FirstOrDefaultAsync(cancelToken);

            return question is not null 
                ? question
                : new NotFound("La pregunta no se ha encontrado");
        })
        select question;
}
