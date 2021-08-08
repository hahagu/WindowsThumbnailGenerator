using Ookii.Dialogs.Wpf;
using System;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using Thumbnail_Generator_Library;

namespace Thumbnail_Generator_GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow mainWindowInstance = new();

        public MainWindow()
        {
            InitializeComponent();
            mainWindowInstance = this;
            maxThreadsCount.Maximum = Convert.ToInt32(Environment.ProcessorCount);
            maxThreadsCount.Value = Convert.ToInt32(Environment.ProcessorCount);
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
            
            ProcessHandler processHandler = new ProcessHandler();

            _ = processHandler.generateThumbnailsForFolder(
                targetFolder.Text,
                (int)maxThumbCount.Value,
                (int)maxThreadsCount.Value,
                recursiveChk.IsChecked.GetValueOrDefault(),
                skipExistingChk.IsChecked.GetValueOrDefault(),
                useShortChk.IsChecked.GetValueOrDefault(),
                cleanChk.IsChecked.GetValueOrDefault()
            );
        }

        private void cleanChk_Checked(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.MessageBox.Show("Choosing this option will restart explorer!\nSave your work before proceeding!" , "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        public static void enableControls()
        {
            mainWindowInstance.Dispatcher.Invoke(new Action(() =>
            {
                mainWindowInstance.startBtn.IsEnabled = true;
                mainWindowInstance.startBtn.Visibility = Visibility.Visible;
                mainWindowInstance.currentProgress.Visibility = Visibility.Hidden;
                mainWindowInstance.progressLabel.Visibility = Visibility.Hidden;
                mainWindowInstance.currentProgress.Value = 0;
                mainWindowInstance.progressLabel.Content = "0%";
                
                mainWindowInstance.targetFolder.IsEnabled = true;
                mainWindowInstance.browseBtn.IsEnabled = true;
                mainWindowInstance.recursiveChk.IsEnabled = true;
                mainWindowInstance.cleanChk.IsEnabled = true;
                mainWindowInstance.skipExistingChk.IsEnabled = true;
                mainWindowInstance.useShortChk.IsEnabled = true;
                mainWindowInstance.maxThumbCount.IsEnabled = true;
                mainWindowInstance.maxThreadsCount.IsEnabled = true;
            }));
        }

        public static void disableControls()
        {
            mainWindowInstance.Dispatcher.Invoke(new Action(() =>
            {
                mainWindowInstance.startBtn.IsEnabled = false;
                mainWindowInstance.startBtn.Visibility = Visibility.Hidden;
                mainWindowInstance.currentProgress.Visibility = Visibility.Visible;
                mainWindowInstance.progressLabel.Visibility = Visibility.Visible;

                mainWindowInstance.targetFolder.IsEnabled = false;
                mainWindowInstance.browseBtn.IsEnabled = false;
                mainWindowInstance.recursiveChk.IsEnabled = false;
                mainWindowInstance.cleanChk.IsEnabled = false;
                mainWindowInstance.skipExistingChk.IsEnabled = false;
                mainWindowInstance.useShortChk.IsEnabled = false;
                mainWindowInstance.maxThumbCount.IsEnabled = false;
                mainWindowInstance.maxThreadsCount.IsEnabled = false;
            }));
        }

        public static void setProgress(float progressPercentage)
        {
            mainWindowInstance.Dispatcher.Invoke(new Action(() =>
            {
                mainWindowInstance.currentProgress.Value = progressPercentage;
                mainWindowInstance.progressLabel.Content = string.Format("{0:0.##}", progressPercentage) + "%";
            }));
        }
    }
}
