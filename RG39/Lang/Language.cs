using RG39.Properties;
using System.Globalization;
using System.Threading;

namespace RG39.Lang
{
    internal static class AppLanguage
    {
        // ToDo: Intentar generar los ComboBoxItems a partir de esta enum
        internal enum Languages
        {
            English = 0,
            Español = 1
        }

        /// <summary>
        /// 0 = English
        /// 1 = Español
        /// </summary>
        /// <param name="langIndex"></param>
        internal static void ChangeLanguage(int langIndex)
        {
            string lang = "en";

            switch (langIndex)
            {
                case (int)Languages.Español:
                    lang = "es";
                    break;
            }

            // ToDo: buscar otra manera de recordar el item seleccionado del ComboBox "langSelected"
            Settings.Default.LangIndex = langIndex;
            Settings.Default.Save();

            Thread.CurrentThread.CurrentCulture = new CultureInfo(lang);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(lang);
        }
    }
}
