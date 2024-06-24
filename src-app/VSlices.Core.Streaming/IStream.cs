using VSlices.Base;

namespace VSlices.Core.Stream;

/// <summary>
/// Represents the start point of a streamed resource
/// </summary>
/// <typeparam name="TResult">The expected response of this request</typeparam>
public interface IStream<TResult> : IFeature<IAsyncEnumerable<TResult>> { }
