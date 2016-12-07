using System.Drawing;

namespace WHL
{
    public partial class WindowMonitoring
    {
        private void ResizeWindow()
        {
            Size = ContainerTabs.Active().Size;

            ResizeButtonsPanelLocation();

            Refresh();
        }

        private void ResizeButtonsPanelLocation()
        {
            TitleBar.Width = Width;

            

            if (ContainerTabs.Active().Name == "WebBrowser")
            {
                ResizeWebBrowser();
            }

            if (_windowIsMinimaze == false)
            {
                VersionBar.Width = Width - 8;
                VersionBar.Location = new Point(10, Height - 38);
            }

            pnlContainer.Width = Width - 18;
            pnlContainer.Height = Height - 63 - 42;

            Refresh();
        }

        private void ResizeWebBrowser()
        {
            if (_windowIsMinimaze) return;

            containerWebBrowserPanel.Width = Width - 26;
            panel1.Width = containerWebBrowserPanel.Width;
            webBrowser1.Width = panel1.Width;

            containerWebBrowserPanel.Height = Height - 63 - 11 - 40;
            panel1.Height = containerWebBrowserPanel.Height - 30;
            webBrowser1.Height = panel1.Height;

            txtUrl.Width = containerWebBrowserPanel.Width - txtUrl.Location.X ;

            loadingGif.Location = new Point(containerWebBrowserPanel.Width / 2 - loadingGif.Width / 2, containerWebBrowserPanel.Height / 2 - loadingGif.Height / 2);

            ContainerTabs.Active().Size = new Size(Width, Height);
        }

        private void HideAllContainers()
        {
            
            containerBookmarksAndSignatures.Visible = false;
            containerPilotInfo.Visible = false;
            containerSolarSystemInformation.Visible = false;
            containerAuthirization.Visible = false;
            containerWebBrowserPanel.Visible = false;
            containerSignatures.Visible = false;
        }
    }
}
