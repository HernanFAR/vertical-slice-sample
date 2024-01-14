using System.Diagnostics;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using VSlices.Base.Responses;

namespace VSlices.Core.Presentation.AspNetCore.IntegTests.Extensions;

public class ResponseExtensionsTests
{
    [Fact]
    public void MatchEndpointResult_ShouldCallSuccessFunction()
    {
        Result<Success> oneOf = Success.Value;

        var result = oneOf.MatchEndpointResult(_ => TypedResults.Ok());

        result.Should().BeOfType<Ok>();
    }

    [Fact]
    public void MatchEndpointResult_ShouldCallReturnProblemHttpResult_BadRequestStatusCode()
    {
        const string expTitle = "Title";
        const string expDetail = "Detail";

        Result<Success> oneOf = new Failure(FailureKind.Unspecified, Title: expTitle, Detail: expDetail);

        var result = oneOf.MatchEndpointResult(_ => throw new UnreachableException());

        var problemDetails = result.Should().BeOfType<ProblemHttpResult>()
            .Subject.ProblemDetails;

        problemDetails.Status.Should().Be(StatusCodes.Status400BadRequest);
        problemDetails.Title.Should().Be(expTitle);
        problemDetails.Detail.Should().Be(expDetail);

    }

    [Fact]
    public void MatchEndpointResult_ShouldCallReturnUnauthorizedResult()
    {
        const string expTitle = "Title";
        const string expDetail = "Detail";

        Result<Success> oneOf = new Failure(FailureKind.UserNotAuthenticated, 
            Title: expTitle, Detail: expDetail);

        var result = oneOf.MatchEndpointResult(_ => throw new UnreachableException());

        var problemDetails = result.Should().BeOfType<ProblemHttpResult>()
            .Subject.ProblemDetails;

        problemDetails.Status.Should().Be(StatusCodes.Status401Unauthorized);
        problemDetails.Title.Should().Be(expTitle);
        problemDetails.Detail.Should().Be(expDetail);

    }

    [Fact]
    public void MatchEndpointResult_ShouldCallReturnProblemHttpResult_ForbiddenStatusCode()
    {
        const string expTitle = "Title";
        const string expDetail = "Detail";

        Result<Success> oneOf = new Failure(FailureKind.UserNotAllowed, 
            Title: expTitle, Detail: expDetail);

        var result = oneOf.MatchEndpointResult(_ => throw new UnreachableException());

        var problemDetails = result.Should().BeOfType<ProblemHttpResult>()
            .Subject.ProblemDetails;

        problemDetails.Status.Should().Be(StatusCodes.Status403Forbidden);
        problemDetails.Title.Should().Be(expTitle);
        problemDetails.Detail.Should().Be(expDetail);
    }

    [Fact]
    public void MatchEndpointResult_ShouldCallReturnProblemHttpResult_NotFoundStatusCode()
    {
        const string expTitle = "Title";
        const string expDetail = "Detail";

        Result<Success> oneOf = new Failure(FailureKind.ResourceNotFound,
            Title: expTitle, Detail: expDetail);

        var result = oneOf.MatchEndpointResult(_ => throw new UnreachableException());

        var problemDetails = result.Should().BeOfType<ProblemHttpResult>()
            .Subject.ProblemDetails;

        problemDetails.Status.Should().Be(StatusCodes.Status404NotFound);
        problemDetails.Title.Should().Be(expTitle);
        problemDetails.Detail.Should().Be(expDetail);
    }

    [Fact]
    public void MatchEndpointResult_ShouldCallReturnConflict()
    {
        const string expTitle = "Title";
        const string expDetail = "Detail";

        Result<Success> oneOf = new Failure(FailureKind.ConcurrencyError,
            Title: expTitle, Detail: expDetail);

        var result = oneOf.MatchEndpointResult(_ => throw new UnreachableException());

        var problemDetails = result.Should().BeOfType<ProblemHttpResult>()
            .Subject.ProblemDetails;

        problemDetails.Status.Should().Be(StatusCodes.Status409Conflict);
        problemDetails.Title.Should().Be(expTitle);
        problemDetails.Detail.Should().Be(expDetail);
    }

    [Fact]
    public void MatchEndpointResult_ShouldCallReturnUnprocessableEntity_DetailWithErrors()
    {
        const string expTitle = "Title";
        const string expDetail = "Detail";
        const string expErrorName1 = "ErrorName1";
        const string expErrorName2 = "ErrorName2";
        const string expErrorDetail1_1 = "ErrorDetail1";
        const string expErrorDetail1_2 = "ErrorDetail2";
        const string expErrorDetail2_1 = "ErrorDetail3";
        var errors = new ValidationError[]
        {
            new (expErrorName1, expErrorDetail1_1),
            new (expErrorName1, expErrorDetail1_2),
            new (expErrorName2, expErrorDetail2_1)
        };

        Result<Success> oneOf = new Failure(FailureKind.ValidationError,
            Title: expTitle, 
            Detail: expDetail, 
            Errors: errors);

        var result = oneOf.MatchEndpointResult(_ => throw new UnreachableException());

        var problemDetails = result.Should().BeOfType<ProblemHttpResult>()
            .Subject.ProblemDetails;

        problemDetails.Status.Should().Be(StatusCodes.Status422UnprocessableEntity);
        problemDetails.Title.Should().Be(expTitle);
        problemDetails.Detail.Should().Be(expDetail);
        ((Dictionary<string, string[]>) problemDetails.Extensions["Errors"].Should().BeOfType<Dictionary<string, string[]>>()
            .And.Subject)
            .Should().BeEquivalentTo(new Dictionary<string, string[]>
            {
                { expErrorName1, new [] { expErrorDetail1_1, expErrorDetail1_2 } },
                { expErrorName2, new [] { expErrorDetail2_1 } }
            });

    }

    [Fact]
    public void MatchEndpointResult_ShouldCallReturnUnprocessableEntity_DetailWithErrorsAsExtensions()
    {
        const string expTitle = "Title";
        const string expDetail = "Detail";
        const string expErrorName1 = "ErrorName1";
        const string expErrorName2 = "ErrorName2";
        const string expErrorDetail1_1 = "ErrorDetail1";
        const string expErrorDetail1_2 = "ErrorDetail2";
        const string expErrorDetail2_1 = "ErrorDetail3";
        var errors = new ValidationError[]
        {
            new (expErrorName1, expErrorDetail1_1),
            new (expErrorName1, expErrorDetail1_2),
            new (expErrorName2, expErrorDetail2_1)
        };

        Result<Success> oneOf = new Failure(FailureKind.ValidationError,
            Title: expTitle, 
            Detail: expDetail, 
            CustomExtensions: new Dictionary<string, object?>()  { { "Errors", errors } });

        var result = oneOf.MatchEndpointResult(_ => throw new UnreachableException());

        var problemDetails = result.Should().BeOfType<ProblemHttpResult>()
            .Subject.ProblemDetails;

        problemDetails.Status.Should().Be(StatusCodes.Status422UnprocessableEntity);
        problemDetails.Title.Should().Be(expTitle);
        problemDetails.Detail.Should().Be(expDetail);
        problemDetails.Extensions["Errors"].Should().Be(errors);

    }

    [Fact]
    public void MatchEndpointResult_ShouldCallReturnStatusCodeHttpResult()
    {
        const string expTitle = "Title";
        const string expDetail = "Detail";

        Result<Success> oneOf = new Failure(FailureKind.UnhandledException,
            Title: expTitle, Detail: expDetail);

        var result = oneOf.MatchEndpointResult(_ => throw new UnreachableException());

        var problemDetails = result.Should().BeOfType<ProblemHttpResult>()
            .Subject.ProblemDetails;

        problemDetails.Status.Should().Be(StatusCodes.Status500InternalServerError);
        problemDetails.Title.Should().Be(expTitle);
        problemDetails.Detail.Should().Be(expDetail);

    }

    [Fact]
    public void MatchEndpointResult_ShouldThrowArgumentOutOfRange()
    {

        Result<Success> oneOf = new Failure((FailureKind)99);

        var act = () => oneOf.MatchEndpointResult(_ => throw new UnreachableException());

        act.Should().ThrowExactly<ArgumentOutOfRangeException>();

    }
}