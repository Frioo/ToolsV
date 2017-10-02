using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System.Threading.Tasks;
using System.Windows;
using ToolsV3.API;

namespace ToolsV3
{
    public partial class SettingsWindow : MetroWindow
    {
        public SettingsWindow()
        {
            InitializeComponent();
        }

        private async void CheckForUpdateButton_Click(object sender, RoutedEventArgs e)
        {
            if (!await Updater.GetIsLatest())
            {
                string latest = await Updater.GetLatestTag();
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
            // TODO
        }

        private void ScriptHookCheckBox_StateChanged(object sender, RoutedEventArgs e)
        {
            // TODO
        }

        private void AutoCloseCheckBox_StateChanged(object sender, RoutedEventArgs e)
        {
            // TODO
        }

        private void ViewChangelogButton_Click(object sender, RoutedEventArgs e)
        {
            new ChangelogWindow().Show();
        }
    }
}
