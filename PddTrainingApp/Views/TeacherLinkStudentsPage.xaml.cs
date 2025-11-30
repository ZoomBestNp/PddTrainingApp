using PddTrainingApp.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace PddTrainingApp.Views
{
    public partial class TeacherLinkStudentsPage : Page
    {
        public TeacherLinkStudentsPage()
        {
            InitializeComponent();
            LoadStudents();
        }


        private void LoadStudents()
        {
            using (var context = new PddTrainingDbContext())
            {
                var students = context.TeacherStudents
                    .Where(ts => ts.TeacherId == App.CurrentUser.UserId)
                    .Include(ts => ts.Student)
                    .ToList();

                if (students.Any())
                {
                    StudentsItemsControl.ItemsSource = students;
                    NoStudentsText.Visibility = Visibility.Collapsed;
                }
                else
                {
                    StudentsItemsControl.ItemsSource = null;
                    NoStudentsText.Visibility = Visibility.Visible;
                }
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void LinkStudent_Click(object sender, RoutedEventArgs e)
        {
            string studentCode = StudentCodeTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(studentCode))
            {
                MessageBox.Show("Введите код студента");
                return;
            }

            using (var context = new PddTrainingDbContext())
            {

                var student = context.Users.FirstOrDefault(u =>
                    (u.StudentCode == studentCode || u.Login == studentCode) && u.Role == "Student");

                if (student != null)
                {

                    bool alreadyLinked = context.TeacherStudents
                        .Any(ts => ts.TeacherId == App.CurrentUser.UserId && ts.StudentId == student.UserId);

                    if (!alreadyLinked)
                    {
                        var teacherStudent = new TeacherStudent
                        {
                            TeacherId = App.CurrentUser.UserId,
                            StudentId = student.UserId
                        };

                        context.TeacherStudents.Add(teacherStudent);
                        context.SaveChanges();

                        MessageBox.Show($"Студент {student.FullName} успешно привязан!", "Успех");
                        LoadStudents();
                        StudentCodeTextBox.Clear();
                    }
                    else
                    {
                        MessageBox.Show("Этот студент уже привязан к вам", "Информация");
                    }
                }
                else
                {
                    MessageBox.Show("Студент с таким кодом не найден", "Ошибка");
                }
            }
        }
    }
}