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
        public bool MsdnLinks { get; set; }
        public string MsdnView { get; set; }
        public string TypeIndexTitle { get; set; } = "All types";
        public int TypeIndexColumnCount { get; set; } = 3;
    }
}
