using PddTrainingApp.Models;
using PddTrainingApp.Services;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace PddTrainingApp.Views
{
    public partial class LoginPage : Page
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginTextBox.Text.Trim();
            string password = PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Введите логин и пароль");
                return;
            }

            using (var context = new PddTrainingDbContext())
            {
                var user = context.Users.FirstOrDefault(u => u.Login == login);

                if (user != null && PasswordHasher.VerifyPassword(password, user.PasswordHash))
                {
                    App.CurrentUser = user;
                    user.LastLoginDate = System.DateTime.Now;
                    context.SaveChanges();

                    var mainWindow = (MainWindow)Application.Current.MainWindow;

                    switch (user.Role.ToLower())
                    {
                        case "student":
                            mainWindow.MainFrame.Navigate(new StudentDashboardPage());
                            break;
                        case "teacher":
                            mainWindow.MainFrame.Navigate(new TeacherDashboardPage());
                            break;
                        case "admin":
                            mainWindow.MainFrame.Navigate(new AdminDashboardPage());
                            break;
                        default:
                            mainWindow.MainFrame.Navigate(new ModulesPage());
                            break;
                    }
                }
                else
                {
                    MessageBox.Show("Неверный логин или пароль");
                }
            }
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new RegisterPage());
        }
    }
}