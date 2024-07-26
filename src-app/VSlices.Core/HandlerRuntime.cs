using LanguageExt.Effects.Traits;
using LanguageExt.Sys.Traits;
using VSlices.Core.Traits;
using Implementations = LanguageExt.Sys.Live.Implementations;

namespace VSlices.Core;

/// <summary>
/// Handler runtime implementation
/// </summary>
public sealed class HandlerRuntime
    : HasIO<HandlerRuntime>,
      Has<Eff<HandlerRuntime>, DependencyProvider>,
      Has<Eff<HandlerRuntime>, FileIO>,
      Has<Eff<HandlerRuntime>, DirectoryIO>
{
    private readonly DependencyProvider _dependencyProvider;
    private readonly FileIO _fileIo;
    private readonly DirectoryIO _directoryIo;

    private HandlerRuntime(DependencyProvider dependencyProvider, 
                           EnvIO envIo, 
                           FileIO fileIo,
                           DirectoryIO directoryIo)
    {
        _dependencyProvider = dependencyProvider;
        _fileIo             = fileIo;
        _directoryIo        = directoryIo;
        EnvIO               = envIo;
    }

    /// <inheritdoc />
    public HandlerRuntime WithIO(EnvIO envIO)
    {
        return new HandlerRuntime(_dependencyProvider, envIO, _fileIo, _directoryIo);
    }

    /// <inheritdoc />
    public EnvIO EnvIO { get; }

    K<Eff<HandlerRuntime>, DependencyProvider>
        Has<Eff<HandlerRuntime>, DependencyProvider>.Trait =>
        liftEff((HandlerRuntime _) => _dependencyProvider);

    K<Eff<HandlerRuntime>, FileIO>
        Has<Eff<HandlerRuntime>, FileIO>.Trait =>
        liftEff((HandlerRuntime _) => Implementations.FileIO.Default);

    K<Eff<HandlerRuntime>, DirectoryIO>
        Has<Eff<HandlerRuntime>, DirectoryIO>.Trait =>
        liftEff((HandlerRuntime _) => Implementations.DirectoryIO.Default);

    /// <summary>
    /// Creates a <see cref="HandlerRuntime"/> by specifying all the dependencies
    /// </summary>
    public static HandlerRuntime New(DependencyProvider dependencyProvider,
                              EnvIO envIo,
                              FileIO fileIo,
                              DirectoryIO directoryIo)
        => new(dependencyProvider, envIo, fileIo, directoryIo);

    /// <summary>
    /// Creates a <see cref="HandlerRuntime"/> by specifying all the dependencies
    /// </summary>
    public static HandlerRuntime New(DependencyProvider dependencyProvider,
                                     EnvIO envIo)
        => new(dependencyProvider, envIo, 
               Implementations.FileIO.Default, 
               Implementations.DirectoryIO.Default); 

}
