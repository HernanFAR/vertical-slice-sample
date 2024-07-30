using VSlices.Base.Traits;
using Implementations = LanguageExt.Sys.Live.Implementations;

namespace VSlices.Base;

/// <summary>
/// Handler runtime implementation
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

    K<Eff<VSlicesRuntime>, DependencyProvider>
        Has<Eff<VSlicesRuntime>, DependencyProvider>.Trait =>
        liftEff((VSlicesRuntime _) => _dependencyProvider);

    K<Eff<VSlicesRuntime>, FileIO>
        Has<Eff<VSlicesRuntime>, FileIO>.Trait =>
        liftEff((VSlicesRuntime _) => _fileIo);

    K<Eff<VSlicesRuntime>, DirectoryIO>
        Has<Eff<VSlicesRuntime>, DirectoryIO>.Trait =>
        liftEff((VSlicesRuntime _) => _directoryIo);

    /// <summary>
    /// Creates a <see cref="VSlicesRuntime"/> by specifying all the dependencies
    /// </summary>
    public static VSlicesRuntime New(DependencyProvider dependencyProvider, FileIO fileIo, DirectoryIO directoryIo)
    {
        return new(dependencyProvider, fileIo, directoryIo);
    }

    /// <summary>
    /// Creates a <see cref="VSlicesRuntime"/> by specifying all the dependencies
    /// </summary>
    public static VSlicesRuntime New(DependencyProvider dependencyProvider)
    {
        return new(dependencyProvider, Implementations.FileIO.Default, Implementations.DirectoryIO.Default);
    }
}
