using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using ToolsV3.API;

namespace ToolsV3
{
    public partial class MainWindow : MetroWindow
    {
        public GameManager Manager = new GameManager();
        public CommandlineManager CommandlineManager;
        public ObservableCollection<GameProperty> GameProperties = new ObservableCollection<GameProperty>();
        public Utils.LaunchMode SelectedLaunchMode = Utils.LaunchMode.NORMAL;

        public MainWindow()
        {
            InitializeComponent();
            Core.Initialize();
            Setup();
            CheckForUpdateAsync();
        }

        private void Setup()
        {
            // throw error if GTA is not installed
            if (IsAnyNullOrEmpty(Manager))
            {
                MessageBox.Show("GTA V does not appear to be installed (missing registry keys)\n" +
                    "ToolsV will quit.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(0);
            }

            // add game properties to the datagrid
            foreach (GameProperty p in Manager.GameProperties)
            {
                GameProperties.Add(p);
            }
            GameInfoDataGrid.DataContext = GameProperties;
            CommandlineManager = new CommandlineManager(Manager);
            DeleteUpdater();
            SetCheckboxStates();
            Utils.Log("MainWindow: setup complete");
        }

        private async void CheckForUpdateAsync()
        {
            if (!await Updater.GetIsLatest())
            {
                string latest = await Updater.GetLatestTag();
                await Updater.ShowUpdateAvailableDialog(this, latest);
                Utils.Log("Current version: " + Updater.VERSION_TAG);
                Utils.Log("Latest: " + latest);
            }
        }

        public void HandleLaunchModeChange(object sender, RoutedEventArgs e)
        {
            var selectedRadio = sender as RadioButton;
            switch (selectedRadio.Name)
            {
                case "NormalRadio":
                    SelectedLaunchMode = Utils.LaunchMode.NORMAL;
                    break;

                case "SingleplayerWithModsRadio":
                    SelectedLaunchMode = Utils.LaunchMode.SINGLEPLAYER_WITH_MODS;
                    break;

                case "SingleplayerNoModsRadio":
                    SelectedLaunchMode = Utils.LaunchMode.SINGLEPLAYER_WITHOUT_MODS;
                    break;

                case "OnlineRadio":
                    SelectedLaunchMode = Utils.LaunchMode.ONLINE;
                    break;

                default:
                    SelectedLaunchMode = Utils.LaunchMode.NORMAL;
                    break;
            }
            Utils.Log("Selected launch mode: " + SelectedLaunchMode);
        }

        #region MenuItem clicks
        private void ItemInstallationFolder_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(Manager.InstallFolder);
        }

        private async void ItemModsFolder_Click(object sender, RoutedEventArgs e)
        {
            string path = Manager.InstallFolder + @"\mods";
            if (Directory.Exists(path))
            {
                Process.Start(path);
            }
            else
            {
                await ShowModsFolderErrorDialog();
            }
        }

        private void ItemSettings_Click(object sender, RoutedEventArgs e)
        {
            new SettingsWindow().Show();
        }

        private void ItemAbout_Click(object sender, RoutedEventArgs e)
        {
            new AboutWindow().Show();
        }

        private void ItemModManager_Click(object sender, RoutedEventArgs e)
        {
            new ModManagerWindow(Manager).Show();
        }
        #endregion

        #region other methods
        // check whether any field of given object is empty/null
        bool IsAnyNullOrEmpty(object o)
        {
            foreach (PropertyInfo pi in o.GetType().GetProperties())
            {
                if (pi.PropertyType == typeof(string))
                {
                    string value = (string)pi.GetValue(o);
                    if (string.IsNullOrEmpty(value))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void SetCheckboxStates()
        {
            List<Flag> flags = CommandlineManager.GetCommandlineArguments();
            for (int i = 0; i < flags.Count; i++)
            {
                if (flags[i].FlagCode.Equals("-verify"))
                {
                    VerifyCheckbox.SetCurrentValue(CheckBox.IsCheckedProperty, true);
                }
                else if (flags[i].FlagCode.Equals("-safemode"))
                {
                    SafeCheckbox.SetCurrentValue(CheckBox.IsCheckedProperty, true);
                }
                else if (flags[i].FlagCode.Equals("-scOfflineOnly"))
                {
                    SPOfflineCheckbox.SetCurrentValue(CheckBox.IsCheckedProperty, true);
                }
                else if (flags[i].FlagCode.Equals("-StraightIntoFreemode"))
                {
                    MPFreemodeCheckbox.SetCurrentValue(CheckBox.IsCheckedProperty, true);
                }
            }
        }

        public async Task ShowModsFolderErrorDialog()
        {
            MessageDialogResult res = await this.ShowMessageAsync("Error", "Mods folder does not exist. Would you like to create one?", MessageDialogStyle.AffirmativeAndNegative);
            if (res == MessageDialogResult.Affirmative)
            {
                Directory.CreateDirectory(Manager.InstallFolder + @"\mods");
                Process.Start(Manager.InstallFolder + @"\mods");
            }
        }

        private void DeleteUpdater()
        {
            string currentPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (File.Exists(currentPath + @"\Updater.exe"))
            {
                try
                {
                    File.Delete(currentPath + @"\Updater.exe");
                    Utils.Log("Deleted updater executable!");
                }
                catch (Exception ex)
                {
                    Utils.Log($"Could not delete updater executable!{Environment.NewLine}{ex.Message}");
                }
            }
        }
        #endregion

        private void ShowAllFlagsLink_Click(object sender, RoutedEventArgs e)
        {
            new FlagsWindow(Manager).ShowDialog();
        }

        private void LaunchGameTile_Click(object sender, RoutedEventArgs e)
        {
            Utils.Log("MainWindow: launch game");
            Utils.Log("selected launch mode: " + SelectedLaunchMode.ToString());
            if (this.SelectedLaunchMode.Equals(Utils.LaunchMode.NORMAL))
            {
                // do not move mods
                CommandlineManager.RemoveMultiplayerFlag();
            }
            else if (this.SelectedLaunchMode.Equals(Utils.LaunchMode.SINGLEPLAYER_WITHOUT_MODS))
            {
                // disable mods
                Manager.DisableMods();
                CommandlineManager.RemoveMultiplayerFlag();
            }
            else if (this.SelectedLaunchMode.Equals(Utils.LaunchMode.SINGLEPLAYER_WITH_MODS))
            {
                // enable mods
                Manager.EnableMods();
                CommandlineManager.RemoveMultiplayerFlag();
            }
            else if (this.SelectedLaunchMode.Equals(Utils.LaunchMode.ONLINE))
            {
                // disable mods
                Manager.DisableMods();
                CommandlineManager.SetCommandLineArgument(new Flag("-goStraightToMP", String.Empty));
            }
            //manager.StartGame();
        }

        private void QuickFlagChangeCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            if ((sender as CheckBox).Name.Equals(VerifyCheckbox.Name))
            {
                if ((bool) (sender as CheckBox).GetValue(CheckBox.IsCheckedProperty))
                {
                    CommandlineManager.SetCommandLineArgument(new Flag("-verify", String.Empty));
                }
                else
                {
                    CommandlineManager.RemoveCommandlineArgument("-verify");
                }
            }
            else if ((sender as CheckBox).Name.Equals(SafeCheckbox.Name))
            {
                if ((bool)(sender as CheckBox).GetValue(CheckBox.IsCheckedProperty))
                {
                    CommandlineManager.SetCommandLineArgument(new Flag("-safemode", String.Empty));
                }
                else
                {
                    CommandlineManager.RemoveCommandlineArgument("-safemode");
                }
            }
            else if ((sender as CheckBox).Name.Equals(SPOfflineCheckbox.Name))
            {
                if ((bool)(sender as CheckBox).GetValue(CheckBox.IsCheckedProperty))
                {
                    CommandlineManager.SetCommandLineArgument(new Flag("-scOfflineOnly", String.Empty));
                }
                else
                {
                    CommandlineManager.RemoveCommandlineArgument("-scOfflineOnly");
                }
            }
            else if ((sender as CheckBox).Name.Equals(MPFreemodeCheckbox.Name))
            {
                if ((bool)(sender as CheckBox).GetValue(CheckBox.IsCheckedProperty))
                {
                    CommandlineManager.SetCommandLineArgument(new Flag("-StraightIntoFreemode", String.Empty));
                }
                else
                {
                    CommandlineManager.RemoveCommandlineArgument("-StraightIntoFreemode");
                }
            }
        }

        private void DonateButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://www.patreon.com/Frio");
        }
    }
}
