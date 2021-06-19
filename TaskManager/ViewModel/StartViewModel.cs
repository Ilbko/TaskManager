using GalaSoft.MvvmLight.Command;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TaskManager.Model;
using TaskManager.View;

namespace TaskManager.ViewModel
{
    public class StartViewModel : INotifyPropertyChanged
    {
        private string taskName;
        public string TaskName
        {
            get { return taskName; }
            set { taskName = value; OnPropertyChanged("TaskName"); }
        }
        private bool isAdmin = false;
        public bool IsAdmin
        {
            get { return isAdmin; }
            set { isAdmin = value; OnPropertyChanged("IsAdmin"); }
        }

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

        private RelayCommand<TaskStartWindow> cancelCommand;
        public RelayCommand<TaskStartWindow> CancelCommand
        {
            get
            {
                return cancelCommand ?? new RelayCommand<TaskStartWindow>(act => act.Close());
            }
        }

        private RelayCommand viewCommand;
        public RelayCommand ViewCommand
        {
            get
            {
                return viewCommand ?? new RelayCommand(() => 
                {
                    Logic.FileDialogTask(ref taskName);
                    OnPropertyChanged("TaskName");
                }); 
            }
        }




        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }
}
