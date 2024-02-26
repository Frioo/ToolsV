using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsV.Models;

namespace ToolsV.GUI.ViewModels
{
    public class HomePageViewModel
    {
        public GameInfo GameInfo { get; set; }

        public HomePageViewModel(VManager vmanager)
        {
            GameInfo = vmanager.GameInfo;
        }
    }
}
