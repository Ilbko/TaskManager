using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TaskManager.Model;
using TaskManager.ViewModel;

namespace TaskManager
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TaskViewModel taskViewModel;
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = taskViewModel = new TaskViewModel(ref this.TaskListView, ref this.TaskButton, ref this.AutorunButton);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            foreach (Button button in this.ButtonsDockPanel.Children)
            {
                button.Background = new SolidColorBrush(SystemColors.ControlColor);
                button.BorderThickness = new Thickness(1);
            }

            (sender as Button).Background = new SolidColorBrush(Colors.White);
            (sender as Button).BorderThickness = new Thickness(1, 1, 1, 0);
        }

        private void MenuItemTimer_Click(object sender, RoutedEventArgs e) => (sender as MenuItem).IsChecked = true;
        private void MenuItemUpdate_Click(object sender, RoutedEventArgs e) => Logic.GetProcesses(taskViewModel.processes);
    }
}
