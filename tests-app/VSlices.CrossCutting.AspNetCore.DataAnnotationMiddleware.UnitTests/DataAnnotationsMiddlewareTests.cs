using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Runtime.InteropServices.JavaScript;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace VSlices.CrossCutting.AspNetCore.DataAnnotationMiddleware.UnitTests;

public class DataAnnotationsMiddlewareTests
{
    public sealed record Request(
        [property: Required(ErrorMessage = "Name is required")]
        string? Name, 
        [property: Range(0, 100, ErrorMessage = "Age is not valid")]
        int Age);
    [Fact]
    public async Task Process_Success_ShouldGoToNextAction()
    {
        var request = new Request("Name", 10);
        var @object = new object();
        var checker = new AutoResetEvent(false);
        var context = EndpointFilterInvocationContext.Create(
            new DefaultHttpContext(),
            request);

        EndpointFilterDelegate next = context =>
        {
            checker.Set();

            return ValueTask.FromResult<object?>(@object);
        };

        var result = await DataAnnotationValidationMiddleware
            .Process<Request>(context, next);

        Assert.Equal(@object, result);
        Assert.True(checker.WaitOne(1000)); 
    }

    [Fact]
    public async Task Process_Failure_ShouldReturnProblemDetails()
    {
        // Arrange
        var request = new Request(null, -1);
        var context = EndpointFilterInvocationContext.Create(
            new DefaultHttpContext(),
            request);

        EndpointFilterDelegate next = context => throw new UnreachableException();

        // Act
        var result = await DataAnnotationValidationMiddleware
            .Process<Request>(context, next);

        // Assert
        var httpResult = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, httpResult.StatusCode);
        Assert.Equal("application/problem+json", httpResult.ContentType);

        var problemDetails = Assert.IsType<HttpValidationProblemDetails>(httpResult.ProblemDetails);
        Assert.Equal("Name is required", problemDetails.Errors["Name"][0]);
        Assert.Equal("Age is not valid", problemDetails.Errors["Age"][0]);

    }
}
