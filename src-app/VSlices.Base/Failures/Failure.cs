using LanguageExt.Common;

namespace VSlices.Base.Failures;

/// <summary>
/// A subtype of <see cref="Expected"/> that might be extended to include failure details
/// </summary>
public sealed record ExtensibleExpected(string Message, int Code, Dictionary<string, object?> Extensions)
    : Expected(Message, Code)
{
    /// <inheritdoc />
    public override string ToString()
    {
        var extensions = string.Join(", ", Extensions.Select(x => $"{x.Key}: {x.Value}"));

        return $"Code: {Code}, {Message}. Extensions: {extensions}";
    }

    /// <summary>
    /// Generates a bad request error (code 400)
    /// </summary>
    public static ExtensibleExpected BadRequest(string message, Dictionary<string, object?> extensions)
    {
        return new ExtensibleExpected(message, 400, extensions);
    }

    /// <summary>
    /// Generates an unauthenticated error (code 401)
    /// </summary>
    public static ExtensibleExpected Unauthenticated(string message, Dictionary<string, object?> extensions)
    {
        return new ExtensibleExpected(message, 401, extensions);
    }

    /// <summary>
    /// Generates an unauthenticated error (code 403)
    /// </summary>
    public static ExtensibleExpected Forbidden(string message, Dictionary<string, object?> extensions)
    {
        return new ExtensibleExpected(message, 403, extensions);
    }

    /// <summary>
    /// Generates a not found error (code 404)
    /// </summary>
    public static ExtensibleExpected NotFound(string message, Dictionary<string, object?> extensions)
    {
        return new ExtensibleExpected(message, 404, extensions);
    }

    /// <summary>
    /// Generates a conflict error (code 409)
    /// </summary>
    public static ExtensibleExpected Conflict(string message, Dictionary<string, object?> extensions)
    {
        return new ExtensibleExpected(message, 409, extensions);
    }

    /// <summary>
    /// Generates a gone error (code 410)
    /// </summary>
    public static ExtensibleExpected Gone(string message, Dictionary<string, object?> extensions)
    {
        return new ExtensibleExpected(message, 410, extensions);
    }

    /// <summary>
    /// Generates a teapot error (code 418)
    /// </summary>
    public static ExtensibleExpected IAmTeapot(string message, Dictionary<string, object?> extensions)
    {
        return new ExtensibleExpected(message, 418, extensions);
    }

    /// <summary>
    /// Generates an unprocessable error (code 422)
    /// </summary>
    public static ExtensibleExpected Unprocessable(string message, 
                                                   IEnumerable<ValidationDetail> errors,
                                                   Dictionary<string, object?> extensions)
    {
        extensions[nameof(errors)] = errors
                                     .Select(x => x.Name)
                                     .Distinct()
                                     .ToDictionary(propertyName => propertyName,
                                                   propertyName => errors
                                                                   .Where(x => x.Name == propertyName)
                                                                   .Select(e => e.Detail)
                                                                   .ToArray());

        return new ExtensibleExpected(message, 422, extensions);
    }

    /// <summary>
    /// Generates a locked error (code 423)
    /// </summary>  
    public static ExtensibleExpected Locked(string message, Dictionary<string, object?> extensions)
    {
        return new ExtensibleExpected(message, 423, extensions);
    }

    /// <summary>
    /// Generates a failed dependency error (code 424)
    /// </summary>  
    public static ExtensibleExpected FailedDependency(string message, Dictionary<string, object?> extensions)
    {
        return new ExtensibleExpected(message, 424, extensions);
    }   

    /// <summary>
    /// Generates a too early error (code 425)
    /// </summary>  
    public static ExtensibleExpected TooEarly(string message, Dictionary<string, object?> extensions)
    {
        return new ExtensibleExpected(message, 425, extensions);
    }

    /// <summary>
    /// Generates a server error (code 500)
    /// </summary>  
    public static ExtensibleExpected ServerError(string message, Dictionary<string, object?> extensions)
    {
        return new ExtensibleExpected(message, 500, extensions);
    }
}
