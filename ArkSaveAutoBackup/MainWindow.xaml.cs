using System;
using System.Collections.Generic;
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
using Hardcodet.Wpf.TaskbarNotification;
using AutomaticFileBackup;

#if DEBUG
using System.Diagnostics;
#endif

namespace ArkSaveAutoBackup
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string arkSaveDirectory;
        string backupDirectory;
        bool backupEnabled;
        FileWatcher fileWatcher;

        bool closingFromTaskbarIcon = false;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            arkSaveDirectory = Properties.ArkSaveAutoBackup.Default.SourceDirectory;
            TextBox_SourceFolder.Text = arkSaveDirectory;

            backupDirectory = Properties.ArkSaveAutoBackup.Default.DestinationDirectory;
            TextBox_DestinationFolder.Text = backupDirectory;

            backupEnabled = Properties.ArkSaveAutoBackup.Default.BackupEnabled;
            CheckBox_Enabled.IsChecked = backupEnabled;

            UpdateStatus();
        }

        private void Button_ChooseSource_Click(object sender, RoutedEventArgs e)
        {
            string sourcePath = GetFolderFromDialog("Choose folder where your ark saves are.");
            TextBox_SourceFolder.Text = sourcePath;
            TextBox_SourceFolder.Focus();
            TextBox_SourceFolder.CaretIndex = TextBox_SourceFolder.Text.Length;

            arkSaveDirectory = sourcePath;

            Properties.ArkSaveAutoBackup.Default.SourceDirectory = sourcePath;
            Properties.ArkSaveAutoBackup.Default.Save();
            UpdateStatus();
        }

        private void Button_ChooseDestination_Click(object sender, RoutedEventArgs e)
        {
            string destinationPath = GetFolderFromDialog("Choose folder where you want the backup to be.");
            TextBox_DestinationFolder.Text = destinationPath;
            TextBox_DestinationFolder.Focus();
            TextBox_DestinationFolder.CaretIndex = TextBox_DestinationFolder.Text.Length;

            backupDirectory = destinationPath;

            Properties.ArkSaveAutoBackup.Default.DestinationDirectory = destinationPath;
            Properties.ArkSaveAutoBackup.Default.Save();
            UpdateStatus();
        }

        private string GetFolderFromDialog(string description)
        {
            var folderDialog = new System.Windows.Forms.FolderBrowserDialog();
            folderDialog.Description = description;
            folderDialog.ShowNewFolderButton = true;

            var result = folderDialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                return folderDialog.SelectedPath;
            }

            return "";
        }
        private void UpdateStatus()
        {
            string status = "";

            CheckBox_Enabled.IsEnabled = true;

            if (!Directory.Exists(arkSaveDirectory))
            {
                status += "Ark save folder path is invalid.\n";
                CheckBox_Enabled.IsChecked = false;
                CheckBox_Enabled.IsEnabled = false;
            }
            else if (Directory.GetFiles(arkSaveDirectory, "*.ark").Length == 0)
            {
                status += "Warning: Selected source directory contains no ark save files.\n\tWill continue to monitor this folder in case of new ARK install.\n";
            }

            if (!Directory.Exists(backupDirectory))
            {
                status += "Backup folder path is invalid.\n";
                CheckBox_Enabled.IsChecked = false;
                CheckBox_Enabled.IsEnabled = false;
            }

            if (CheckBox_Enabled.IsChecked.Value == true)
            {
                status += "Your save files are bing backed up.";
            }
            else
            {
                status += "Save is not being backed up.";
            }
            TextBlock_Status.Text = status;
            Properties.ArkSaveAutoBackup.Default.BackupEnabled = CheckBox_Enabled.IsChecked.Value;
            Properties.ArkSaveAutoBackup.Default.Save();

            UpdateFileWatchingStatus();
        }

        private void UpdateFileWatchingStatus()
        {
            if (backupEnabled)
            {
                EnableFileWatching();
            }
            else
            {
                DisableFileWatching();
            }
        }

        private void EnableFileWatching()
        {
            if (fileWatcher != null)
            {
                fileWatcher.Dispose();
            }
            fileWatcher = FileWatcherFactory.GetArkFileWatcher(arkSaveDirectory, backupDirectory);
        }

        private void DisableFileWatching()
        {
            if (fileWatcher == null)
            {
                return;
            }
            fileWatcher.Dispose();
        }

        private void CheckBox_Enabled_Checked(object sender, RoutedEventArgs e)
        {
            backupEnabled = true;
            UpdateStatus();
        }

        private void CheckBox_Enabled_Unchecked(object sender, RoutedEventArgs e)
        {
            backupEnabled = false;
            UpdateStatus();
        }

        private void Window_MainWindow_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized)
            {
                this.Hide();
            }
        }

        private void Window_MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!closingFromTaskbarIcon)
            {
                this.Hide();
                e.Cancel = true;
                ShowBaloon("Ark Save Auto Backup", "Minimized to tray.\n\nRight click on the tray icon and select close to actualy exit.\n", BalloonIcon.Info);
            }
        }

        private void TaskbarIcon_Menu_ShowGUI_Click(object sender, RoutedEventArgs e)
        {
            this.Show();
            this.WindowState = WindowState.Normal;
        }

        private void TaskbarIcon_Menu_Exit_Click(object sender, RoutedEventArgs e)
        {
            string messageBoxText = "Are you sure you want to stop automagically backing up your ARK save?";
            string caption = "Ark Save Auto Backup";
            MessageBoxButton button = MessageBoxButton.YesNo;
            MessageBoxImage icon = MessageBoxImage.Warning;
            MessageBoxOptions options = MessageBoxOptions.DefaultDesktopOnly;
            MessageBoxResult result = MessageBox.Show(messageBoxText, caption, button, icon, MessageBoxResult.No, options);

            switch (result)
            {
                case MessageBoxResult.None:
                    break;
                case MessageBoxResult.OK:
                    break;
                case MessageBoxResult.Cancel:
                    break;
                case MessageBoxResult.Yes:
                    closingFromTaskbarIcon = true;
                    this.Close();
                    break;
                case MessageBoxResult.No:
                    break;
                default:
                    break;
            }
        }

        private void ShowBaloon(string title, string text, BalloonIcon baloonIcon = BalloonIcon.None)
        {
            TaskbarIcon_Default.ShowBalloonTip(title, text, baloonIcon);
        }

        private void ShowBaloon(string title, string text, System.Drawing.Icon icon, bool largeIcon = false)
        {
            TaskbarIcon_Default.ShowBalloonTip(title, text, icon, largeIcon);
        }

        private void DebugMessage(string message)
        {
#if DEBUG
            Debug.WriteLine(message);
#endif
        }

        private void Window_MainWindow_Closed(object sender, EventArgs e)
        {
            fileWatcher.Dispose();
        }
    }
}
