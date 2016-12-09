using System;
using System.Drawing;
using System.Linq;
using WHL.BLL;
using WHL.UiTools;

namespace WHL
{
    public partial class WindowMonitoring
    {
        private void OpenSolarSystemPanel()
        {
            if (Global.Pilots.Count() == 0 || Global.Pilots.Selected == null || Global.Pilots.Selected.Location == null || Global.Pilots.Selected.Location.System == "unknown") return;

            HideAllContainers();
            containerSolarSystemInformation.Visible = true;

            ContainerTabs.Activate("Location");


            pnlContainer.BringToFront();

            pnlContainerWebBrowser.BringToFront();
            pnlAuthirization.BringToFront();
            pnlPilotsInformation.BringToFront();
            pnlBookmarksAndSignatures.BringToFront();
            pnlContainerSignatures.BringToFront();

            ResizeWindow();


            containerSolarSystemInformation.BackColor = Color.Black;
            containerSolarSystemInformation.Location = new Point(pnlContainer.Location.X + 6, pnlContainer.Location.Y + 6);
            containerSolarSystemInformation.BringToFront();

        }

        private void RefreshSolarSystemInformation()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(RefreshSolarSystemInformation));
            }

            if (Global.Pilots.Count() > 0)
            {
                txtSolarSystemName.Text = Global.Pilots.Selected.Location.System;
                txtSolarSystemClass.Text = Global.Pilots.Selected.Location.Class;
                txtSolarSystemEffect.Text = Global.Pilots.Selected.Location.Effect;
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
        }
    }
}
