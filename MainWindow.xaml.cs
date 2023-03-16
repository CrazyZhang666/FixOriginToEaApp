using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Windows;
using System.Windows.Navigation;
using System.Diagnostics;
using System.Threading;
using System.Text;

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
                // 屏蔽更新
                MakeOrigin();
                // 修改XML
                ChangeXML();

                MessageBox.Show("修复完成，请重启《Origin》客户端", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"发生了未知的异常，查看异常提示以获取更多信息\n\n{ex.Message}", "异常", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MakeOrigin()
        {
            var pArray = Process.GetProcessesByName("OriginWebHelperService");

            var originPath = pArray[0].MainModule.FileName;
            var originDir = Path.GetDirectoryName(originPath);

            // 创建 OriginThinSetupInternal 文件夹
            Directory.CreateDirectory(Path.Combine(originDir, "OriginThinSetupInternal"));

            // 修改 OriginThinSetupInternal.exe 后缀名
            var exePath = Path.Combine(originDir, "OriginThinSetupInternal.exe");
            var exeBakPath = Path.Combine(originDir, "OriginThinSetupInternal.exe.bak");

            if (File.Exists(exePath))
            {
                // 先删除旧版本备份
                if (File.Exists(exeBakPath))
                    File.Delete(exeBakPath);

                Thread.Sleep(500);

                // 重命名
                File.Move(exePath, exeBakPath);
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

        private void Hyperlink_FixOriginNetWorkBug_Click(object sender, RoutedEventArgs e)
        {
            string cmd = "@echo off\r\n@ echo.\r\n@ echo.　=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=\r\n@ echo.　　　　　　\r\n@ echo.　「按下任意键开始修复，该修复无需重登 Origin」\r\n@ echo.　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　\r\n@ echo.　=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=\r\n@ echo.　　　　　　\r\npause\r\ntaskkill /f /im Origin.exe\r\nipconfig /flushdns\r\ndel /f /s /q \"%LOCALAPPDATA%\\Origin\\*.*\" \r\ncls\r\n@ echo.\r\n@ echo.　=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=　　　　　\r\n@ echo.　　　　　　\r\n@ echo.　「修复已完成，Origin 客户端可正常运行」\r\n@ echo.　　　\r\n@ echo.　=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=\r\n@ echo.\r\necho. & pause";
            File.WriteAllText("FixOriginNetWorkBug.bat", cmd, Encoding.GetEncoding("GB2312"));

            if (File.Exists("FixOriginNetWorkBug.bat"))
                Process.Start("FixOriginNetWorkBug.bat");
        }
    }
}
