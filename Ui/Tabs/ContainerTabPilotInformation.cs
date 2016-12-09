using System.Drawing;

namespace WHL
{
    public partial class WindowMonitoring
    {
        private void OpenPilotInformationPanel()
        {
            ContainerTabs.Activate("Pilots");

            HideAllContainers();
            containerPilotInfo.Visible = true;

            pnlContainer.BringToFront();

            pnlContainerWebBrowser.BringToFront();
            pnlAuthirization.BringToFront();
            pnlSolarSystemInformation.BringToFront();
            pnlBookmarksAndSignatures.BringToFront();
            pnlContainerSignatures.BringToFront();

            ResizeWindow();

            containerPilotInfo.BackColor = Color.Black;
            containerPilotInfo.Location = new Point(pnlContainer.Location.X + 6, pnlContainer.Location.Y + 6);
            containerPilotInfo.BringToFront();
        }
    }
}
