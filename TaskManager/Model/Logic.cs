using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows;
using TaskManager.ViewModel;
using TaskManager.Model.Base;
using System.Management;
using System.Drawing;

namespace TaskManager.Model
{
    public static class Logic
    {
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
                                DisplayMemberBinding = new Binding("Id")
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
                            break;
                        }

                }

                listView.View = gridView;
                return true;
            }

            currentChoice = act;
            return false;
        }

        internal static ObservableCollection<Process> GetProcesses()
        {
            ObservableCollection<Process> processes = new ObservableCollection<Process>(Process.GetProcesses().ToList().OrderBy(x => x.ProcessName));
            foreach (Process item in processes)
            {
                item.Refresh();
            }

            return processes;
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
    }
}
