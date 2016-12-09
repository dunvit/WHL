using System;
using System.Drawing;
using System.Linq;

namespace WHL
{
    public partial class WindowMonitoring
    {
        private void OpenAuthorizationPanel()
        {
            ContainerTabs.Activate("Authorization");

            HideAllContainers();
            containerAuthirization.Visible = true;

            pnlContainer.BringToFront();

            pnlContainerSignatures.BringToFront();
            pnlContainerWebBrowser.BringToFront();
            pnlPilotsInformation.BringToFront();
            pnlSolarSystemInformation.BringToFront();
            pnlBookmarksAndSignatures.BringToFront();

            if (Global.Pilots.Selected != null)
            {
                crlPilotPortrait.Image = Global.Pilots.Selected.Portrait;
                crlPilotPortrait.Refresh();

                crlPilotPortrait.Visible = true;

                lblAuthorizationInfo.Text = TextAfterAuthorizationInfo + Environment.NewLine + Environment.NewLine + TextAuthorizationInfo;
            }

            ResizeWindow();

            containerAuthirization.BackColor = Color.Black;
            containerAuthirization.Location = new Point(pnlContainer.Location.X + 6, pnlContainer.Location.Y + 6);
            containerAuthirization.BringToFront();
        }
    }
}
