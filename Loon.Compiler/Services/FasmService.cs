using Loon.Analyzer._Analyzer;
using Loon.Compiler.Models;
using Loon.Parser;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Loon.Compiler.Services
{
    internal static class FasmService
    {
        private static string FasmPath => $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\\fasm\\fasm.exe";
        private static string FasmDirectory => $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\\fasm\\";
        
        public static string? RunFasm(CompilationSettings settings)
        {
            var assemblyFile = settings.AssemblyOutputPath;
            var outputFile = settings.FinalOutputPath;

            var startInfo = new ProcessStartInfo
            {
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                FileName = $"\"{FasmPath}\" {string.Join(' ', $"\"{assemblyFile}\"", $"\"{outputFile}\"")}",
                CreateNoWindow = true,
                WorkingDirectory = FasmDirectory,
                UseShellExecute = false
            };
            var proc = Process.Start(startInfo);
            proc?.WaitForExit();
            if (proc == null) return "unable to start fasm.exe";
            if (proc.ExitCode != 0) return $"fasm error: {string.Join("\r\n..", ReadAllLines(proc.StandardError))}";
            return null;
        }

        

        private static IEnumerable<string> ReadAllLines(StreamReader streamReader)
        {
            var lines = new List<string>();
            var line = "";
            while ((line = streamReader.ReadLine()) != null)
            {
                lines.Add(line);
            }

            return lines;
        }
    }
}
