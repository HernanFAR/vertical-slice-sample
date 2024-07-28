using LanguageExt.Common;

namespace VSlices.Base.Failures;

/// <summary>
/// A subtype of <see cref="Expected"/> that might be extended to include failure details
/// </summary>
public abstract record ExtensibleExpectedError(string Message, int Code, Dictionary<string, object?> Extensions)
    : Expected(Message, Code)
{
    public Error AsError() => this;
}

/// <summary>
/// Represents an extensible expected error, that can contain additional information
/// </summary>
/// <remarks>
/// This implementation can be used in a generic way
/// </remarks>
/// <param name="Message">Detail of the error</param>
/// <param name="Extensions">Additional information</param>
public sealed record BadRequest(string Message, Dictionary<string, object?> Extensions) : ExtensibleExpectedError(Message, 400, Extensions)
{
    /// <summary>
    /// Represents an expected error
    /// </summary>
    /// <remarks>
    /// This implementation can be used in a generic way
    /// </remarks>
    /// <param name="Message">Detail of the error</param>
    public BadRequest(string Message) : this(Message, new Dictionary<string, object?>()) { }
}

/// <summary>
/// Represents an extensible expected error, that can contain additional information
/// </summary>
/// <remarks>
/// This implementation must be used when dealing with authentication errors
/// </remarks>
/// <param name="Message">Detail of the error</param>
/// <param name="Extensions">Additional information</param>
public sealed record Unauthenticated(string Message, Dictionary<string, object?> Extensions) : ExtensibleExpectedError(Message, 401, Extensions)
{
    /// <summary>
    /// Represents an extensible expected error, that can contain additional information
    /// </summary>
    /// <remarks>
    /// This implementation must be used when dealing with authentication errors
    /// </remarks>
    /// <param name="Message">Detail of the error</param>
    public Unauthenticated(string Message) : this(Message, new Dictionary<string, object?>()) { }
}

/// <summary>
/// Represents an extensible expected error, that can contain additional information
/// </summary>
/// <remarks>
/// This implementation must be used when dealing with authorization errors
/// </remarks>
/// <param name="Message">Detail of the error</param>
/// <param name="Extensions">Additional information</param>
public sealed record Forbidden(string Message, Dictionary<string, object?> Extensions) : ExtensibleExpectedError(Message, 403, Extensions)
{
    /// <summary>
    /// Represents an extensible expected error, that can contain additional information
    /// </summary>
    /// <remarks>
    /// This implementation must be used when dealing with authorization errors
    /// </remarks>
    /// <param name="Message">Detail of the error</param>
    public Forbidden(string Message) : this(Message, new Dictionary<string, object?>()) { }
}

/// <summary>
/// Represents an extensible expected error, that can contain additional information
/// </summary>
/// <remarks>
/// This implementation must be used when a resource is not found, but because it never exist
/// </remarks>
/// <param name="Message">Detail of the error</param>
/// <param name="Extensions">Additional information</param>
public sealed record NotFound(string Message, Dictionary<string, object?> Extensions) : ExtensibleExpectedError(Message, 404, Extensions)
{
    /// <summary>
    /// Represents an extensible expected error, that can contain additional information
    /// </summary>
    /// <remarks>
    /// This implementation must be used when a resource is not found, but because it never exist
    /// </remarks>
    /// <param name="Message">Detail of the error</param>
    public NotFound(string Message) : this(Message, new Dictionary<string, object?>()) { }
}

/// <summary>
/// Represents an extensible expected error, that can contain additional information
/// </summary>
/// <remarks>
/// This implementation must be used when there is a conflict in the requested resource, like a concurrency one
/// </remarks>
/// <param name="Message">Detail of the error</param>
/// <param name="Extensions">Additional information</param>
public sealed record Conflict(string Message, Dictionary<string, object?> Extensions) : ExtensibleExpectedError(Message, 409, Extensions)
{
    /// <summary>
    /// Represents an extensible expected error, that can contain additional information
    /// </summary>
    /// <remarks>
    /// This implementation must be used when there is a conflict in the requested resource, like a concurrency one
    /// </remarks>
    /// <param name="Message">Detail of the error</param>
    public Conflict(string Message) : this(Message, new Dictionary<string, object?>()) { }
}

/// <summary>
/// Represents an extensible expected error, that can contain additional information
/// </summary>
/// <remarks>
/// This implementation must be used when a resource exist but is no longer available (Like a entity with soft delete/trashed status)
/// </remarks>
/// <param name="Message">Detail of the error</param>
/// <param name="Extensions">Additional information</param>
public sealed record Gone(string Message, Dictionary<string, object?> Extensions) : ExtensibleExpectedError(Message, 410, Extensions)
{
    /// <summary>
    /// Represents an extensible expected error, that can contain additional information
    /// </summary>
    /// <remarks>
    /// This implementation must be used when a resource exist but is no longer available (Like a entity with soft delete/trashed status)
    /// </remarks>
    /// <param name="Message">Detail of the error</param>
    public Gone(string Message) : this(Message, new Dictionary<string, object?>()) { }
}

/// <summary>
/// Represents an extensible expected error, that can contain additional information
/// </summary>
/// <remarks>
/// This implementation must be used when the request ask for coffee, but the server is a teapot
/// </remarks>
/// <param name="Message">Detail of the error</param>
/// <param name="Extensions">Additional information</param>
public sealed record IAmTeapot(string Message, Dictionary<string, object?> Extensions) : ExtensibleExpectedError(Message, 418, Extensions)
{
    /// <summary>
    /// Represents an extensible expected error, that can contain additional information
    /// </summary>
    /// <remarks>
    /// This implementation must be used when the request ask for coffee, but the server is a teapot
    /// </remarks>
    /// <param name="Message">Detail of the error</param>
    public IAmTeapot(string Message) : this(Message, new Dictionary<string, object?>()) { }
}

/// <summary>
/// Represents an extensible expected error, that can contain additional information
/// </summary>
/// <remarks>
/// This implementation must be used when the request have semactic errors
/// </remarks>
/// <param name="Message">Detail of the error</param>
/// <param name="Errors">Detail of the validation errors</param>
/// <param name="Extensions">Additional information</param>
public sealed record Unprocessable(string Message, ValidationDetail[] Errors, Dictionary<string, object?> Extensions) : ExtensibleExpectedError(Message, 422, Extensions)
{
    /// <summary>
    /// Represents an extensible expected error, that can contain additional information
    /// </summary>
    /// <remarks>
    /// This implementation must be used when the request have semactic errors
    /// </remarks>
    /// <param name="message">Detail of the error</param>
    /// <param name="errors">Detail of the validation errors</param>
    public Unprocessable(string message, ValidationDetail[] errors) : this(message, errors, new Dictionary<string, object?>()) { }
}

/// <summary>
/// Represents an extensible expected error, that can contain additional information
/// </summary>
/// <remarks>
/// This implementation must be used when the resource is locked by others
/// </remarks>
/// <param name="Message">Detail of the error</param>
/// <param name="Extensions">Additional information</param>
public sealed record Locked(string Message, Dictionary<string, object?> Extensions) : ExtensibleExpectedError(Message, 423, Extensions)
{
    /// <summary>
    /// Represents an extensible expected error, that can contain additional information
    /// </summary>
    /// <remarks>
    /// This implementation must be used when the resource is locked by others
    /// </remarks>
    /// <param name="Message">Detail of the error</param>
    public Locked(string Message) : this(Message, new Dictionary<string, object?>()) { }
}

/// <summary>
/// Represents an extensible expected error, that can contain additional information
/// </summary>
/// <remarks>
/// This implementation must be used when in the process of the request, a internal dependency (that is not the main process) failed
/// </remarks>
/// <param name="Message">Detail of the error</param>
/// <param name="Extensions">Additional information</param>
public sealed record FailedDependency(string Message, Dictionary<string, object?> Extensions) : ExtensibleExpectedError(Message, 424, Extensions)
{
    /// <summary>
    /// Represents an extensible expected error, that can contain additional information
    /// </summary>
    /// <remarks>
    /// This implementation must be used when in the process of the request, a internal dependency (that is not the main process) failed
    /// </remarks>
    /// <param name="Message">Detail of the error</param>
    public FailedDependency(string Message) : this(Message, new Dictionary<string, object?>()) { }
}

/// <summary>
/// Represents an extensible expected error, that can contain additional information
/// </summary>
/// <remarks>
/// This implementation must be used when the request was sent too early, and might derive in errors
/// </remarks>
/// <param name="Message">Detail of the error</param>
/// <param name="Extensions">Additional information</param>
public sealed record TooEarly(string Message, Dictionary<string, object?> Extensions) : ExtensibleExpectedError(Message, 425, Extensions)
{
    /// <summary>
    /// Represents an extensible expected error, that can contain additional information
    /// </summary>
    /// <remarks>
    /// This implementation must be used when the request was sent too early, and might derive in errors
    /// </remarks>
    /// <param name="Message">Detail of the error</param>
    public TooEarly(string Message) : this(Message, new Dictionary<string, object?>()) { }
}

/// <summary>
/// Represents an extensible expected error, that can contain additional information
/// </summary>
/// <remarks>
/// This implementation must be used when the server had an unhandled exception while processing the request
/// </remarks>
/// <param name="Message">Detail of the error</param>
/// <param name="Extensions">Additional information</param>
public sealed record ServerError(string Message, Dictionary<string, object?> Extensions) : ExtensibleExpectedError(Message, 500, Extensions)
{
    /// <summary>
    /// Represents an extensible expected error, that can contain additional information
    /// </summary>
    /// <remarks>
    /// This implementation must be used when the server had an unhandled exception while processing the request
    /// </remarks>
    /// <param name="Message">Detail of the error</param>
    public ServerError(string Message) : this(Message, new Dictionary<string, object?>()) { }
}

/// <summary>
/// Extensions to create <see cref="ExtensibleExpectedError"/> implementations
/// </summary>
public static class FailureExtensions
{
    public static Unprocessable ToUnprocessable(this IEnumerable<ValidationDetail> errorsDetails, string? message = null)
        => new(message ?? "No se ha podido", errorsDetails.ToArray());
}
