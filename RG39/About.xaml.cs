using RG39.Util;
using System.Diagnostics;
using System.Windows;

namespace RG39
{
    /// <summary>
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class About : Window
    {
        public About()
        {
            InitializeComponent();
            AppIcon.Source = Properties.Resources.icon.ToImageSource();
        }

        private void GoToRepo_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo()
            {
                UseShellExecute = true,
                FileName = "https://github.com/IgnacioVeiga/RG39"
            });
        }
    }
}
