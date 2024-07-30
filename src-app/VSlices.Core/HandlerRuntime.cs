using LanguageExt.Effects.Traits;
using LanguageExt.Sys.Traits;
using VSlices.Core.Traits;
using Implementations = LanguageExt.Sys.Live.Implementations;

namespace VSlices.Core;

/// <summary>
/// Handler runtime implementation
/// </summary>
public sealed class HandlerRuntime
    : Has<Eff<HandlerRuntime>, DependencyProvider>,
      Has<Eff<HandlerRuntime>, FileIO>,
      Has<Eff<HandlerRuntime>, DirectoryIO>
{
    private readonly DependencyProvider _dependencyProvider;
    private readonly FileIO _fileIo;
    private readonly DirectoryIO _directoryIo;

    private HandlerRuntime(DependencyProvider dependencyProvider,
                           FileIO fileIo,
                           DirectoryIO directoryIo)
    {
        _dependencyProvider = dependencyProvider;
        _fileIo             = fileIo;
        _directoryIo        = directoryIo;
    }

    K<Eff<HandlerRuntime>, DependencyProvider>
        Has<Eff<HandlerRuntime>, DependencyProvider>.Trait =>
        liftEff((HandlerRuntime _) => _dependencyProvider);

    K<Eff<HandlerRuntime>, FileIO>
        Has<Eff<HandlerRuntime>, FileIO>.Trait =>
        liftEff((HandlerRuntime _) => _fileIo);

    K<Eff<HandlerRuntime>, DirectoryIO>
        Has<Eff<HandlerRuntime>, DirectoryIO>.Trait =>
        liftEff((HandlerRuntime _) => _directoryIo);

    /// <summary>
    /// Creates a <see cref="HandlerRuntime"/> by specifying all the dependencies
    /// </summary>
    public static HandlerRuntime New(DependencyProvider dependencyProvider, FileIO fileIo, DirectoryIO directoryIo)
        => new(dependencyProvider,  fileIo, directoryIo);

    /// <summary>
    /// Creates a <see cref="HandlerRuntime"/> by specifying all the dependencies
    /// </summary>
    public static HandlerRuntime New(DependencyProvider dependencyProvider)
        => new(dependencyProvider, Implementations.FileIO.Default, Implementations.DirectoryIO.Default); 

}
