using Ookii.Dialogs.Wpf;
using System;
using System.IO;
using System.Windows;
using Core_Library;

namespace Thumbnail_Generator_GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
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

        private async void startBtn_Click(object sender, RoutedEventArgs e)
        {
            disableControls();
            resetProgress();

            if (targetFolder.Text.Length <= 0)
            {
                MessageBox.Show("You didn't choose a folder!", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                enableControls();
                return;
            } else if (!Directory.Exists(targetFolder.Text))
            {
                MessageBox.Show("The directory you chose does not exist!", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                enableControls();
                return;
            }
            
            Progress<float> progress = new Progress<float>(percentage => setProgress(percentage));
            int result = await ProcessHandler.generateThumbnailsForFolder(
                progress,
                targetFolder.Text,
                (int)maxThumbCount.Value,
                recursiveChk.IsChecked.GetValueOrDefault(),
                skipExistingChk.IsChecked.GetValueOrDefault(),
                useShortChk.IsChecked.GetValueOrDefault(),
                cleanChk.IsChecked.GetValueOrDefault(),
                (int)maxThreadsCount.Value
            );

            enableControls();
        }

        private void cleanChk_Checked(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Choosing this option will restart explorer!\nSave your work before proceeding!" , "Warning!", MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }

        public void enableControls()
        {
            startBtn.IsEnabled = true;
            startBtn.Visibility = Visibility.Visible;
            currentProgress.Visibility = Visibility.Hidden;
            progressLabel.Visibility = Visibility.Hidden;
            currentProgress.Value = 0;
            progressLabel.Content = "0%";
            
            targetFolder.IsEnabled = true;
            browseBtn.IsEnabled = true;
            recursiveChk.IsEnabled = true;
            cleanChk.IsEnabled = true;
            skipExistingChk.IsEnabled = true;
            useShortChk.IsEnabled = true;
            maxThumbCount.IsEnabled = true;
            maxThreadsCount.IsEnabled = true;
        }

        public void disableControls()
        {
            startBtn.IsEnabled = false;
            startBtn.Visibility = Visibility.Hidden;
            currentProgress.Visibility = Visibility.Visible;
            progressLabel.Visibility = Visibility.Visible;
            
            targetFolder.IsEnabled = false;
            browseBtn.IsEnabled = false;
            recursiveChk.IsEnabled = false;
            cleanChk.IsEnabled = false;
            skipExistingChk.IsEnabled = false;
            useShortChk.IsEnabled = false;
            maxThumbCount.IsEnabled = false;
            maxThreadsCount.IsEnabled = false;
        }

        public void setProgress(float progressPercentage)
        {
            currentProgress.Value = progressPercentage;
            progressLabel.Content = string.Format("{0:0.##}", progressPercentage) + "%";
        }

        public void resetProgress()
        {
            currentProgress.Value = 0;
            progressLabel.Content = "Initializing..";
        }
    }
}
