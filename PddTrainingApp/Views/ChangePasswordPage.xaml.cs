using PddTrainingApp.Models;
using PddTrainingApp.Services;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace PddTrainingApp.Views
{
    public partial class ChangePasswordPage : Page
    {
        public ChangePasswordPage()
        {
            InitializeComponent();
        }

        private void ChangePasswordButton_Click(object sender, RoutedEventArgs e)
        {
            string currentPassword = CurrentPasswordBox.Password;
            string newPassword = NewPasswordBox.Password;
            string confirmPassword = ConfirmPasswordBox.Password;

            // Валидация
            if (string.IsNullOrWhiteSpace(currentPassword))
            {
                MessageBox.Show("Введите текущий пароль");
                return;
            }

            if (string.IsNullOrWhiteSpace(newPassword))
            {
                MessageBox.Show("Введите новый пароль");
                return;
            }

            if (newPassword.Length < 4)
            {
                MessageBox.Show("Новый пароль должен содержать минимум 4 символа");
                return;
            }

            if (newPassword != confirmPassword)
            {
                MessageBox.Show("Новые пароли не совпадают");
                return;
            }

            using (var context = new PddTrainingDbContext())
            {
                var user = context.Users.FirstOrDefault(u => u.UserId == App.CurrentUser.UserId);

                if (user != null)
                {

                    if (!PasswordHasher.VerifyPassword(currentPassword, user.PasswordHash))
                    {
                        MessageBox.Show("Неверный текущий пароль");
                        return;
                    }

                    user.PasswordHash = PasswordHasher.HashPassword(newPassword);
                    context.SaveChanges();

                    MessageBox.Show("Пароль успешно изменен!", "Успех");
                    NavigationService.GoBack();
                }
                else
                {
                    MessageBox.Show("Ошибка: пользователь не найден");
                }
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}