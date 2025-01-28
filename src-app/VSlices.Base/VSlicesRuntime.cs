using VSlices.Base.Traits;
using Implementations = LanguageExt.Sys.Live.Implementations;

namespace VSlices.Base;

/// <summary>
/// Behavior runtime implementation
/// </summary>
public sealed class VSlicesRuntime
    : Has<Eff<VSlicesRuntime>, DependencyProvider>,
      Has<Eff<VSlicesRuntime>, FileIO>,
      Has<Eff<VSlicesRuntime>, DirectoryIO>
{
    private readonly DependencyProvider _dependencyProvider;
    private readonly FileIO _fileIo;
    private readonly DirectoryIO _directoryIo;

    private VSlicesRuntime(DependencyProvider dependencyProvider,
                           FileIO fileIo,
                           DirectoryIO directoryIo)
    {
        _dependencyProvider = dependencyProvider;
        _fileIo = fileIo;
        _directoryIo = directoryIo;
    }

    static K<Eff<VSlicesRuntime>, DependencyProvider> Has<Eff<VSlicesRuntime>, DependencyProvider>.Ask =>
        asks(runtime => runtime._dependencyProvider);

    static K<Eff<VSlicesRuntime>, FileIO> Has<Eff<VSlicesRuntime>, FileIO>.Ask =>
        asks(runtime => runtime._fileIo);

    static K<Eff<VSlicesRuntime>, DirectoryIO> Has<Eff<VSlicesRuntime>, DirectoryIO>.Ask =>
        asks(runtime => runtime._directoryIo);

    /// <summary>
    /// Creates a <see cref="VSlicesRuntime"/> by specifying all the dependencies
    /// </summary>
    public static VSlicesRuntime New(DependencyProvider dependencyProvider, FileIO fileIo, DirectoryIO directoryIo) =>
        new(dependencyProvider, fileIo, directoryIo);

    /// <summary>
    /// Creates a <see cref="VSlicesRuntime"/> by specifying all the dependencies
    /// </summary>
    public static VSlicesRuntime New(DependencyProvider dependencyProvider) => 
        new(dependencyProvider, Implementations.FileIO.Default, Implementations.DirectoryIO.Default);

    static K<Eff<VSlicesRuntime>, A> asks<A>(Func<VSlicesRuntime, A> f) =>
        Readable.asks<Eff<VSlicesRuntime>, VSlicesRuntime, A>(f);
}
