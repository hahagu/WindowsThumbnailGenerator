using Ookii.Dialogs.Wpf;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;

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

        public MainWindow()
        {
            InitializeComponent();
        }

        private void generateThumbnailsForFolder(string rootFolder, int fileCount, bool recursive)
        {
            ImageHandler imageHandler = new ImageHandler();
            string[] pathList = { rootFolder };

            if (recursive)
            {
                pathList = pathList.Concat(Directory.GetDirectories(rootFolder, "*", SearchOption.AllDirectories)).ToArray();
            }

            foreach (string directory in pathList)
            {
                unsetSystem(directory + "\\thumb.ico");

                string[] fileList = { };
                foreach (string fileFormat in supportedFiles)
                {
                    fileList = fileList.Concat(Directory.GetFiles(directory, "*." + fileFormat)).ToArray();
                }

                if (fileList.Length <= 0) continue;
                if (fileList.Length > fileCount) fileList = fileList.Take(fileCount).ToArray();

                imageHandler.generateThumbnail(fileList, directory + "\\thumb");

                setSystem(directory + "\\thumb.ico");
                applyFolderIcon(directory, directory + "\\thumb.ico");
            }
        }

        private void applyFolderIcon(string targetFolderPath, string iconFilePath)
        {
            var iniPath = Path.Combine(targetFolderPath, "desktop.ini");
            unsetSystem(iniPath);

            // Writes desktop.ini Contents
            var iniContents = new StringBuilder()
                .AppendLine("[.ShellClassInfo]")
                .AppendLine($"IconResource={iconFilePath},0")
                .AppendLine($"IconFile={iconFilePath}")
                .AppendLine("IconIndex=0")
                .ToString();
            File.WriteAllText(iniPath, iniContents);

            // Set Folder SYSTEM flag, to show thumbnail
            File.SetAttributes(
                targetFolderPath,
                File.GetAttributes(targetFolderPath) | FileAttributes.System);

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
