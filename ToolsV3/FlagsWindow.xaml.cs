using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
using System.Windows.Shapes;
using ToolsV3.API;

namespace ToolsV3
{
    public partial class FlagsWindow : MetroWindow
    {
        private GameManager gameManager;
        private CommandlineManager cmdManager;
        private ObservableCollection<Flag> flags = new ObservableCollection<Flag>(); // the collection we will edit and use as data source.
        private List<Flag> savedFlags = new List<Flag>(); // list of flags already saved in commandline.txt file
        private List<Flag> pendingFlags = new List<Flag>(); // list of flags pending to be saved
        private List<Flag> availableFlags = new List<Flag>(); // list of all flags (used for comparison)

        public FlagsWindow(GameManager m)
        {
            InitializeComponent();
            gameManager = m;
            cmdManager = new CommandlineManager(m);
            Setup();
        }

        private void Setup()
        {
            // add elements to the datagrid
            this.availableFlags = cmdManager.GetAllFlags();
            this.savedFlags = cmdManager.GetCommandlineArguments();
            for (int i = 0; i < availableFlags.Count; i++)
            {
                Flag f = availableFlags[i];
                for (int j = 0; j < savedFlags.Count; j++)
                {
                    if (savedFlags[j].FlagCode.Equals(availableFlags[i].FlagCode))
                    {
                        f = savedFlags[j];
                    }
                }
                flags.Add(f);
            }
            FlagsDataGrid.DataContext = flags;
        }

        private void FlagsDataGrid_TargetUpdated(object sender, DataTransferEventArgs e)
        {
            DataGrid dg = sender as DataGrid;
            if (dg.SelectedIndex != -1)
            {
                Flag selected = dg.SelectedItem as Flag;
                Utils.Log("Flag updated: " + selected.FlagCode);
                Utils.Log("Enabled: " + selected.IsEnabled);
                if (!selected.IsEnabled && savedFlags.Contains(selected))
                {
                    pendingFlags.Remove(selected);
                }
                else if (selected.IsEnabled && !savedFlags.Contains(selected) && !pendingFlags.Contains(selected))
                {
                    pendingFlags.Add(selected);
                }
            }
        }

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Utils.Log("Saving flags to " + gameManager.CommandlinePath);
            List<Flag> final = new List<Flag>();
            List<Flag> currentFlags = flags.ToList<Flag>();
            for (int i = 0; i < currentFlags.Count; i++)
            {
                if (currentFlags[i].IsEnabled)
                {
                    final.Add(currentFlags[i]);
                }
            }
            cmdManager.SetCommandlineArguments(final);
        }
    }
}
