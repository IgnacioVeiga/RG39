using RandomGameLauncher.Properties;
using RandomGameLauncher.Resources.Language;
using RandomGameLauncher.Services;
using RandomGameLauncher.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace RandomGameLauncher;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;

        // Generate menu items for every lang.
        foreach (var language in AppLanguageService.Languages)
        {
            bool isLangSelected = Settings.Default.Language == language.Key;
            MenuItem menuItem = new()
            {
                Tag = language.Key,
                Header = language.Value,
                IsCheckable = true,
                IsChecked = isLangSelected,
                IsEnabled = !isLangSelected
            };

            menuItem.Click += LanguageSelected_Click;
            LanguagesMenu.Items.Add(menuItem);
        }
    }

    private void LanguageSelected_Click(object sender, RoutedEventArgs e)
    {
        string? language = (sender as MenuItem)?.Tag?.ToString();
        AppLanguageService.ChangeLanguage(language);
        MessageBox.Show(Strings.TOGGLE_LANG_MSG, Strings.RESTARTING, MessageBoxButton.OK, MessageBoxImage.Exclamation);

        App.RestartApp();
    }
}
