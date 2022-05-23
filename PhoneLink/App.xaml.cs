using Hardcodet.Wpf.TaskbarNotification;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using Application = System.Windows.Application;
using MessageBox = System.Windows.Forms.MessageBox;

namespace PhoneLink
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static System.Threading.Mutex mutex;
        private ico mainIco;
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            mainIco = new ico();
            mainIco.Show();
            /*
            mutex = new System.Threading.Mutex(true, "PhoneLink");
            if (mutex.WaitOne(0, false))
            {
            }
            else
            {
                MessageBox.Show("程序已经在运行！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Environment.Exit(0);
            }
            */
        }
    }
}
