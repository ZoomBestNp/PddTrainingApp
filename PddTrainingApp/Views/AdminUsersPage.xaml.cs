using PddTrainingApp.Models;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PddTrainingApp.Views
{
    public partial class AdminUsersPage : Page
    {
        public AdminUsersPage()
        {
            InitializeComponent();
            LoadUsers();
            LoadUserStatistics();
        }

        private void LoadUserStatistics()
        {
            using (var context = new PddTrainingDbContext())
            {
                var totalUsers = context.Users.Count();
                var studentsCount = context.Users.Count(u => u.Role == "Student");
                var teachersCount = context.Users.Count(u => u.Role == "Teacher");

                TotalUsersText.Text = totalUsers.ToString();
                StudentsCountText.Text = studentsCount.ToString();
                TeachersCountText.Text = teachersCount.ToString();
            }
        }

        private void LoadUsers()
        {
            using (var context = new PddTrainingDbContext())
            {
                var users = context.Users.ToList();
                UsersItemsControl.ItemsSource = users;
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void SaveUserRole_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            int userId = (int)button.Tag;


            var parent = VisualTreeHelper.GetParent(button);
            while (parent != null && !(parent is Border))
                parent = VisualTreeHelper.GetParent(parent);

            if (parent is Border border)
            {

                var comboBox = FindVisualChild<ComboBox>(border);
                if (comboBox?.SelectedItem is ComboBoxItem selectedItem)
                {
                    string newRole = selectedItem.Content.ToString();

                    using (var context = new PddTrainingDbContext())
                    {
                        var user = context.Users.FirstOrDefault(u => u.UserId == userId);
                        if (user != null)
                        {
                            user.Role = newRole;
                            context.SaveChanges();
                            MessageBox.Show($"Роль пользователя обновлена на: {newRole}", "Успех");
                            LoadUsers();
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Выберите роль из списка", "Ошибка");
                }
            }
        }

        private T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T result)
                    return result;
                var descendant = FindVisualChild<T>(child);
                if (descendant != null)
                    return descendant;
            }
            return null;
        }
    }
}