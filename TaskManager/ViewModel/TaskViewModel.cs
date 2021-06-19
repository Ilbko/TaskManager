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
using TaskManager.Model.Base;
using TaskManager.View;

namespace TaskManager.ViewModel
{
    public class TaskViewModel : INotifyPropertyChanged
    {
        private ListView listView;
        private ScrollViewer listViewScrollViewer;
        private double scrollValue;
        private Button taskButton;
        private Button autorunButton;

        public ObservableCollection<Process> processes { get; set; }
        public ObservableCollection<WinAutorun> autoruns { get; set; } 

        private int pastProcessId;
        private Process selectedProcess;
        public Process SelectedProcess
        {
            get { return selectedProcess; }
            set 
            { 
                selectedProcess = value;
                OnPropertyChanged("SelectedProcess");
                try
                {
                    taskButton.IsEnabled = selectedProcess != null ? true : false;
                    autorunButton.IsEnabled = selectedProcess != null ? true : false;
                } catch (System.InvalidOperationException e) { }
                pastProcessId = selectedProcess != null ? selectedProcess.Id : pastProcessId;
            }
        }

        private WinAutorun selectedAutorun;
        public WinAutorun SelectedAutorun
        {
            get { return selectedAutorun; }
            set 
            { 
                selectedAutorun = value; 
                OnPropertyChanged("SelectedAutorun");

                taskButton.IsEnabled = selectedAutorun != null ? true : false;
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
                                    listView.SetBinding(ListView.ItemsSourceProperty, new Binding() { Source = this.processes });

                                    BindingOperations.SetBinding(listView, ListView.SelectedItemProperty, new Binding()
                                    {
                                        Source = this,
                                        Path = new PropertyPath("SelectedProcess"),
                                        Mode = BindingMode.TwoWay,
                                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                                    });

                                    this.taskButton.Content = "Снять задачу";
                                    this.taskButton.Tag = "processes";

                                    this.autorunButton.Visibility = Visibility.Visible;
                                }
                                break;
                            }
                        case "autorun":
                            {
                                if (Logic.ButtonClick(act, ref listView))
                                {
                                    Logic.GetAutoruns(this.autoruns);
                                    listView.SetBinding(ListView.ItemsSourceProperty, new Binding() { Source = this.autoruns });

                                    BindingOperations.SetBinding(listView, ListView.SelectedItemProperty, new Binding()
                                    {
                                        Source = this,
                                        Path = new PropertyPath("SelectedAutorun"),
                                        Mode = BindingMode.TwoWay,
                                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                                    });

                                    this.taskButton.Content = "Снять автозапуск";
                                    this.taskButton.Tag = "autorun";

                                    this.autorunButton.Visibility = Visibility.Hidden;
                                }
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
                    if (this.taskButton.Tag as string == "processes")
                    {
                        if (Logic.Kill(selectedProcess))
                            Logic.GetProcesses(this.processes);
                    }
                    else if (this.taskButton.Tag as string == "autorun")
                    {
                        if (Logic.RemoveAutorun(selectedAutorun))
                            Logic.GetAutoruns(this.autoruns);
                    }
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

        private RelayCommand<MainWindow> closeCommand;
        public RelayCommand<MainWindow> CloseCommand
        {
            get
            {
                return closeCommand ?? new RelayCommand<MainWindow>(act => act.Close());
            }
        }

        private RelayCommand startCommand;
        public RelayCommand StartCommand
        {
            get
            {
                return startCommand ?? new RelayCommand(() => 
                {
                    new TaskStartWindow().ShowDialog();
                    Logic.GetProcesses(this.processes);
                });
            }
        }

        private RelayCommand autorunCommand;
        public RelayCommand AutorunCommand
        {
            get
            {
                return autorunCommand ?? new RelayCommand(() => Logic.AddAutorun(selectedProcess));
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        public TaskViewModel(ref ListView listView, ref Button taskButton, ref Button autorunButton)
        {
            this.listView = listView;
            this.taskButton = taskButton;
            this.autorunButton = autorunButton;

            this.taskButton.Tag = "processes";
            this.processes = new ObservableCollection<Process>();
            this.autoruns = new ObservableCollection<WinAutorun>();

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

            this.RefreshTime = 5;
        }

        private void RefreshTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            this.listViewScrollViewer = Logic.GetScrollViewer(this.listView);
            this.scrollValue = listViewScrollViewer.VerticalOffset;

            Logic.GetProcesses(this.processes);
            App.Current.Dispatcher.Invoke(() => listViewScrollViewer.ScrollToVerticalOffset(this.scrollValue));

            Process currentProcess = this.processes.SingleOrDefault(x => x.Id == this.pastProcessId);
            //App.Current.Dispatcher.Invoke(() => this.listView.ScrollIntoView(currentProcess));
            App.Current.Dispatcher.Invoke(() => this.listView.SelectedItem = currentProcess);
        }
    }
}
