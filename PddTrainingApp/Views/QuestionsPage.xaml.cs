using PddTrainingApp.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Threading.Tasks;

namespace PddTrainingApp.Views
{
    public partial class QuestionsPage : Page
    {
        private List<Question> _questions;
        private int _currentQuestionIndex = 0;
        private int _correctAnswers = 0;
        private RadioButton _selectedRadioButton;
        private int? _assignmentId;
        private int? _moduleId;


        public QuestionsPage(int? moduleId = null, int? assignmentId = null)
        {
            InitializeComponent();
            _moduleId = moduleId;
            _assignmentId = assignmentId;

            if (_assignmentId.HasValue)
            {
                LoadAssignmentQuestions(_assignmentId.Value);
            }
            else if (_moduleId.HasValue)
            {
                LoadModuleQuestions(_moduleId.Value);
            }
            else
            {
                QuestionText.Text = "Не указан модуль или задание";
                CheckAnswerButton.IsEnabled = false;
            }
        }

        private void LoadAssignmentQuestions(int assignmentId)
        {
            using (var context = new PddTrainingDbContext())
            {
                var assignment = context.StudentAssignments
                    .Include(a => a.Block)
                    .FirstOrDefault(a => a.AssignmentId == assignmentId);

                if (assignment != null)
                {
                    var query = context.Questions.AsQueryable();

                    // Фильтруем по модулю если указан
                    if (assignment.Block.ModuleId.HasValue)
                    {
                        query = query.Where(q => q.ModuleId == assignment.Block.ModuleId.Value);
                    }

                    // Фильтруем по уровню сложности если указан
                    if (assignment.Block.DifficultyLevel.HasValue)
                    {
                        query = query.Where(q => q.DifficultyLevel == assignment.Block.DifficultyLevel.Value);
                    }

                    _questions = query
                    .OrderBy(q => EF.Functions.Random())
                    .Take(assignment.Block.QuestionsCount)
                    .ToList();
                }
            }

            if (_questions?.Any() == true)
            {
                ShowCurrentQuestion();
            }
            else
            {
                QuestionText.Text = "Вопросы для задания не найдены";
                CheckAnswerButton.IsEnabled = false;


                var backButton = new Button
                {
                    Content = "ВЕРНУТЬСЯ НАЗАД",
                    Background = Brushes.SteelBlue,
                    Foreground = Brushes.White,
                    Padding = new Thickness(25, 15, 25, 15),
                    FontSize = 14,
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(0, 20, 0, 0),
                    Cursor = System.Windows.Input.Cursors.Hand
                };
                backButton.Click += (s, e) => NavigationService.GoBack();
                OptionsPanel.Children.Add(backButton);

                ProgressText.Visibility = Visibility.Collapsed;
            }
        }

        private void LoadModuleQuestions(int moduleId)
        {
            using (var context = new PddTrainingDbContext())
            {
                _questions = context.Questions
                    .Where(q => q.ModuleId == moduleId)
                    .ToList();

                if (_questions.Any())
                {
                    ShowCurrentQuestion();
                }
                else
                {
                    QuestionText.Text = "Вопросы не найдены";
                    CheckAnswerButton.IsEnabled = false;

                    var backButton = new Button
                    {
                        Content = "ВЕРНУТЬСЯ К МОДУЛЯМ",
                        Background = Brushes.SteelBlue,
                        Foreground = Brushes.White,
                        Padding = new Thickness(25, 15, 25, 15),
                        FontSize = 14,
                        FontWeight = FontWeights.Bold,
                        Margin = new Thickness(0, 20, 0, 0),
                        Cursor = System.Windows.Input.Cursors.Hand
                    };
                    backButton.Click += (s, e) => NavigationService.GoBack();
                    OptionsPanel.Children.Add(backButton);

                    ProgressText.Visibility = Visibility.Collapsed;
                }
            }
        }


        private void ShowCurrentQuestion()
        {
            ResetCardTransformations();

            var currentQuestion = _questions[_currentQuestionIndex];
            QuestionText.Text = currentQuestion.Content;

            OptionsPanel.Children.Clear();
            _selectedRadioButton = null;

            var options = System.Text.Json.JsonSerializer.Deserialize<List<string>>(currentQuestion.Options);
            for (int i = 0; i < options.Count; i++)
            {
                var radioButton = new RadioButton
                {
                    Content = new TextBlock
                    {
                        Text = options[i],
                        TextWrapping = TextWrapping.Wrap,
                        FontSize = 14
                    },
                    Margin = new Thickness(0, 8, 0, 8),
                    Padding = new Thickness(15, 12, 15, 12),
                    Background = Brushes.Transparent,
                    BorderBrush = Brushes.LightGray,
                    BorderThickness = new Thickness(1),
                    Tag = i,
                    Cursor = System.Windows.Input.Cursors.Hand
                };

                radioButton.Checked += RadioButton_Checked;
                OptionsPanel.Children.Add(radioButton);
            }

            UpdateProgressText();

            var showAnimation = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromSeconds(0.3)
            };
            QuestionCard.BeginAnimation(OpacityProperty, showAnimation);
        }

        private void ResetCardTransformations()
        {
            QuestionCard.BeginAnimation(OpacityProperty, null);
            QuestionCard.BeginAnimation(RenderTransformProperty, null);
            QuestionCard.RenderTransform = null;
            QuestionCard.Opacity = 1;
            QuestionCard.Background = Brushes.White;
            QuestionText.FontSize = 18;
            QuestionText.FontWeight = FontWeights.SemiBold;
            QuestionText.Foreground = Brushes.Black;
            QuestionText.TextAlignment = TextAlignment.Center;
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            _selectedRadioButton = (RadioButton)sender;
        }

        private async void CheckAnswerButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedRadioButton == null)
            {
                MessageBox.Show("Выберите вариант ответа!");
                return;
            }

            var currentQuestion = _questions[_currentQuestionIndex];
            int selectedAnswerIndex = (int)_selectedRadioButton.Tag;
            bool isCorrect = selectedAnswerIndex == currentQuestion.Answer;

            ShowAnswerFeedback(isCorrect);
            CheckAnswerButton.IsEnabled = false;
            await StartCardAnimationAsync(isCorrect);
        }

        private void ShowAnswerFeedback(bool isCorrect)
        {
            if (_selectedRadioButton != null)
            {
                _selectedRadioButton.Background = isCorrect ?
                    new SolidColorBrush(Color.FromRgb(220, 255, 220)) :
                    new SolidColorBrush(Color.FromRgb(255, 220, 220));
                _selectedRadioButton.BorderBrush = isCorrect ? Brushes.Green : Brushes.Red;
            }
        }

        private async Task StartCardAnimationAsync(bool isCorrect)
        {
            var flyAwayAnimation = new DoubleAnimation
            {
                To = isCorrect ? 800 : -800,
                Duration = TimeSpan.FromSeconds(0.8),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            var rotateAnimation = new DoubleAnimation
            {
                To = isCorrect ? 15 : -15,
                Duration = TimeSpan.FromSeconds(0.8),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            var opacityAnimation = new DoubleAnimation
            {
                To = 0,
                Duration = TimeSpan.FromSeconds(0.6),
                BeginTime = TimeSpan.FromSeconds(0.2)
            };

            var transformGroup = new TransformGroup();
            transformGroup.Children.Add(new TranslateTransform());
            transformGroup.Children.Add(new RotateTransform());
            QuestionCard.RenderTransform = transformGroup;

            QuestionCard.BeginAnimation(OpacityProperty, opacityAnimation);
            transformGroup.Children[0].BeginAnimation(TranslateTransform.XProperty, flyAwayAnimation);
            transformGroup.Children[1].BeginAnimation(RotateTransform.AngleProperty, rotateAnimation);

            await Task.Delay(1000);
            OnAnimationCompleted(isCorrect);
        }

        private void OnAnimationCompleted(bool isCorrect)
        {
            if (isCorrect) _correctAnswers++;


            SaveResult(isCorrect);

            _currentQuestionIndex++;

            if (_currentQuestionIndex < _questions.Count)
            {
                ShowCurrentQuestion();
                CheckAnswerButton.IsEnabled = true;
            }
            else
            {
                ShowResults();
                UpdateStudentDashboard();
            }
        }

        private void SaveResult(bool isCorrect)
        {
            using (var context = new PddTrainingDbContext())
            {
                var result = new Result
                {
                    UserId = App.CurrentUser.UserId,
                    QuestionId = _questions[_currentQuestionIndex].QuestionId,
                    Date = DateTime.Now,
                    IsCorrect = isCorrect
                };

                context.Results.Add(result);
                context.SaveChanges();
            }
        }

        private void UpdateProgressText()
        {
            ProgressText.Text = $"Вопрос {_currentQuestionIndex + 1} из {_questions.Count} | Правильно: {_correctAnswers}";
        }

        private void ShowResults()
        {
            ResetCardTransformations();

            QuestionCard.Background = new LinearGradientBrush(Colors.LightBlue, Colors.White, 45);
            QuestionText.Text = $"ТЕСТ ЗАВЕРШЕН!\n\nПравильных ответов: {_correctAnswers} из {_questions.Count}";
            QuestionText.FontSize = 20;
            QuestionText.FontWeight = FontWeights.Bold;
            QuestionText.Foreground = Brushes.DarkBlue;
            QuestionText.TextAlignment = TextAlignment.Center;

            OptionsPanel.Children.Clear();

            int percentage = _questions.Count > 0 ? _correctAnswers * 100 / _questions.Count : 0;

            SaveAssignmentResult(percentage);
            UpdateStudentDashboard();

            var resultText = new TextBlock
            {
                Text = $"Результат: {percentage}%",
                FontSize = 18,
                FontWeight = FontWeights.SemiBold,
                Foreground = percentage >= 70 ? Brushes.Green : Brushes.Red,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 10, 0, 20)
            };
            OptionsPanel.Children.Add(resultText);

            var backButton = new Button
            {
                Content = "ВЕРНУТЬСЯ К МОДУЛЯМ",
                Background = Brushes.SteelBlue,
                Foreground = Brushes.White,
                Padding = new Thickness(25, 15, 25, 15),
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 10, 0, 0),
                Cursor = System.Windows.Input.Cursors.Hand
            };
            backButton.Click += (s, e) => NavigationService.GoBack();
            OptionsPanel.Children.Add(backButton);

            CheckAnswerButton.Visibility = Visibility.Collapsed;
            ProgressText.Visibility = Visibility.Collapsed;

            QuestionCard.Opacity = 0;
            var showAnimation = new DoubleAnimation { To = 1, Duration = TimeSpan.FromSeconds(0.5) };
            QuestionCard.BeginAnimation(OpacityProperty, showAnimation);
        }

        private void UpdateStudentDashboard()
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow?.MainFrame?.Content is StudentDashboardPage studentPage)
            {
                studentPage.LoadData();
            }
            else
            {
                if (NavigationService?.Content is StudentDashboardPage studentPageFromNav)
                {
                    studentPageFromNav.LoadData();
                }
            }
        }
        private void SaveAssignmentResult(int percentage)
        {
            using (var context = new PddTrainingDbContext())
            {
                // Находим активное задание для студента (если есть)
                var assignment = context.StudentAssignments
                    .FirstOrDefault(sa => sa.StudentId == App.CurrentUser.UserId &&
                                         sa.IsCompleted != true);

                if (assignment != null)
                {
                    assignment.IsCompleted = true;
                    assignment.CompletedDate = DateTime.Now;
                    assignment.Score = percentage;
                    context.SaveChanges();
                }
            }
        }
    }
}