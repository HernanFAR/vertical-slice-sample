using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace VSlices.CrossCutting.AspNetCore.DataAnnotationMiddleware;

public static class DataAnnotationValidationMiddleware
{
    public static RouteHandlerBuilder DataAnnotationsValidate<T>(this RouteHandlerBuilder builder, 
        int statusCode = StatusCodes.Status422UnprocessableEntity, string? title = null, string? detail = null)
    {
        builder.AddEndpointFilter(async (invocationContext, next) =>
        {
            T argument = invocationContext.Arguments
                                          .OfType<T>()
                                          .FirstOrDefault()
                         ?? throw new InvalidOperationException("Tried to access inaccessible argument");

            List<ValidationResult> results = [];
            ValidationContext      context = new(argument);

            bool isValid = Validator.TryValidateObject(argument, context, results, true);

            if (isValid) return await next(invocationContext);

            return TypedResults.Problem(new HttpValidationProblemDetails
            {
                Status = statusCode,
                Detail = detail,
                Title  = title,
                Errors = results.Select(x => x.MemberNames.First())
                                .Distinct()
                                .ToDictionary(p => p,
                                              p => results.Where(x => x.MemberNames.Contains(p))
                                                          .Where(e => e.ErrorMessage is not null)
                                                          .Select(e => e.ErrorMessage!)
                                                          .ToArray())
            });
        });

        return builder;
    }
}
