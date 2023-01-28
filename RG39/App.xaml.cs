using RG39.Lang;
using RG39.Properties;
using RG39.Util;
using System;
using System.Globalization;
using System.Threading;
using System.Windows;

namespace RG39
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static Mutex _mutex = null;

        App()
        {
            if (string.IsNullOrEmpty(Settings.Default.LangString))
                MyFunctions.ChangeLang(0);

            Thread.CurrentThread.CurrentCulture = new CultureInfo(Settings.Default.LangString);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(Settings.Default.LangString);
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            const bool initiallyOwned = true;
            const string name = "RG39";
            _mutex = new Mutex(initiallyOwned, name, out bool createdNew);
            if (createdNew) Exit += CloseMutexHandler;
            else
            {
                MessageBox.Show(strings.MULTI_INSTANCE_MSG);
                Current.Shutdown();
            }
            base.OnStartup(e);
        }

        protected virtual void CloseMutexHandler(object sender, EventArgs e)
        {
            _mutex?.Close();
        }
    }
}
