using System;
using System.Reflection;
using CefSharp;
using EveJimaCore.WhlControls;
using log4net;

namespace EveJimaCore
{
    public class InternalBrowser
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(whlVersion));

        public whlBrowser Browser;

        public InternalBrowser()
        {
            InitializeChromium();
        }

        private static void InitializeChromium()
        {
            try
            {
                var settings = new CefSettings();
                //var v = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                //settings.BrowserSubprocessPath = v + @"lib\CefSharp.BrowserSubprocess.exe";
                //Log.DebugFormat("[InternalBrowser.InitializeChromium] BrowserSubprocessPath = {0}", settings.BrowserSubprocessPath);
                //settings.LocalesDirPath = v + @"\lib\locales";
                //Log.DebugFormat("[InternalBrowser.InitializeChromium] LocalesDirPath = {0}", settings.LocalesDirPath);
                //settings.ResourcesDirPath = v + @"\lib";
                //Log.DebugFormat("[InternalBrowser.InitializeChromium] ResourcesDirPath = {0}", settings.ResourcesDirPath);
                // Initialize cef with the provided settings
                Cef.Initialize(settings);
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("[InternalBrowser.InitializeChromium] Critical error. Exception {0}", ex);
            }

            
        }
    }
}
