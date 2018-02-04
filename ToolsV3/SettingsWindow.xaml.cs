using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using ToolsV3.API;

namespace ToolsV3
{
    public partial class SettingsWindow : MetroWindow
    {
        public SettingsWindow()
        {
            InitializeComponent();
            SetCheckBoxStates();
        }

        private async void CheckForUpdateButton_Click(object sender, RoutedEventArgs e)
        {
            if (!await Updater.GetIsLatest())
            {
                var latest = await Updater.GetLatestTag();
                await Updater.ShowUpdateAvailableDialog(this, latest);
                Utils.Log("Current version: " + Updater.VERSION_TAG);
                Utils.Log("Latest: " + latest);
            }
            else
            {
                await Updater.ShowNoUpdateAvailableDialog(this);
            }
        }

        private void VisitWebsiteButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(Utils.GTA5MODS_PAGE_URL);
        }

        private void ScriptHookCheckBox_StateChanged(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.CheckScriptHookOnStartup = ScriptHookCheckbox.IsChecked.Value;
        }

        private void AutoCloseCheckBox_StateChanged(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.QuitAfterGameLaunch = (sender as CheckBox).IsChecked.Value;
        }

        private void ViewChangelogButton_Click(object sender, RoutedEventArgs e)
        {
            new ChangelogWindow().Show();
        }

        private void SetCheckBoxStates()
        {
            this.AutoCloseCheckbox.IsChecked = Properties.Settings.Default.QuitAfterGameLaunch;
            this.ScriptHookCheckbox.IsChecked = Properties.Settings.Default.CheckScriptHookOnStartup;
        }

        private void SettingsWindow_OnClosing(object sender, CancelEventArgs e)
        {
            Utils.Log("SettingsWindow: saving settings...");
            Properties.Settings.Default.Save();
        }
    }
}
