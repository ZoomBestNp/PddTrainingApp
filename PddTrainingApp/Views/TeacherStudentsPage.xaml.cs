using PddTrainingApp.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace PddTrainingApp.Views
{
    public partial class TeacherStudentsPage : Page
    {
        public TeacherStudentsPage()
        {
            InitializeComponent();
            LoadMyStudents();
        }

        private void LoadMyStudents()
        {
            using (var context = new PddTrainingDbContext())
            {
                var myStudents = context.TeacherStudents
                    .Where(ts => ts.TeacherId == App.CurrentUser.UserId)
                    .Include(ts => ts.Student)
                    .Select(ts => new
                    {
                        ts.Student.UserId,
                        ts.Student.FullName,
                        ts.Student.Email,
                        ts.Student.StudentCode,
                        ts.LinkedDate,
                        ActiveAssignments = context.StudentAssignments
                            .Count(sa => sa.StudentId == ts.StudentId && sa.IsCompleted != true)
                    })
                    .ToList();

                if (myStudents.Any())
                {
                    StudentsItemsControl.ItemsSource = myStudents;
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

        private void AssignTask_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            int studentId = (int)button.Tag;

            NavigationService.Navigate(new TeacherAssignmentsPage(studentId));
        }
    }
}