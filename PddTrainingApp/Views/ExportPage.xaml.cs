using PddTrainingApp.Models;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;

namespace PddTrainingApp.Views
{
    public partial class ExportPage : Page
    {
        public ExportPage()
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

        private async void ExportQuestion_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(QuestionIdTextBox.Text, out int questionId))
            {
                MessageBox.Show("Введите корректный ID вопроса");
                return;
            }

            try
            {
                using (var client = new System.Net.Http.HttpClient())
                {
                    client.BaseAddress = new System.Uri("http://localhost:5239");
                    var response = await client.GetAsync($"/api/exportimport/export/question/{questionId}");

                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();

                        var saveDialog = new Microsoft.Win32.SaveFileDialog
                        {
                            Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                            FileName = $"question_{questionId}_{DateTime.Now:yyyyMMdd_HHmmss}.json",
                            DefaultExt = ".json"
                        };

                        if (saveDialog.ShowDialog() == true)
                        {
                            File.WriteAllText(saveDialog.FileName, json);
                            ExportResultText.Text = $"Вопрос успешно экспортирован в файл:\n{saveDialog.FileName}";
                            ExportResultText.Foreground = System.Windows.Media.Brushes.Green;
                        }
                    }
                    else
                    {
                        ExportResultText.Text = $"Ошибка: {response.StatusCode}";
                        ExportResultText.Foreground = System.Windows.Media.Brushes.Red;
                    }
                }
            }
            catch (Exception ex)
            {
                ExportResultText.Text = $"Ошибка экспорта: {ex.Message}";
                ExportResultText.Foreground = System.Windows.Media.Brushes.Red;
            }
        }

        private async void ExportModule_Click(object sender, RoutedEventArgs e)
        {
            var selectedModule = ModuleComboBox.SelectedItem as Module;
            if (selectedModule == null)
            {
                MessageBox.Show("Выберите модуль");
                return;
            }

            try
            {
                using (var client = new System.Net.Http.HttpClient())
                {
                    client.BaseAddress = new System.Uri("http://localhost:5239");
                    var response = await client.GetAsync($"/api/exportimport/export/module/{selectedModule.ModuleId}");

                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();

                        var saveDialog = new Microsoft.Win32.SaveFileDialog
                        {
                            Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                            FileName = $"module_{selectedModule.ModuleId}_{DateTime.Now:yyyyMMdd_HHmmss}.json",
                            DefaultExt = ".json"
                        };

                        if (saveDialog.ShowDialog() == true)
                        {
                            File.WriteAllText(saveDialog.FileName, json);
                            MessageBox.Show($"Экспортировано {selectedModule.Name} в файл:\n{saveDialog.FileName}",
                                "Экспорт завершен", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    else
                    {
                        MessageBox.Show($"Ошибка экспорта: {response.StatusCode}");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка экспорта: {ex.Message}");
            }
        }
    }
}