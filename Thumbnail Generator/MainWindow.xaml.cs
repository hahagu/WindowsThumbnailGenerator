using Ookii.Dialogs.Wpf;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace Thumbnail_Generator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly string[] supportedFiles = { 
            "jpg", "jpeg", "png", "mp4", "mov", "wmv", "avi", "mkv"
        };

        private volatile int progressCount;
        private volatile float progressPercentage;

        public MainWindow()
        {
            InitializeComponent();
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        private async void generateThumbnailsForFolder(string rootFolder, int fileCount, bool recursive)
        {
            progressCount = 0;
            progressPercentage = 0;

            startBtn.IsEnabled = false;
            startBtn.Visibility = Visibility.Hidden;
            currentProgress.Visibility = Visibility.Visible;
            progressLabel.Visibility = Visibility.Visible;

            string[] pathList = { rootFolder };

            if (recursive)
            {
                pathList = pathList.Concat(Directory.GetDirectories(rootFolder, "*", SearchOption.AllDirectories)).ToArray();
            }

            await Task.Run(() => processParallel(pathList, fileCount));

            startBtn.IsEnabled = true;
            startBtn.Visibility = Visibility.Visible;
            currentProgress.Visibility = Visibility.Hidden;
            progressLabel.Visibility = Visibility.Hidden;
            currentProgress.Value = 0;
            progressLabel.Content = "0%";
        }

        private void processParallel(string[] pathList, int fileCount)
        {
            Parallel.ForEach(pathList, directory =>
            {
                ImageHandler imageHandler = new();
                unsetSystem(directory + "\\thumb.ico");

                string[] fileList = { };
                foreach (string fileFormat in supportedFiles)
                {
                    fileList = fileList.Concat(Directory.GetFiles(directory, "*." + fileFormat)).ToArray();
                }

                if (fileList.Length <= 0) return;
                if (fileList.Length > fileCount) fileList = fileList.Take(fileCount).ToArray();

                imageHandler.generateThumbnail(fileList, directory + "\\thumb");

                setSystem(directory + "\\thumb.ico");
                applyFolderIcon(directory, directory + "\\thumb.ico");

                progressCount++;
                progressPercentage = (float)progressCount / pathList.Length * 100;

                Dispatcher.Invoke(new Action(() =>
                {
                    currentProgress.Value = progressPercentage;
                    progressLabel.Content = string.Format("{0:0.##}", progressPercentage) + "%";
                }));

            });
        }

        private void applyFolderIcon(string targetFolderPath, string iconFilePath)
        {
            var iniPath = Path.Combine(targetFolderPath, "desktop.ini");
            unsetSystem(iniPath);

            // Writes desktop.ini Contents
            var iniContents = new StringBuilder()
                .AppendLine("[.ShellClassInfo]")
                .AppendLine("IconResource=" + iconFilePath +",0")
                .AppendLine($"IconFile=" + iconFilePath)
                .AppendLine("IconIndex=0")
                .AppendLine("[ViewState]")
                .AppendLine("Mode=")
                .AppendLine("Mode=")
                .AppendLine("Vid=")
                .ToString();

            int lcid = CultureInfo.CurrentCulture.LCID;
            int codepage = CultureInfo.GetCultureInfo(lcid).TextInfo.ANSICodePage;
            Encoding encoding = Encoding.GetEncoding(codepage);

            File.WriteAllText(iniPath, iniContents, encoding);

            // Set Folder SYSTEM flag, to show thumbnail
            File.SetAttributes(
                targetFolderPath,
                FileAttributes.ReadOnly);

            setSystem(iniPath);
        }

        private void unsetSystem(string targetPath)
        {
            if (File.Exists(targetPath))
            {
                // Make Read Writable
                File.SetAttributes(
                   targetPath,
                   File.GetAttributes(targetPath) &
                   ~(FileAttributes.Hidden | FileAttributes.System));
            }
        }

        private void setSystem(string targetPath)
        {
            File.SetAttributes(
               targetPath,
               File.GetAttributes(targetPath) | FileAttributes.Hidden | FileAttributes.System);
        }

        private void browseBtn_Click(object sender, RoutedEventArgs e)
        {
            VistaFolderBrowserDialog folderBrowser = new();
            if (!folderBrowser.ShowDialog().GetValueOrDefault())
            {
                return;
            }
            targetFolder.Text = folderBrowser.SelectedPath;
        }

        private void startBtn_Click(object sender, RoutedEventArgs e)
        {
            generateThumbnailsForFolder(targetFolder.Text, 3, recursiveChk.IsChecked.GetValueOrDefault());
        }
    }
}
