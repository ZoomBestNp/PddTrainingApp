using PddTrainingApp.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;

namespace PddTrainingApp.Views
{
    public partial class AdminEditQuestionPage : Page
    {
        private Question _currentQuestion;

        public AdminEditQuestionPage(int questionId)
        {
            InitializeComponent();
            LoadQuestionData(questionId);
            LoadModules();
        }

        private void LoadQuestionData(int questionId)
        {
            using (var context = new PddTrainingDbContext())
            {
                _currentQuestion = context.Questions
                    .Include(q => q.Module)
                    .FirstOrDefault(q => q.QuestionId == questionId);

                if (_currentQuestion != null)
                {
                    // Загружаем данные вопроса в форму
                    QuestionText.Text = _currentQuestion.Content;

                    // Загружаем варианты ответов
                    var options = JsonSerializer.Deserialize<List<string>>(_currentQuestion.Options);
                    Option1TextBox.Text = options[0];
                    Option2TextBox.Text = options[1];
                    Option3TextBox.Text = options[2];
                    Option4TextBox.Text = options[3];

                    //приведение типов для правильного ответа
                    foreach (ComboBoxItem item in CorrectAnswerComboBox.Items)
                    {
                        if (item.Tag != null && int.TryParse(item.Tag.ToString(), out int tagValue))
                        {
                            if (tagValue == _currentQuestion.Answer)
                            {
                                CorrectAnswerComboBox.SelectedItem = item;
                                break;
                            }
                        }
                    }

                    //приведение типов для уровня сложности
                    foreach (ComboBoxItem item in DifficultyComboBox.Items)
                    {
                        if (item.Tag != null && int.TryParse(item.Tag.ToString(), out int tagValue))
                        {
                            if (tagValue == _currentQuestion.DifficultyLevel)
                            {
                                DifficultyComboBox.SelectedItem = item;
                                break;
                            }
                        }
                    }
                }
            }
        }

        private void LoadModules()
        {
            using (var context = new PddTrainingDbContext())
            {
                ModuleComboBox.ItemsSource = context.Modules.ToList();

                // Устанавливаем текущий модуль вопроса
                if (_currentQuestion != null)
                {
                    ModuleComboBox.SelectedValue = _currentQuestion.ModuleId;
                }
            }
        }

        private void SaveQuestionButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(QuestionText.Text))
            {
                MessageBox.Show("Введите текст вопроса");
                return;
            }

            if (ModuleComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите модуль");
                return;
            }

            var options = new List<string>
            {
                Option1TextBox.Text,
                Option2TextBox.Text,
                Option3TextBox.Text,
                Option4TextBox.Text
            };

            if (options.Any(string.IsNullOrWhiteSpace))
            {
                MessageBox.Show("Заполните все варианты ответов");
                return;
            }

            if (CorrectAnswerComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите правильный ответ");
                return;
            }

            using (var context = new PddTrainingDbContext())
            {
                var question = context.Questions.FirstOrDefault(q => q.QuestionId == _currentQuestion.QuestionId);
                if (question != null)
                {
                    question.ModuleId = (ModuleComboBox.SelectedItem as Module).ModuleId;
                    question.Content = QuestionText.Text;
                    question.Options = JsonSerializer.Serialize(options);

                    //получение значения из ComboBox
                    if (CorrectAnswerComboBox.SelectedItem is ComboBoxItem selectedAnswerItem &&
                        selectedAnswerItem.Tag != null &&
                        int.TryParse(selectedAnswerItem.Tag.ToString(), out int answer))
                    {
                        question.Answer = answer;
                    }

                    //получение уровня сложности
                    if (DifficultyComboBox.SelectedItem is ComboBoxItem selectedDifficultyItem &&
                        selectedDifficultyItem.Tag != null &&
                        int.TryParse(selectedDifficultyItem.Tag.ToString(), out int difficulty))
                    {
                        question.DifficultyLevel = difficulty;
                    }

                    context.SaveChanges();
                    MessageBox.Show("Вопрос успешно обновлен!", "Успех");
                    NavigationService.GoBack();
                }
            }
        }

        private void DeleteQuestionButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Вы уверены, что хотите удалить этот вопрос?",
                "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                using (var context = new PddTrainingDbContext())
                {
                    var question = context.Questions.FirstOrDefault(q => q.QuestionId == _currentQuestion.QuestionId);
                    if (question != null)
                    {
                        context.Questions.Remove(question);
                        context.SaveChanges();
                        MessageBox.Show("Вопрос удален", "Успех");
                        NavigationService.GoBack();
                    }
                }
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}