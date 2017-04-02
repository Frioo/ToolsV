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
        private GameManager manager;
        private ObservableCollection<Flag> flags = new ObservableCollection<Flag>();

        public FlagsWindow(GameManager m)
        {
            InitializeComponent();
            manager = m;
            Setup();
        }

        private void Setup()
        {
            foreach (Flag f in GetAllFlags())
            {
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
            }
        }

        private List<Flag> GetAllFlags()
        {
            List<Flag> flags = new List<Flag>();
            flags.Add(new Flag("-verify", "verifies game files integrity and checks for updates"));
            flags.Add(new Flag("-safemode", "starts the game with minimal settings but doesn't save them"));
            flags.Add(new Flag("-ignoreprofile", "ignores current profile settings"));
            flags.Add(new Flag("-useMinimumSettings", "starts the game with minimal settings"));
            flags.Add(new Flag("-useAutoSettings", "game uses automatic settings"));
            flags.Add(new Flag("-DX10", "forces DirectX 10.0"));
            flags.Add(new Flag("-DX10_1", "forces DirectX 10.1"));
            flags.Add(new Flag("-DX11", "forces DirectX 11.0"));
            flags.Add(new Flag("-noChunkedDownload", "forces downloading all updates at once instead of parts"));
            flags.Add(new Flag("-benchmark", "runs a system performance test"));
            flags.Add(new Flag("-goStraightToMP", "automatically loads online mode"));
            flags.Add(new Flag("-StraightIntoFreemode", "load GTA online freemode"));
            flags.Add(new Flag("-windowed", "forces the game to run in a window"));
            flags.Add(new Flag("-fullscreen", "forces fullscreen mode"));
            flags.Add(new Flag("-borderless", "hides window borders"));
            flags.Add(new Flag("-disallowResizeWindow", "locks window size"));
            // TODO: add language switcher

            return flags;
        }
    }
}
