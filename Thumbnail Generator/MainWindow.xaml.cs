using Ookii.Dialogs.Wpf;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
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
            maxThreadsCount.Maximum = Convert.ToInt32(Environment.ProcessorCount);
            maxThreadsCount.Value = Convert.ToInt32(Environment.ProcessorCount);
        }

        private async void generateThumbnailsForFolder(string rootFolder, int fileCount, int threadCount, bool recursive, bool skipExisting, bool useShort)
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

            await Task.Run(() => processParallel(pathList, fileCount, skipExisting, useShort, threadCount));

            startBtn.IsEnabled = true;
            startBtn.Visibility = Visibility.Visible;
            currentProgress.Visibility = Visibility.Hidden;
            progressLabel.Visibility = Visibility.Hidden;
            currentProgress.Value = 0;
            progressLabel.Content = "0%";

            if (cleanChk.IsChecked.GetValueOrDefault()) clearCache();
        }

        private void processParallel(string[] pathList, int fileCount, bool skipExisting, bool useShort, int maxThreads = 4)
        {
            _ = Parallel.ForEach(pathList,
                new ParallelOptions { MaxDegreeOfParallelism = maxThreads },
                directory =>
                  {
                      string iconLocation = Path.Combine(directory, "thumb.ico");
                      string iniLocation = Path.Combine(directory, "desktop.ini");

                      if (File.Exists(iniLocation) && skipExisting) return;

                      ImageHandler imageHandler = new();
                      unsetSystem(iconLocation);

                      string[] fileList = { };
                      foreach (string fileFormat in supportedFiles)
                      {
                          fileList = fileList.Concat(Directory.GetFiles(directory, "*." + fileFormat)).ToArray();
                      }

                      if (fileList.Length <= 0) return;
                      if (fileList.Length > fileCount) fileList = fileList.Take(fileCount).ToArray();

                      imageHandler.generateThumbnail(fileList, iconLocation, useShort);

                      setSystem(iconLocation);
                      applyFolderIcon(directory, iconLocation);

                      progressCount++;
                      progressPercentage = (float)progressCount / pathList.Length * 100;

                      Dispatcher.Invoke(new Action(() =>
                      {
                          currentProgress.Value = progressPercentage;
                          progressLabel.Content = string.Format("{0:0.##}", progressPercentage) + "%";
                      }));
                  }
              );
        }

        private void applyFolderIcon(string targetFolderPath, string iconFilePath)
        {
            var iniPath = Path.Combine(targetFolderPath, "desktop.ini");
            unsetSystem(iniPath);

            // Writes desktop.ini Contents
            var iniContents = new StringBuilder()
                .AppendLine("[.ShellClassInfo]")
                .AppendLine("IconResource=" + iconFilePath + ",0")
                .AppendLine("IconFile=" + iconFilePath)
                .AppendLine("IconIndex=0")
                .AppendLine("[ViewState]")
                .AppendLine("Mode=")
                .AppendLine("Mode=")
                .AppendLine("Vid=")
                .ToString();

            File.WriteAllText(iniPath, iniContents);

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

        private void clearCache()
        {
            using RestartManagerSession rm = new();
            rm.RegisterProcess(GetShellProcess());
            rm.Shutdown(RestartManagerSession.ShutdownType.ForceShutdown);

            string localAppData = Environment.GetEnvironmentVariable("LocalAppData");
            string targetFolder = Path.Combine(localAppData, @"Microsoft\Windows\Explorer\");

            string[] targetFiles = Directory.GetFiles(targetFolder, "thumbcache_*.db");
            targetFiles = targetFiles.Concat(Directory.GetFiles(targetFolder, "iconcache_*.db")).ToArray();

            foreach (string file in targetFiles)
            {
                File.Delete(file);
            }

            rm.Restart();
        }

        [DllImport("user32")]
        private static extern IntPtr GetShellWindow();

        [DllImport("user32", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(IntPtr windowHandle, out uint processId);

        public static Process GetShellProcess()
        {
            try
            {
                var shellWindowHandle = GetShellWindow();

                if (shellWindowHandle != IntPtr.Zero)
                {
                    GetWindowThreadProcessId(shellWindowHandle, out var shellPid);

                    if (shellPid > 0)
                    {
                        return Process.GetProcessById((int) shellPid);
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }

            return null;
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
            if (targetFolder.Text.Length <= 0)
            {
                System.Windows.Forms.MessageBox.Show("You didn't choose a folder!", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            } else if (!Directory.Exists(targetFolder.Text))
            {
                System.Windows.Forms.MessageBox.Show("The directory you chose does not exist!", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            generateThumbnailsForFolder(
                targetFolder.Text,
                (int)maxThumbCount.Value,
                (int)maxThreadsCount.Value,
                recursiveChk.IsChecked.GetValueOrDefault(),
                skipExistingChk.IsChecked.GetValueOrDefault(),
                useShortChk.IsChecked.GetValueOrDefault()
            );
        }

        private void cleanChk_Checked(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.MessageBox.Show("Choosing this option will restart explorer!\nSave your work before proceeding!" , "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }
    }
}
