using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private ObservableCollection<Flag> flags = new ObservableCollection<Flag>();
        private List<Flag> enabledFlags = new List<Flag>();

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
            List<Flag> allFlags = cmdManager.GetAllFlags();
            List<Flag> enabledFlags = cmdManager.GetCommandlineArguments();
            for (int i = 0; i < allFlags.Count; i++)
            {
                for (int j = 0; i < enabledFlags.Count; i++)
                {
                    if (allFlags[i].FlagCode.Equals(enabledFlags[i].FlagCode))
                    {
                        allFlags[i].Enabled = true;
                    }
                }
                flags.Add(allFlags[i]);
            }
            FlagsDataGrid.DataContext = flags;
        }

        private void FlagsDataGrid_TargetUpdated(object sender, DataTransferEventArgs e)
        {
            DataGrid dg = sender as DataGrid;
            if (dg.SelectedIndex != -1)
            {
                Flag selected = dg.SelectedItem as Flag;
                if (!selected.Enabled && enabledFlags.Contains(selected))
                {
                    enabledFlags.Remove(selected);
                }
                else if (selected.Enabled && !enabledFlags.Contains(selected))
                {
                    enabledFlags.Add(selected);
                }
            }
        }

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            cmdManager.SetCommandlineArguments(enabledFlags);
        }

        public List<Flag> GetEnabledFlags()
        {
            List<Flag> res = new List<Flag>();
            List<Flag> allFlags = cmdManager.GetAllFlags();
            for (int i = 0; i < allFlags.Count; i++)
            {
                if (allFlags[i].Enabled)
                {
                    res.Add(allFlags[i]);
                }
            }
            return res;
        }
    }
}
