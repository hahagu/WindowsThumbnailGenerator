using Ookii.Dialogs.Wpf;
using System;
using System.IO;
using System.Windows;
using Thumbnail_Generator_Library;

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
            MaxThreadsCount.Maximum = Convert.ToInt32(Environment.ProcessorCount);
            MaxThreadsCount.Value = Convert.ToInt32(Environment.ProcessorCount);
        }

        private void BrowseBtn_Click(object sender, RoutedEventArgs e)
        {
            VistaFolderBrowserDialog folderBrowser = new();
            if (!folderBrowser.ShowDialog().GetValueOrDefault())
            {
                return;
            }
            TargetFolder.Text = folderBrowser.SelectedPath;
        }

        private async void StartBtn_Click(object sender, RoutedEventArgs e)
        {
            DisableControls();
            ResetProgress();

            if (TargetFolder.Text.Length <= 0)
            {
                _ = ModernWpf.MessageBox.Show("You didn't choose a folder!", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                EnableControls();
                return;
            } else if (!Directory.Exists(TargetFolder.Text))
            {
                _ = ModernWpf.MessageBox.Show("The directory you chose does not exist!", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                EnableControls();
                return;
            }
            
            Progress<float> progress = new Progress<float>(percentage => SetProgress(percentage));
            int result = await ProcessHandler.GenerateThumbnailsForFolder(
                progress,
                TargetFolder.Text,
                (int)MaxThumbCount.Value,
                (int)MaxThreadsCount.Value,
                RecursiveChk.IsChecked.GetValueOrDefault(),
                CleanChk.IsChecked.GetValueOrDefault(),
                SkipExistingChk.IsChecked.GetValueOrDefault(),
                UseShortChk.IsChecked.GetValueOrDefault()
            );

            EnableControls();
        }

        private void CleanChk_Checked(object sender, RoutedEventArgs e)
        {
            _ = ModernWpf.MessageBox.Show("Choosing this option will restart explorer!\nSave your work before proceeding!", "Warning!", MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }

        public void EnableControls()
        {
            StartBtn.IsEnabled = true;
            StartBtn.Visibility = Visibility.Visible;
            CurrentProgress.Visibility = Visibility.Hidden;
            ProgressLabel.Visibility = Visibility.Hidden;
            CurrentProgress.Value = 0;
            ProgressLabel.Content = "0%";
            
            TargetFolder.IsEnabled = true;
            BrowseBtn.IsEnabled = true;
            RecursiveChk.IsEnabled = true;
            CleanChk.IsEnabled = true;
            SkipExistingChk.IsEnabled = true;
            UseShortChk.IsEnabled = true;
            MaxThumbCount.IsEnabled = true;
            MaxThreadsCount.IsEnabled = true;
        }

        public void DisableControls()
        {
            StartBtn.IsEnabled = false;
            StartBtn.Visibility = Visibility.Hidden;
            CurrentProgress.Visibility = Visibility.Visible;
            ProgressLabel.Visibility = Visibility.Visible;
            
            TargetFolder.IsEnabled = false;
            BrowseBtn.IsEnabled = false;
            RecursiveChk.IsEnabled = false;
            CleanChk.IsEnabled = false;
            SkipExistingChk.IsEnabled = false;
            UseShortChk.IsEnabled = false;
            MaxThumbCount.IsEnabled = false;
            MaxThreadsCount.IsEnabled = false;
        }

        public void SetProgress(float progressPercentage)
        {
            CurrentProgress.Value = progressPercentage;
            ProgressLabel.Content = string.Format("{0:0.##}", progressPercentage) + "%";
        }

        public void ResetProgress()
        {
            CurrentProgress.Value = 0;
            ProgressLabel.Content = "Initializing..";
        }
    }
}
