using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

// ReSharper disable once CheckNamespace
namespace VSlices.Base.Responses;

/// <summary>
/// <see cref="Failure"/> extensions to convert into <see cref="ProblemDetails"/>
/// </summary>
public static class BusinessFailureExtensions
{
    /// <summary>
    /// Converts the <see cref="Failure"/> instance into a <see cref="ProblemDetails"/>
    /// </summary>
    /// <param name="failure">Failure</param>
    /// <returns>ProblemDetails instance</returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static ProblemDetails ToProblemDetails(this Failure failure)
    {
        var statusCode = failure.Kind switch
        {
            FailureKind.Unspecified => StatusCodes.Status400BadRequest,
            FailureKind.UserNotAuthenticated => StatusCodes.Status401Unauthorized,
            FailureKind.UserNotAllowed => StatusCodes.Status403Forbidden,
            FailureKind.ResourceNotFound => StatusCodes.Status404NotFound,
            FailureKind.ConcurrencyError => StatusCodes.Status409Conflict,
            FailureKind.ValidationError => StatusCodes.Status422UnprocessableEntity,
            FailureKind.UnhandledException => StatusCodes.Status500InternalServerError,
            _ => throw new ArgumentOutOfRangeException(nameof(failure))
        };

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Detail = failure.Detail,
            Title = failure.Title,
            Instance = failure.Instance,
            Type = failure.Type
        };

        if (failure.CustomExtensions is not null)
        {
            // Pass Failure.Extensions to ProblemDetails.Extensions 
            foreach (var (key, value) in failure.CustomExtensions)
            {
                problemDetails.Extensions[key] = value;
            }
        }

        if (failure.Errors is null)
        {
            return problemDetails;
        }

        problemDetails.Extensions["Errors"] = failure.Errors
            .Select(x => x.Name)
            .Distinct()
            .ToDictionary(
                propertyName => propertyName,
                propertyName => failure.Errors
                    .Where(x => x.Name == propertyName)
                    .Select(e => e.Detail)
                    .ToArray()); ;

        return problemDetails;
    }
}