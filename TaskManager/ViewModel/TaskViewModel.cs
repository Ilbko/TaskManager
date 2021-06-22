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
        //Элементы главного окна (передаются по ссылке для взаимодействия с ними)
        private ListView listView;
        //СкроллВью для ЛистВью (получает информацию о скроллбаре ЛистВью) и переменная для хранения значения скроллбара
        private ScrollViewer listViewScrollViewer;
        private double scrollValue;
        //Кнопка задачи (снять задачу или снять автозапуск) и кнопка добавления в автозапуск
        private Button taskButton;
        private Button autorunButton;

        //Наблюдаемые коллекции процессов и автозапусков
        public ObservableCollection<Process> processes { get; set; }
        public ObservableCollection<WinAutorun> autoruns { get; set; }

        #region Счётчики производительности
        //Счётчик производительности нагрузки процессора в процентах и строка, куда записывается значение счётчика
        private PerformanceCounter cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        
        private string cpuString;
        public string CPUString
        {
            get { return cpuString; }
            set { cpuString = value; OnPropertyChanged("CPUString"); }
        }

        //Счётчик производительности количества занятой оперативной памяти в мегабайтах и строка, куда записывается значение счётчика
        private PerformanceCounter ramCounter = new PerformanceCounter("Memory", "Available MBytes");
        
        private string ramString;
        public string RAMString
        {
            get { return ramString; }
            set { ramString = value; OnPropertyChanged("RAMString"); }
        }
        #endregion

        #region SelectedItems
        //Выбранный в ЛистВью процесс
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
                    //Если был выбран процесс, то кнопки снятия задачи и добавления становятся активными и наоборот
                    taskButton.IsEnabled = selectedProcess != null ? true : false;
                    autorunButton.IsEnabled = selectedProcess != null ? true : false;
                } catch (System.InvalidOperationException e) { }
                //Установка значения айди прошлого процесса (Если есть выбранный процесс, то айди обновляется)
                pastProcessId = selectedProcess != null ? selectedProcess.Id : pastProcessId;
            }
        }
        //Айди прошлого процесса (при обновлении списка процессов выбранный процесс обнуляется. Этот айди нужен для восстановки выбора)
        private int pastProcessId;

        //Выбранный в ЛистВью автозапуск
        private WinAutorun selectedAutorun;
        public WinAutorun SelectedAutorun
        {
            get { return selectedAutorun; }
            set 
            { 
                selectedAutorun = value; 
                OnPropertyChanged("SelectedAutorun");

                //Если был выбран автозапуск, то кнопка снятия автозапуска становится активной и наоборот
                taskButton.IsEnabled = selectedAutorun != null ? true : false;
            }
        }
        #endregion

        #region Таймер, fullprop и событие таймера
        //Таймер обновления процессов
        private Timer refreshTimer = new Timer();

        //Переменная, хранящая интервал обновления таймера в секундах
        private int refreshTime;
        public int RefreshTime
        {
            get { return refreshTime; }
            set 
            { 
                refreshTime = value; 
                OnPropertyChanged("RefreshTime");

                //Интервал таймера не может быть 0, поэтому в случае выбора скорости обновления "Приостановлено" таймер останавливается
                if (refreshTime != 0)
                {
                    refreshTimer.Interval = refreshTime * 1000;
                    refreshTimer.Start();
                }
                else
                    refreshTimer.Stop();        
            }
        }

        //Событие таймера
        private void RefreshTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            //Получение СкроллВьювера ЛистВью и установка значения скроллбара
            this.listViewScrollViewer = Logic.GetScrollViewer(this.listView);
            this.scrollValue = listViewScrollViewer.VerticalOffset;

            //Обновление списка процессов и установка скроллбара на положение до обновления
            Logic.GetProcesses(this.processes);
            //Иногда действия не успевают выполняться за таймером и начинают выполняться другими потоками, что влечёт за собой исключение
            //System.InvalidOperationException: "Вызывающий поток не может получить доступ к главному объекту,"
            //"так как владельцем этого объекта является другой поток". (также может возникнуть при взаимодействии с 
            //UI-элементов, как в методе GetScrollViewer). Поэтому они выполняются от имени главного потока
            App.Current.Dispatcher.Invoke(() => listViewScrollViewer.ScrollToVerticalOffset(this.scrollValue));

            //Нахождение процесса с ранее сохранённым айди
            Process currentProcess = this.processes.SingleOrDefault(x => x.Id == this.pastProcessId);
            //Перенос фокусировки к элементу ЛистВью (неточный перенос)
            //App.Current.Dispatcher.Invoke(() => this.listView.ScrollIntoView(currentProcess));

            //Установка выбранного элемента
            App.Current.Dispatcher.Invoke(() => this.listView.SelectedItem = currentProcess);

            //Обновление счётчиков производительности
            this.UpdatePerfCounters();
        }

        #endregion

        #region Методы
        //Метод изменения ЛистВью (с процессов на автозагрузки и обратно)
        private void ChangeListView(string act)
        {
            switch (act)
            {
                //Если передаваемый в команду параметр == "процессы"
                case "processes":
                    {
                        //В GUI могла быть нажата одна и та же кнопка дважды, поэтому происходит проверка на это. Если была нажата другая кнопка, то свойства ЛистВью меняются
                        if (Logic.ButtonClick(act, ref listView))
                        {
                            //Обновление списка процессов и установка этого списка как источника данных для ЛистВью
                            Logic.GetProcesses(this.processes);
                            listView.SetBinding(ListView.ItemsSourceProperty, new Binding() { Source = this.processes });

                            //Установка привязки к свойству выбранного предмета для ЛистВью
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
                            this.refreshTimer.Start();
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
                            this.refreshTimer.Stop();
                        }
                        break;
                    }
            }
        }

        //Метод обновления счётчиков производительности
        private void UpdatePerfCounters()
        {
            this.CPUString = "CPU: " + this.cpuCounter.NextValue() + "%";
            this.RAMString = "RAM: " + this.ramCounter.NextValue() + " MB";
        }
        #endregion

        #region Команды
        //Команда нажатия кнопок выбора просмотра процессов или автозагрузок
        private RelayCommand<string> buttonCommand;
        public RelayCommand<string> ButtonCommand
        {
            get
            {
                return buttonCommand ?? new RelayCommand<string>(act => this.ChangeListView(act));
            }
        }

        //Команда нажатия кнопки снятия задачи или автозапуска
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

        //Команда нажатия на МенюАйтем "Обновить сейчас"
        private RelayCommand updateCommand;
        public RelayCommand UpdateCommand
        {
            get
            {
                return updateCommand ?? new RelayCommand(() => 
                {
                    Logic.GetProcesses(this.processes);
                    this.UpdatePerfCounters();
                });

            }
        }

        //Команда нажатия на МенюАйтем выхода
        private RelayCommand<MainWindow> closeCommand;
        public RelayCommand<MainWindow> CloseCommand
        {
            get
            {
                return closeCommand ?? new RelayCommand<MainWindow>(act => act.Close());
            }
        }

        //Команда нажатия на МенюАйтем запуска нового процесса
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

        //Команда нажатия на кнопку добавления в автозапуск
        private RelayCommand autorunCommand;
        public RelayCommand AutorunCommand
        {
            get
            {
                return autorunCommand ?? new RelayCommand(() => Logic.AddAutorun(selectedProcess));
            }
        }
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        //Конструктор
        public TaskViewModel(ref ListView listView, ref Button taskButton, ref Button autorunButton)
        {
            //Получение ссылок на элементы окна, инициализация коллекций и ЛистВью, установка времени обновления,
            //обновление счётчиков и подписка на событие
            this.listView = listView;
            this.taskButton = taskButton;
            this.autorunButton = autorunButton;

            this.processes = new ObservableCollection<Process>();
            this.autoruns = new ObservableCollection<WinAutorun>();

            this.ChangeListView("processes");

            this.RefreshTime = 5;

            this.UpdatePerfCounters();
            this.refreshTimer.Elapsed += RefreshTimer_Elapsed;
        }
        
    }
}
