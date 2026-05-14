using MdDox.Localization.Interfaces;

namespace MdDox.Localization
{
    #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public class LocalizedStrings : ILocalizedStrings
    {
        public string CultureName => "en-us";

        public string VersionPrefix => "v";
        public string DefaultTitleFormat => "{assembly} {version} API documentation";
        public string AllTypes => "All types";
        public string CreatedBy => "Created by ";
        public string CreatedByOn => " on ";
        public string CommandLine => "Command line: ";
        public string Values => "Values";
        public string Name => "Name";
        public string Summary => "Summary";
        public string BaseClass => "Base class: ";
        public string Type => "Type";
        public string Properties => "Properties";
        public string Constructors => "Constructors";
        public string Methods => "Methods";
        public string Returns => "Returns";
        public string Fields => "Fields";
        public string Parameter => "Parameter";
        public string Description => "Description";
        public string Enum => "Enum";
        public string Interface => "Interface";
        public string Struct => "Struct";
        public string Class => "Class";
        public string Record => "Record";
        public string Namespace => "Namespace: ";
        public string Examples => "Examples";
        public string Remarks => "Remarks";
    }
}
