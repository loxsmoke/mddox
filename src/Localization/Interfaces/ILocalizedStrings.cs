using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdDox.Localization.Interfaces
{
    public interface ILocalizedStrings
    {
        string CultureName { get; }

        string VersionPrefix { get; }
        string DefaultTitleFormat { get; }
        string AllTypes { get; }
        string CreatedBy { get; }
        string CreatedByOn { get; }
        string CommandLine { get; }

        string Values { get; }
        string Name { get; }
        string Summary { get; }
        string BaseClass { get; }
        string Type { get; }
        string Properties { get; }
        string Constructors { get; }
        string Methods { get; }
        string Returns { get; }
        string Fields { get; }
        string Parameter {  get; }
        string Description {  get; }
        string Enum { get; }
        string Interface { get; }
        string Struct { get; }
        string Class { get; }
        string Namespace { get; }
        string Examples { get; }
        string Remarks { get; }
    }
}
