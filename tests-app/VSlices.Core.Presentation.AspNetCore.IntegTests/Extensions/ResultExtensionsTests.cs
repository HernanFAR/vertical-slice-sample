using FluentAssertions;
using System.Diagnostics;
using FluentAssertions.Primitives;
using LanguageExt;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using static LanguageExt.Prelude;
using Failures = VSlices.Base.Failures;
using VSlices.Base.Failures;

namespace VSlices.Core.Presentation.AspNetCore.IntegTests.Extensions;

public class ResponseExtensionsTests
{
    [Fact]
    public void MatchResult_ShouldAwaitThenCallSuccessFunction()
    {
        Eff<Unit> oneOf = unitEff;

        _ = (oneOf.Run()
                  .MatchResult(_ => TypedResults.Ok()))
            .Should()
            .BeOfType<Ok>();

    }

    [Fact]
    public void MatchResult_ShouldCallSuccessFunction()
    {
        Fin<Unit> oneOf = unit;

        oneOf.MatchResult(_ => TypedResults.Ok())
            .Should()
            .BeOfType<Ok>();

    }

    [Fact]
    public void MatchResult_ShouldCallReturnProblemHttpResult_BadRequestStatusCode()
    {
        const string expTitle = "Title";

        Fin<Unit> oneOf = Fin<Unit>.Fail(new Failures.BadRequest(expTitle));

        ProblemDetails result = oneOf.MatchResult(_ => throw new UnreachableException())
            .Should()
            .BeOfType<ProblemHttpResult>()
            .Subject.ProblemDetails;

        result.Status.Should().Be(StatusCodes.Status400BadRequest);
        result.Detail.Should().Be(expTitle);

    }

    [Fact]
    public void MatchResult_ShouldCallReturnUnauthorizedResult()
    {
        const string expTitle = "Title";

        Fin<Unit> oneOf = Fin<Unit>.Fail(new Failures.Unauthenticated(expTitle));

        ProblemDetails result = oneOf.MatchResult(_ => throw new UnreachableException())
            .Should()
            .BeOfType<ProblemHttpResult>()
            .Subject.ProblemDetails;

        result.Status.Should().Be(StatusCodes.Status401Unauthorized);
        result.Detail.Should().Be(expTitle);

    }

    [Fact]
    public void MatchResult_ShouldCallReturnProblemHttpResult_ForbiddenStatusCode()
    {
        const string expTitle = "Title";

        Fin<Unit> oneOf = Fin<Unit>.Fail(new Failures.Forbidden(expTitle));

        ProblemDetails result = oneOf.MatchResult(_ => throw new UnreachableException())
            .Should()
            .BeOfType<ProblemHttpResult>()
            .Subject.ProblemDetails;

        result.Status.Should().Be(StatusCodes.Status403Forbidden);
        result.Detail.Should().Be(expTitle);
    }

    [Fact]
    public void MatchResult_ShouldCallReturnProblemHttpResult_NotFoundStatusCode()
    {
        const string expTitle = "Title";

        Fin<Unit> oneOf = Fin<Unit>.Fail(new Failures.NotFound(expTitle));

        ProblemDetails result = oneOf.MatchResult(_ => throw new UnreachableException())
            .Should()
            .BeOfType<ProblemHttpResult>()
            .Subject.ProblemDetails;

        result.Status.Should().Be(StatusCodes.Status404NotFound);
        result.Detail.Should().Be(expTitle);
    }

    [Fact]
    public void MatchResult_ShouldCallReturnConflict()
    {
        const string expTitle = "Title";

        Fin<Unit> oneOf = Fin<Unit>.Fail(new Failures.Conflict(expTitle));

        ProblemDetails result = oneOf.MatchResult(_ => throw new UnreachableException())
            .Should()
            .BeOfType<ProblemHttpResult>()
            .Subject.ProblemDetails;

        result.Status.Should().Be(StatusCodes.Status409Conflict);
        result.Detail.Should().Be(expTitle);
    }

    [Fact]
    public void MatchResult_ShouldCallReturnGone()
    {
        const string expTitle = "Title";

        Fin<Unit> oneOf = Fin<Unit>.Fail(new Failures.Gone(expTitle));

        ProblemDetails result = oneOf.MatchResult(_ => throw new UnreachableException())
            .Should()
            .BeOfType<ProblemHttpResult>()
            .Subject.ProblemDetails;

        result.Status.Should().Be(StatusCodes.Status410Gone);
        result.Detail.Should().Be(expTitle);
    }

    [Fact]
    public void MatchResult_ShouldCallReturnIAmTeapot()
    {
        const string expTitle = "Title";

        Fin<Unit> oneOf = Fin<Unit>.Fail(new Failures.IAmTeapot(expTitle));

        ProblemDetails result = oneOf.MatchResult(_ => throw new UnreachableException())
            .Should()
            .BeOfType<ProblemHttpResult>()
            .Subject.ProblemDetails;

        result.Status.Should().Be(StatusCodes.Status418ImATeapot);
        result.Detail.Should().Be(expTitle);
    }

    [Fact]
    public void MatchResult_ShouldCallReturnUnprocessableEntity_DetailWithErrors()
    {
        const string expTitle = "Title";
        const string expErrorName1 = "ErrorName1";
        const string expErrorName2 = "ErrorName2";
        const string expErrorDetail1_1 = "ErrorDetail1";
        const string expErrorDetail1_2 = "ErrorDetail2";
        const string expErrorDetail2_1 = "ErrorDetail3";

        ValidationDetail[] errors =
        [
            new ValidationDetail(expErrorName1, expErrorDetail1_1),
            new ValidationDetail(expErrorName1, expErrorDetail1_2),
            new ValidationDetail(expErrorName2, expErrorDetail2_1)
        ];

        Fin<Unit> oneOf = Fin<Unit>.Fail(new Failures.Unprocessable(expTitle, errors));

        ProblemDetails result = oneOf.MatchResult(_ => throw new UnreachableException())
            .Should()
            .BeOfType<ProblemHttpResult>()
            .Subject.ProblemDetails;

        result.Status.Should().Be(StatusCodes.Status422UnprocessableEntity);
        result.Detail.Should().Be(expTitle);
        ((Dictionary<string, string[]>)result.Extensions["errors"]
                .Should()
                .BeOfType<Dictionary<string, string[]>>()
            .And.Subject)
            .Should()
            .BeEquivalentTo(new Dictionary<string, string[]>
            {
                { expErrorName1, new [] { expErrorDetail1_1, expErrorDetail1_2 } },
                { expErrorName2, new [] { expErrorDetail2_1 } }
            });

    }

    [Fact]
    public void MatchResult_ShouldCallReturnUnprocessableEntity()
    {
        const string expTitle = "Title";

        Fin<Unit> oneOf = Fin<Unit>.Fail(new Failures.Locked(expTitle));

        ProblemDetails result = oneOf.MatchResult(_ => throw new UnreachableException())
            .Should()
            .BeOfType<ProblemHttpResult>()
            .Subject.ProblemDetails;

        result.Status.Should().Be(StatusCodes.Status423Locked);
        result.Detail.Should().Be(expTitle);

    }

    [Fact]
    public void MatchResult_ShouldCallReturnFailedDependency()
    {
        const string expTitle = "Title";

        Fin<Unit> oneOf = Fin<Unit>.Fail(new Failures.FailedDependency(expTitle));

        ProblemDetails result = oneOf.MatchResult(_ => throw new UnreachableException())
            .Should()
            .BeOfType<ProblemHttpResult>()
            .Subject.ProblemDetails;

        result.Status.Should().Be(StatusCodes.Status424FailedDependency);
        result.Detail.Should().Be(expTitle);

    }

    [Fact]
    public void MatchResult_ShouldCallReturnTooEarly()
    {
        const string expTitle = "Title";

        Fin<Unit> oneOf = Fin<Unit>.Fail(new Failures.TooEarly(expTitle));

        ProblemDetails result = oneOf.MatchResult(_ => throw new UnreachableException())
            .Should()
            .BeOfType<ProblemHttpResult>()
            .Subject.ProblemDetails;

        result.Status.Should().Be(425);
        result.Detail.Should().Be(expTitle);

    }
}