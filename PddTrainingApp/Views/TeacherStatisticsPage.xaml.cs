using PddTrainingApp.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PddTrainingApp.Views
{
    public partial class TeacherStatisticsPage : Page
    {
        public TeacherStatisticsPage()
        {
            InitializeComponent();
            LoadAssignments();
        }

        private void LoadAssignments()
        {
            using (var context = new PddTrainingDbContext())
            {
                var assignments = context.QuestionBlocks
                    .Where(q => q.TeacherId == App.CurrentUser.UserId)
                    .ToList();
                AssignmentComboBox.ItemsSource = assignments;
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void ShowStatistics_Click(object sender, RoutedEventArgs e)
        {
            var selectedAssignment = AssignmentComboBox.SelectedItem as QuestionBlock;
            if (selectedAssignment == null)
            {
                MessageBox.Show("Выберите задание");
                return;
            }

            LoadStatistics(selectedAssignment.BlockId);
        }

        private void LoadStatistics(int assignmentId)
        {
            using (var context = new PddTrainingDbContext())
            {
                var teacherId = App.CurrentUser.UserId;

                var statistics = context.StudentAssignments
                    .Where(sa => sa.BlockId == assignmentId)
                    .Where(sa => context.TeacherStudents
                        .Any(ts => ts.TeacherId == teacherId && ts.StudentId == sa.StudentId))
                    .Include(sa => sa.Student)
                    .Include(sa => sa.Block)
                    .ToList()
                    .Select(sa => new
                    {
                        StudentName = sa.Student.FullName,
                        AssignmentName = sa.Block.Name,
                        // Для НЕ начатых заданий - всегда 0
                        CorrectAnswers = sa.IsCompleted == true ? (sa.Score.HasValue ? (int)(sa.Score.Value * sa.Block.QuestionsCount / 100.0) : 0) : 0,
                        TotalQuestions = sa.Block.QuestionsCount,
                        Percentage = sa.IsCompleted == true ? (sa.Score.HasValue ? sa.Score.Value : 0) : 0,
                        Status = sa.IsCompleted == true ? "Завершено" : "Не начато",
                        StatusColor = sa.IsCompleted == true ? Brushes.Green : Brushes.Orange,
                        Score = sa.IsCompleted == true ? (sa.Score.HasValue ? $"{sa.Score}%" : "Не оценено") : "Не начато",
                        CompletedDate = sa.CompletedDate,
                        DetailedStatus = sa.IsCompleted == true ?
                            $"Завершено ({sa.CompletedDate:dd.MM.yyyy})" :
                            "Не начато"
                    })
                    .ToList();

                if (statistics.Any())
                {
                    StatisticsItemsControl.ItemsSource = statistics;
                    NoDataText.Visibility = Visibility.Collapsed;
                }
                else
                {
                    StatisticsItemsControl.ItemsSource = null;
                    NoDataText.Text = "Нет данных по выбранному заданию";
                    NoDataText.Visibility = Visibility.Visible;
                }
            }
        }
    }
}