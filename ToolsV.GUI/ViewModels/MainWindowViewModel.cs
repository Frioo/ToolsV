using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsV.GUI.Views.Pages;
using ToolsV.Models;
using Wpf.Ui.Controls;

namespace ToolsV.GUI.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        public string Title { get; set; } = "ToolsV";

        public const double MenuItemWidth = 180;

        [ObservableProperty]
        public ICollection<object> _menuItems = new ObservableCollection<object>()
        {
            new NavigationViewItem("Home", SymbolRegular.Home24, typeof(HomePage)) { Width = MenuItemWidth, MaxWidth = MenuItemWidth },
            new NavigationViewItem("Mods", SymbolRegular.BoxMultiple24, typeof(ModsPage)) { Width = MenuItemWidth },
        };


        public MainWindowViewModel()
        {
        }

    }
}
