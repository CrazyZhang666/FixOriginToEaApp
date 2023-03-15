using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Windows;
using System.Windows.Navigation;
using System.Diagnostics;

namespace FixOriginToEaApp
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string path = "C:\\ProgramData\\Origin\\local.xml";

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.OriginalString);
            e.Handled = true;
        }

        private void Button_OneClickFix_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Process.GetProcessesByName("Origin").Length <= 0)
                {
                    MessageBox.Show("请先启动 Origin 客户端", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                ChangeXML();
                MakeOrigin();

                MessageBox.Show("修复完成，请重启 Origin 客户端", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"发生了未知的异常，查看异常提示以获取更多信息\n\n{ex.Message}", "异常", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ChangeXML()
        {
            var xmlDoc = XDocument.Load(path);
            var xmlNode = xmlDoc.Descendants("Setting");

            var result = xmlNode.ToList().Find(x => x.Attribute("key").Value == "MigrationDisabled");
            if (result != null)
            {
                result.SetAttributeValue("key", "MigrationDisabled");
                result.SetAttributeValue("value", "true");
                result.SetAttributeValue("type", "1");

                xmlDoc.Save(path);
            }
            else
            {
                var xml = new XmlDocument();
                xml.Load(path);

                var root = xml.DocumentElement;
                var newNode = xml.CreateElement("Setting");
                newNode.SetAttribute("key", "MigrationDisabled");
                newNode.SetAttribute("value", "true");
                newNode.SetAttribute("type", "1");
                root.AppendChild(newNode);

                xml.Save(path);
            }
        }

        private void MakeOrigin()
        {
            var pArray = Process.GetProcessesByName("Origin");
            if (pArray.Length > 0)
            {
                var process = pArray[0];

                var originPath = process.MainModule.FileName;
                var originDir = Path.GetDirectoryName(originPath);

                // 创建 OriginThinSetupInternal 文件夹
                Directory.CreateDirectory(Path.Combine(originDir, "OriginThinSetupInternal"));

                // 修改 OriginThinSetupInternal.exe 后缀名
                var exePath = Path.Combine(originDir, "OriginThinSetupInternal.exe");
                var fileInfo = new FileInfo(exePath);
                if (fileInfo.Exists)
                {
                    fileInfo.MoveTo(Path.Combine(originDir, "OriginThinSetupInternal.exe.bak"));
                }
            }
        }
    }
}
