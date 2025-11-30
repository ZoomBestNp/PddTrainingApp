using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PddTrainingApp.Views
{
    public partial class UserHeader : UserControl
    {
        public UserHeader()
        {
            InitializeComponent();
            Loaded += UserHeader_Loaded;
        }

        private void UserHeader_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateUserInfo();
        }

        public void UpdateUserInfo()
        {
            if (App.CurrentUser != null)
            {
                UserNameText.Text = App.CurrentUser.FullName;
                UserInitialsText.Text = GetUserInitials();

                if (App.CurrentUser.Role == "Student")
                {
                    StudentCodeBorder.Visibility = Visibility.Visible;
                    StudentCodeText.Text = App.CurrentUser.StudentCode ?? "Нет кода";
                }
                else
                {
                    StudentCodeBorder.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                UserNameText.Text = "Гость";
                UserInitialsText.Text = "?";
                StudentCodeBorder.Visibility = Visibility.Collapsed;
            }
        }

        private string GetUserInitials()
        {
            if (App.CurrentUser?.FullName == null) return "?";

            var names = App.CurrentUser.FullName.Split(' ');
            if (names.Length >= 2)
                return $"{names[0][0]}{names[1][0]}".ToUpper();
            else if (App.CurrentUser.FullName.Length >= 2)
                return App.CurrentUser.FullName.Substring(0, 2).ToUpper();
            else
                return App.CurrentUser.FullName.ToUpper();
        }

        private void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            var contextMenu = new ContextMenu();

            if (App.CurrentUser != null)
            {
                var userInfoItem = new MenuItem
                {
                    Header = CreateUserInfoHeader(),
                    IsEnabled = false
                };
                contextMenu.Items.Add(userInfoItem);
                contextMenu.Items.Add(new Separator());
            }

            var changePasswordItem = new MenuItem
            {
                Header = "Сменить пароль",
                Icon = new TextBlock { Text = "🔒", FontSize = 12 }
            };
            changePasswordItem.Click += (s, args) =>
            {
                var mainWindow = Application.Current.MainWindow as MainWindow;
                if (mainWindow != null)
                {
                    mainWindow.MainFrame.Navigate(new ChangePasswordPage());
                }
            };

            var logoutItem = new MenuItem
            {
                Header = "Выйти",
                Icon = new TextBlock { Text = "🚪", FontSize = 12 }
            };
            logoutItem.Click += (s, args) => Logout();

            contextMenu.Items.Add(changePasswordItem);
            contextMenu.Items.Add(new Separator());
            contextMenu.Items.Add(logoutItem);

            contextMenu.PlacementTarget = sender as Button;
            contextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            contextMenu.IsOpen = true;
        }

        private StackPanel CreateUserInfoHeader()
        {
            var stackPanel = new StackPanel { Orientation = Orientation.Vertical };

            if (App.CurrentUser != null)
            {
                stackPanel.Children.Add(new TextBlock
                {
                    Text = App.CurrentUser.FullName,
                    FontWeight = FontWeights.Bold
                });

                stackPanel.Children.Add(new TextBlock
                {
                    Text = $"Роль: {App.CurrentUser.Role}",
                    FontSize = 11,
                    Foreground = Brushes.Gray
                });

                if (App.CurrentUser.Role == "Student" && !string.IsNullOrEmpty(App.CurrentUser.StudentCode))
                {
                    stackPanel.Children.Add(new TextBlock
                    {
                        Text = $"Код: {App.CurrentUser.StudentCode}",
                        FontSize = 11,
                        Foreground = Brushes.Blue,
                        FontWeight = FontWeights.Bold
                    });
                }
            }

            return stackPanel;
        }

        private void Logout()
        {
            var result = MessageBox.Show("Вы уверены, что хотите выйти?", "Подтверждение выхода",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                App.CurrentUser = null;

                var mainWindow = Application.Current.MainWindow as MainWindow;
                if (mainWindow != null)
                {
                    mainWindow.MainFrame.Navigate(new LoginPage());
                }
            }
        }
    }
}