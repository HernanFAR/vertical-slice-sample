using Crud.CrossCutting;
using Crud.Domain;
using Crud.Domain.Repositories;
using Crud.Domain.ValueObjects;
using LanguageExt.Effects.Traits;
using LanguageExt.SysX.Live;
using Microsoft.EntityFrameworkCore;
using VSlices.Base.Failures;

namespace Crud.Infrastructure;

public sealed class EfQuestionRepository(AppDbContext context) : IQuestionRepository
{
    readonly AppDbContext _context = context;

    public Aff<Runtime, Unit> Create(Question question) =>
        from cancelToken in cancelToken<Runtime>()
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


    public Aff<Runtime, Question> Read(QuestionId questionId) =>
        from question_ in ReadModel(questionId.Value)
        select new Question(new QuestionId(question_.Id), new NonEmptyString(question_.Text));

    public Aff<Runtime, Unit> Update(Question question) =>
        from cancelToken in cancelToken<Runtime>()
        from question_ in ReadModel(question.Id.Value)
        from _ in Aff(async () =>
        {
            question_.Text = question.Text.Value;

            _context.Questions.Update(question_);
            await _context.SaveChangesAsync(cancelToken);

            return unit;
        })
        select unit;

    public Aff<Runtime, bool> Exists(QuestionId id) =>
        from cancelToken in cancelToken<Runtime>()
        from exist in Aff(async () => await _context
            .Questions
            .AnyAsync(x => x.Id == id.Value, cancelToken))
        select exist;

    public Aff<Runtime, bool> Exists(NonEmptyString text) =>
        from cancelToken in cancelToken<Runtime>()
        from exist in Aff(async () => await _context
            .Questions
            .AnyAsync(x => x.Text == text.Value, cancelToken))
        select exist;

    public Aff<Runtime, Unit> Delete(Question question) =>
        from cancelToken in cancelToken<Runtime>()
        from question_ in ReadModel(question.Id.Value)
        from _ in Aff(async () =>
        {
            _context.Questions.Remove(question_);

            await _context.SaveChangesAsync(cancelToken);

            return unit;
        })
        select unit;

    Aff<Runtime, TQuestion> ReadModel(Guid id) =>
        from cancelToken in cancelToken<Runtime>()
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
