using CommandLine;
using System;
using System.Collections.Generic;
using MdDox.CommandLineOptions;

namespace MdDox.CommandLineOptions
{
    /// <summary>
    /// Mddox command line definition
    /// </summary>
    public class CliProgram
    {
        public static ParserResult<CommandLineOptions>
            Parse(string[] args)
        {
            return CommandLine.Parser.Default
                .ParseArguments<CommandLineOptions>(args);
        }
        public static void Run(
            string[] args,
            Action<CommandLineOptions> CommandLineOptionsHandler,
            Action<IEnumerable<Error>> errorHandler = null)
        {
            Parse(args)
                .WithParsed(CommandLineOptionsHandler)
                .WithNotParsed(errorHandler ?? ((_) => { }));
        }
        public static int RunWithReturnValue(
            string[] args,
            Func<CommandLineOptions, int> CommandLineOptionsHandler,
            Func<IEnumerable<Error>, int> errorHandler = null)
        {
            var returnCode = 0;
            Parse(args)
                .WithParsed<CommandLineOptions>((x) => returnCode = CommandLineOptionsHandler(x))
                .WithNotParsed((x) => { if (errorHandler != null) returnCode = errorHandler(x); });
            return returnCode;
        }
    }
}
