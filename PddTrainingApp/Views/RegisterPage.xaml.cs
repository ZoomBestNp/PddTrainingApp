using PddTrainingApp.Models;
using PddTrainingApp.Services;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace PddTrainingApp.Views
{
    public partial class RegisterPage : Page
    {
        public RegisterPage()
        {
            InitializeComponent();
        }
        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(FullNameTextBox.Text) || FullNameTextBox.Text.Trim().Length < 2)
            {
                MessageBox.Show("Введите корректное ФИО (минимум 2 символа)");
                FullNameTextBox.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(LoginTextBox.Text) || LoginTextBox.Text.Trim().Length < 3)
            {
                MessageBox.Show("Логин должен содержать минимум 3 символа");
                LoginTextBox.Focus();
                return;
            }

            if (!IsValidEmail(EmailTextBox.Text))
            {
                MessageBox.Show("Введите корректный email адрес");
                EmailTextBox.Focus();
                return;
            }

            if (PasswordBox.Password.Length < 4)
            {
                MessageBox.Show("Пароль должен содержать минимум 4 символа");
                PasswordBox.Focus();
                return;
            }

            using (var context = new PddTrainingDbContext())
            {
                if (context.Users.Any(u => u.Login == LoginTextBox.Text))
                {
                    MessageBox.Show("Этот логин уже занят");
                    return;
                }


                string userPassword = PasswordBox.Password;
                string hashedPassword = PasswordHasher.HashPassword(userPassword);

                var user = new User
                {
                    FullName = FullNameTextBox.Text.Trim(),
                    Login = LoginTextBox.Text.Trim(),
                    Email = EmailTextBox.Text.Trim(),
                    PasswordHash = hashedPassword, 
                    Role = "Student",
                    StudentCode = GenerateStudentCode(context),
                    RegistrationDate = System.DateTime.Now
                };

                context.Users.Add(user);
                context.SaveChanges();

                // Если указан код преподавателя, привязываем студента
                if (!string.IsNullOrWhiteSpace(TeacherCodeTextBox.Text))
                {
                    LinkStudentToTeacher(context, user.UserId, TeacherCodeTextBox.Text.Trim());
                }

                MessageBox.Show($"✅ Регистрация успешна!\n\n👤 Ваш логин: {user.Login}\n🔑 Ваш пароль: {userPassword}\n🎫 Ваш код студента: {user.StudentCode}\n\nСохраните эти данные!", "Успешная регистрация");

                App.CurrentUser = user;
                var mainWindow = (MainWindow)Application.Current.MainWindow;
                mainWindow.MainFrame.Navigate(new StudentDashboardPage());
            }
        }

        private string GenerateStudentCode(PddTrainingDbContext context)
        {
            string code;
            do
            {
                code = "STU" + GenerateRandomString(6).ToUpper();
            } while (context.Users.Any(u => u.StudentCode == code));

            return code;
        }

        private string GenerateRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new System.Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private void LinkStudentToTeacher(PddTrainingDbContext context, int studentId, string teacherCode)
        {
            var teacher = context.Users.FirstOrDefault(u =>
                (u.Login == teacherCode || u.StudentCode == teacherCode) && u.Role == "Teacher");

            if (teacher != null)
            {
                if (!context.TeacherStudents.Any(ts => ts.TeacherId == teacher.UserId && ts.StudentId == studentId))
                {
                    var teacherStudent = new TeacherStudent
                    {
                        TeacherId = teacher.UserId,
                        StudentId = studentId
                    };
                    context.TeacherStudents.Add(teacherStudent);
                    context.SaveChanges();
                    MessageBox.Show($"✅ Вы привязаны к преподавателю: {teacher.FullName}", "Привязка выполнена");
                }
                else
                {
                    MessageBox.Show("⚠️ Вы уже привязаны к этому преподавателю", "Информация");
                }
            }
            else
            {
                MessageBox.Show("❌ Преподаватель с таким кодом не найден", "Ошибка");
            }
        }

        private void BackToLogin_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}