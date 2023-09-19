
using CliParser;
using Logger;
using Loon.Cmd;

args.ResolveWithTryCatch<StartupService>((ex) => CliLogger.LogError(ex.Message));
