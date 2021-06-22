using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using TaskManager.Model.Base;
using TaskManager.ViewModel;

namespace TaskManager.Model
{
    public static class Logic
    {
        //Объект класса работы с регистром и строка текущего выбора (просмотр задач или автозапусков)
        private static RegistryKey currentRegistry;
        private static string currentChoice = string.Empty;

        //Метод нажатия на кнопку просмотра
        internal static bool ButtonClick(string act, ref ListView listView)
        {
            //Если текущий выбор равняется переданному параметру, то значит, что кнопка просмотра была нажата дважды и нет смысла настраивать ЛистВью заново
            if (!(currentChoice == act))
            {
                //ГридВью, который потом станет свойством ListView.View
                GridView gridView = new GridView();
                switch (act)
                {
                    case "processes":
                        {
                            //Добавление колонок с текстом сверху и привязке (Привязка происходит к полям типа данных наблюдаемой коллекции, к которому привязан ЛистВью)
                            //(ItemsSource = ObservableCollection<*Process*>)
                            gridView.Columns.Add(new GridViewColumn
                            {
                                Header = "Имя",
                                DisplayMemberBinding = new Binding("ProcessName"),
                                Width = 150
                            });
                            gridView.Columns.Add(new GridViewColumn
                            {
                                Header = "ИД", 
                                DisplayMemberBinding = new Binding("Id"),
                            });
                            //gridView.Columns.Add(new GridViewColumn
                            //{
                            //    Header = "Имя пользователя",
                            //    //StartInfo.UserName не возвращает имя пользователя вообще.
                            //    DisplayMemberBinding = new Binding { Path = new PropertyPath("(StartInfo.UserName)")}
                            //});
                            gridView.Columns.Add(new GridViewColumn
                            {
                                Header = "Память (Кб)",
                                DisplayMemberBinding = new Binding { Path = new PropertyPath("WorkingSet64"), Converter = new MemoryConverter()}
                            });
                            gridView.Columns.Add(new GridViewColumn
                            {
                                Header = "Время начала",
                                DisplayMemberBinding = new Binding("StartTime")
                            });

                            //Медленное решение
                            //gridView.Columns.Add(new GridViewColumn
                            //{
                            //    Header = "Имя",
                            //    DisplayMemberBinding = new Binding("Name"),
                            //    Width = 150
                            //});
                            //gridView.Columns.Add(new GridViewColumn
                            //{
                            //    Header = "ИД",
                            //    DisplayMemberBinding = new Binding("Id")
                            //});
                            //gridView.Columns.Add(new GridViewColumn
                            //{
                            //    Header = "Имя пользователя",
                            //    DisplayMemberBinding = new Binding("UserName")
                            //});
                            //gridView.Columns.Add(new GridViewColumn
                            //{
                            //    Header = "Память (Кб)",
                            //    DisplayMemberBinding = new Binding("Memory")
                            //});

                            break;
                        }
                    case "autorun":
                        {
                            gridView.Columns.Add(new GridViewColumn()
                            {
                                Header = "Имя",
                                DisplayMemberBinding = new Binding("Name")
                            });
                            gridView.Columns.Add(new GridViewColumn() 
                            {
                                Header = "Путь и аттрибуты",
                                DisplayMemberBinding = new Binding("Value")
                            });
                            break;
                        }

                }

                listView.View = gridView;
                currentChoice = act;
                return true;
            }

            currentChoice = act;
            return false;
        }

        #region Методы окна запуска нового процесса
        //Метод поиска программы для запуска в файловом диалоге
        internal static void FileDialogTask(ref string taskName)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                //Название фильтра | значения фильтра
                Filter = "Программы (*.exe;*.pif;*.com;*.bat;*.cmd | *.exe; *.pif; *.com; *.bat; *.cmd;"
            };

            //Если был выбран файл
            if (openFileDialog.ShowDialog() == true) 
                taskName = openFileDialog.FileName;
        }

        //Метод запуска новой задачи
        internal static void StartTask(string taskName, bool isAdmin)
        {
            try
            {
                //Запуск от имени админа
                if (isAdmin)
                    Process.Start(new ProcessStartInfo(taskName)
                    {
                        Verb = "runas",
                        UseShellExecute = true
                    });
                else
                    Process.Start(new ProcessStartInfo(taskName));

            } catch (System.ComponentModel.Win32Exception e)
            {
                MessageBox.Show($"Не удаётся найти \"{taskName}\". Проверьте, правильно ли указано имя и повторите попытку.",
                                taskName, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region Методы работы с автозапусками
        //Метод добавления процесса в автозапуск
        internal static void AddAutorun(Process selectedProcess)
        {
            //Открытие "папки" регистра с разрешением на редактирование (запись)
            currentRegistry = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);

            try
            {
                //Добавление нового значения в виде имени процесса и его пути
                currentRegistry.SetValue(selectedProcess.ProcessName, selectedProcess.MainModule.FileName);
                MessageBox.Show("Процесс успешно добавлен в автозагрузку.", "Автозагрузка", MessageBoxButton.OK, MessageBoxImage.Information);               
            } catch (System.Exception e)
            {
                MessageBox.Show(e.Message, "Исключение", MessageBoxButton.OK, MessageBoxImage.Error);              
            }

            currentRegistry.Close();
        }

        //Метод получения текущих автозапусков
        internal static void GetAutoruns(ObservableCollection<WinAutorun> autoruns)
        {
            //Очищение прошлого списка
            if (autoruns.Count != 0)
                autoruns.Clear();

            //Открытие "папки" регистра
            currentRegistry = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run");

            //Добавление в список имени и значения
            foreach (var item in currentRegistry.GetValueNames())
            {
                autoruns.Add(new WinAutorun() { Name = item, Value = currentRegistry.GetValue(item).ToString() });
            }

            currentRegistry.Close();
        }

        //Метод удаления автозапуска
        internal static bool RemoveAutorun(WinAutorun selectedAutorun)
        {
            //Открытие "папки" регистра с разрешением на редактирование (удаление)
            currentRegistry = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);

            try
            {
                currentRegistry.DeleteValue(selectedAutorun.Name);

                currentRegistry.Close();

                return true;
            }
            catch (System.Exception e)
            {
                MessageBox.Show(e.Message, "Исключение", MessageBoxButton.OK, MessageBoxImage.Error);

                currentRegistry.Close();

                return false;
            }
        }
        #endregion

        #region Методы работы с задачами
        //Метод завершения процесса
        internal static bool Kill(Process selectedProcess)
        {
            try
            {
                selectedProcess.Kill();

                return true;
            }
            catch (System.Exception e)
            {
                MessageBox.Show(e.Message, "Исключение", MessageBoxButton.OK, MessageBoxImage.Error);

                return false;
            }
        }

        //internal static ObservableCollection<Process> GetProcesses() => new ObservableCollection<Process>(Process.GetProcesses().ToList().OrderBy(x => x.ProcessName));

        //Метод получения списка всех процессов
        internal static void GetProcesses(ObservableCollection<Process> processes)
        {
            if (processes.Count != 0)
                App.Current.Dispatcher.Invoke(() => processes.Clear());

            foreach (var item in Process.GetProcesses().AsEnumerable().OrderBy(x => x.ProcessName))
            {
                item.Refresh();
                App.Current.Dispatcher.Invoke(() => processes.Add(item));
            }
        }

        //Слишком медленно

        //internal static ObservableCollection<WinProcess> GetProcesses()
        //{
        //    ObservableCollection<WinProcess> winProcesses = new ObservableCollection<WinProcess>();
        //    ManagementObjectSearcher processes = new ManagementObjectSearcher("SELECT * FROM Win32_Process");                     

        //    foreach (ManagementObject process in processes.Get())
        //    {
        //        //https://celitel.info/klad/wsh/process.htm
        //        try
        //        {
        //            if (process["ExecutablePath"] != null)
        //            {
        //                WinProcess tempProcess = new WinProcess();
        //                string[] userInfo = new string[2];

        //                tempProcess.Picture = Icon.ExtractAssociatedIcon(process["ExecutablePath"].ToString()).ToBitmap();
        //                tempProcess.Name = process["Name"].ToString();
        //                tempProcess.Id = int.Parse(process["Handle"].ToString());
        //                //Свойство "Status" не реализовано...
        //                //tempProcess.Status = process["Status"].ToString();

        //                process.InvokeMethod("GetOwner", (object[])userInfo);
        //                tempProcess.UserName = userInfo[0];

        //                //то же самое, что и название
        //                //tempProcess.Description = process["Description"].ToString();

        //                if (winProcesses != null)
        //                    winProcesses.Add(tempProcess);

        //                tempProcess = null;
        //                userInfo = null;
        //            }
        //        } catch (System.Exception e) { }
        //    }

        //    return winProcesses;
        //}
        #endregion

        //Метод получения СкроллВью
        public static ScrollViewer GetScrollViewer(UIElement element)
        {
            if (element == null)
                return null;

            ScrollViewer scrollViewer = null;

            //VisualTreeHelper - дерево всех визуальных частей UI-элемента
            //Цикл выполняется, пока ещё остались части элемента или пока не был найден СкроллВью
            for (int i = 0; i < App.Current.Dispatcher.Invoke(() => VisualTreeHelper.GetChildrenCount(element)) && scrollViewer == null; i++)
            {
                //Если найденный элемент - СкроллВью
                if (App.Current.Dispatcher.Invoke(() => VisualTreeHelper.GetChild(element, i) is ScrollViewer))
                    scrollViewer = App.Current.Dispatcher.Invoke(() => (ScrollViewer)VisualTreeHelper.GetChild(element, i));
                else
                    //Углубление по дереву дальше
                    scrollViewer = App.Current.Dispatcher.Invoke(() => GetScrollViewer(VisualTreeHelper.GetChild(element, i) as UIElement));
            }

            return scrollViewer;
        }
    }
}
