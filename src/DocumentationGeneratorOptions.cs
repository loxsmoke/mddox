using MdDox.Localization;
using MdDox.Localization.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace MdDox
{
    public class DocumentationGeneratorOptions
    {
        public bool DocumentMethodDetails { get; set; }
        public string DocumentTitle { get; set; }
        /// <summary>
        /// Show "created by mddox at date" in the document.
        /// </summary>
        public bool ShowDocumentDateTime { get; set; }
        /// <summary>
        /// Show the command line used to generate the document.
        /// </summary>
        public bool ShowCommandLine { get; set; }
        /// <summary>
        /// True if MSDN links should be generated.
        /// </summary>
        public bool AddMsdnLinks { get; set; }
        /// <summary>
        /// Which version of the framework to link to.
        /// </summary>
        public string MsdnViewParameter { get; set; }
        /// <summary>
        /// The locale for the MSDN pages.
        /// </summary>
        public string MsdnCultureName { get; set; } = "en-us";
        /// <summary>
        /// The number of columns to use for the table of contents
        /// </summary>
        public int TypeIndexColumnCount { get; set; } = 3;
        /// <summary>
        /// Localized strings.
        /// </summary>
        public ILocalizedStrings Strings { get; set; }
    }
}
