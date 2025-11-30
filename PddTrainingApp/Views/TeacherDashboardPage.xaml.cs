using PddTrainingApp.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace PddTrainingApp.Views
{
    public partial class TeacherDashboardPage : Page
    {
        public TeacherDashboardPage()
        {
            InitializeComponent();
            LoadRealStatistics();
            LoadActiveAssignments();
        }

        private void LoadRealStatistics()
        {
            using (var context = new PddTrainingDbContext())
            {
                var teacherId = App.CurrentUser.UserId;


                var studentsCount = context.TeacherStudents
                    .Count(ts => ts.TeacherId == teacherId);
                StudentsCountText.Text = studentsCount.ToString();

             
            }
        }

        private void LoadActiveAssignments()
        {
            using (var context = new PddTrainingDbContext())
            {
                var teacherId = App.CurrentUser.UserId;

                var activeAssignments = context.QuestionBlocks
                    .Where(q => q.TeacherId == teacherId)
                    .Select(q => new
                    {
                        q.BlockId,
                        q.Name,
                        TotalStudents = context.TeacherStudents
                            .Count(ts => ts.TeacherId == teacherId),
                        CompletedCount = context.StudentAssignments
                            .Count(sa => sa.BlockId == q.BlockId && sa.IsCompleted == true)
                    })
                    .ToList();

                if (activeAssignments.Any())
                {
                    ActiveAssignmentsItemsControl.ItemsSource = activeAssignments;
                    NoActiveAssignmentsText.Visibility = Visibility.Collapsed;
                }
                else
                {
                    ActiveAssignmentsItemsControl.ItemsSource = null;
                    NoActiveAssignmentsText.Visibility = Visibility.Visible;
                }
            }
        }


        private void LinkStudents_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new TeacherLinkStudentsPage());
        }

        private void ManageStudents_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new TeacherStudentsPage());
        }

        private void ViewStatistics_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new TeacherStatisticsPage());
        }
    }
}