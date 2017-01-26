using System;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
//using WHL;


namespace WHLStarter
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

            // Time to cloase updater.exe
            //Thread.Sleep(3000);

            //var DLL = Assembly.LoadFrom("EvajimaBase.dll");

            //var myObject = DLL.GetType("WHL.WindowMonitoring");

            //var classInst = Activator.CreateInstance(myObject, null);
            //var dllWinForm = (Form)classInst;


            //Application.Run(dllWinForm);
           
            //if (WHL.Global.Settings.IsLoadedSuccessfully)
            //{
            //    Application.Run(new WHL.WindowLoadingError());
            //    //Application.Run(new WHL.WindowMonitoring());
            //}
            //else
            //{
            //    Application.Run(new WHL.WindowLoadingError());
            //}
            Application.Run(new EveJimaCore.WindowLoadingError());
        }

    }
}
