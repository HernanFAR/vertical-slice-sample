using Crud.CrossCutting;
using Crud.Domain;
using Crud.Domain.Repositories;
using Crud.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using VSlices.Base.Failures;
using VSlices.Core;

namespace Crud.Infrastructure;

public sealed class EfQuestionRepository : IQuestionRepository
{
    public Eff<HandlerRuntime, Unit> Create(Question question) =>
        from token in cancelToken
        from context in provide<AppDbContext>()
        from question_ in liftEff(() => new TQuestion
                                        {
                                            Id   = question.Id.Value,
                                            Text = question.Text.Value
                                        })
        from _ in liftEff(async () =>
                          {
                              question_.Text = question.Text.Value;

                              context.Questions.Add(question_);
                              await context.SaveChangesAsync(token);

                              return unit;
                          })
        select unit;


    public Eff<HandlerRuntime, Question> Read(QuestionId questionId) =>
        from question_ in ReadModel(questionId.Value)
        select new Question(new QuestionId(question_.Id), new NonEmptyString(question_.Text));

    public Eff<HandlerRuntime, Unit> Update(Question question) =>
        from token in cancelToken
        from context in provide<AppDbContext>()
        from question_ in ReadModel(question.Id.Value)
        from _ in liftEff(async () =>
                          {
                              question_.Text = question.Text.Value;

                              context.Questions.Update(question_);
                              await context.SaveChangesAsync(token);

                              return unit;
                          })
        select unit;

    public Eff<HandlerRuntime, bool> Exists(QuestionId id) =>
        from token in cancelToken
        from context in provide<AppDbContext>()
        from exist in liftEff(() => context
                                    .Questions
                                    .AnyAsync(x => x.Id == id.Value, token))
        select exist;

    public Eff<HandlerRuntime, bool> Exists(QuestionId id, NonEmptyString name) =>
        from token in cancelToken
        from context in provide<AppDbContext>()
        from exist in liftEff(() => context
                                    .Questions
                                    .Where(e => e.Id != id.Value)
                                    .AnyAsync(x => x.Text == name.Value, token))
        select exist;

    public Eff<HandlerRuntime, bool> Exists(NonEmptyString text) =>
        from token in cancelToken
        from context in provide<AppDbContext>()
        from exist in liftEff(() => context
                                    .Questions
                                    .AnyAsync(x => x.Text == text.Value, token))
        select exist;

    public Eff<HandlerRuntime, Unit> Delete(Question question) =>
        from cancelToken in cancelToken
        from context in provide<AppDbContext>()
        from question_ in ReadModel(question.Id.Value)
        from _ in liftEff(async () =>
                          {
                              context.Questions.Remove(question_);

                              await context.SaveChangesAsync(cancelToken);

                              return unit;
                          })
        select unit;

    private Eff<HandlerRuntime, TQuestion> ReadModel(Guid id) =>
        from cancelToken in cancelToken
        from context in provide<AppDbContext>()
        from question in liftEff<TQuestion>(async () =>
        {
            TQuestion? question = await context.Questions
                .Where(x => x.Id == id)
                .FirstOrDefaultAsync(cancelToken);

            return question is not null
                ? question
                : new NotFound("La pregunta no se ha encontrado");
        })
        select question;
}
