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
            await File.WriteAllTextAsync(dest.Text, JsonConvert.SerializeObject(csf));
            MessageBox.Show("完成!");
        }

        Csf ParseBuff(byte[] buff)
        {
            MemoryStream ms = new MemoryStream(buff);
            var intBuff = new byte[4];

            ms.Read(intBuff, 0, 4);
            var csfHeaderIdentifier = System.Text.Encoding.ASCII.GetString(intBuff, 0, 4);
            if(csfHeaderIdentifier != " FSC")
            {
                return null;
            }
            var csf = new Csf();
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
                ms.Read(intBuff, 0, 4);
                var labelIdentifier = System.Text.Encoding.ASCII.GetString(intBuff, 0, 4);
                if(labelIdentifier != " LBL")
                {
                    continue;
                }
                var label = new Label();
                csf.LablelList.Add(label);
                ms.Read(intBuff, 0, 4);
                label.Count = BitConverter.ToInt32(intBuff);
                ms.Read(intBuff, 0, 4);
                var labelNameLength = BitConverter.ToInt32(intBuff);
                var labelNameBuff = new byte[labelNameLength];
                ms.Read(labelNameBuff, 0, labelNameLength);
                label.LabelName = System.Text.Encoding.ASCII.GetString(labelNameBuff, 0, labelNameLength);

                for (int i = 0; i < label.Count; i++)
                {
                    ms.Read(intBuff, 0, 4);
                    var valueIdentifier = System.Text.Encoding.ASCII.GetString(intBuff, 0, 4);
                    if(valueIdentifier != " RTS" && valueIdentifier != "WRTS")
                    {
                        Debug.WriteLine($"label {label.LabelName} value {i} is error");
                        continue;
                    }

                    ms.Read(intBuff, 0, 4);
                    var valueLength = BitConverter.ToInt32(intBuff) * 2;
                    var valueBuff = new byte[valueLength];
                    ms.Read(valueBuff, 0, valueLength);
                    ConvertUnicode(valueBuff, 0, valueLength);
                    label.ValueList.Add(System.Text.Encoding.Unicode.GetString(valueBuff));
                    if(valueIdentifier == "WRTS")
                    {
                        ms.Read(intBuff, 0, 4);
                        var valueExtLength = BitConverter.ToInt32(intBuff);
                        var valueExtBuff = new byte[valueExtLength];
                        ms.Read(valueExtBuff, 0, valueExtLength);
                        //ConvertUnicode(valueExtBuff, 0, valueExtLength);
                        label.ExtraValueList.Add(System.Text.Encoding.ASCII.GetString(valueExtBuff));
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
