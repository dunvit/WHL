using System;
using System.Windows.Forms;
using WHL.Properties;

namespace WHL
{
    public partial class WindowMonitoring
    {
        private string _previousUrl = "";

        private void BrowserUrlExecute(string url)
        {

            Global.Browser.History.Add(url);

            if (webBrowser1.Url != null)
            {
                _previousUrl = webBrowser1.Url.AbsoluteUri;
            }

            if (!CheckIsNeedUpdateWebBrowser(url))return;

            if (url.Trim() == "http://") return;

            loadingGif.Visible = true;

            webBrowser1.Url = new Uri(url);
            webBrowser1.Visible = false;
            txtUrl.Text = url;
                
            OpenWebBrowserPanel();
            ResizeWindow();

            txtUrl.Focus();
        }

        private void BrowserUrlRefresh(string url)
        {
            

            loadingGif.Visible = true;

            if (url == webBrowser1.Url.AbsoluteUri)
            {
                webBrowser1.Refresh();
            }
            else
            {
                webBrowser1.Url = new Uri(url);
            }
            
            webBrowser1.Visible = false;
            txtUrl.Text = url;

            OpenWebBrowserPanel();
            ResizeWindow();

            txtUrl.Focus();
        }

        private void BrowserOpen()
        {
            loadingGif.Visible = false;

            webBrowser1.Visible = true;

            OpenWebBrowserPanel();
            ResizeWindow();

            txtUrl.Focus();
        }

        private bool CheckIsNeedUpdateWebBrowser(string url)
        {
            if (webBrowser1.Url == null) return true;

            if (url != webBrowser1.Url.AbsoluteUri) return true;

            return false;
        }

        #region Events

        private void Event_ExecuteUrlInWhlBrowser(object sender, EventArgs e)
        {
            if (txtUrl.Text.StartsWith("http"))
            {
                BrowserUrlExecute(txtUrl.Text);
            }
            else
            {
                BrowserUrlExecute("http://" + txtUrl.Text);
            }
        }

        private void Event_OpenBrowserContainer(object sender, EventArgs e)
        {
            ContainerTabs.Activate("WebBrowser");

            var urlFromClipboard = Clipboard.GetText();

            if (urlFromClipboard.StartsWith("http"))
            {
                BrowserUrlExecute(urlFromClipboard);
            }
            else
            {
                BrowserOpen();
            }

            _windowIsMinimaze = false;
            cmdMinimazeRestore.Image = Resources.minimize;
        }

        private void label11_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtSelectedPilotName.Text))
            {
                return;
            }

            var characterId = Global.Infrastructure.EveXmlApi.GetPilotIdByName(txtSelectedPilotName.Text.Trim());

            if (crlPilotsHistory.Items.Contains(txtSelectedPilotName.Text.Trim()) == false)
            {
                crlPilotsHistory.Items.Add(txtSelectedPilotName.Text.Trim());
            }

            BrowserUrlExecute("https://zkillboard.com/character/" + characterId + "/");
        }

        private void ShowEllatha()
        {
            if (Global.Pilots.Selected != null && Global.Pilots.Selected.Location != null && Global.Pilots.Selected.Location.System != "unknown")
            {
                if (Global.Pilots.Selected.Location.System.Contains("J") == false)
                {
                    MessageBox.Show("Ellatha only for W-Space systems");
                    return;
                }

                BrowserUrlExecute("http://www.ellatha.com/eve/WormholeSystemview.asp?key=" + Global.Pilots.Selected.Location.System.Replace("J", "") + "");
            }
        }

        private void cmdShowDotlanPanel_Click(object sender, EventArgs e)
        {
            ShowDotlan();
        }

        private void ShowDotlan()
        {
            if (Global.Pilots.Selected != null && Global.Pilots.Selected.Location != null && Global.Pilots.Selected.Location.System != "unknown")
            {
                BrowserUrlExecute("http://evemaps.dotlan.net/system/" + Global.Pilots.Selected.Location.System + "");
            }
        }

        private void cmdShowSuperputePanel_Click(object sender, EventArgs e)
        {
            ShowSuperpute();
        }

        private void cmdShowEllathaPanel_Click(object sender, EventArgs e)
        {
            ShowEllatha();
        }

        private void label10_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtSelectedPilotName.Text))
            {
                return;
            }

            BrowserUrlExecute("http://eve-hunt.net/hunt/" + txtSelectedPilotName.Text + "/");

            if (crlPilotsHistory.Items.Contains(txtSelectedPilotName.Text.Trim()) == false)
            {
                crlPilotsHistory.Items.Add(txtSelectedPilotName.Text.Trim());
            }
        }

        private void Event_WebBrowserCompleteLoadinf(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            webBrowser1.Visible = true;
        }

        #endregion
    }
}
