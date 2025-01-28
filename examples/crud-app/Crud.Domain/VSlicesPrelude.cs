global using LanguageExt;

using Crud.Domain.ValueObjects;

namespace Crud.Domain;

public static class VSlicesPrelude
{
    public static NonEmptyString ToNonEmpty(this string value) => new(value);
    
}