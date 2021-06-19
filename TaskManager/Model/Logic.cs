using Microsoft.Win32;
using System;
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
        private static RegistryKey currentRegistry;
        private static string currentChoice = string.Empty;
        internal static bool ButtonClick(string act, ref ListView listView)
        {
            if (!(currentChoice == act))
            {
                GridView gridView = new GridView();
                switch (act)
                {
                    case "processes":
                        {                           
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
                return true;
            }

            currentChoice = act;
            return false;
        }

        internal static void GetAutoruns(ObservableCollection<WinAutorun> autoruns)
        {
            if (autoruns.Count != 0)
                autoruns.Clear();

            currentRegistry = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run");

            foreach (var item in currentRegistry.GetValueNames())
            {
                autoruns.Add(new WinAutorun() { Name = item, Value = currentRegistry.GetValue(item).ToString() });
            }

            currentRegistry.Close();
        }
        internal static void FileDialogTask(ref string taskName)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Filter = "Программы (*.exe;*.pif;*.com;*.bat;*.cmd | *.exe; *.pif; *.com; *.bat; *.cmd;"
            };

            if (openFileDialog.ShowDialog() == true) 
                taskName = openFileDialog.FileName;
        }

        internal static void StartTask(string taskName, bool isAdmin)
        {
            try
            {
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

        internal static bool Kill(Process selectedProcess)
        {
            try
            {
                selectedProcess.Kill();

                return true;
            } catch (System.Exception e) 
            {
                MessageBox.Show(e.Message, "Исключение", MessageBoxButton.OK, MessageBoxImage.Error);

                return false; 
            }
        }

        internal static bool RemoveAutorun(WinAutorun selectedAutorun)
        {
            currentRegistry = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);

            try
            {
                currentRegistry.DeleteValue(selectedAutorun.Name);
                
                currentRegistry.Close();

                return true;
            } catch (System.Exception e)
            {
                MessageBox.Show(e.Message, "Исключение", MessageBoxButton.OK, MessageBoxImage.Error);
                
                currentRegistry.Close();

                return false;
            }
        }

        //internal static ObservableCollection<Process> GetProcesses()
        //{
        //    ObservableCollection<Process> processes = new ObservableCollection<Process>(Process.GetProcesses().ToList().OrderBy(x => x.ProcessName));
        //    foreach (Process item in processes)
        //    {
        //        item.Refresh();
        //    }

        //    return processes;
        //}

        //internal static ObservableCollection<Process> GetProcesses() => new ObservableCollection<Process>(Process.GetProcesses().ToList().OrderBy(x => x.ProcessName));
        internal static void GetProcesses(ObservableCollection<Process> processes)
        {
            if (processes.Count != 0)
                App.Current.Dispatcher.Invoke(() => processes.Clear());

            foreach(var item in Process.GetProcesses().AsEnumerable().OrderBy(x => x.ProcessName))
            {
                item.Refresh();
                App.Current.Dispatcher.Invoke(() => processes.Add(item));
            }
        }

        internal static void AddAutorun(Process selectedProcess)
        {
            currentRegistry = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);

            try
            {
                currentRegistry.SetValue(selectedProcess.ProcessName, selectedProcess.MainModule.FileName);
                MessageBox.Show("Процесс успешно добавлен в автозагрузку.", "Автозагрузка", MessageBoxButton.OK, MessageBoxImage.Information);
                
                currentRegistry.Close();
            } catch (System.Exception e)
            {
                MessageBox.Show(e.Message, "Исключение", MessageBoxButton.OK, MessageBoxImage.Error);
                
                currentRegistry.Close();
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

        public static ScrollViewer GetScrollViewer(UIElement element)
        {
            if (element == null)
                return null;

            ScrollViewer scrollViewer = null;

            for (int i = 0; i < App.Current.Dispatcher.Invoke(() => VisualTreeHelper.GetChildrenCount(element)) && scrollViewer == null; i++)
            {
                if (App.Current.Dispatcher.Invoke(() => VisualTreeHelper.GetChild(element, i) is ScrollViewer))
                    scrollViewer = App.Current.Dispatcher.Invoke(() => (ScrollViewer)VisualTreeHelper.GetChild(element, i));
                else
                    scrollViewer = App.Current.Dispatcher.Invoke(() => GetScrollViewer(VisualTreeHelper.GetChild(element, i) as UIElement));
            }

            return scrollViewer;
        }
    }
}
