using System;
using System.Drawing;
using System.Windows.Forms;
using log4net;
using WHL.Browser;

namespace WHL.WhlControls
{
    public partial class whlBrowser : UserControl
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(whlAuthorization));

        private readonly OpenWebBrowser _runOpenWebBrowser;

        public History History { get; set; }

        public whlBrowser(OpenWebBrowser openWebBrowserFunction)
        {
            InitializeComponent();

            History = new History();

            _runOpenWebBrowser = openWebBrowserFunction;
        }

        public void BrowserUrlExecute(string url)
        {
            if (!CheckIsNeedUpdateWebBrowser(url)) return;

            if (_runOpenWebBrowser != null)
            {
                try
                {
                    _runOpenWebBrowser();
                }
                catch (Exception ex)
                {
                    Log.Error("[Browser.History.History] Critical error in load history. Exception = " + ex);
                }
            }

            History.Add(url);

            if (url.Trim() == "http://") return;

            webBrowser1.Visible = false;

            loadingGif.Visible = true;

            webBrowser1.Url = new Uri(url);
            
            txtUrl.Text = url;

            txtUrl.Focus();
        }

        private void BrowserUrlRefresh(string url)
        {
            loadingGif.Visible = true;

            if (webBrowser1.Url == null) return;

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

            txtUrl.Focus();
        }

        public void ResizeWebBrowser(int width, int height)
        {
            Width = width - 24;

            Height = height - 110;
            
            webBrowser1.Width = Width - 24;
            
            webBrowser1.Height = Height - 62;

            txtUrl.Width = Width - 58 - txtUrl.Location.X;

            loadingGif.Location = new Point((Width - 24) / 2 - loadingGif.Width / 2, (Height-62) / 2 - loadingGif.Height / 2);
        }

        public void BrowserOpen()
        {
            loadingGif.Visible = false;

            if (webBrowser1.Url == null)
            {
                webBrowser1.Visible = false;
            }
            else
            {
                webBrowser1.Visible = true;
            }

            txtUrl.Focus();
        }

        public bool CheckIsNeedUpdateWebBrowser(string url)
        {
            if (webBrowser1.Url == null) return true;

            if (url != webBrowser1.Url.AbsoluteUri) return true;

            return false;
        }

        private void BrowserCommandExecute_Click(object sender, EventArgs e)
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

        private void BrowserCommandRefresh_Click(object sender, EventArgs e)
        {
            if (txtUrl.Text.StartsWith("http"))
            {
                BrowserUrlRefresh(txtUrl.Text);
            }
            else
            {
                BrowserUrlRefresh("http://" + txtUrl.Text);
            }
        }

        private void BrowserCommandForward_Click(object sender, EventArgs e)
        {
            var url = History.Next();

            if (string.IsNullOrEmpty(url)) return;

            BrowserUrlExecute(url);
        }

        private void BrowserCommandBack_Click(object sender, EventArgs e)
        {
            var url = History.Previous();

            if (string.IsNullOrEmpty(url)) return;

            BrowserUrlExecute(url);
        }

        private void txtUrl_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
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
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            loadingGif.Visible = false;
            webBrowser1.Visible = true;
        }

        private void cmdBlank_Click(object sender, EventArgs e)
        {
            
        }

        private void cmdBlank_Click_1(object sender, EventArgs e)
        {
            webBrowser1.Navigate("about:blank");
        }
    }
}
