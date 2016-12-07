using System;
using System.Drawing;
using System.Linq;

namespace WHL
{
    public partial class WindowMonitoring
    {
        private void OpenSignaturesPanel()
        {
            ContainerTabs.Activate("Signatures");

            HideAllContainers();
            containerSignatures.Visible = true;

            pnlContainer.BringToFront();

            pnlContainerWebBrowser.BringToFront();
            pnlPilotsInformation.BringToFront();
            pnlSolarSystemInformation.BringToFront();
            pnlBookmarksAndSignatures.BringToFront();
            pnlAuthirization.BringToFront();

            if (Global.Pilots.Any())
            {
                //crlPilotPortrait.Image = Global.Pilots.Selected.Portrait;
                //crlPilotPortrait.Refresh();

                //crlPilotPortrait.Visible = true;

                //lblAuthorizationInfo.Text = TextAfterAuthorizationInfo + Environment.NewLine + Environment.NewLine + TextAuthorizationInfo;
            }

            ResizeWindow();

            containerSignatures.BackColor = Color.Black;
            containerSignatures.Location = new Point(pnlContainer.Location.X + 2, pnlContainer.Location.Y + 2);
            containerSignatures.BringToFront();
        }
    }
}
