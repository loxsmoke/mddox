using CommandLine;

namespace MdDox.CommandLineOptions
{
    /// <summary>
    /// Mddox command line definition
    /// </summary>
    public class CliProgram
    {
        /// <summary>
        /// Main parse function
        /// </summary>
        /// <param name="args"></param>
        /// <returns>Parsed command line</returns>
        public static ParserResult<CommandLineOptions>
            Parse(string[] args)
        {
            return CommandLine.Parser.Default
                .ParseArguments<CommandLineOptions>(args);
        }
    }
}
