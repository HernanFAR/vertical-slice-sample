using System.Linq.Expressions;
using Crud.CrossCutting;
using Crud.Domain;
using Crud.Domain.DataAccess;
using Crud.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using VSlices.Base;
using VSlices.Base.Failures;
using VSlices.Infrastructure.Domain.EntityFrameworkCore;

namespace Crud.Infrastructure;

internal sealed class QuestionRepository : EfCoreRepository<AppDbContext, 
                                                            Question,
                                                            QuestionId, 
                                                            TQuestion>, 
                                           IQuestionRepository
{
    public Eff<VSlicesRuntime, bool> Exists(QuestionId id, NonEmptyString name) =>
        from token in cancelToken
        from context in provide<AppDbContext>()
        from exist in liftEff(() => context
                                    .Questions
                                    .Where(e => e.Id != id.Value)
                                    .AnyAsync(x => x.Text == name.Value, token))
        select exist;

    public Eff<VSlicesRuntime, bool> Exists(NonEmptyString text) =>
        from token in cancelToken
        from context in provide<AppDbContext>()
        from exist in liftEff(() => context
                                    .Questions
                                    .AnyAsync(x => x.Text == text.Value, token))
        select exist;

    public override Expression<Func<TQuestion, bool>> DomainKeySelector(QuestionId key) =>
        question => question.Id == key.Value;

    public override Question ToDomain(TQuestion projection) =>
        Question.Create(QuestionId.From(projection.Id), projection.Text.ToNonEmpty());

    public override TQuestion ToProjection(Question root) => 
        new()
        {
            Id = root.Id.Value,
            Text = root.Text.Value
        };
}
