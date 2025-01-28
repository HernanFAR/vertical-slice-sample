using Crud.Domain.ValueObjects;
using VSlices.Domain;

namespace Crud.Domain;

public sealed class CategoryType(CategoryId id, NonEmptyString text) : Maintainer<CategoryType, CategoryId>(id)
{
    public static CategoryType Life { get; } = new(CategoryId.New(new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1)), 
                                                   "Life".ToNonEmpty());

    public static CategoryType Science { get; } = new(CategoryId.New(new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2)), 
                                                      "Science".ToNonEmpty());

    public static CategoryType History { get; } = new(CategoryId.New(new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3)), 
                                                      "History".ToNonEmpty());

    public static CategoryType GeneralCulture { get; } = new(CategoryId.New(new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 4)), 
                                                             "General culture".ToNonEmpty());

    public static CategoryType Math { get; } = new(CategoryId.New(new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 5)), 
                                                   "Mathematics".ToNonEmpty());

    public static CategoryType Physics { get; } = new(CategoryId.New(new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 6)),
                                                      "Physics".ToNonEmpty());

    public static CategoryType Politics { get; } = new(CategoryId.New(new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 7)), 
                                                       "Politics".ToNonEmpty());

    public static CategoryType Technology { get; } = new(CategoryId.New(new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 8)), 
                                                         "Technology".ToNonEmpty());

    public static CategoryType Music { get; } = new(CategoryId.New(new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 9)), 
                                                    "Music".ToNonEmpty());

    public static CategoryType Sports { get; } = new(CategoryId.New(new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 10)), 
                                                     "Sports".ToNonEmpty());

    public static CategoryType[] All { get; } = [Life, Science, History, GeneralCulture, Math, Physics, Politics, Technology, Music, Sports];

    public NonEmptyString Text { get; } = text;

}
