using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

// ReSharper disable once CheckNamespace
namespace VSlices.Base.Failures;

/// <summary>
/// <see cref="ExtensibleExpected"/> extensions to convert into <see cref="ProblemDetails"/>
/// </summary>
public static class BusinessFailureExtensions
{
    /// <summary>
    /// Converts an <see cref="ExtensibleExpected"/> instance into a <see cref="ProblemDetails"/>
    /// </summary>
    /// <param name="failure">Failure</param>
    /// <returns>ProblemDetails instance</returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static ProblemDetails ToProblemDetails(this ExtensibleExpected failure)
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

        return problemDetails;
    }
}