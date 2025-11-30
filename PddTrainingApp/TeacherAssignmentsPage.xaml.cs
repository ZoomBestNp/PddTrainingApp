using PddTrainingApp.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace PddTrainingApp.Views
{
    public partial class TeacherAssignmentsPage : Page
    {
        private int? _studentId;

        public TeacherAssignmentsPage()
        {
            InitializeComponent();
            LoadModules();
        }

        public TeacherAssignmentsPage(int? studentId = null)
        {
            InitializeComponent();
            LoadModules();
            _studentId = studentId;
        }

        private void LoadModules()
        {
            using (var context = new PddTrainingDbContext())
            {
                ModuleComboBox.ItemsSource = context.Modules.ToList();
            }
        }

        private void CreateAssignmentButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(AssignmentNameTextBox.Text))
            {
                MessageBox.Show("Введите название задания");
                return;
            }

            if (ModuleComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите модуль");
                return;
            }

            if (!int.TryParse(QuestionsCountTextBox.Text, out int questionsCount) || questionsCount <= 0)
            {
                MessageBox.Show("Введите корректное количество вопросов");
                return;
            }

            using (var context = new PddTrainingDbContext())
            {
                // Создаем блок вопросов
                var assignment = new QuestionBlock
                {
                    TeacherId = App.CurrentUser.UserId,
                    Name = AssignmentNameTextBox.Text,
                    Description = DescriptionTextBox.Text,
                    ModuleId = (ModuleComboBox.SelectedItem as Module).ModuleId,
                    DifficultyLevel = (DifficultyComboBox.SelectedItem as ComboBoxItem)?.Tag as int?,
                    QuestionsCount = questionsCount
                };

                context.QuestionBlocks.Add(assignment);
                context.SaveChanges();

                // Назначаем задание конкретному студенту
                if (_studentId.HasValue)
                {
                    var studentAssignment = new StudentAssignment
                    {
                        StudentId = _studentId.Value,
                        BlockId = assignment.BlockId,
                        DueDate = System.DateTime.Now.AddDays(7) // Срок 7 дней
                    };
                    context.StudentAssignments.Add(studentAssignment);
                }
                else
                {
                    // Назначаем всем студентам преподавателя
                    var studentIds = context.TeacherStudents
                        .Where(ts => ts.TeacherId == App.CurrentUser.UserId)
                        .Select(ts => ts.StudentId)
                        .ToList();

                    foreach (int studentId in studentIds)
                    {
                        var studentAssignment = new StudentAssignment
                        {
                            StudentId = studentId,
                            BlockId = assignment.BlockId,
                            DueDate = System.DateTime.Now.AddDays(7)
                        };
                        context.StudentAssignments.Add(studentAssignment);
                    }
                }

                context.SaveChanges();
                MessageBox.Show("Задание успешно создано и назначено!", "Успех");
                NavigationService.GoBack();
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}