using PddTrainingApp.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace PddTrainingApp.Views
{
    public partial class AdminEditQuestionPage : Page
    {
        private Question _currentQuestion;
        private List<Option> _currentOptions = new List<Option>();

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
                    .Include(q => q.Options.OrderBy(o => o.OptionOrder))
                    .FirstOrDefault(q => q.QuestionId == questionId);

                if (_currentQuestion != null)
                {
                    // Загружаем данные вопроса в форму
                    QuestionText.Text = _currentQuestion.Content;
                    _currentOptions = _currentQuestion.Options.ToList();

                    // Загружаем варианты ответов
                    if (_currentOptions.Count >= 4)
                    {
                        Option1TextBox.Text = _currentOptions[0].OptionText;
                        Option2TextBox.Text = _currentOptions[1].OptionText;
                        Option3TextBox.Text = _currentOptions[2].OptionText;
                        Option4TextBox.Text = _currentOptions[3].OptionText;
                    }

                    // Находим правильный ответ
                    int correctOrder = _currentOptions.FirstOrDefault(o => o.OptionId == _currentQuestion.Answer)?.OptionOrder ?? 1;

                    // Устанавливаем правильный ответ в ComboBox
                    foreach (ComboBoxItem item in CorrectAnswerComboBox.Items)
                    {
                        if (item.Tag != null && int.TryParse(item.Tag.ToString(), out int tagValue))
                        {
                            if (tagValue == correctOrder - 1) // -1 потому что Tag хранит индекс (0-3)
                            {
                                CorrectAnswerComboBox.SelectedItem = item;
                                break;
                            }
                        }
                    }

                    // Устанавливаем уровень сложности
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
                // Загружаем вопрос с вариантами
                var question = context.Questions
                    .Include(q => q.Options)
                    .FirstOrDefault(q => q.QuestionId == _currentQuestion.QuestionId);

                if (question != null)
                {
                    question.ModuleId = (ModuleComboBox.SelectedItem as Module).ModuleId;
                    question.Content = QuestionText.Text;

                    // Обновляем варианты ответов
                    if (question.Options.Count >= 4)
                    {
                        var optionsList = question.Options.OrderBy(o => o.OptionOrder).ToList();

                        optionsList[0].OptionText = options[0];
                        optionsList[1].OptionText = options[1];
                        optionsList[2].OptionText = options[2];
                        optionsList[3].OptionText = options[3];
                    }
                    else
                    {
                        // Если вариантов нет, создаем новые
                        context.Options.RemoveRange(question.Options);

                        for (int i = 0; i < options.Count; i++)
                        {
                            var option = new Option
                            {
                                QuestionId = question.QuestionId,
                                OptionText = options[i],
                                OptionOrder = i + 1
                            };
                            context.Options.Add(option);
                        }
                        context.SaveChanges();

                        // Перезагружаем варианты
                        question.Options = context.Options.Where(o => o.QuestionId == question.QuestionId).ToList();
                    }

                    // Получаем правильный ответ
                    if (CorrectAnswerComboBox.SelectedItem is ComboBoxItem selectedAnswerItem &&
                        selectedAnswerItem.Tag != null &&
                        int.TryParse(selectedAnswerItem.Tag.ToString(), out int answerIndex))
                    {
                        var correctOrder = answerIndex + 1; // +1 потому что Tag хранит индекс (0-3)
                        var correctOption = question.Options.FirstOrDefault(o => o.OptionOrder == correctOrder);
                        if (correctOption != null)
                        {
                            question.Answer = correctOption.OptionId;
                        }
                    }

                    // Обновляем уровень сложности
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
                        // Варианты удалятся каскадно из-за настроек в контексте
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