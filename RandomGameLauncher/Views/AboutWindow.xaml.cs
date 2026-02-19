using System.Diagnostics;
using System.Windows;

namespace RandomGameLauncher.Views
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
            AppIcon.Source = Utils.ByteArrayToImage(Properties.Resources.icon);
        }

        private void GoToRepo_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo()
            {
                UseShellExecute = true,
                FileName = "https://github.com/IgnacioVeiga/RandomGameLauncher"
            });
        }
    }
}
