using PddTrainingApp.Models;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace PddTrainingApp.Views
{
    public partial class ModulesPage : Page
    {
        public ModulesPage()
        {
            InitializeComponent();
            LoadModules();
        }

        private void LoadModules()
        {
            using (var context = new PddTrainingDbContext())
            {
                var modules = context.Modules.ToList();
                ModulesItemsControl.ItemsSource = modules;
            }
        }

        private void StartModule_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            int moduleId = (int)button.Tag;
            NavigationService.Navigate(new QuestionsPage(moduleId: moduleId));
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}