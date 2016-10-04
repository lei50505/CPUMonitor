using CPUMonitor.source.util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CPUMonitor
{

    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private delegate void TextBlockCpuTextGate(string text);
        private void TextBlockCpuText(string text)
        {
            if (TextBlockCpu.Dispatcher.Thread != Thread.CurrentThread)
            {
                TextBlockCpuTextGate sg = new TextBlockCpuTextGate(TextBlockCpuText);
                Dispatcher.Invoke(sg, new object[] { text });
            }
            else
            {
                TextBlockCpu.Text = text;
            }
        }
        private delegate void TextBlockMemTextGate(string text);
        private void TextBlockMemText(string text)
        {
            if (TextBlockMem.Dispatcher.Thread != Thread.CurrentThread)
            {
                TextBlockMemTextGate sg = new TextBlockMemTextGate(TextBlockMemText);
                Dispatcher.Invoke(sg, new object[] { text });
            }
            else
            {
                TextBlockMem.Text = text;
            }
        }
        private static string[] GetCategoryNames()
        {
            List<string> categoryNameList = new List<string>();
            PerformanceCounterCategory[] pccs = PerformanceCounterCategory.GetCategories();
            foreach (PerformanceCounterCategory pcc in pccs)
            {
                categoryNameList.Add(pcc.CategoryName);
            }
            return categoryNameList.ToArray<string>();

        }
        private static string[] GetInstanceNames(string categoryName)
        {
            PerformanceCounterCategory pcc = new PerformanceCounterCategory(categoryName);
            return pcc.GetInstanceNames();
        }

        private static string[] GetCounterNames(string categoryName,string instanceName)
        {
            List<string> counterNameList = new List<string>();
            PerformanceCounterCategory pcc = new PerformanceCounterCategory(categoryName);
            PerformanceCounter[] pcs = pcc.GetCounters(instanceName);

            foreach (PerformanceCounter pc in pcs)
            {
                counterNameList.Add(pc.CounterName);
            }
            return counterNameList.ToArray<string>();
        }
        PerformanceCounter ProcessorPercentCounter = null;
        ManagementClass MemeryManagement = null;
        ManagementObjectCollection MemeryCollection = null;
        public MainWindow()
        {
            InitializeComponent();

            //判断多开
            Process current = Process.GetCurrentProcess();
            Process[] processes = Process.GetProcessesByName(current.ProcessName);
            if (processes.Length > 1)
            {
                Application.Current.Shutdown();
            }


            //设置初始位置
            string ConfigWindowLeftValue = UConfig.get(ConfigWindowLeftKey);
            string ConfigWindowTopValue = UConfig.get(ConfigWindowTopKey);

            if (ConfigWindowLeftValue == null || ConfigWindowTopValue == null)
            {
                double screenWidth = SystemParameters.PrimaryScreenWidth;
                double screenHeight = SystemParameters.PrimaryScreenHeight;

                Left = screenWidth - 150;
                Top = screenHeight - 130;
            }
            else
            {
                Left = Convert.ToDouble(ConfigWindowLeftValue);
                Top = Convert.ToDouble(ConfigWindowTopValue);
            }



           

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            this.ResizeMode = ResizeMode.NoResize;

            string ConfigMenuItemLockMoveIsCheckedValue = UConfig.get(ConfigMenuItemLockMoveIsCheckedKey);
            if (ConfigMenuItemLockMoveIsCheckedValue == null)
            {
                FlagMenuItemLockMoveIsChecked = false;
                MenuItemLockMove.IsChecked = false;
            }
            else
            {
                FlagMenuItemLockMoveIsChecked = Convert.ToBoolean(ConfigMenuItemLockMoveIsCheckedValue);
                MenuItemLockMove.IsChecked = Convert.ToBoolean(ConfigMenuItemLockMoveIsCheckedValue);
            }

            string ConfigMenuItemTopMostIsCheckedValue = UConfig.get(ConfigMenuItemTopMostIsCheckedKey);

            if (ConfigMenuItemTopMostIsCheckedKey == null)
            {
                this.Topmost = false;
                MenuItemTopMost.IsChecked = false;
            }
            else
            {
                this.Topmost = Convert.ToBoolean(ConfigMenuItemTopMostIsCheckedValue);
                MenuItemTopMost.IsChecked = Convert.ToBoolean(ConfigMenuItemTopMostIsCheckedValue);
            }

            ProcessorPercentCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");

            MemeryManagement = new ManagementClass("Win32_OperatingSystem");

            System.Timers.Timer t = new System.Timers.Timer(1000);
            t.Elapsed += new ElapsedEventHandler(run);
            t.Enabled = true;
        }

        long VisibleMemery = 0;
        long FreeMemery = 0;
        double MemeryPercent = 0.0;
        private void run(object source, ElapsedEventArgs e)
        {
            MemeryCollection = MemeryManagement.GetInstances();

            foreach (ManagementObject mo in MemeryCollection)
            {
                if (mo["TotalVisibleMemorySize"] != null)
                {
                    VisibleMemery = long.Parse(mo["TotalVisibleMemorySize"].ToString());
                }

                if (mo["FreePhysicalMemory"] != null)
                {
                    FreeMemery = long.Parse(mo["FreePhysicalMemory"].ToString());
                }
            }
            MemeryPercent = (VisibleMemery - FreeMemery) / (VisibleMemery * 1.0);
            TextBlockMemText(MemeryPercent.ToString("#0.00 %"));

            float cpuPercent = ProcessorPercentCounter.NextValue();
            TextBlockCpuText(cpuPercent.ToString("0.00")+" %");
            
        }

        bool FlagMenuItemLockMoveIsChecked = false;

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (FlagMenuItemLockMoveIsChecked == true)
            {
                return;
            }
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
            double currentWindowLeft = this.Left;
            double currentWindowTop = this.Top;
            UConfig.add(ConfigWindowLeftKey, currentWindowLeft.ToString());
            UConfig.add(ConfigWindowTopKey, currentWindowTop.ToString());
        }
        private string ConfigWindowLeftKey = "ConfigWindowLeft";
        private string ConfigWindowTopKey = "ConfigWindowTop";
        private void MenuItemExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MenuItemTopMost_Click(object sender, RoutedEventArgs e)
        {
            if (MenuItemTopMost.IsChecked == false)
            {
                this.Topmost = true;
                MenuItemTopMost.IsChecked = true;
                UConfig.add(ConfigMenuItemTopMostIsCheckedKey, "true");
                return;
            }
            this.Topmost = false;
            MenuItemTopMost.IsChecked = false;
            UConfig.add(ConfigMenuItemTopMostIsCheckedKey, "false");
        }
        private string ConfigMenuItemTopMostIsCheckedKey = "ConfigMenuItemTopMostIsChecked";
        private string ConfigMenuItemLockMoveIsCheckedKey = "ConfigMenuItemLockMoveIsChecked";

        private void MenuItemLockMove_Click(object sender, RoutedEventArgs e)
        {
            if (MenuItemLockMove.IsChecked == true)
            {
                MenuItemLockMove.IsChecked = false;
                FlagMenuItemLockMoveIsChecked = false;
                UConfig.add(ConfigMenuItemLockMoveIsCheckedKey, "false");
                return;
            }
            FlagMenuItemLockMoveIsChecked = true;
            MenuItemLockMove.IsChecked = true;
            UConfig.add(ConfigMenuItemLockMoveIsCheckedKey, "true");
        }
    }
}
