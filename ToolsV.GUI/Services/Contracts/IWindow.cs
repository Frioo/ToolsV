using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace ToolsV.GUI.Services.Contracts
{
    interface IWindow
    {
        event RoutedEventHandler Loaded;

        void Show();
    }
}
