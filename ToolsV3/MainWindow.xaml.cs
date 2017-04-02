using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
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
using ToolsV3.API;

namespace ToolsV3
{
    public partial class MainWindow : MetroWindow
    {
        public GameManager manager = new GameManager();
        public ObservableCollection<GameProperty> gameProperties = new ObservableCollection<GameProperty>();
        public Utils.LaunchMode selectedLaunchMode = Utils.LaunchMode.NORMAL;

        public MainWindow()
        {
            InitializeComponent();
            Setup();
        }

        private void Setup()
        {
            // throw error if GTA is not installed
            if (IsAnyNullOrEmpty(manager))
            {
                MessageBox.Show("GTA V does not appear to be installed (missing registry keys)\nToolsV will quit.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(0);
            }

            // add game properties to the datagrid
            foreach (GameProperty p in manager.GameProperties)
            {
                gameProperties.Add(p);
            }
            GameInfoDataGrid.DataContext = gameProperties;

        }

        public void HandleLaunchModeChange(object sender, RoutedEventArgs e)
        {
            var selectedRadio = sender as RadioButton;
            switch (selectedRadio.Name)
            {
                case "radioNormal":
                    selectedLaunchMode = Utils.LaunchMode.NORMAL;
                    break;

                case "radioSingleplayerWithMods":
                    selectedLaunchMode = Utils.LaunchMode.SINGLEPLAYER_WITH_MODS;
                    break;

                case "radioSingleplayerNoMods":
                    selectedLaunchMode = Utils.LaunchMode.SINGLEPLAYER_WITHOUT_MODS;
                    break;

                case "radioOnline":
                    selectedLaunchMode = Utils.LaunchMode.ONLINE;
                    break;

                default:
                    selectedLaunchMode = Utils.LaunchMode.NORMAL;
                    break;
            }
            Utils.Log("Selected launch mode: " + selectedLaunchMode);
        }

        #region MenuItem clicks
        private void ItemInstallationFolder_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(manager.InstallFolder);
        }

        private void ItemModsFolder_Click(object sender, RoutedEventArgs e)
        {
            string path = manager.InstallFolder + @"\mods";
            if (Directory.Exists(path))
            {
                Process.Start(path);
            }
            else
            {
                ShowModsFolderErrorDialog();
            }
        }

        private void ItemSettings_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ItemAbout_Click(object sender, RoutedEventArgs e)
        {

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

        public async Task ShowModsFolderErrorDialog()
        {
            MessageDialogResult res = await this.ShowMessageAsync("Error", "Mods folder does not exist. Would you like to create one?", MessageDialogStyle.AffirmativeAndNegative);
            if (res == MessageDialogResult.Affirmative)
            {
                Directory.CreateDirectory(manager.InstallFolder + @"\mods");
                Process.Start(manager.InstallFolder + @"\mods");
            }
        }
        #endregion

        private void linkShowAllFlags_Click(object sender, RoutedEventArgs e)
        {
            FlagsWindow fw = new FlagsWindow(manager);
            fw.ShowDialog();
        }
    }
}
