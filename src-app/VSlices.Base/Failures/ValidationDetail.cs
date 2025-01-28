namespace VSlices.Base.Failures;

/// <summary>
/// Represents a validation error
/// </summary>
/// <param name="Name">
/// Name of the property 
/// </param>
/// <param name="Detail">
/// A human-readable explanation of the error
/// </param>
public sealed record ValidationDetail(string Name, string Detail);
