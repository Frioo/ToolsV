using MahApps.Metro.Controls;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Data;

namespace ToolsV3
{
    public partial class ModManagerWindow : MetroWindow
    {
        private readonly GameManager _gameManager;
        private readonly ObservableCollection<Mod> _mods = new ObservableCollection<Mod>();
        private readonly List<Mod> _modsToDisable = new List<Mod>();
        private readonly List<Mod> _modsToEnable = new List<Mod>();

        public ModManagerWindow(GameManager m)
        {
            InitializeComponent();
            _gameManager = m;
            Setup();
        }

        private void Setup()
        {
            var allMods = _gameManager.GetMods(true);
            for (int i = 0; i < allMods.Count; i++) _mods.Add(allMods[i]);
            ModsDataGrid.DataContext = _mods;
        }

        private void ModWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Utils.Log("Saving mods...");
            _gameManager.DisableMods(_modsToDisable);
            _gameManager.EnableMods(_modsToEnable);
        }

        private void ModsDataGrid_TargetUpdated(object sender, DataTransferEventArgs e)
        {
            var dg = sender as DataGrid;
            if (dg != null && dg.SelectedIndex == -1) return;
            if (!(dg.SelectedItem is Mod selected)) return;
            Utils.Log("Mod updated: " + selected.Filename);
            Utils.Log("Enabled: " + selected.IsEnabled);
            if (!selected.IsEnabled && !_modsToDisable.Contains(selected))
            {
                if (_modsToEnable.Contains(selected)) _modsToEnable.Remove(selected);
                _modsToDisable.Add(selected);
            }
            else if (selected.IsEnabled && !_modsToEnable.Contains(selected))
            {
                if (_modsToDisable.Contains(selected)) _modsToDisable.Remove(selected);
                _modsToEnable.Add(selected);
            }
        }
    }
}
