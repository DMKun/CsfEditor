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
            var intBuff = new byte[4];

            var csf = new Csf();
            // 读取csf文件头
            ms.Read(intBuff, 0, 4);
            csf.TagIdentifier = System.Text.Encoding.ASCII.GetString(intBuff, 0, 4);
            if(csf.TagIdentifier != " FSC")
            {
                return null;
            }
            ms.Read(intBuff, 0, 4);
            csf.CSFVersion = BitConverter.ToInt32(intBuff);
            ms.Read(intBuff, 0, 4);
            csf.NumLabels = BitConverter.ToInt32(intBuff);
            ms.Read(intBuff, 0, 4);
            csf.NumStrings = BitConverter.ToInt32(intBuff);
            ms.Read(intBuff, 0, 4);
            csf.ExtraInformation = BitConverter.ToInt32(intBuff);
            ms.Read(intBuff, 0, 4);
            int lVal = BitConverter.ToInt32(intBuff);
            csf.Language = lVal>9? LanguageType.Unknown : (LanguageType)lVal;

            while (ms.Position<ms.Length)
            {
                var label = new LabelHeader();
                csf.LablelList.Add(label);

                // 读取标签标头(Label header)
                ms.Read(intBuff, 0, 4);
                label.TagIdentifier = System.Text.Encoding.ASCII.GetString(intBuff, 0, 4);
                if(label.TagIdentifier != " LBL")
                {
                    
                    Debug.WriteLine($"识别错误 {ms.Position.ToString("X")}:{label.TagIdentifier}!");
                    continue;
                }
                ms.Read(intBuff, 0, 4);
                label.Count = BitConverter.ToInt32(intBuff);
                ms.Read(intBuff, 0, 4);
                label.LabelNameLength = BitConverter.ToInt32(intBuff);
                var labelNameBuff = new byte[label.LabelNameLength];
                ms.Read(labelNameBuff, 0, label.LabelNameLength);
                label.LabelName = System.Text.Encoding.ASCII.GetString(labelNameBuff, 0, label.LabelNameLength);

                // 读取值(Values)
                for (int i = 0; i < label.Count; i++)
                {
                    var value = new LabelValue();
                    label.LabelValues.Add(value);
                    ms.Read(intBuff, 0, 4);
                    value.TagIdentifier = System.Text.Encoding.ASCII.GetString(intBuff, 0, 4);
                    if(value.TagIdentifier != " RTS" && value.TagIdentifier != "WRTS")
                    {
                        Debug.WriteLine($"label {label.LabelName} value {i} is error");
                        continue;
                    }

                    ms.Read(intBuff, 0, 4);
                    value.ValueLength = BitConverter.ToInt32(intBuff);
                    //var valueLength = BitConverter.ToInt32(intBuff) * 2;
                    var valueBuff = new byte[value.ValueLength * 2];
                    ms.Read(valueBuff, 0, value.ValueLength * 2);
                    ConvertUnicode(valueBuff, 0, value.ValueLength * 2);
                    value.Value = System.Text.Encoding.Unicode.GetString(valueBuff);
                    //label.ValueList.Add(System.Text.Encoding.Unicode.GetString(valueBuff));
                    if(value.TagIdentifier == "WRTS")
                    {
                        ms.Read(intBuff, 0, 4);
                        value.ExtraValueLength = BitConverter.ToInt32(intBuff);
                        //var valueExtLength = BitConverter.ToInt32(intBuff);
                        var valueExtBuff = new byte[value.ExtraValueLength];
                        ms.Read(valueExtBuff, 0, value.ExtraValueLength);
                        //ConvertUnicode(valueExtBuff, 0, valueExtLength);
                        value.ExtraValue = System.Text.Encoding.ASCII.GetString(valueExtBuff);
                    }
                }
            }

            return csf;
        }

        void ConvertUnicode(byte[] buff, int offset, int count)
        {
            for(int i=0; i< count; i++)
            {
                buff[i] = (byte)~buff[i];
            }
        }


    }
}
