namespace VSlices.Base;


public interface IHandler<in TFeature, TResult>
{
    Eff<VSlicesRuntime, TResult> Define(TFeature feature);
}