using RandomGameLauncher.Models;
using System.Windows;

namespace RandomGameLauncher.Views
{
    /// <summary>
    /// Interaction logic for AddGameWindow.xaml
    /// </summary>
    public partial class AddGameWindow : Window
    {
        public Game NewGame { get; private set; }

        public AddGameWindow()
        {
            InitializeComponent();
            DataContext = this;
            Libraries = Enum.GetValues(typeof(LibraryEnum));
            NewGame = new Game(LibraryEnum.Other, "", "");
        }

        public Array Libraries { get; }

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
