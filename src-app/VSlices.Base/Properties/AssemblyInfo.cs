global using LanguageExt;
global using LanguageExt.Traits;
global using LanguageExt.Sys.Traits;

global using static LanguageExt.Prelude;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("VSlices.Base.UnitTests")]
[assembly: InternalsVisibleTo("VSlices.Core.UseCases.Reflection")]
[assembly: InternalsVisibleTo("VSlices.Core.Events.ReflectionRunner")]
[assembly: InternalsVisibleTo("VSlices.CrossCutting.Pipeline.ExceptionHandling")]
[assembly: InternalsVisibleTo("VSlices.CrossCutting.Pipeline.ExceptionHandling.UnitTests")]
[assembly: InternalsVisibleTo("VSlices.CrossCutting.Pipeline.FluentValidation")]
[assembly: InternalsVisibleTo("VSlices.CrossCutting.Pipeline.FluentValidation.UnitTests")]
[assembly: InternalsVisibleTo("VSlices.CrossCutting.Pipeline.Logging")]
[assembly: InternalsVisibleTo("VSlices.CrossCutting.Pipeline.Logging.UnitTests")]
[assembly: InternalsVisibleTo("VSlices.CrossCutting.Pipeline.Filtering")]
[assembly: InternalsVisibleTo("VSlices.CrossCutting.Pipeline.Filtering.UnitTests")]
