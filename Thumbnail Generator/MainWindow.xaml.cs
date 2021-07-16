using Ookii.Dialogs.Wpf;
using System.IO;
using System.Linq;
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
            string[] fileList = { };

            if (recursive)
            {
                pathList = Directory.GetDirectories(rootFolder, "*", SearchOption.AllDirectories);
            }

            foreach (string directory in pathList)
            {
                foreach (string fileFormat in supportedFiles)
                {
                    fileList = fileList.Concat(Directory.GetFiles(directory, "*." + fileFormat)).ToArray();
                }

                if (fileList.Length <= 0) break;
                if (fileList.Length > fileCount) fileList = fileList.Take(fileCount).ToArray();

                imageHandler.generateThumbnail(fileList, directory + "\\thumb");
            }
        }

        private void browseBtn_Click(object sender, RoutedEventArgs e)
        {
            VistaFolderBrowserDialog folderBrowser = new VistaFolderBrowserDialog();
            if (folderBrowser.ShowDialog().GetValueOrDefault())
            {
                targetFolder.Text = folderBrowser.SelectedPath;
            }
        }

        private void startBtn_Click(object sender, RoutedEventArgs e)
        {
            generateThumbnailsForFolder(targetFolder.Text, 3, recursiveChk.IsChecked.GetValueOrDefault());
        }
    }
}
