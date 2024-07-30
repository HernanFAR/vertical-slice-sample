using LanguageExt;
using LanguageExt.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

// ReSharper disable once CheckNamespace
namespace VSlices.Base.Failures;

/// <summary>
/// <see cref="Fin{A}"/> extensions to match AspNetCore's <see cref="IResult"/>
/// </summary>
public static class ResponseExtensions
{
    /// <summary>
    /// Maps a <see cref="Fin{TResult}"/> to a <see cref="IResult"/>, using the provided function in success case.
    /// <para>For the errors, returns a <see cref="ProblemDetails"/>, which is an implementation of <see href="https://datatracker.ietf.org/doc/html/rfc7807"/></para>
    /// </summary>
    /// <typeparam name="TSuccess">Return type in success</typeparam>
    /// <param name="result">Result</param>
    /// <param name="Succ">Function to execute in success case</param>
    /// <param name="Fail">Optional function to execute in failure case, defaults to convert ExtensibleExpectedError to ProblemDetails</param>
    /// <returns>The <see cref="IResult"/> of the use case</returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static IResult MatchResult<TSuccess>(this Fin<TSuccess> result,
        Func<TSuccess, IResult> Succ,
        Func<Error, IResult>? Fail = null)
    {
        return result.Match(Succ,
            error => Fail is not null
                ? error switch
                {
                    ExtensibleExpectedError extensible => Fail(extensible),
                    _ => TypedResults.Problem(statusCode: 500)
                }
                : error switch
                {
                    ExtensibleExpectedError extensible => TypedResults.Problem(extensible.ToProblemDetails()),
                    _ => TypedResults.Problem(statusCode: 500)
                });
    }
    /// <summary>
    /// Maps a <see cref="Fin{TResult}"/> to a <see cref="IResult"/>, using the provided function in success case.
    /// <para>For the errors, returns a <see cref="ProblemDetails"/>, which is an implementation of <see href="https://datatracker.ietf.org/doc/html/rfc7807"/></para>
    /// </summary>
    /// <typeparam name="TSuccess">Return type in success</typeparam>
    /// <param name="result">Result</param>
    /// <param name="Succ">Function to execute in success case</param>
    /// <param name="Fail">Optional function to execute in failure case, defaults to convert ExtensibleExpectedError to ProblemDetails</param>
    /// <returns>The <see cref="IResult"/> of the use case</returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static IResult MatchResult<TSuccess>(this Fin<TSuccess> result,
                                                IResult Succ,
                                                Func<Error, IResult>? Fail = null)
    {
        return result.MatchResult(_ => Succ, Fail);
    }
}