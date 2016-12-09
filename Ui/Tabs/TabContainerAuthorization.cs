using System;
using System.Drawing;
using System.Linq;

namespace WHL.Ui
{
    public static class TabContainerAuthorization
    {
        public static void OpenAuthorizationPanel(WindowMonitoring window)
        {
            window.ContainerTabs.Activate("Authorization");

            
            window.pnlContainer.BringToFront();

            window.pnlContainerWebBrowser.BringToFront();
            window.pnlPilotsInformation.BringToFront();
            window.pnlSolarSystemInformation.BringToFront();
            window.pnlBookmarksAndSignatures.BringToFront();

            if (Global.Pilots.Any())
            {
                window.crlPilotPortrait.Image = Global.Pilots.Selected.Portrait;
                window.crlPilotPortrait.Refresh();

                window.crlPilotPortrait.Visible = true;

                window.lblAuthorizationInfo.Text = WindowMonitoring.TextAfterAuthorizationInfo + Environment.NewLine + Environment.NewLine + WindowMonitoring.TextAuthorizationInfo;
            }

            window.containerAuthirization.BackColor = Color.Black;
            window.containerAuthirization.Location = new Point(window.pnlContainer.Location.X + 6, window.pnlContainer.Location.Y + 6);
            window.containerAuthirization.BringToFront();
        }
    }
}
