using System;
using System.Windows;
using ToolsV3.API;

namespace ToolsV3
{
    /// <summary>
    /// Logika interakcji dla klasy ChangelogWindow.xaml
    /// </summary>
    public partial class ChangelogWindow : Window
    {
        public ChangelogWindow()
        {
            InitializeComponent();
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ChangelogTextBox.Text = await Updater.GetChangelog();
        }
    }
}
