using System;
using System.Collections.Generic;
using System.Text;

namespace CsfEditor
{
    public class Csf
    {
        public int CSFVersion { get; set; }
        public int NumLabels { get; set; }
        public int NumStrings { get; set; }
        public int ExtraInformation { get; set; }
        public LanguageType Language { get; set; }

        public List<Label> LablelList = new List<Label>();
    }
}
