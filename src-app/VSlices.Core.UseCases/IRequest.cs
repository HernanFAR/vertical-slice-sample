using VSlices.Base;
using VSlices.Base.Responses;

namespace VSlices.Core.UseCases;

/// <summary>
/// Represents the start point of a use case, with a specific response type
/// </summary>
/// <typeparam name="TResponse">The expected response of this request</typeparam>
public interface IRequest<TResponse> : IFeature<TResponse> { }

/// <summary>
/// Represents the start point of a use case, with a success response
/// </summary>
public interface IRequest : IRequest<Success> { }
