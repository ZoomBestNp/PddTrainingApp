using PddTrainingApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace PddTrainingApp.Views
{
    public partial class AdminDashboardPage : Page
    {
        public AdminDashboardPage()
        {
            InitializeComponent();
            LoadRealStatistics();
            LoadRecentActivity();
        }

        private void LoadRealStatistics()
        {
            using (var context = new PddTrainingDbContext())
            {
                // КОЛИЧЕСТВО ПОЛЬЗОВАТЕЛЕЙ
                var usersCount = context.Users.Count();
                UsersCountText.Text = usersCount.ToString();

                //КОЛИЧЕСТВО ВОПРОСОВ
                var questionsCount = context.Questions.Count();
                QuestionsCountText.Text = questionsCount.ToString();

                // РЕАЛЬНОЕ КОЛИЧЕСТВО МОДУЛЕЙ
                var modulesCount = context.Modules.Count();
                ModulesCountText.Text = modulesCount.ToString();
            }
        }

        private void LoadRecentActivity()
        {
            using (var context = new PddTrainingDbContext())
            {
                var activities = new List<string>();
                var today = DateTime.Today;

                // ПОСЛЕДНИЕ ЗАРЕГИСТРИРОВАННЫЕ ПОЛЬЗОВАТЕЛИ
                var newUsers = context.Users
                    .Where(u => u.RegistrationDate >= today)
                    .OrderByDescending(u => u.RegistrationDate)
                    .Take(3)
                    .ToList();

                foreach (var user in newUsers)
                {
                    activities.Add($"• Новый пользователь: {user.FullName} ({user.Role})");
                }

                // ПОСЛЕДНИЕ СОЗДАННЫЕ ВОПРОСЫ
                var newQuestions = context.Questions
                    .OrderByDescending(q => q.QuestionId)
                    .Take(2)
                    .Select(q => new { q.Content, q.Module.Name })
                    .ToList();

                foreach (var question in newQuestions)
                {
                    var shortContent = question.Content.Length > 50
                        ? question.Content.Substring(0, 50) + "..."
                        : question.Content;
                    activities.Add($"• Добавлен вопрос: {shortContent} ({question.Name})");
                }

                // ПОСЛЕДНИЕ РЕЗУЛЬТАТЫ ТЕСТОВ
                var recentResults = context.Results
                    .Where(r => r.Date >= today)
                    .GroupBy(r => r.User.FullName)
                    .Select(g => new { UserName = g.Key, Count = g.Count() })
                    .Take(2)
                    .ToList();

                foreach (var result in recentResults)
                {
                    activities.Add($"• {result.UserName} прошел {result.Count} вопросов");
                }

                if (activities.Any())
                {
                    RecentActivityItemsControl.ItemsSource = activities;
                    NoActivityText.Visibility = Visibility.Collapsed;
                }
                else
                {
                    RecentActivityItemsControl.ItemsSource = null;
                    NoActivityText.Visibility = Visibility.Visible;
                }
            }
        }

        private void ManageUsers_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AdminUsersPage());
        }

        private void ManageQuestions_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AdminQuestionsPage());
        }

        private void SystemStatistics_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Функция детальной статистики системы будет реализована позже", "Системная статистика");
        }

        private void SystemSettings_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Функция настроек системы будет реализована позже", "Настройки системы");
        }
    }
}