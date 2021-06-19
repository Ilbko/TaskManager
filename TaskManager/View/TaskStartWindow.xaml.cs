using System.Windows;
using TaskManager.ViewModel;

namespace TaskManager.View
{
    /// <summary>
    /// Логика взаимодействия для TaskStartWindow.xaml
    /// </summary>
    public partial class TaskStartWindow : Window
    {
        public TaskStartWindow()
        {
            InitializeComponent();
            this.DataContext = new StartViewModel();
        }
    }
}
