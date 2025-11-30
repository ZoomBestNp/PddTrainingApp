using PddTrainingApp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PddTrainingApp.Views
{
    public partial class StudentDashboardPage : Page
    {
        public string WelcomeMessage => $"Добро пожаловать, {App.CurrentUser?.FullName ?? "Студент"}!";

        public StudentDashboardPage()
        {
            InitializeComponent();
            DataContext = this;

            Loaded += StudentDashboardPage_Loaded;
        }


        private void StudentDashboardPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadData();
        }


        public void LoadData()
        {
            LoadAssignments();
            LoadStatistics();
            LoadRecentActivity();
        }

        private void LoadAssignments()
        {
            using (var context = new PddTrainingDbContext())
            {
                var assignments = context.StudentAssignments
                    .Where(a => a.StudentId == App.CurrentUser.UserId && a.IsCompleted != true)
                    .Include(a => a.Block)
                    .ToList();

                if (assignments.Any())
                {
                    AssignmentsItemsControl.ItemsSource = assignments;
                    AssignmentsItemsControl.Visibility = Visibility.Visible;


                    var noAssignmentsText = FindName("NoAssignmentsText") as TextBlock;
                    if (noAssignmentsText != null)
                        noAssignmentsText.Visibility = Visibility.Collapsed;
                }
                else
                {

                    AssignmentsItemsControl.Visibility = Visibility.Collapsed;

                    // Показываем сообщение "Нет заданий"
                    var noAssignmentsText = FindName("NoAssignmentsText") as TextBlock;
                    if (noAssignmentsText != null)
                        noAssignmentsText.Visibility = Visibility.Visible;
                }
            }
        }

        private void LoadStatistics()
        {
            using (var context = new PddTrainingDbContext())
            {

                var statistics = context.Results
                    .Where(r => r.UserId == App.CurrentUser.UserId)
                    .GroupBy(r => new { r.Question.ModuleId, r.Question.Module.Name })
                    .Select(g => new
                    {
                        ModuleId = g.Key.ModuleId,
                        ModuleName = g.Key.Name,

                        CorrectAnswers = g.Count(r => r.IsCorrect == true),
  
                        TotalQuestions = context.Questions.Count(q => q.ModuleId == g.Key.ModuleId),

                        Percentage = context.Questions.Count(q => q.ModuleId == g.Key.ModuleId) > 0 ?
                            (g.Count(r => r.IsCorrect == true) * 100 / context.Questions.Count(q => q.ModuleId == g.Key.ModuleId)) : 0
                    })
                    .Where(s => s.TotalQuestions > 0) // Показываем только модули с вопросами
                    .ToList();

                if (statistics.Any())
                {
                    StatisticsItemsControl.ItemsSource = statistics;
                    NoStatisticsText.Visibility = Visibility.Collapsed;
                    StatisticsItemsControl.Visibility = Visibility.Visible;
                }
                else
                {
                    StatisticsItemsControl.Visibility = Visibility.Collapsed;
                    NoStatisticsText.Visibility = Visibility.Visible;
                    NoStatisticsText.Text = "Вы еще не прошли ни одного теста";
                }
            }
        }

        private void LoadRecentActivity()
        {
            using (var context = new PddTrainingDbContext())
            {
                var today = DateTime.Today;
                var recentResults = context.Results
                    .Where(r => r.UserId == App.CurrentUser.UserId && r.Date >= today)
                    .Include(r => r.Question.Module)
                    .GroupBy(r => r.Question.Module.Name)
                    .Select(g => new
                    {
                        ModuleName = g.Key,
                        Count = g.Count(),
                        Correct = g.Count(r => r.IsCorrect == true)
                    })
                    .ToList();

                var activityList = recentResults.Select(r =>
                    $"• {r.ModuleName} - {r.Count} вопросов ({r.Correct} правильных)"
                ).ToList();

                if (activityList.Any())
                {
                    RecentActivityItemsControl.ItemsSource = activityList;
                    NoActivityText.Visibility = Visibility.Collapsed;
                    RecentActivityItemsControl.Visibility = Visibility.Visible;
                }
                else
                {
                    RecentActivityItemsControl.Visibility = Visibility.Collapsed;
                    NoActivityText.Visibility = Visibility.Visible;
                    NoActivityText.Text = "Сегодня вы еще не проходили тесты";
                }
            }
        }

        private void StartAssignment_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            int assignmentId = (int)button.Tag;


            NavigationService.Navigate(new QuestionsPage(assignmentId: assignmentId));
        }

        private void AllModules_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new ModulesPage());
        }

        private void DetailedStatistics_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Функция детальной статистики будет реализована позже", "Статистика");
        }

        // Вспомогательный метод для поиска родительского элемента
        private T FindVisualParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);
            if (parentObject == null) return null;

            T parent = parentObject as T;
            if (parent != null) return parent;

            return FindVisualParent<T>(parentObject);
        }
    }
}