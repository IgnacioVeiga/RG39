using RandomGameLauncher.Models;
using RandomGameLauncher.ViewModels;
using System.Windows;

namespace RandomGameLauncher.Views
{
    /// <summary>
    /// Interaction logic for AddGameWindow.xaml
    /// </summary>
    public partial class AddGameWindow : Window
    {
        public AddGameWindow()
        {
            InitializeComponent();
            DataContext = new AddGameViewModel();
        }

        public Game NewGame => ((AddGameViewModel)DataContext).NewGame;

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }

}
