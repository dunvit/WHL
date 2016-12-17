using System;
using System.Windows.Forms;
using WHL.BLL;
using WHL.UiTools;

namespace WHL.WhlControls
{
    public partial class whlSolarSystem : UserControl
    {
        public StarSystemEntity SolarSystem { get; set; }

        private ToolTip toolTip1 = new ToolTip();
        private ToolTip toolTip2 = new ToolTip();

        public whlSolarSystem()
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
            SolarSystem = location.Clone() as StarSystemEntity;;

            txtSolarSystemName.Text = Global.Pilots.Selected.Location.System;
            txtSolarSystemClass.Text = Global.Pilots.Selected.Location.Class;
            txtSolarSystemEffect.Text = Global.Pilots.Selected.Location.Effect.Trim();
            txtSolarSystemRegion.Text = Global.Pilots.Selected.Location.Region.Replace(" Unknown (", "").Replace(")", "");
            txtSolarSystemConstellation.Text = Global.Pilots.Selected.Location.Constelation.Replace(" Unknown (", "").Replace(")", "");

            txtSolarSystemStaticI.Text = "";
            txtSolarSystemStaticII.Text = "";

            txtSolarSystemStaticIData.Text = "";
            txtSolarSystemStaticIIData.Text = "";

            txtSolarSystemStaticI.Visible = false;
            txtSolarSystemStaticII.Visible = false;
            txtSolarSystemStaticIData.Visible = false;
            txtSolarSystemStaticIIData.Visible = false;

            if (string.IsNullOrEmpty(Global.Pilots.Selected.Location.Static) == false)
            {
                var wormholeI = Global.Space.Wormholes[Global.Pilots.Selected.Location.Static.Trim()];

                txtSolarSystemStaticI.Text = wormholeI.Name;
                txtSolarSystemStaticI.Visible = true;
                txtSolarSystemStaticI.ForeColor = Tools.GetColorBySolarSystem(wormholeI.LeadsTo);
                txtSolarSystemStaticIData.Text = wormholeI.LeadsTo;
                txtSolarSystemStaticIData.Visible = true;

                toolTip1.SetToolTip(txtSolarSystemStaticI, "Max Stable Mass=" + wormholeI.TotalMass + "\r\nMax Jump  Mass=" + wormholeI.SingleMass);

            }

            if (string.IsNullOrEmpty(Global.Pilots.Selected.Location.Static2) == false)
            {
                var wormholeII = Global.Space.Wormholes[Global.Pilots.Selected.Location.Static2.Trim()];


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
                Global.Browser.BrowserUrlExecute("https://zkillboard.com/system/" + Global.Pilots.Selected.Location.Id.Replace("J", "") + "/");
        }

        private void Event_ShowSuperpute(object sender, EventArgs e)
        {
            if (SolarSystem != null && SolarSystem.System != "unknown")
                Global.Browser.BrowserUrlExecute("http://superpute.com/system/" + Global.Pilots.Selected.Location.System + "");
        }

        private void Event_ShowEllatha(object sender, EventArgs e)
        {
            if (SolarSystem != null && SolarSystem.System != "unknown")
            {
                if (Global.Pilots.Selected.Location.System.Contains("J") == false)
                {
                    MessageBox.Show("Ellatha only for W-Space systems");
                    return;
                }

                Global.Browser.BrowserUrlExecute("http://www.ellatha.com/eve/WormholeSystemview.asp?key=" + Global.Pilots.Selected.Location.System.Replace("J", "") + "");
            }
        }

        private void Event_ShowDotlan(object sender, EventArgs e)
        {
            if (SolarSystem != null && SolarSystem.System != "unknown" )
                Global.Browser.BrowserUrlExecute("http://evemaps.dotlan.net/system/" + Global.Pilots.Selected.Location.System + "");
        }
    }
}
