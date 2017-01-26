﻿using System;
using System.IO;
using log4net;
using Newtonsoft.Json.Linq;

namespace EvaJimaSettings
{
    public class Settings
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Settings));

        public bool IsLoadedSuccessfully = true;

        public string Version = "1.26";
        public string CurrentVersion = "1.26";
        public bool IsAuthorizationEnabled = true;

        public string CCPSSO_AUTH_CLIENT_ID = "";
        public string CCPSSO_AUTH_CLIENT_SECRET = "";
        public string CCPSSO_AUTH_CLIENT_STATE = "";
        public string CCPSSO_AUTH_PORT = "";
        public string CCPSSO_AUTH_SCOPES = "";

        public string Server_update_uri_version = "";
        public string Server_update_content_version = "";

        public Settings()
        {
            Log.Debug("[Settings.LoadSettings] Start load settings");

            try
            {
                using (var reader = new StreamReader("settings.txt"))
                {
                    dynamic data = JObject.Parse(reader.ReadToEnd());

                    CCPSSO_AUTH_CLIENT_ID       = data.CCPSSO_AUTH_CLIENT_ID;
                    CCPSSO_AUTH_CLIENT_SECRET   = data.CCPSSO_AUTH_CLIENT_SECRET;
                    CCPSSO_AUTH_CLIENT_STATE    = data.CCPSSO_AUTH_CLIENT_STATE;
                    CCPSSO_AUTH_PORT            = data.CCPSSO_AUTH_PORT;
                    CCPSSO_AUTH_SCOPES          = data.CCPSSO_AUTH_SCOPES;
                    Server_update_uri_version   = data.Server_update_uri_version;
                    Server_update_content_version = data.Server_update_content_version;
                }

                CurrentVersion = File.ReadAllText(@"Version.txt");

                using (var wc = new System.Net.WebClient())
                    Version = wc.DownloadString(Server_update_uri_version);

                Log.DebugFormat("[Settings.LoadSettings] Read version {0}", CurrentVersion);

            }
            catch (Exception ex)
            {
                Log.ErrorFormat("[Settings.LoadSettings] Critical error. Exception {0}", ex);
                IsLoadedSuccessfully = false;
            }
            
        }


    }
}
