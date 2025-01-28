global using LanguageExt;
global using LanguageExt.Traits;
global using LanguageExt.Sys.Traits;

global using static LanguageExt.Prelude;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("VSlices.Base.UnitTests")]
[assembly: InternalsVisibleTo("VSlices.Core.UseCases.Reflection")]
[assembly: InternalsVisibleTo("VSlices.Core.Events.ReflectionRunner")]
[assembly: InternalsVisibleTo("VSlices.CrossCutting.Interceptor.ExceptionHandling")]
[assembly: InternalsVisibleTo("VSlices.CrossCutting.Interceptor.ExceptionHandling.UnitTests")]
[assembly: InternalsVisibleTo("VSlices.CrossCutting.Interceptor.FluentValidation")]
[assembly: InternalsVisibleTo("VSlices.CrossCutting.Interceptor.FluentValidation.UnitTests")]
[assembly: InternalsVisibleTo("VSlices.CrossCutting.Interceptor.Logging")]
[assembly: InternalsVisibleTo("VSlices.CrossCutting.Interceptor.Logging.UnitTests")]
[assembly: InternalsVisibleTo("VSlices.CrossCutting.Interceptor.Filtering")]
[assembly: InternalsVisibleTo("VSlices.CrossCutting.Interceptor.Filtering.UnitTests")]
[assembly: InternalsVisibleTo("VSlices.Core.Events.ReflectionRunner.UnitTests")]
