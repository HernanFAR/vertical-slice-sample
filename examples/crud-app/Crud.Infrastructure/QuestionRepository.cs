﻿using System.Linq.Expressions;
using Crud.CrossCutting;
using Crud.Domain;
using Crud.Domain.Rules.DataAccess;
using Crud.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using VSlices.Base;
using VSlices.Infrastructure.Domain.EntityFrameworkCore;

namespace Crud.Infrastructure;

internal sealed class QuestionRepository : EfCoreRepository<AppDbContext, 
                                                            QuestionType,
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

    public override QuestionType ToDomain(TQuestion projection) => 
        new(QuestionId.New(projection.Id), CategoryType.Find(CategoryId.New(projection.CategoryId)), projection.Text.ToNonEmpty());

    public override TQuestion ToProjection(QuestionType root) => 
        new()
        {
            Id = root.Id.Value,
            Text = root.Text.Value
        };
}
