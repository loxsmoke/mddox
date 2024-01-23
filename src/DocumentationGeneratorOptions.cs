using mddox.Localization;
using mddox.Localization.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace MdDox
{
    public class DocumentationGeneratorOptions
    {
        public bool DocumentMethodDetails { get; set; }
        public string DocumentTitle { get; set; }
        public bool ShowDocumentDateTime { get; set; }
        /// <summary>
        /// Trur if MSDN links should be generated.
        /// </summary>
        public bool MsdnLinks { get; set; }
        /// <summary>
        /// Which version of the framework to link to.
        /// </summary>
        public string MsdnView { get; set; }
        /// <summary>
        /// The locale for the MSDN pages.
        /// </summary>
        public string MsdnCultureName { get; set; } = "en-us";
        public int TypeIndexColumnCount { get; set; } = 3;
        /// <summary>
        /// Localized strings.
        /// </summary>
        public ILocalizedStrings Strings { get; set; }
    }
}
