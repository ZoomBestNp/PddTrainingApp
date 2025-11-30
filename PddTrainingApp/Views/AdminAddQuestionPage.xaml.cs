using PddTrainingApp.Models;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace PddTrainingApp.Views
{
    public partial class AdminAddQuestionPage : Page
    {
        public AdminAddQuestionPage()
        {
            InitializeComponent();
            LoadModules();
        }

        private void LoadModules()
        {
            using (var context = new PddTrainingDbContext())
            {
                ModuleComboBox.ItemsSource = context.Modules.ToList();
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void AddQuestionButton_Click(object sender, RoutedEventArgs e)
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
                var question = new Question
                {
                    ModuleId = (ModuleComboBox.SelectedItem as Module).ModuleId,
                    Content = QuestionText.Text,
                    Options = JsonSerializer.Serialize(options),
                    Answer = (CorrectAnswerComboBox.SelectedItem as ComboBoxItem).Tag as int? ?? 0,
                    DifficultyLevel = (DifficultyComboBox.SelectedItem as ComboBoxItem).Tag as int? ?? 1
                };

                context.Questions.Add(question);
                context.SaveChanges();

                MessageBox.Show("Вопрос успешно добавлен!", "Успех");
                NavigationService.GoBack();
            }
        }
    }
}