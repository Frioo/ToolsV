using MahApps.Metro.Controls;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Data;

namespace ToolsV3
{
    public partial class ModManagerWindow : MetroWindow
    {
        private GameManager gameManager;
        private ObservableCollection<Mod> mods = new ObservableCollection<Mod>();
        private List<Mod> modsToDisable = new List<Mod>();
        private List<Mod> modsToEnable = new List<Mod>();

        public ModManagerWindow(GameManager m)
        {
            InitializeComponent();
            gameManager = m;
            Setup();
        }

        private void Setup()
        {
            List<Mod> allMods = gameManager.GetMods(true);
            for (int i = 0; i < allMods.Count; i++) mods.Add(allMods[i]);
            ModsDataGrid.DataContext = mods;
        }

        private void ModWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Utils.Log("Saving mods...");
            gameManager.DisableMods(modsToDisable);
            gameManager.EnableMods(modsToEnable);
        }

        private void ModsDataGrid_TargetUpdated(object sender, DataTransferEventArgs e)
        {
            DataGrid dg = sender as DataGrid;
            if (dg.SelectedIndex != -1)
            {
                Mod selected = dg.SelectedItem as Mod;
                Utils.Log("Mod updated: " + selected.Filename);
                Utils.Log("Enabled: " + selected.IsEnabled);
                if (!selected.IsEnabled && !modsToDisable.Contains(selected))
                {
                    if (modsToEnable.Contains(selected)) modsToEnable.Remove(selected);
                    modsToDisable.Add(selected);
                }
                else if (selected.IsEnabled && !modsToEnable.Contains(selected))
                {
                    if (modsToDisable.Contains(selected)) modsToDisable.Remove(selected);
                    modsToEnable.Add(selected);
                }
            }
        }
    }
}
