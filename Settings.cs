using System;
using System.IO;
using log4net;
using Newtonsoft.Json.Linq;

namespace WHL
{
    public class Settings
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Settings));

        public string Version = "1.26";
        public bool IsAuthorizationEnabled = true;

        #region Data for tests
        //public string CCPSSO_CLIENT_ID = "8f1e2ac9d4aa467c88b12674926dc5e6";//"30b47abc588f4731b0556e5280874f46"; 
        //public string CCPSSO_CLIENT_SECRET = "GZyvG71OxmfHzcDrTMreHw6CV7sDUwiBMiPSpbPn";//"Omj4FvniATMIDgTSJr8mQbFkK1ukPcD1khTfeTrF";
        //public string CCPSSO_CLIENT_STATE = "75c68f04aec80589a157fd13";
        //public string CCPSSO_PORT = "8080";
        //public string CCPSSO_AUTH_SCOPES = "characterLocationRead";

        //public string CCPSSO_AUTH_CLIENT_ID = "30b47abc588f4731b0556e5280874f46"; 
        //public string CCPSSO_AUTH_CLIENT_SECRET = "Omj4FvniATMIDgTSJr8mQbFkK1ukPcD1khTfeTrF";
        //public string CCPSSO_AUTH_CLIENT_STATE = "evejimestate20170115";
        //public string CCPSSO_AUTH_PORT = "80";
        //public string CCPSSO_AUTH_SCOPES = "characterLocationRead%20characterNavigationWrite";
        #endregion

        public string CCPSSO_AUTH_CLIENT_ID = "";
        public string CCPSSO_AUTH_CLIENT_SECRET = "";
        public string CCPSSO_AUTH_CLIENT_STATE = "";
        public string CCPSSO_AUTH_PORT = "";
        public string CCPSSO_AUTH_SCOPES = "";

        public Settings()
        {
            #region Write setting Old version
            //var json = JsonConvert.SerializeObject(this, Formatting.Indented);
            //File.WriteAllText(@"settings.txt", json);
            #endregion

            try
            {
                using (var reader = new StreamReader("settings.txt"))
                {
                    dynamic data = JObject.Parse(reader.ReadToEnd());

                    Version = data.Version;
                    CCPSSO_AUTH_CLIENT_ID       = data.CCPSSO_AUTH_CLIENT_ID;
                    CCPSSO_AUTH_CLIENT_SECRET   = data.CCPSSO_AUTH_CLIENT_SECRET;
                    CCPSSO_AUTH_CLIENT_STATE    = data.CCPSSO_AUTH_CLIENT_STATE;
                    CCPSSO_AUTH_PORT            = data.CCPSSO_AUTH_PORT;
                    CCPSSO_AUTH_SCOPES          = data.CCPSSO_AUTH_SCOPES;
                }
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("[Settings.Settings] Critical error. Exception {0}", ex);
                throw;
            }

            
        }
    }
}
