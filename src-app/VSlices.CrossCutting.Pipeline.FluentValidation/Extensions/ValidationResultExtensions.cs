using LanguageExt.Common;
using VSlices.Base.Failures;
using static VSlices.VSlicesPrelude;

// ReSharper disable once CheckNamespace
namespace FluentValidation.Results;

internal static class ValidationResultExtensions
{
    public static Error ToUnprocessable(this ValidationResult result, string? message = null) =>
        unprocessable(message, 
                      result.Errors
                            .Select(e => new ValidationDetail(e.PropertyName, e.ErrorMessage)));
}
