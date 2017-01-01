using System;
using System.Diagnostics;
using System.Windows.Forms;
using log4net;
using WHL.BLL;
using WHL.UiTools;

namespace WHL.WhlControls
{
    public partial class whlSolarSystemOffline : UserControl
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(whlSolarSystemOffline));

        public StarSystemEntity SolarSystem { get; set; }


        private ToolTip toolTip1 = new ToolTip();
        private ToolTip toolTip2 = new ToolTip();

        public whlSolarSystemOffline()
        {
            InitializeComponent();

            toolTip1.AutoPopDelay = 5000;
            toolTip1.InitialDelay = 1000;
            toolTip1.ReshowDelay = 500;
            toolTip1.ShowAlways = true;

            toolTip2.AutoPopDelay = 5000;
            toolTip2.InitialDelay = 1000;
            toolTip2.ReshowDelay = 500;
            toolTip2.ShowAlways = true;
        }

        public void RefreshSolarSystem(StarSystemEntity location)
        {
            SolarSystem = location.Clone() as StarSystemEntity;

            txtSolarSystemClass.Text = location.Class;
            txtSolarSystemEffect.Text = location.Effect.Trim();
            txtSolarSystemRegion.Text = location.Region.Replace(" Unknown (", "").Replace(")", "");
            txtSolarSystemConstellation.Text = location.Constelation.Replace(" Unknown (", "").Replace(")", "");

            txtSolarSystemStaticI.Text = "";
            txtSolarSystemStaticII.Text = "";

            txtSolarSystemStaticIData.Text = "";
            txtSolarSystemStaticIIData.Text = "";

            txtSolarSystemStaticI.Visible = false;
            txtSolarSystemStaticII.Visible = false;
            txtSolarSystemStaticIData.Visible = false;
            txtSolarSystemStaticIIData.Visible = false;

            if (string.IsNullOrEmpty(location.Static) == false)
            {
                var wormholeI = Global.Space.Wormholes[location.Static.Trim()];

                txtSolarSystemStaticI.Text = wormholeI.Name;
                txtSolarSystemStaticI.Visible = true;
                txtSolarSystemStaticI.ForeColor = Tools.GetColorBySolarSystem(wormholeI.LeadsTo);
                txtSolarSystemStaticIData.Text = wormholeI.LeadsTo;
                txtSolarSystemStaticIData.Visible = true;

                toolTip1.SetToolTip(txtSolarSystemStaticI, "Max Stable Mass=" + wormholeI.TotalMass + "\r\nMax Jump  Mass=" + wormholeI.SingleMass);

            }

            if (string.IsNullOrEmpty(location.Static2) == false)
            {
                var wormholeII = Global.Space.Wormholes[location.Static2.Trim()];


                txtSolarSystemStaticII.Text = wormholeII.Name;
                txtSolarSystemStaticII.Visible = true;
                txtSolarSystemStaticII.ForeColor = Tools.GetColorBySolarSystem(wormholeII.LeadsTo);
                txtSolarSystemStaticIIData.Text = wormholeII.LeadsTo;
                txtSolarSystemStaticIIData.Visible = true;

                toolTip2.SetToolTip(txtSolarSystemStaticII, "Max Stable Mass=" + wormholeII.TotalMass + "\r\nMax Jump  Mass=" + wormholeII.SingleMass);
            }
        }




        private void Event_ShowZkillboard(object sender, EventArgs e)
        {
            if (SolarSystem != null && SolarSystem.System != "unknown")
                Global.Browser.BrowserUrlExecute("https://zkillboard.com/system/" + SolarSystem.Id.Replace("J", "") + "/");
        }

        private void Event_ShowSuperpute(object sender, EventArgs e)
        {
            if (SolarSystem != null && SolarSystem.System != "unknown")
                Global.Browser.BrowserUrlExecute("http://superpute.com/system/" + SolarSystem.System + "");
        }

        private void Event_ShowEllatha(object sender, EventArgs e)
        {
            if (SolarSystem != null && SolarSystem.System != "unknown")
            {
                if (SolarSystem.System.Contains("J") == false)
                {
                    MessageBox.Show(@"Ellatha only for W-Space systems");
                    return;
                }

                Global.Browser.BrowserUrlExecute("http://www.ellatha.com/eve/WormholeSystemview.asp?key=" + SolarSystem.System.Replace("J", "") + "");
            }
        }

        private void Event_ShowDotlan(object sender, EventArgs e)
        {
            if (SolarSystem != null && SolarSystem.System != "unknown" )
                Global.Browser.BrowserUrlExecute("http://evemaps.dotlan.net/system/" + SolarSystem.System + "");
        }

        private void Event_TripwireShow(object sender, EventArgs e)
        {
            if (SolarSystem != null && SolarSystem.System != "unknown")
                Process.Start("https://tripwire.eve-apps.com/?system=" + SolarSystem.System + "");
        }

        private void Event_AnalizeSolarSystem(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtSolarSystem.Text)) return;

            LoadLocationInfo(txtSolarSystem.Text);

            RefreshSolarSystem(SolarSystem);
        }

        

        private void LoadLocationInfo(string solarSystemName)
        {
            Log.DebugFormat("[whlSolarSystemOffline.LoadLocationInfo] starting for solarSystemName = {0}", solarSystemName);

            try
            {
                if (SolarSystem == null) SolarSystem = new StarSystemEntity();

                if (Global.Space.SolarSystems.ContainsKey(solarSystemName))
                {
                    var location = Global.Space.SolarSystems[solarSystemName];

                    SolarSystem = location.Clone() as StarSystemEntity;

                    if (SolarSystem != null)
                    {
                        SolarSystem.Id = solarSystemName;
                    }
                }
                else
                {
                    SolarSystem.Region = "";
                    SolarSystem.Constelation = "";
                    SolarSystem.Effect = "";
                    SolarSystem.Class = "";
                    SolarSystem.Static2 = "";
                    SolarSystem.Static = "";

                    SolarSystem.Id = solarSystemName;

                    SolarSystem.System = solarSystemName;

                }
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("[whlSolarSystemOffline.LoadLocationInfo] Critical error. Exception {0}", ex);

                if (SolarSystem != null)
                {
                    SolarSystem.System = "unknown";
                }
            }
        }
    }
}
