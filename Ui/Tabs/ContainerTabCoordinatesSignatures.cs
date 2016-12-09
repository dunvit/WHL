using System;
using System.Drawing;

namespace WHL
{
    public partial class WindowMonitoring
    {
        private void OpenCoordinatesSignaturesPanel(object sender, EventArgs e)
        {
            ContainerTabs.Activate("Bookmarks");

            HideAllContainers();
            containerBookmarksAndSignatures.Visible = true;

            pnlContainer.BringToFront();

            pnlContainerSignatures.BringToFront();
            pnlContainerWebBrowser.BringToFront();
            pnlAuthirization.BringToFront();
            pnlSolarSystemInformation.BringToFront();
            pnlPilotsInformation.BringToFront();

            ResizeWindow();

            containerBookmarksAndSignatures.BackColor = Color.Black;
            containerBookmarksAndSignatures.Location = new Point(pnlContainer.Location.X + 6, pnlContainer.Location.Y + 6);
            containerBookmarksAndSignatures.BringToFront();
        }
    }
}
