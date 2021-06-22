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

        //Событие нажатия на кнопки выбора просмотра процессов или автозапуска
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //Для всех кнопок докпанели устанавливаются свойства цвета и толщины краёв
            foreach (var button in this.ButtonsDockPanel.Children)
            {
                if (button is Button)
                {
                    (button as Button).Background = new SolidColorBrush(SystemColors.ControlColor);
                    (button as Button).BorderThickness = new Thickness(1);
                }
            }

            //Для нажатой кнопки устанавливается цвет посветлее и убирается нижний край (для имитации настоящего Диспетчера задач)
            (sender as Button).Background = new SolidColorBrush(Colors.White);
            (sender as Button).BorderThickness = new Thickness(1, 1, 1, 0);
        }

        //Событие нажатия на МенюАйтемы выбора скорости обновления списка процессов
        private void MenuItemTimer_Click(object sender, RoutedEventArgs e) => (sender as MenuItem).IsChecked = true;   
    }
}
