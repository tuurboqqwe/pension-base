using System;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Markup;
using PensionBase.Data;
using PensionBase.Views;

namespace PensionBase
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Даты и суммы показываем в русском формате
            var culture = new CultureInfo("ru-RU");
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
            FrameworkElement.LanguageProperty.OverrideMetadata(
                typeof(FrameworkElement),
                new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(culture.IetfLanguageTag)));

            try
            {
                Db.Initialize();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Не удалось подключиться к базе данных. Проверьте, что установлен SQL Server LocalDB.\n\n" + ex.Message,
                    "Ошибка базы данных", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown(1);
                return;
            }

            new LoginWindow().Show();
        }
    }
}
