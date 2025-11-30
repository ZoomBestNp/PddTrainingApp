using Microsoft.EntityFrameworkCore;
using PddTrainingApp.Models;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace PddTrainingApp.Views
{
    public partial class AdminQuestionsPage : Page
    {
        public AdminQuestionsPage()
        {
            InitializeComponent();
            LoadQuestions();
        }

        private void LoadQuestions()
        {
            using (var context = new PddTrainingDbContext())
            {
                var questions = context.Questions
                    .Include(q => q.Module)
                    .ToList()
                    .Select(q => new
                    {
                        q.QuestionId,
                        q.Content,
                        q.Module,
                        q.Answer,
                        OptionsList = JsonSerializer.Deserialize<List<string>>(q.Options),
                        CorrectAnswerText = $"Правильный ответ: {JsonSerializer.Deserialize<List<string>>(q.Options)[q.Answer]}"
                    })
                    .ToList();

                if (questions.Any())
                {
                    QuestionsItemsControl.ItemsSource = questions;
                    NoQuestionsText.Visibility = Visibility.Collapsed;
                }
                else
                {
                    QuestionsItemsControl.ItemsSource = null;
                    NoQuestionsText.Visibility = Visibility.Visible;
                }
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void AddQuestion_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AdminAddQuestionPage());
        }

        private void EditQuestion_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            int questionId = (int)button.Tag;
            NavigationService.Navigate(new AdminEditQuestionPage(questionId));
        }

        private void DeleteQuestion_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            int questionId = (int)button.Tag;

            var result = MessageBox.Show("Вы уверены, что хотите удалить этот вопрос?", "Подтверждение удаления",
                MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                using (var context = new PddTrainingDbContext())
                {
                    var question = context.Questions.FirstOrDefault(q => q.QuestionId == questionId);
                    if (question != null)
                    {
                        context.Questions.Remove(question);
                        context.SaveChanges();
                        LoadQuestions(); // Перезагружаем список
                        MessageBox.Show("Вопрос удален", "Успех");
                    }
                }
            }
        }
    }
}