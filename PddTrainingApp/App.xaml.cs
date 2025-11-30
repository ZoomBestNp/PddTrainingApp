using PddTrainingApp.Models;
using PddTrainingApp.Views;
using System;
using System.Windows;

namespace PddTrainingApp
{
    public partial class App : Application
    {
        public static User CurrentUser { get; set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Глобальная обработка необработанных исключений
            DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            var mainWindow = new MainWindow();
            mainWindow.MainFrame.Navigate(new LoginPage());
            mainWindow.Show();
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            HandleException(e.Exception);
            e.Handled = true;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            HandleException(e.ExceptionObject as Exception);
        }

        private void HandleException(Exception ex)
        {
            string errorMessage = "Произошла непредвиденная ошибка:\n\n";

            if (ex != null)
            {
                errorMessage += $"{ex.Message}\n\n";

                if (ex.InnerException != null)
                {
                    errorMessage += $"Внутренняя ошибка: {ex.InnerException.Message}";
                }
            }
            else
            {
                errorMessage += "Неизвестная ошибка";
            }

            MessageBox.Show(errorMessage, "Ошибка приложения",
                MessageBoxButton.OK, MessageBoxImage.Error);

            // Логирование ошибки
            System.Diagnostics.Debug.WriteLine($"ERROR: {ex?.ToString()}");
        }
    }
}