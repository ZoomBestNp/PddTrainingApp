using System.Windows;
using System.Windows.Controls;
using PddTrainingApp.Views;

namespace PddTrainingApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainFrame_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            // Показываем/скрываем Header в зависимости от страницы
            if (e.Content is LoginPage || e.Content is RegisterPage)
            {
                MainHeader.Visibility = Visibility.Collapsed;
            }
            else
            {
                MainHeader.Visibility = Visibility.Visible;

                // Обновляем информацию в Header
                if (MainHeader is UserHeader userHeader)
                {
                    userHeader.UpdateUserInfo();
                }
            }
        }
    }
}