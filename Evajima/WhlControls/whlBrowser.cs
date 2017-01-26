using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using log4net;
using CefSharp;
using CefSharp.WinForms;
using CefSharp.WinForms.Internals;
using EveJimaCore.Browser;

namespace EveJimaCore.WhlControls
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

            AddTab("about:blank");

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

        //public ChromiumWebBrowser chromeBrowser;



        public void BrowserUrlExecute(string url)
        {
            try
            {
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

                if (!CheckIsNeedUpdateWebBrowser(url)) return;

                if (url.Trim() == "http://") return;

                txtUrl.Text = url;

                txtUrl.Refresh();

                LoadUrl(url);

                txtUrl.Focus();
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("[whlBrowser.BrowserUrlExecute] Critical error. Exception {0}", ex);
            }

            
        }

        private void LoadUrl(string url)
        {
            try
            {
                ((ChromiumWebBrowser)(tabControl1.SelectedTab.Controls[0])).Load(url);
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("[whlBrowser.LoadUrl] Critical error. Exception {0}", ex);
            }
        }

        private void AddTab(string url, int? insertIndex = null)
        {
            try
            {
                browserTabControl.SuspendLayout();

                var browser = new ChromiumWebBrowser("about:blank");

                browser.LoadingStateChanged += OnBrowserLoadingStateChanged;
                browser.TitleChanged += OnBrowserTitleChanged;

                // Add it to the form and fill it to the form window.
                browser.Dock = DockStyle.Fill;

                var tabPage = new TabPage(url)
                {
                    Dock = DockStyle.Fill
                };

                //This call isn't required for the sample to work. 
                //It's sole purpose is to demonstrate that #553 has been resolved.
                browser.CreateControl();

                browser.Tag = tabPage;

                tabPage.Controls.Add(browser);

                if (insertIndex == null)
                {
                    tabControl1.TabPages.Add(tabPage);
                }
                else
                {
                    tabControl1.TabPages.Insert(insertIndex.Value, tabPage);
                }

                //Make newly created tab active
                tabControl1.SelectedTab = tabPage;

                browserTabControl.ResumeLayout(true);
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("[whlBrowser.AddTab] Critical error. Exception {0}", ex);
            }

            
        }

        public void Dispose()
        {
            Cef.Shutdown();
        }

        private void BrowserUrlRefresh(string url)
        {
            try
            {
                loadingGif.Visible = true;

                if (((ChromiumWebBrowser)(tabControl1.SelectedTab.Controls[0])).Address == null) return;

                LoadUrl(url);

                browserTabControl.Visible = false;
                txtUrl.Text = url;

                txtUrl.Focus();
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("[whlBrowser.BrowserUrlRefresh] Critical error. Exception {0}", ex);
            }
  
        }

        public void ResizeWebBrowser(int width, int height)
        {
            Width = width - 24;

            Height = height - 110;
            
            browserTabControl.Width = Width - 24;
            
            browserTabControl.Height = Height - 62;

            txtUrl.Width = Width - 8 - txtUrl.Location.X;

            loadingGif.Location = new Point((Width - 24) / 2 - loadingGif.Width / 2, (Height-62) / 2 - loadingGif.Height / 2);

            tabControl1.Size = new Size(browserTabControl.Width, browserTabControl.Height);
        }

        public void BrowserOpen()
        {
            try
            {
                loadingGif.Visible = false;

                if (((ChromiumWebBrowser)(tabControl1.SelectedTab.Controls[0])).Address == null)
                {
                    browserTabControl.Visible = false;
                }
                else
                {
                    browserTabControl.Visible = true;
                }

                BuildContextMenuFromHistory();

                txtUrl.Focus();
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("[whlBrowser.BrowserOpen] Critical error. Exception {0}", ex);
            }
            
        }

        public bool CheckIsNeedUpdateWebBrowser(string url)
        {
            try
            {
                if (((ChromiumWebBrowser)(tabControl1.SelectedTab.Controls[0])).Address == null) return true;

                if (url != ((ChromiumWebBrowser)(tabControl1.SelectedTab.Controls[0])).Address) return true;
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("[whlBrowser.CheckIsNeedUpdateWebBrowser] Critical error. Url {1} Exception {0}", ex, url);
            }

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
            try
            {
                if (((ChromiumWebBrowser)(tabControl1.SelectedTab.Controls[0])).Address == null) return;

                if (txtUrl.Text.StartsWith("http"))
                {
                    BrowserUrlRefresh(txtUrl.Text);
                }
                else
                {
                    BrowserUrlRefresh("http://" + txtUrl.Text);
                }
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("[whlBrowser.BrowserCommandRefresh_Click] Critical error. Exception {0}", ex);
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

        private void OnBrowserLoadingStateChanged(object sender, LoadingStateChangedEventArgs args)
        {
            try
            {
                this.InvokeOnUiThreadIfRequired(() => SetchromeBrowserVisible(!args.CanReload));

                this.InvokeOnUiThreadIfRequired(BuildContextMenuFromHistory);
                this.InvokeOnUiThreadIfRequired(IsUrlInBookmarks);
                BuildContextMenuFromHistory();

            }
            catch (Exception ex)
            {
                Log.ErrorFormat("[whlBrowser.OnBrowserLoadingStateChanged] Critical error. Exception {0}", ex);
            }

        }

        private void OnBrowserTitleChanged(object sender, TitleChangedEventArgs args)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => OnBrowserTitleChanged(sender, args)));
                return;
            }

            try
            {
                var tab = (TabPage)((ChromiumWebBrowser)sender).Tag;

                var title = args.Title;
                if (args.Title.Length > 20)
                {
                    title = args.Title.Substring(0, 20);
                }

                tab.Text = title;

                History.UpdateTitle(args.Title);
               
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("[whlBrowser.InvokeOnUiThreadIfRequired] Critical error. Exception {0}", ex);
            }
        }

        private void SetchromeBrowserVisible(bool b)
        {
            try
            {
                History.Add(((ChromiumWebBrowser)(tabControl1.SelectedTab.Controls[0])).Address);

                ((ChromiumWebBrowser)(tabControl1.SelectedTab.Controls[0])).Visible = true;

            }
            catch (Exception ex)
            {
                Log.ErrorFormat("[whlBrowser.SetchromeBrowserVisible] Critical error. Exception {0}", ex);
            }

            
        }


        private void cmdFavorits_Click(object sender, EventArgs e)
        {
            var cmFavorits = BuildContextMenuForFavorites();

            cmFavorits.Show(cmdFavorits, cmdFavorits.PointToClient(Cursor.Position));
        }

        private void Event_NavigateToBlank(object sender, EventArgs e)
        {
            LoadUrl("about:blank");
        }

        private ContextMenu BuildContextMenuForFavorites()
        {
            var cmFavorits = new ContextMenu();

            try
            {
                foreach (var address in Bookmarks.List.Values.ToList().OrderByDescending(k => k.Id).ToList())
                {
                    var menuItem = new MenuItem(address.Title, (sender, args) => BrowserUrlExecute(address.Url));

                    cmFavorits.MenuItems.Add(menuItem);
                }
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("[whlBrowser.BuildContextMenuForFavorites] Critical error. Exception {0}", ex);
            }

            return cmFavorits;
        }

        private void BuildContextMenuFromHistory()
        {
            try
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
            catch (Exception ex)
            {
                Log.ErrorFormat("[whlBrowser.BuildContextMenuFromHistory] Critical error. Exception {0}", ex);
            }
           
        }


        private void IsUrlInBookmarks()
        {
            try
            {
                if (((ChromiumWebBrowser)(tabControl1.SelectedTab.Controls[0])).Address == null) return;

                if (Bookmarks.IsExist(((ChromiumWebBrowser)(tabControl1.SelectedTab.Controls[0])).Address) == false)
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
            catch (Exception ex)
            {
                Log.ErrorFormat("[whlBrowser.IsUrlInBookmarks] Critical error. Exception {0}", ex);
            }

            
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

        private void Event_AddNewTab(object sender, EventArgs e)
        {
            AddTab("about:blank");
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                txtUrl.Text = ((ChromiumWebBrowser)(tabControl1.SelectedTab.Controls[0])).Address;
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("[whlBrowser.tabControl1_SelectedIndexChanged] Critical error. Exception {0}", ex);
            }
            
        }

        private void Event_CloseTab(object sender, EventArgs e)
        {
            if (tabControl1.Controls.Count == 0)
            {
                return;
            }

            try
            {
                var currentIndex = tabControl1.SelectedIndex;

                var tabPage = tabControl1.Controls[currentIndex];

                tabControl1.Controls.Remove(tabPage);

                tabControl1.SelectedIndex = currentIndex - 1;
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("[whlBrowser.Event_CloseTab] Critical error. Exception {0}", ex);
            }

            


        }

        private void tabControl1_MouseClick(object sender, MouseEventArgs e)
        {
            var tabControl = sender as TabControl;
            var tabs = tabControl.TabPages;

            if (tabControl.Controls.Count == 0)
            {
                return;
            }

            if (e.Button == MouseButtons.Middle)
            {
                tabs.Remove(tabs.Cast<TabPage>()
                        .Where((t, i) => tabControl.GetTabRect(i).Contains(e.Location))
                        .First());
            }
        }
    }
}
