using System;
using System.Drawing;
using System.Net;
using log4net;

namespace WHL.BLL
{
    public class PilotEntity
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(CrestAuthorization));
        public long Id { get; set; }

        public string Name { get; set; }

        public Image Portrait { get; set; }

        public StarSystemEntity Location { get; set; }

        public CrestAuthorization CrestData { get; set; }

        private DateTime LastTokenUpdate;

        public void Initialization(string token)
        {
            Log.DebugFormat("[Pilot.Initialization] starting for token = {0}", token);

            CrestData = new CrestAuthorization(token);

            dynamic data = CrestData.ObtainingCharacterData();

            Id = data.CharacterID;
            Name = data.CharacterName;

            LoadLocationInfo();

            LoadCharacterInfo();

            LastTokenUpdate = DateTime.Now;

        }

        private void LoadCharacterInfo()
        {
            Log.DebugFormat("[Pilot.LoadCharacterInfo] starting for Id = {0}", Id);

            dynamic characterInfo = CrestData.GetCharacterInfo(Id);

            var portraitAddress = characterInfo.SelectToken("portrait.64x64.href").Value;

            var request = WebRequest.Create(portraitAddress);

            using (var response = request.GetResponse())
            using (var stream = response.GetResponseStream())
            {
                Portrait = Image.FromStream(stream);
            }
        }

        private void LoadLocationInfo()
        {
            Log.DebugFormat("[Pilot.LoadLocationInfo] starting for Id = {0}", Id);

            try
            {
                if (Location == null) Location = new StarSystemEntity();

                dynamic locationInfo = CrestData.GetLocation(Id);

                if (Location.System == locationInfo.SelectToken("solarSystem.name").Value) return;

                if (Global.Space.SolarSystems.ContainsKey(locationInfo.SelectToken("solarSystem.name").Value.ToString()))
                {
                    var location = (StarSystemEntity)Global.Space.SolarSystems[locationInfo.SelectToken("solarSystem.name").Value.ToString()];

                    Location = location.Clone() as StarSystemEntity;
                   
                    Location.Id = locationInfo.SelectToken("solarSystem.id").Value.ToString();
                }
                else
                {
                    Location.Region = "";
                    Location.Constelation = "";
                    Location.Effect = "";
                    Location.Class = "";
                    Location.Static2 = "";
                    Location.Static = "";

                    Location.Id = locationInfo.SelectToken("solarSystem.id").Value.ToString();

                    Location.System = locationInfo.SelectToken("solarSystem.name").Value;

                }
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("[Pilot.LoadLocationInfo] pilot Id = {0} not login in game. Exception {1}", Id, ex);

                Location.System = "unknown";
            }
            
        }

        private bool isBusy = false;

        public void RefreshInfo()
        {
            Log.DebugFormat("[Pilot.RefreshInfo] starting for Id = {0}", Id);

            var span = DateTime.Now - LastTokenUpdate;
            var ms = (int)span.TotalMilliseconds;

            if (ms > CrestData.ExpiresIn * 1000 - 20000)
            {
                CrestData.Refresh();

                LastTokenUpdate = DateTime.Now;

                Log.DebugFormat("[Pilot.RefreshInfo] set LastTokenUpdate for Id = {0}", Id);
            }

            if (isBusy == false)
            {
                isBusy = true;

                LoadLocationInfo();

                isBusy = false;
            }


        }

        
    }
}
