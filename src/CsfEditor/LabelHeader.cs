using System;
using System.Collections.Generic;
using System.Text;

namespace CsfEditor
{
    public class LabelHeader
    {
        /// <summary>
        /// 标签标识符
        /// </summary>
        public string TagIdentifier;

        /// <summary>
        /// 字符串对的数目
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// 标签长度
        /// </summary>
        public int LabelNameLength { get; set;}
        
        /// <summary>
        /// 标签名
        /// </summary>
        public string LabelName { get; set; }

        /// <summary>
        /// 值
        /// </summary>
        public List<LabelValue> LabelValues { get; set; } = new List<LabelValue>();

        //public List<string> ValueList = new List<string>();
        //public List<string> ExtraValueList = new List<string>();

        public override string ToString()
        {
            return LabelName;
        }
    }
}
