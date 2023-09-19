using CliParser;
using Logger;
using Loon.Compiler;
using Loon.Compiler.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loon.Cmd
{
    [Entry("loon")]
    internal class StartupService
    {
        [Command()]
        public static int Compile(string inputPath, string? outputPath = null, string? assemblyOutputPath = null, string? target = "exe", bool noGC = false)
        {
            if (!Enum.TryParse<OutputType>(target, true, out var compilerOutputType))
            {
                CliLogger.LogError($"target type {target} is not supported");
                return -1;
            }
            try
            {
                var settings = new CompilationSettings();
                settings.OutputType = compilerOutputType;
                settings.NoGC = noGC;
                settings.InputFilePath = inputPath;
                settings.AssemblyOutputPath = assemblyOutputPath ?? string.Empty;
                settings.FinalOutputPath = outputPath ?? string.Empty;
                var compiler = new ProgramCompiler();
                var err = compiler.Compile(settings);
                if (err == null)
                {
                    if (settings.AssemblyOutputPath != null) CliLogger.LogSuccess($"{inputPath} => {settings.AssemblyOutputPath}");
                    CliLogger.LogSuccess($"{inputPath} => {settings.FinalOutputPath}");
                    return 0;
                }
                CliLogger.LogWarning(err);
                return 1;
            }catch (Exception ex)
            {
                CliLogger.LogError($"fatal error: {ex.Message}");
                return 2;
            }
        }
    }
}
