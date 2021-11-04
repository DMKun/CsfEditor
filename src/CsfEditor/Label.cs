using System;
using System.Collections.Generic;
using System.Text;

namespace CsfEditor
{
    public class Label
    {
        /// <summary>
        /// Number of string pairs
        /// This is the number of string pairs associated with this label.Usual value is 1.
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// LabelName
        /// A non-zero-terminated string that is as long as the DWORD at 0x8 says.If it is longer, the rest will be cut off.
        /// </summary>
        public string LabelName { get; set; }

        public List<string> ValueList = new List<string>();
        public List<string> ExtraValueList = new List<string>();

        public override string ToString()
        {
            return LabelName;
        }
    }
}
