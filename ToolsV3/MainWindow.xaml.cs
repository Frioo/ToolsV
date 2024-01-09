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
using ToolsV.Properties;
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

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (Settings.Default.CheckScriptHookOnStartup)
            {
                CheckScriptHookVersion();
            }
        }

        private void Setup()
        {
            // throw error if GTA is not installed
            if (IsAnyNullOrEmpty(Manager))
            {
                ShowInitErrorAndExit();
                Environment.Exit(0);
            }

            // add game properties to the datagrid
            foreach (var property in Manager.GameProperties)
            {
                GameProperties.Add(property);
            }
            GameInfoDataGrid.DataContext = GameProperties;
            CommandlineManager = new CommandlineManager(Manager);
            SetCheckboxStates();
            if (!Manager.IsScriptHookInstalled())
            {
                Utils.Log("ScriptHookV not installed, disabling menu item...");
                itemScriptHookChecker.IsEnabled = false;
            }
            Utils.Log("MainWindow: setup complete");
        }

        private async void CheckForUpdateAsync()
        {
            if (await Updater.GetIsLatest()) return;
            var latest = await Updater.GetLatestTag();
            await Updater.ShowUpdateAvailableDialog(this, latest);
            Utils.Log("Current version: " + Updater.VERSION_TAG);
            Utils.Log("Latest: " + latest);
        }

        private async Task CheckScriptHookVersion()
        {
            if (!Manager.IsScriptHookCompatible()) await ShowScriptHookOutdatedWarningDialog();
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
        public static bool IsAnyNullOrEmpty(object o)
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

        public async Task ShowScriptHookUpToDateDialog()
        {
            await this.ShowMessageAsync("ScriptHookV is up-to-date",
                $"ScriptHookV is compatible with game patch version." +
                $"{Environment.NewLine}" +
                $"Patch version: {Manager.PatchVersion}" +
                $"{Environment.NewLine}" +
                $"ScriptHookV version: {Manager.GetScriptHookVersion()}",
                MessageDialogStyle.Affirmative);
        }

        public async Task ShowScriptHookOutdatedWarningDialog()
        {
            await this.ShowMessageAsync("ScriptHookV is outdated",
                $"ScriptHookV version is not compatible with installed patch." +
                $"{Environment.NewLine}" +
                $"GTA patch version: {Manager.PatchVersion}" +
                $"{Environment.NewLine}" +
                $"ScriptHookV version: {Manager.GetScriptHookVersion()}",
                MessageDialogStyle.Affirmative);
        }

        public void ShowScriptHookOutdatedWarningDialogAlt()
        {
            MessageBox.Show($"ScriptHookV is not compatible with installed game patch." +
                            $"{Environment.NewLine}" +
                            $"Game patch version: {Manager.PatchVersion}" +
                            $"{Environment.NewLine}" +
                            $"ScriptHookV version: {Manager.GetScriptHookVersion()}", "ScriptHookV is outdated",
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        public async Task ShowModsFolderErrorDialog()
        {
            var res = await this.ShowMessageAsync("Error", "Mods folder does not exist. Would you like to create one?", MessageDialogStyle.AffirmativeAndNegative);
            if (res == MessageDialogResult.Affirmative)
            {
                Directory.CreateDirectory(Manager.InstallFolder + @"\mods");
                Process.Start(Manager.InstallFolder + @"\mods");
            }
        }

        public static void ShowInitErrorAndExit()
        {
            MessageBox.Show($"GTA V does not appear to be installed.{Environment.NewLine}If you are certain it is, please review GTA V registry keys on your system.",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Environment.Exit(0);
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
            Manager.StartGame();
            if (Settings.Default.QuitAfterGameLaunch)
            {
                Environment.Exit(0);
            }
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

        private async void ItemScriptHookChecker_OnClick(object sender, RoutedEventArgs e)
        {
            if (!Manager.IsScriptHookCompatible())
            {
                await ShowScriptHookOutdatedWarningDialog();
            }
            else
            {
                await ShowScriptHookUpToDateDialog();
            }
        }
    }
}
