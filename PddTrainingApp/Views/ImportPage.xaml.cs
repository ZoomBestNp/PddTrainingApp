using PddTrainingApp.Models;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;

namespace PddTrainingApp.Views
{
    public partial class ImportPage : Page
    {
        private string _selectedFilePath = string.Empty;

        public ImportPage()
        {
            InitializeComponent();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void SelectFile_Click(object sender, RoutedEventArgs e)
        {
            var openDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                Title = "Выберите файл вопроса для импорта"
            };

            if (openDialog.ShowDialog() == true)
            {
                _selectedFilePath = openDialog.FileName;
                SelectedFileText.Text = $"Выбран файл: {Path.GetFileName(_selectedFilePath)}";
                ImportResultText.Text = "";
            }
        }

        private async void ImportFile_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedFilePath) || !File.Exists(_selectedFilePath))
            {
                MessageBox.Show("Выберите файл для импорта");
                return;
            }

            try
            {
                string json = File.ReadAllText(_selectedFilePath);

                using (var client = new System.Net.Http.HttpClient())
                {
                    client.BaseAddress = new System.Uri("http://localhost:5239");

                    var content = new System.Net.Http.StringContent(json,
                        System.Text.Encoding.UTF8, "application/json");

                    var response = await client.PostAsync("/api/exportimport/import", content);

                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadAsStringAsync();
                        ImportResultText.Text = "✅ Вопрос успешно импортирован!";
                        ImportResultText.Foreground = System.Windows.Media.Brushes.Green;

                        _selectedFilePath = string.Empty;
                        SelectedFileText.Text = "";
                    }
                    else
                    {
                        var error = await response.Content.ReadAsStringAsync();
                        ImportResultText.Text = $"❌ Ошибка импорта: {response.StatusCode}\n{error}";
                        ImportResultText.Foreground = System.Windows.Media.Brushes.Red;
                    }
                }
            }
            catch (Exception ex)
            {
                ImportResultText.Text = $"❌ Ошибка импорта: {ex.Message}";
                ImportResultText.Foreground = System.Windows.Media.Brushes.Red;
            }
        }
    }
}