using System;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using log4net;
using WHL.Browser;

namespace WHL.WhlControls
{
    public partial class whlBrowser : UserControl
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(whlAuthorization));

        private readonly OpenWebBrowser _runOpenWebBrowser;

        #region ToolTips
        private readonly ToolTip _toolTipForBookmarkButton = new ToolTip();
        private readonly ToolTip _toolTipForHistoryBackButton = new ToolTip();
        private readonly ToolTip _toolTipForHistoryNextButton = new ToolTip();
        private readonly ToolTip _toolTipForRefreshButton = new ToolTip();
        private readonly ToolTip _toolTipForFavoritsButton = new ToolTip();
        private readonly ToolTip _toolTipForNavigateToBlankButton = new ToolTip();
        #endregion

        public History History { get; set; }

        public Bookmarks Bookmarks { get; set; }

        public whlBrowser(OpenWebBrowser openWebBrowserFunction)
        {
            InitializeComponent();

            History = new History();

            Bookmarks = new Bookmarks();

            _runOpenWebBrowser = openWebBrowserFunction;

            #region ToolTips
            _toolTipForBookmarkButton.SetToolTip(cmdBookmark, "Add to bookmarks");
            _toolTipForHistoryBackButton.SetToolTip(BrowserCommandBack, "Click to go back, hold to see history");
            _toolTipForHistoryNextButton.SetToolTip(BrowserCommandForward, "Click to go forward, hold to see history");
            _toolTipForRefreshButton.SetToolTip(BrowserCommandRefresh, "Reload this page");
            _toolTipForFavoritsButton.SetToolTip(cmdFavorits, "Bookmarks");
            _toolTipForNavigateToBlankButton.SetToolTip(cmdBlank, "Click to go to blank page");
            #endregion

            cmdFavorits.ContextMenu = BuildContextMenuForFavorites();
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

            webBrowser1.Url = new Uri(url);

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

            txtUrl.Width = Width - 8 - txtUrl.Location.X;

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

            BuildContextMenuFromHistory();

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
            if (webBrowser1.Url == null) return;

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

            var title = Regex.Match(webBrowser1.DocumentText, @"\<title\b[^>]*\>\s*(?<Title>[\s\S]*?)\</title\>", RegexOptions.IgnoreCase).Groups["Title"].Value;

            History.Add(webBrowser1.Url.AbsoluteUri);

            History.UpdateTitle(title);

            txtUrl.Text = webBrowser1.Url.AbsoluteUri;

            BuildContextMenuFromHistory();

            IsUrlInBookmarks();
        }

        private void cmdFavorits_Click(object sender, EventArgs e)
        {
            var cmFavorits = BuildContextMenuForFavorites();

            cmFavorits.Show(cmdFavorits, cmdFavorits.PointToClient(Cursor.Position));
        }

        private void Event_NavigateToBlank(object sender, EventArgs e)
        {
            webBrowser1.Navigate("about:blank");
        }

        private ContextMenu BuildContextMenuForFavorites()
        {
            var cmFavorits = new ContextMenu();

            foreach (var address in Bookmarks.List.Values.ToList().OrderByDescending(k => k.Id).ToList())
            {
                var menuItem = new MenuItem(address.Title, (sender, args) => BrowserUrlExecute(address.Url));

                cmFavorits.MenuItems.Add(menuItem);
            }

            return cmFavorits;
        }

        private void BuildContextMenuFromHistory()
        {
            var cmHistoryBack = new ContextMenu();

            for (var i = 1; i <= 10; i++)
            {
                var index = History.CurrentIndex - i;

                if (History.List.ContainsKey(index))
                {
                    var address = History.List[index];

                    var menuItem = new MenuItem(address.Title, (sender, args) => BrowserUrlExecute(address.Url));

                    cmHistoryBack.MenuItems.Add(menuItem);
                }
            }

            BrowserCommandBack.ContextMenu = cmHistoryBack;

            var cmHistoryForvard = new ContextMenu();

            for (var i = 1; i <= 10; i++)
            {
                var index = History.CurrentIndex + i;

                if (History.List.ContainsKey(index))
                {
                    var address = History.List[index];

                    var menuItem = new MenuItem(address.Title, (sender, args) => BrowserUrlExecute(address.Url));

                    cmHistoryForvard.MenuItems.Add(menuItem);
                }
            }

            BrowserCommandForward.ContextMenu = cmHistoryForvard;


            
        }


        private void IsUrlInBookmarks()
        {
            if (webBrowser1.Url == null) return;

            if (Bookmarks.IsExist(webBrowser1.Url.AbsoluteUri) == false)
            {
                cmdBookmark.Image = Properties.Resources.not_bookmark;
                _toolTipForBookmarkButton.SetToolTip(cmdBookmark, "Add to bookmarks");
            }
            else
            {
                cmdBookmark.Image = Properties.Resources.bookmark;
                _toolTipForBookmarkButton.SetToolTip(cmdBookmark, "Remove from bookmarks");
            }

            cmdFavorits.ContextMenu = BuildContextMenuForFavorites();
        }

        private void Event_ClickBookmarkButton(object sender, EventArgs e)
        {
            var address = History.GetCurrentAddress();

            if (address == null) return;

            if (Bookmarks.IsExist(address.Url))
            {
                Bookmarks.Remove(address.Url);
            }
            else
            {
                Bookmarks.Add(address);
            }

            IsUrlInBookmarks();
        }
    }
}
