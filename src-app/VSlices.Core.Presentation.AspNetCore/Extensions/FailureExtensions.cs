using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

// ReSharper disable once CheckNamespace
namespace VSlices.Base.Failures;

/// <summary>
/// <see cref="ExtensibleExpectedError"/> extensions to convert into <see cref="ProblemDetails"/>
/// </summary>
public static class BusinessFailureExtensions
{
    /// <summary>
    /// Converts an <see cref="ExtensibleExpectedError"/> instance into a <see cref="ProblemDetails"/>
    /// </summary>
    /// <param name="failure">Failure</param>
    /// <returns>ProblemDetails instance</returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static ProblemDetails ToProblemDetails(this ExtensibleExpectedError failure)
    {
        var problemDetails = new ProblemDetails
        {
            Status = failure.Code,
            Detail = failure.Message
        };

        foreach ((string key, object? value) in failure.Extensions)
        {
            problemDetails.Extensions[key] = value;
        }

        if (failure is not Unprocessable unprocessable)
        {
            return problemDetails;
        }

        problemDetails.Extensions["errors"] = unprocessable.Errors
            .Select(x => x.Name)
            .Distinct()
            .ToDictionary(
                propertyName => propertyName,
                propertyName => unprocessable.Errors
                    .Where(x => x.Name == propertyName)
                    .Select(e => e.Detail)
                    .ToArray());

        return problemDetails;
    }
}