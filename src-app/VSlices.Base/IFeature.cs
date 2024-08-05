namespace VSlices.Base;

/// <summary>
/// Not intented to be used in development, use <see cref="IFeature{TResult}"/>
/// </summary>
public interface IFeature;

/// <summary>
/// Represents the start point of any feature
/// </summary>
/// <typeparam name="TResult">The expected result of the feature</typeparam>
public interface IFeature<TResult> : IFeature;