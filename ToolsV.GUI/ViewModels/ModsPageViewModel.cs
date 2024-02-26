using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsV.Models;
using VManager;

namespace ToolsV.GUI.ViewModels
{
    public class ModsPageViewModel : ObservableObject
    {
        private readonly VManager _vmanager;

        private ICollection<Mod> _mods = new ObservableCollection<Mod>();

        public ModsPageViewModel(VManager vmanager)
        {
            _vmanager = vmanager;
            _mods.Concat(vmanager.ModManager.GetMods(true));
        }
    }
}
