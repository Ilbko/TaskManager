using GalaSoft.MvvmLight.Command;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using TaskManager.Model;
using TaskManager.Model.Base;

namespace TaskManager.ViewModel
{
    public class TaskViewModel : INotifyPropertyChanged
    {
        private ListView listView;
        public ObservableCollection<Process> processes;
        //public ObservableCollection<WinProcess> winProcesses { get; set; }

        //private Process selectedProcess;
        //public Process SelectedProcess
        //{
        //    get { return selectedProcess; }
        //    set { selectedProcess = value; OnPropertyChanged("SelectedProcess"); }
        //}


        private RelayCommand<string> buttonCommand;
        public RelayCommand<string> ButtonCommand
        {
            get
            {
                //return buttonCommand ?? new RelayCommand<string>(act 
                //    => Logic.ButtonClick(act, ref listView));
                return buttonCommand ?? new RelayCommand<string>(act =>
                {
                    string button = act;

                    switch (button)
                    {
                        case "processes":
                            {
                                if (Logic.ButtonClick(act, ref listView))
                                {
                                    this.processes = Logic.GetProcesses();
                                    OnPropertyChanged("processes");
                                }
                                else
                                    this.listView.ItemsSource = processes;
                                //this.listView.ItemsSource = winProcesses;
                                break;
                            }
                        case "autorun":
                            {
                                break;
                            }
                    }
                });

            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        public TaskViewModel(ref ListView listView)
        {
            this.listView = listView;
            this.processes = Logic.GetProcesses();     
            //this.winProcesses = Logic.GetProcesses();

            Logic.ButtonClick("processes", ref listView);

            this.listView.ItemsSource = processes;
            //this.listView.ItemsSource = winProcesses;

            OnPropertyChanged("processes");
            //OnPropertyChanged("winProcesses");
        }
    }
}
