using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using log4net;
using WHL.BLL;
using WHL.Properties;
using WHL.Ui;
using WHL.WhlControls;

namespace WHL
{
    public delegate void OpenWebBrowser();

    public partial class WindowMonitoring : Form
    {
        //TODO: webBrowser1.Url = new Uri("http://www.ellatha.com/eve/wormholelist.asp");


        #region private variables

        private static readonly ILog Log = LogManager.GetLogger(typeof(CrestApiListener));

        private bool _windowIsPinned;
        private bool _windowIsMinimaze;

        #endregion

        public Tabs ContainerTabs { get; set; }

        public whlPilotInfo ucContainerPilotInfo = new whlPilotInfo();
        public whlBookmarks ucContainerBookmarks;

        public whlSolarSystem ucContainreSolarSystem;

        public whlAuthorization ucContainreAuthorization;

        

        #region WinAPI

        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        #endregion

        public WindowMonitoring()
        {
            InitializeComponent();

            ContainerTabs = new Tabs();

            Size = ContainerTabs.Active().Size;
        }



        private void WindowMonitoring_Load(object sender, EventArgs e)
        {
            lblVersionID.Text = @"1.24";
            // EvaJima

            //ucContainerPilotInfo = new whlPilotInfo();

            ucContainreSolarSystem = new whlSolarSystem();
            
            ucContainerBookmarks = new whlBookmarks();

            ucContainreAuthorization = new whlAuthorization();

            Controls.Add(ucContainerPilotInfo);
            Controls.Add(ucContainerBookmarks);
            Controls.Add(ucContainreSolarSystem);
            Controls.Add(ucContainreAuthorization);

            Global.Browser = new whlBrowser(OpenWebBrowserPanel);

            Controls.Add(Global.Browser);

            Log.DebugFormat("[WindowMonitoring] Version: {0}", lblVersionID.Text);

            

            Size = ContainerTabs.Active().Size;
            ResizeWindow();
            CreateTooltipsForStatics();

            System.Security.Principal.WindowsIdentity identity = System.Security.Principal.WindowsIdentity.GetCurrent();
            System.Security.Principal.WindowsPrincipal principal = new System.Security.Principal.WindowsPrincipal(identity);

            if (principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator))
            {
                MessageBox.Show("For use return tokens from EVE CREST we need run WHL as administrator");
            }

            DelegateStartProcess startProcessFunction = StartPilotAuthorizeFlow;

            new Thread(() => new CrestApiListener().ListenLocalhost(startProcessFunction)) { IsBackground = true }.Start();

            SetStyle(ControlStyles.ResizeRedraw, true);

            HideAllContainers();

            OpenAuthorizationPanel();
        }

        protected override void WndProc(ref Message m)
        {
            if (ContainerTabs.Active().Name != "WebBrowser")
            {
                base.WndProc(ref m);
                return;
            }

            const UInt32 WM_NCHITTEST = 0x0084;
            const UInt32 WM_MOUSEMOVE = 0x0200;

            const UInt32 HTLEFT = 10;
            const UInt32 HTRIGHT = 11;
            const UInt32 HTBOTTOMRIGHT = 17;
            const UInt32 HTBOTTOM = 15;
            const UInt32 HTBOTTOMLEFT = 16;
            const UInt32 HTTOP = 12;
            const UInt32 HTTOPLEFT = 13;
            const UInt32 HTTOPRIGHT = 14;

            const int RESIZE_HANDLE_SIZE = 10;
            bool handled = false;
            if (m.Msg == WM_NCHITTEST || m.Msg == WM_MOUSEMOVE)
            {
                Size formSize = this.Size;
                Point screenPoint = new Point(m.LParam.ToInt32());
                Point clientPoint = this.PointToClient(screenPoint);

                Dictionary<UInt32, Rectangle> boxes = new Dictionary<UInt32, Rectangle>() {
                    {HTBOTTOMLEFT, new Rectangle(0, formSize.Height - RESIZE_HANDLE_SIZE, RESIZE_HANDLE_SIZE, RESIZE_HANDLE_SIZE)},
                    {HTBOTTOM, new Rectangle(RESIZE_HANDLE_SIZE, formSize.Height - RESIZE_HANDLE_SIZE, formSize.Width - 2*RESIZE_HANDLE_SIZE, RESIZE_HANDLE_SIZE)},
                    {HTBOTTOMRIGHT, new Rectangle(formSize.Width - RESIZE_HANDLE_SIZE, formSize.Height - RESIZE_HANDLE_SIZE, RESIZE_HANDLE_SIZE, RESIZE_HANDLE_SIZE)},
                    {HTRIGHT, new Rectangle(formSize.Width - RESIZE_HANDLE_SIZE, RESIZE_HANDLE_SIZE, RESIZE_HANDLE_SIZE, formSize.Height - 2*RESIZE_HANDLE_SIZE)},
                    {HTTOPRIGHT, new Rectangle(formSize.Width - RESIZE_HANDLE_SIZE, 0, RESIZE_HANDLE_SIZE, RESIZE_HANDLE_SIZE) },
                    {HTTOP, new Rectangle(RESIZE_HANDLE_SIZE, 0, formSize.Width - 2*RESIZE_HANDLE_SIZE, RESIZE_HANDLE_SIZE) },
                    {HTTOPLEFT, new Rectangle(0, 0, RESIZE_HANDLE_SIZE, RESIZE_HANDLE_SIZE) },
                    {HTLEFT, new Rectangle(0, RESIZE_HANDLE_SIZE, RESIZE_HANDLE_SIZE, formSize.Height - 2*RESIZE_HANDLE_SIZE) }
                };

                foreach (KeyValuePair<UInt32, Rectangle> hitBox in boxes)
                {
                    if (hitBox.Value.Contains(clientPoint))
                    {
                        m.Result = (IntPtr)hitBox.Key;
                        handled = true;
                        break;
                    }
                }

            }

            if (!handled)
                base.WndProc(ref m);
        }

        private void DrawPilotPanel(PilotEntity pilot)
        {
            if (pilot.Location.System != "unknown")
            {
                cmdLocation.IsActive = true;
                cmdLocation.Refresh();
            }

            lblPilotName.Text = @"Log in as " + pilot.Name;
            lblPilotName.Visible = true;

            lblSolarSystemName.Text = pilot.Location.System;
        }

        private void CreateTooltipsForStatics()
        {
            


            var toolTipUrlButton = new ToolTip
            {
                AutoPopDelay = 5000,
                InitialDelay = 1000,
                ReshowDelay = 500,
                ShowAlways = true
            };


            toolTipUrlButton.SetToolTip(btnOpenBrowserAndStartUrl, "Open WHL brouser and start url");
        }

        public void StartPilotAuthorizeFlow(string value)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(StartPilotAuthorizeFlow), value);
                return;
            }

            Log.DebugFormat("[WindowMonitoring.StartPilotAuthorizeFlow] get value: {0}", value);

            ucContainreAuthorization.PilotAuthorizeFlow(value);

            BringApplicationToFront();

            RefreshSolarSystemInformation(Global.Pilots.Selected.Location);

            DrawPilotPanel(Global.Pilots.Selected);

            OpenAuthorizationPanel();
        }

        public void BringApplicationToFront()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(BringApplicationToFront));
            }
            else
            {
                var topMost = TopMost;

                TopMost = true;
                Focus();
                BringToFront();
                System.Media.SystemSounds.Beep.Play();
                TopMost = topMost;
            }
        }

        private void OpenWebBrowserPanel()
        {
            Global.Browser.Visible = true;

            Global.Browser.Location = new Point(pnlContainer.Location.X, pnlContainer.Location.Y);

            HideAllContainers();
            Global.Browser.Visible = true;

            pnlContainer.BringToFront();

            Global.Browser.BringToFront();

            ContainerTabs.Activate("WebBrowser");

            ResizeWindow();

            cmdOpenWebBrowser.IsTabControlButton = true;
            cmdOpenWebBrowser.BringToFront();
            cmdOpenWebBrowser.Refresh();
        }

        private void OpenAuthorizationPanel()
        {
            ContainerTabs.Activate("Authorization");

            HideAllContainers();
            ucContainreAuthorization.Visible = true;

            pnlContainer.BringToFront();

            cmdLocation.BringToFront();

            if (Global.Pilots.Selected != null)
            {
                ucContainreAuthorization.RefreshPilotInfo();
            }

            ResizeWindow();

            ucContainreAuthorization.BackColor = Color.Black;
            ucContainreAuthorization.Location = new Point(pnlContainer.Location.X, pnlContainer.Location.Y);
            ucContainreAuthorization.BringToFront();

            cmdAuthirizationPanel.IsTabControlButton = true;
            cmdAuthirizationPanel.BringToFront();
            cmdAuthirizationPanel.Refresh();
        }

        private void Event_OpenBrowserContainer(object sender, EventArgs e)
        {
            ContainerTabs.Activate("WebBrowser");

            OpenWebBrowserPanel();

            var urlFromClipboard = Clipboard.GetText();

            if (urlFromClipboard.StartsWith("http"))
            {
                Global.Browser.BrowserUrlExecute(urlFromClipboard);
            }
            else
            {
                Global.Browser.BrowserOpen();
            }

            _windowIsMinimaze = false;
            cmdMinimazeRestore.Image = Resources.minimize;
        }

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
                if (_windowIsMinimaze == false)
                {
                    Global.Browser.ResizeWebBrowser(Width, Height);
                    ContainerTabs.Active().Size = new Size(Width, Height);
                }
            }

            if (_windowIsMinimaze == false)
            {
                VersionBar.Width = Width - 8;
                VersionBar.Location = new Point(10, Height - 38);
            }

            pnlContainer.Visible = false;

            Refresh();
        }

        private void HideAllContainers()
        {
            ucContainerBookmarks.Visible = false;
            ucContainerPilotInfo.Visible = false;
            ucContainreSolarSystem.Visible = false;
            ucContainreAuthorization.Visible = false;

            Global.Browser.Visible = false;

            cmdLocation.IsTabControlButton = false;
            cmdLocation.Refresh();

            cmdShowContainerPilots.IsTabControlButton = false;
            cmdShowContainerPilots.Refresh();

            cmdAuthirizationPanel.IsTabControlButton = false;
            cmdAuthirizationPanel.Refresh();

            cmdOpenWebBrowser.IsTabControlButton = false;
            cmdOpenWebBrowser.Refresh();

        }

        #region GUI

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.FillRectangle(new SolidBrush(Color.Black), 0, 0, Width, 28);

            e.Graphics.DrawRectangle(new Pen(Color.DarkGray, 2), 0, 0, Width, Height);
        }

        private void cmdClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void cmdPin_Click(object sender, EventArgs e)
        {
            if (_windowIsPinned)
            {
                _windowIsPinned = false;
                cmdPin.Image = Resources.pin;
                TopMost = false;
            }
            else
            {
                _windowIsPinned = true;
                cmdPin.Image = Resources.unpin;
                TopMost = true;
            }
        }

        private void cmdMinimazeRestore_Click(object sender, EventArgs e)
        {
            if (_windowIsMinimaze)
            {
                _windowIsMinimaze = false;
                cmdMinimazeRestore.Image = Resources.minimize;
                lblSolarSystemName.Visible = true;
                lblPilotName.Visible = true;
                Size = ContainerTabs.Active().Size;
            }
            else
            {
                _windowIsMinimaze = true;
                cmdMinimazeRestore.Image = Resources.restore;
                lblSolarSystemName.Visible = true;
                lblPilotName.Visible = false;
                Size = ContainerTabs.Active().CompactSize;
            }

            ResizeButtonsPanelLocation();
        }

        private void WindowMonitoring_MouseDown(object sender, MouseEventArgs e)
        {
            //if (e.Button == MouseButtons.Left)
            //{
            //    ReleaseCapture();
            //    SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            //}
        }

        #endregion

        private void RefreshTokenTimer_Tick(object sender, EventArgs e)
        {
            if (Global.Pilots.Count() > 0)
            {
                foreach (var pilot in Global.Pilots)
                {
                    Task.Run(() =>
                    {
                        var locationId = pilot.Location.System;

                        Log.DebugFormat("[WindowMonitoring.RefreshTokenTimer_Tick] starting get location info for pilot = {0}", pilot.Name);
                        pilot.RefreshInfo();

                        if (locationId != pilot.Location.System && pilot.Id == Global.Pilots.Selected.Id)
                        {
                                
                        }
                    });
                }

                if (ucContainreSolarSystem.SolarSystem.System != Global.Pilots.Selected.Location.System)
                {
                    RefreshSolarSystemInformation(Global.Pilots.Selected.Location);
                }

                lblSolarSystemName.Text = Global.Pilots.Selected.Location.System;
            }
        }

        private void Event_WindowResizeEnd(object sender, EventArgs e)
        {
            if (_windowIsMinimaze) return;

            if (ContainerTabs.Active().Name == "WebBrowser")
            {
                Global.Browser.ResizeWebBrowser(Width, Height);
                ContainerTabs.Active().Size = new Size(Width, Height);
            }
            else
            {
                ResizeWindow();
            }

            ResizeButtonsPanelLocation();
        }

        private void Event_WindowDoubleClick(object sender, EventArgs e)
        {
            if (_windowIsMinimaze)
            {
                _windowIsMinimaze = false;
                cmdMinimazeRestore.Image = Resources.minimize;
                lblSolarSystemName.Visible = true;
                lblPilotName.Visible = true;
                Size = new Size(ContainerTabs.Active().Size.Width, ContainerTabs.Active().Size.Height);
            }
            else
            {
                _windowIsMinimaze = true;
                cmdMinimazeRestore.Image = Resources.restore;
                lblSolarSystemName.Visible = true;
                lblPilotName.Visible = false;
                Size = new Size( ContainerTabs.Active().CompactSize.Width, ContainerTabs.Active().CompactSize.Height);
            }

            ResizeButtonsPanelLocation();
        }

        private void Event_WindowResizeBegin(object sender, EventArgs e)
        {
            string a = "";
        }

        private void Event_WindowMouseMove(object sender, MouseEventArgs e)
        {
            
        }

        private void WindowMonitoring_Resize(object sender, EventArgs e)
        {
            ResizeButtonsPanelLocation();
        }

        private void Event_TitleBarDoubleClick(object sender, EventArgs e)
        {
            if (_windowIsMinimaze)
            {
                _windowIsMinimaze = false;
                cmdMinimazeRestore.Image = Resources.minimize;
                lblSolarSystemName.Visible = true;
                lblPilotName.Visible = true;
                Size = ContainerTabs.Active().Size;
            }
            else
            {
                _windowIsMinimaze = true;
                cmdMinimazeRestore.Image = Resources.restore;
                lblSolarSystemName.Visible = true;
                lblPilotName.Visible = false;
                Size = ContainerTabs.Active().CompactSize;
            }

            ResizeButtonsPanelLocation();
        }

        private void Event_TitleBarMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && e.Clicks == 1)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void Event_ShowContainerPilots(object sender, EventArgs e)
        {
            ContainerTabs.Activate("Pilots");

            HideAllContainers();

            ucContainerPilotInfo.Visible = true;

            ResizeWindow();

            ucContainerPilotInfo.BackColor = Color.Black;
            ucContainerPilotInfo.Location = new Point(pnlContainer.Location.X, pnlContainer.Location.Y);
            ucContainerPilotInfo.BringToFront();

            cmdShowContainerPilots.IsTabControlButton = true;
            cmdShowContainerPilots.BringToFront();
            cmdShowContainerPilots.Refresh();
        }

        private void Event_ShowContainerCoordinates(object sender, EventArgs e)
        {
            ContainerTabs.Activate("Bookmarks");

            HideAllContainers();

            ucContainerBookmarks.Visible = true;

            ResizeWindow();

            ucContainerBookmarks.BackColor = Color.Black;
            ucContainerBookmarks.Location = new Point(pnlContainer.Location.X, pnlContainer.Location.Y);
            ucContainerBookmarks.BringToFront();

            cmdShowContainerBookmarks.IsTabControlButton = true;
            cmdShowContainerBookmarks.BringToFront();
            cmdShowContainerBookmarks.Refresh();
        }

        private void RefreshSolarSystemInformation(StarSystemEntity location)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => RefreshSolarSystemInformation(location)));
            }

            if (Global.Pilots.Count() > 0)
            {
                ucContainreSolarSystem.RefreshSolarSystem(location);
            }
        }

        private void Event_ShowContainerSolarSystemInfo(object sender, EventArgs e)
        {
            if (Global.Pilots.Count() == 0 || Global.Pilots.Selected == null || Global.Pilots.Selected.Location == null || Global.Pilots.Selected.Location.System == "unknown") return;

            HideAllContainers();
            ucContainreSolarSystem.Visible = true;

            ContainerTabs.Activate("Location");


            pnlContainer.BringToFront();

            ResizeWindow();

            ucContainreSolarSystem.BackColor = Color.Black;
            ucContainreSolarSystem.Location = new Point(pnlContainer.Location.X, pnlContainer.Location.Y);
            ucContainreSolarSystem.BringToFront();

            cmdLocation.IsTabControlButton = true;
            cmdLocation.BringToFront();
            cmdLocation.Refresh();
        }

        private void cmdAuthirizationPanel_Click(object sender, EventArgs e)
        {
            OpenAuthorizationPanel();
        }
    }
}
