using System.Diagnostics;
using System.Runtime.CompilerServices;

// ReSharper disable once CheckNamespace
namespace System.ComponentModel.DataAnnotations.Extensions;

/// <summary>
/// Specifies that default values should not be allowed in a property.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public class DenyDefaultValueAttribute : ValidationAttribute
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="DenyDefaultValueAttribute"/> class.
    /// </summary>
    public DenyDefaultValueAttribute() { }

    public override bool IsValid(object? value)
    {
        return IsValidCore(value, validationContext: null);
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        return IsValidCore(value, validationContext)
            ? ValidationResult.Success
            : CreateFailedValidationResult(validationContext);
    }

    private protected ValidationResult CreateFailedValidationResult(ValidationContext validationContext)
    {
        string[]? memberNames = validationContext.MemberName is { } memberName
            ? [memberName]
            : null;

        return new ValidationResult(FormatErrorMessage(validationContext.DisplayName), memberNames);
    }

    private bool IsValidCore(object? value, ValidationContext? validationContext)
    {
        if (value is null)
        {
            return false;
        }

        Type valueType = validationContext?.ObjectType ?? value.GetType();
        if (GetDefaultValueForNonNullableValueType(valueType) is { } defaultValue)
        {
            return !defaultValue.Equals(value);
        }

        return true;
    }

    private object? GetDefaultValueForNonNullableValueType(Type type)
    {
        object? defaultValue = _defaultValueCache;

        if (defaultValue != null && defaultValue.GetType() == type)
        {
            Debug.Assert(type.IsValueType && Nullable.GetUnderlyingType(type) is null);
        }
        else if (type.IsValueType && Nullable.GetUnderlyingType(type) is null)
        {
            defaultValue = RuntimeHelpers.GetUninitializedObject(type);
            _defaultValueCache = defaultValue;
        }
        else
        {
            defaultValue = null;
        }

        return defaultValue;
    }

    private object? _defaultValueCache;
}