using System;
using System.Collections.Generic;
using System.Text;

namespace CsfEditor
{
    public class Csf
    {

        /// <summary>
        /// 0x00
        /// “FSC”
        /// CSF文件的头部标识符。
        /// 如果这里不是“FSC”，游戏将不会加载这个文件。
        /// </summary>
        public string TagIdentifier;

        /// <summary>
        /// 0x04
        /// CSF格式的版本号。
        /// </summary>
        public int CSFVersion { get; set; }

        /// <summary>
        /// 标签数
        /// </summary>
        public int NumLabels { get; set; }

        /// <summary>
        /// 字符串数
        /// </summary>
        public int NumStrings { get; set; }

        /// <summary>
        /// （未使用）
        /// </summary>
        public int ExtraInformation { get; set; }

        /// <summary>
        /// 语言
        /// </summary>
        public LanguageType Language { get; set; }

        /// <summary>
        /// label信息
        /// </summary>
        public List<LabelHeader> LablelList = new List<LabelHeader>();
    }
}
