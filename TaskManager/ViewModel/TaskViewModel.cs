using GalaSoft.MvvmLight.Command;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using TaskManager.Model;

namespace TaskManager.ViewModel
{
    public class TaskViewModel : INotifyPropertyChanged
    {
        private ListView listView;
        private ScrollViewer listViewScrollViewer;
        private Button taskButton;
        public ObservableCollection<Process> processes { get; set; }

        private int pastProcessId;
        private Process selectedProcess;
        public Process SelectedProcess
        {
            get { return selectedProcess; }
            set 
            { 
                selectedProcess = value;
                try
                {
                    taskButton.IsEnabled = selectedProcess != null ? true : false;
                } catch (System.InvalidOperationException e) { }
                pastProcessId = selectedProcess != null ? selectedProcess.Id : pastProcessId;
            }
        }

        //public ObservableCollection<WinProcess> winProcesses { get; set; }

        //private Process selectedProcess;
        //public Process SelectedProcess
        //{
        //    get { return selectedProcess; }
        //    set { selectedProcess = value; OnPropertyChanged("SelectedProcess"); }
        //}
        private Timer refreshTimer = new Timer();

        private int refreshTime;
        public int RefreshTime
        {
            get { return refreshTime; }
            set 
            { 
                refreshTime = value; 
                OnPropertyChanged("RefreshTime");

                if (refreshTime != 0)
                {
                    refreshTimer.Interval = refreshTime * 1000;
                    refreshTimer.Start();
                }
                else
                    refreshTimer.Stop();        
            }
        }


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
                                    Logic.GetProcesses(this.processes);
                                    //this.processes = Logic.GetProcesses();
                                    //OnPropertyChanged("processes");                                   
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

        private RelayCommand killCommand;

        public RelayCommand KillCommand
        {
            get 
            {
                return killCommand ?? new RelayCommand(() => 
                { 
                    if (Logic.KillCommand(selectedProcess))
                        App.Current.Dispatcher.Invoke(() => Logic.GetProcesses(this.processes));
                    //this.processes = Logic.GetProcesses();
                    //OnPropertyChanged("processes");
                });
            }
        }

        private RelayCommand updateCommand;
        public RelayCommand UpdateCommand
        {
            get
            {
                return updateCommand ?? new RelayCommand(() => Logic.GetProcesses(this.processes));
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        public TaskViewModel(ref ListView listView, ref Button taskButton)
        {
            this.listView = listView;
            this.taskButton = taskButton;
            this.processes = new ObservableCollection<Process>();

            Logic.GetProcesses(this.processes);
            //this.processes = Logic.GetProcesses();     
            //this.winProcesses = Logic.GetProcesses();

            Logic.ButtonClick("processes", ref listView);

            BindingOperations.SetBinding(listView, ListView.SelectedItemProperty, new Binding()
            {
                Source = this,
                Path = new PropertyPath("SelectedProcess"),
                Mode = BindingMode.TwoWay, 
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            });

            //this.listView.ItemsSource = this.processes;
            listView.SetBinding(ListView.ItemsSourceProperty, new Binding() { Source = this.processes });
            //this.listView.ItemsSource = winProcesses;

            //OnPropertyChanged("processes");
            //OnPropertyChanged("winProcesses");

            this.refreshTimer.Elapsed += RefreshTimer_Elapsed;

            //Вместо этого можно просто изменить значение refreshTime. Его propfull установит интервал и запустит таймер
            //refreshTimer.Interval = refreshTime * 1000;
            //refreshTimer.Start();

            this.RefreshTime = 0;
        }

        private void RefreshTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            listViewScrollViewer = Logic.GetScrollViewer(this.listView);
            double scrollValue = listViewScrollViewer.VerticalOffset;

            Logic.GetProcesses(this.processes);
            App.Current.Dispatcher.Invoke(() => listViewScrollViewer.ScrollToVerticalOffset(scrollValue));

            Process currentProcess = this.processes.SingleOrDefault(x => x.Id == this.pastProcessId);
            //App.Current.Dispatcher.Invoke(() => this.listView.ScrollIntoView(currentProcess));
            App.Current.Dispatcher.Invoke(() => this.listView.SelectedItem = currentProcess);
        }
    }
}
