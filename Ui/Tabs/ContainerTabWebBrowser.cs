using System.Drawing;

namespace WHL
{
    public partial class WindowMonitoring
    {
        

        private void OpenWebBrowserPanel()
        {
            containerWebBrowserPanel.Visible = true;

            containerWebBrowserPanel.Location = new Point(pnlContainer.Location.X + 2, pnlContainer.Location.Y + 2);

            HideAllContainers();
            containerWebBrowserPanel.Visible = true;

            pnlContainerWebBrowser.BringToFront();

            pnlContainer.BringToFront();
            pnlAuthirization.BringToFront();
            pnlSolarSystemInformation.BringToFront();
            pnlPilotsInformation.BringToFront();
            pnlBookmarksAndSignatures.BringToFront();
            pnlContainerSignatures.BringToFront();

            containerWebBrowserPanel.BringToFront();

            ContainerTabs.Activate("WebBrowser");

            panel1.BringToFront();

            ResizeWindow();

            ResizeWebBrowser();

            containerWebBrowserPanel.Visible = true;
        }
    }
}
