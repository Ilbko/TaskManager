using GalaSoft.MvvmLight.Command;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TaskManager.Model;
using TaskManager.View;

namespace TaskManager.ViewModel
{
    //ВьюМодель запуска новой задачи
    public class StartViewModel : INotifyPropertyChanged
    {
        //Переменная, хранящая текст ТекстБокса
        private string taskName;
        public string TaskName
        {
            get { return taskName; }
            set { taskName = value; OnPropertyChanged("TaskName"); }
        }
        //Переменная, отвечающая за ЧекБокс "Запуск от имени администратора"
        private bool isAdmin = false;
        public bool IsAdmin
        {
            get { return isAdmin; }
            set { isAdmin = value; OnPropertyChanged("IsAdmin"); }
        }

        #region Команды
        //Команда нажатия на кнопку запуска задачи
        private RelayCommand<TaskStartWindow> startCommand;
        public RelayCommand<TaskStartWindow> StartCommand
        {
            get
            {
                return startCommand ?? new RelayCommand<TaskStartWindow>(act =>
                {
                    Logic.StartTask(taskName, isAdmin);
                    act.Close();
                });
            }
        }

        //Команда нажатия на кнопку отмены
        private RelayCommand<TaskStartWindow> cancelCommand;
        public RelayCommand<TaskStartWindow> CancelCommand
        {
            get
            {
                return cancelCommand ?? new RelayCommand<TaskStartWindow>(act => act.Close());
            }
        }

        //Команда нажатия на кнопку обзора файлов
        private RelayCommand viewCommand;
        public RelayCommand ViewCommand
        {
            get
            {
                return viewCommand ?? new RelayCommand(() => 
                {
                    //По ссылке изменяется строка пути к файлу, чтобы после выбора файла был установлен его путь
                    Logic.FileDialogTask(ref taskName);
                    OnPropertyChanged("TaskName");
                }); 
            }
        }
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }
}
