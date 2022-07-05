using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace CsfEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void srcBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "*.csf|*.csf";
            if (openFileDialog.ShowDialog() == true)
                src.Text = openFileDialog.FileName;
        }

        private void destBtn_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "*.json|*.json";
            if (saveFileDialog.ShowDialog() == true)
                dest.Text = saveFileDialog.FileName;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var csf = ParseBuff(File.ReadAllBytes(src.Text));
            await File.WriteAllTextAsync(dest.Text, JsonConvert.SerializeObject(csf, Formatting.Indented));
            MessageBox.Show("完成!");
        }

        Csf ParseBuff(byte[] buff)
        {
            MemoryStream ms = new MemoryStream(buff);
            BinaryReader br = new BinaryReader(ms);

            var csf = new Csf();
            // 读取csf文件头
            csf.TagIdentifier = System.Text.Encoding.ASCII.GetString(br.ReadBytes(4));
            if(csf.TagIdentifier != " FSC")
            {
                return null;
            }
            csf.CSFVersion = br.ReadInt32();
            csf.NumLabels = br.ReadInt32();
            csf.NumStrings = br.ReadInt32();
            csf.ExtraInformation = br.ReadInt32();
            int lVal = br.ReadInt32();
            csf.Language = lVal>9? LanguageType.Unknown : (LanguageType)lVal;
            while (ms.Position<ms.Length)
            {
                var label = new LabelHeader();
                csf.LablelList.Add(label);

                // 读取标签标头(Label header)
                label.TagIdentifier = System.Text.Encoding.ASCII.GetString(br.ReadBytes(4));
                if(label.TagIdentifier != " LBL")
                {
                    
                    Debug.WriteLine($"识别错误 {ms.Position.ToString("X")}:{label.TagIdentifier}!");
                    continue;
                }
                label.Count = br.ReadInt32();
                label.LabelNameLength = br.ReadInt32();
                label.LabelName = System.Text.Encoding.ASCII.GetString(br.ReadBytes(label.LabelNameLength));

                // 读取值(Values)
                for (int i = 0; i < label.Count; i++)
                {
                    var value = new LabelValue();
                    label.LabelValues.Add(value);
                    value.TagIdentifier = System.Text.Encoding.ASCII.GetString(br.ReadBytes(4), 0, 4);
                    if(value.TagIdentifier != " RTS" && value.TagIdentifier != "WRTS")
                    {
                        Debug.WriteLine($"label {label.LabelName} value {i} is error");
                        continue;
                    }

                    value.ValueLength = br.ReadInt32();
                    var valueBuff = br.ReadBytes(value.ValueLength * 2);
                    ConvertUnicode(valueBuff);
                    value.Value = System.Text.Encoding.Unicode.GetString(valueBuff);
                    //label.ValueList.Add(System.Text.Encoding.Unicode.GetString(valueBuff));
                    if(value.TagIdentifier == "WRTS")
                    {
                        value.ExtraValueLength = br.ReadInt32();
                        value.ExtraValue = System.Text.Encoding.ASCII.GetString(br.ReadBytes(value.ExtraValueLength));
                    }
                }
            }

            return csf;
        }

        void ConvertUnicode(byte[] buff)
        {
            for(int i=0; i< buff.Count(); i++)
            {
                buff[i] = (byte)~buff[i];
            }
        }

    }
}
