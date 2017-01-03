using System;
using System.Windows.Forms;

namespace WHL
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            log4net.Config.XmlConfigurator.Configure();

            Application.Run(new WindowMonitoring());

            //Application.Run(new WindowAbout());

        }





    }
}
