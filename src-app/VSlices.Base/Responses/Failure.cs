namespace VSlices.Base.Responses;

/// <summary>
/// Specifies different reasons for a business failure
/// </summary>
public enum FailureKind
{
    /// <summary>
    /// Reason used with authentication errors
    /// </summary>
    UserNotAuthenticated,
    /// <summary>
    /// Reason used with authorization errors
    /// </summary>
    UserNotAllowed,
    /// <summary>
    /// Reason used when a resource is not found
    /// </summary>
    ResourceNotFound,
    /// <summary>
    /// Reason used when a concurrency error occurs
    /// </summary>
    ConcurrencyError,
    /// <summary>
    /// Reason used when a contract validation fails
    /// </summary>
    ValidationError,
    /// <summary>
    /// Reason used to not specify a failure kind
    /// </summary>
    Unspecified,
    /// <summary>
    /// Reason used when an unhandled exception occurs
    /// </summary>
    UnhandledException
}

/// <summary>
/// Represents a validation error
/// </summary>
/// <param name="Name">
/// Name of the property 
/// </param>
/// <param name="Detail">
/// A human-readable explanation of the error
/// </param>
public readonly record struct ValidationError(string Name, string Detail);

/// <summary>
/// A free-interpretation of RFC9457 problem details, it is used to represent a failure in the given feature.
/// </summary>
/// <remarks>
/// RFC detail <see href="https://datatracker.ietf.org/doc/html/rfc9457"/>
/// </remarks>
/// <param name="Kind">
/// Failure kind. See <see cref="FailureKind"/> for more information
/// </param>
/// <param name="Title">
/// A short, human-readable summary of the problem type
/// </param>
/// <param name="Detail">
/// A human-readable explanation specific to this occurrence of the problem
/// </param>
/// <param name="Type">
/// A URI reference that identifies the problem type
/// </param>
/// <param name="Instance">
/// A URI reference that identifies the specific occurrence of the problem
/// </param>
/// <param name="Errors">
/// A list of validation errors
/// </param>
/// <param name="CustomExtensions">
/// Extensions to the problem details, to add more detail response
/// </param>
public readonly record struct Failure(
    FailureKind Kind,
    string? Type = null,
    string? Title = null,
    string? Detail = null,
    string? Instance = null,
    ValidationError[]? Errors = null,
    IDictionary<string, object?>? CustomExtensions = null);

