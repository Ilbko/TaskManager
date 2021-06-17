using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TaskManager.ViewModel;

namespace TaskManager
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new TaskViewModel(ref this.TaskListView);
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
    }
}
