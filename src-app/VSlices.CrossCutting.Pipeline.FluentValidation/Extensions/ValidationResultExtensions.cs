using VSlices.Base.Failures;

// ReSharper disable once CheckNamespace
namespace FluentValidation.Results;

internal static class ValidationResultExtensions
{
    public static Unprocessable ToUnprocessable(this ValidationResult validationResult, string? message = null)
    {
        return validationResult.Errors
            .Select(e => new ValidationDetail(e.PropertyName, e.ErrorMessage))
            .ToUnprocessable();
    }
}
