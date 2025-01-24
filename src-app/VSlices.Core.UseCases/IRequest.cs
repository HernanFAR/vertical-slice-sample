using LanguageExt;
using VSlices.Base.Core;

namespace VSlices.Core.UseCases;

/// <summary>
/// Represents the start point of a use case, with a specific response type.
/// </summary>
/// <typeparam name="TOut">The expected response of this input</typeparam>
public interface IInput<TOut>;

/// <summary>
/// Represents the start point of a use case, with a success response.
/// </summary>
public interface IInput : IInput<Unit>;
