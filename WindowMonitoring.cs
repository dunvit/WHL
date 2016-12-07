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
using WHL.UiTools;

namespace WHL
{
    public partial class WindowMonitoring : Form
    {
        //TODO: webBrowser1.Url = new Uri("http://www.ellatha.com/eve/wormholelist.asp");

        private const string TextAuthorizationInfo =
            "To login you will need to press the button and go to the  CCP SSO (single sign-on) site. All your private data will remain on the CCP's website.";

        private const string TextAfterAuthorizationInfo =
            "You have successfully logged into the system and the WHL (WormHoleLocator) can now keep track of your current position. You can log in again with another character.";

        #region private variables

        private static readonly ILog Log = LogManager.GetLogger(typeof(CrestApiListener));

        private bool _windowIsPinned;
        private bool _windowIsMinimaze;

        #endregion

        public Tabs ContainerTabs { get; set; }

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
            lblVersionID.Text = @"1.23";

            pnlContainerSignatures.Location = new Point(-1000,-1000);

            this.ControlBox = false;
            this.Text = String.Empty;

            Log.DebugFormat("[WindowMonitoring] Version: {0}", lblVersionID.Text);

            lblAuthorizationInfo.Text = TextAuthorizationInfo;

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

        private ToolTip toolTip1 = new ToolTip();
        private ToolTip toolTip2 = new ToolTip();

        private void CreateTooltipsForStatics()
        {
            toolTip1.AutoPopDelay = 5000;
            toolTip1.InitialDelay = 1000;
            toolTip1.ReshowDelay = 500;
            toolTip1.ShowAlways = true;

            toolTip2.AutoPopDelay = 5000;
            toolTip2.InitialDelay = 1000;
            toolTip2.ReshowDelay = 500;
            toolTip2.ShowAlways = true;


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

            PilotAuthorizeFlow(value);

            BringApplicationToFront();

            RefreshSolarSystemInformation();

            OpenAuthorizationPanel();
        }

        private void PilotAuthorizeFlow(string code)
        {
            Log.DebugFormat("[WindowMonitoring.PilotAuthorizeFlow] starting for token = {0}", code);

            var _currentPilot = new PilotEntity();

            _currentPilot.Initialization(code);



            if (Global.Pilots.IsExist(_currentPilot.Id) == false)
            {
                Global.Pilots.Add(_currentPilot);

                cmbPilots.Items.Add(_currentPilot.Name.Trim());
                cmbPilots.Text = _currentPilot.Name.Trim();
            }

            cmbPilots.Visible = true;

            Global.Pilots.Selected = _currentPilot;

            DrawPilotPanel(Global.Pilots.Selected);
        }

        private void DrawPilotPanel(PilotEntity pilot)
        {
            if (pilot.Location.System != "unknown")
            {
                lblSolarSystemInformation.ForeColor = Color.LightGray;
                lblSignaturesInformation.ForeColor = Color.LightGray;
            }
            lblPilotsInformation.ForeColor = Color.LightGray;
            lblCoordinatesSignatures.ForeColor = Color.LightGray;

            lblPilotName.Text = @"Log in as " + pilot.Name;
            lblPilotName.Visible = true;

            lblSolarSystemName.Text = pilot.Location.System;
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


        #region Panel actions GUI functions

        private void pnlAuthirization_Click(object sender, EventArgs e)
        {
            OpenAuthorizationPanel();
        }

        private void pnlPilotsInformation_Click(object sender, EventArgs e)
        {
            OpenPilotInformationPanel();
        }

        private void pnlSolarSystemInformation_Click(object sender, EventArgs e)
        {
            OpenSolarSystemPanel();
        }

        private void lblSolarSystemInformation_Click(object sender, EventArgs e)
        {
            OpenSolarSystemPanel();

        }

        private void lblPilotsInformation_Click(object sender, EventArgs e)
        {
            OpenPilotInformationPanel();
        }

        private void lblAuthirization_Click(object sender, EventArgs e)
        {
            OpenAuthorizationPanel();
        }

        private void pnlAuthirization_Paint(object sender, PaintEventArgs e)
        {

        }

        #endregion

        private void btnLogInWithEveOnline_Click(object sender, EventArgs e)
        {
            var data = WebUtility.UrlEncode(@"http://localhost:8080/WormholeLocator");
            Process.Start("https://login.eveonline.com/oauth/authorize?response_type=code&redirect_uri=" + data +
                          "&client_id=8f1e2ac9d4aa467c88b12674926dc5e6&scope=characterLocationRead&state=75c68f04aec80589a157fd13");

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

                if (txtSolarSystemName.Text != Global.Pilots.Selected.Location.System)
                {
                    RefreshSolarSystemInformation();
                }

                lblSolarSystemName.Text = Global.Pilots.Selected.Location.System;
            }
        }

        private void cmdShowZkillboardLabel_Click(object sender, EventArgs e)
        {
            ShowZkillboard();
        }

        private void ShowZkillboard()
        {
            if (Global.Pilots.Selected != null && Global.Pilots.Selected.Location != null && Global.Pilots.Selected.Location.System != "unknown")
            {
                BrowserUrlExecute("https://zkillboard.com/system/" + Global.Pilots.Selected.Location.Id.Replace("J","") + "/");
            }
        }

        private void cmdShowZkillboardPanel_Click(object sender, EventArgs e)
        {
            ShowZkillboard();
        }

        private void cmdShowSuperputeLabel_Click(object sender, EventArgs e)
        {
            ShowSuperpute();
        }

        private void ShowSuperpute()
        {
            if (Global.Pilots.Selected != null && Global.Pilots.Selected.Location != null && Global.Pilots.Selected.Location.System != "unknown")
            {
                BrowserUrlExecute("http://superpute.com/system/" + Global.Pilots.Selected.Location.System + "");
            }
        }

        private void listBox1_KeyUp(object sender, KeyEventArgs e)
        {

            if (e.KeyData == (Keys.Control | Keys.V))
            {
                listBox1.Items.Clear();

                var txtInClip = Clipboard.GetText();

                if (string.IsNullOrEmpty(txtInClip))
                {
                    return;
                }

                string[] pilots;

                pilots = txtInClip.Split(new[] { '\n' }, StringSplitOptions.None);

                foreach (var pilot in pilots)
                {
                    listBox1.Items.Add(pilot);
                }
            }
        }

        private void listBox1_Click(object sender, EventArgs e)
        {
            txtSelectedPilotName.Text = listBox1.Text;
            
        }

        private void label9_Click(object sender, EventArgs e)
        {
            crlPilotsHistory.Items.Clear();
        }

        private void crlPilotsHistory_Click(object sender, EventArgs e)
        {
            txtSelectedPilotName.Text = crlPilotsHistory.Text;
        }

        private void eventCopyPilotsFromClipboard(object sender, EventArgs e)
        {
            listBox1.Items.Clear();

            var txtInClip = Clipboard.GetText();

            if (string.IsNullOrEmpty(txtInClip))
            {
                return;
            }

            string[] pilots;

            pilots = txtInClip.Split(new[] { '\n' }, StringSplitOptions.None);

            foreach (var pilot in pilots)
            {
                listBox1.Items.Add(pilot);
            }
        }

        private void EventPasteLocationBookmarks(object sender, EventArgs e)
        {
            listLocationBookmarks.Items.Clear();

            var txtInClip = Clipboard.GetText();

            Log.DebugFormat("[WindowMonitoring.EventPasteLocationBookmarks] paste for = {0}", txtInClip);

            if (string.IsNullOrEmpty(txtInClip))
            {
                return;
            }

            string[] lines;

            lines = txtInClip.Split(new[] { '\n' }, StringSplitOptions.None);

            char tab = '\u0009';

            foreach (var line in lines)
            {
                Log.DebugFormat("[WindowMonitoring.EventPasteLocationBookmarks] line = {0}", line);

                try
                {
                    var coordinates = line.Replace(tab.ToString(), "[---StarinForReplace---]");
                    var coordinate = coordinates.Split(new[] { @"[---StarinForReplace---]" }, StringSplitOptions.None)[0];
                    var m1 = Regex.Matches(coordinate, @"\d\d\d", RegexOptions.Singleline);

                    foreach (Match m in m1)
                    {
                        var value = m.Groups[0].Value;

                        listLocationBookmarks.Items.Add("ID = [" + value + "] " + coordinate);
                    }
                }
                catch (Exception ex)
                {
                    Log.ErrorFormat("[WindowMonitoring.EventPasteLocationBookmarks] Critical error = {0}", ex);
                }
            }
        }

        private void eventPasteCosmicSifnatures(object sender, EventArgs e)
        {
            listCosmicSifnatures.Items.Clear();

            var txtInClip = Clipboard.GetText();

            Log.DebugFormat("[WindowMonitoring.eventPasteCosmicSignatures] paste for = {0}", txtInClip);

            if (string.IsNullOrEmpty(txtInClip))
            {
                return;
            }

            string[] lines;

            lines = txtInClip.Split(new[] { '\n' }, StringSplitOptions.None);

            char tab = '\u0009';

            foreach (var line in lines)
            {
                Log.DebugFormat("[WindowMonitoring.eventPasteCosmicSignatures] line = {0}", line);

                try
                {
                    var coordinates = line.Replace(tab.ToString(), "[---StarinForReplace---]");
                    var coordinate = coordinates.Split(new[] { @"[---StarinForReplace---]" }, StringSplitOptions.None)[0];
                    var m1 = Regex.Matches(coordinate, @"\d\d\d", RegexOptions.Singleline);

                    foreach (Match m in m1)
                    {
                        var value = m.Groups[0].Value;

                        listCosmicSifnatures.Items.Add("[" + value + "] " + coordinate);
                    }
                }
                catch (Exception ex)
                {
                    Log.ErrorFormat("[WindowMonitoring.eventPasteCosmicSignatures] Critical error = {0}", ex);
                }
            }
        }

        private void EventExecuteClearBookmarksAndSignatures(object sender, EventArgs e)
        {
            Log.Debug("[WindowMonitoring.EventExecuteClearBookmarksAndSignatures] starting");

            try
            {
                var coordinates = listLocationBookmarks.Items.OfType<string>().ToList();
                var signatures = listCosmicSifnatures.Items.OfType<string>().ToList();

                listLocationBookmarks.Items.Clear();
                listCosmicSifnatures.Items.Clear();

                foreach (var coordinate in coordinates)
                {
                    var coordinateId = coordinate.Split('[')[1].Split(']')[0];

                    var isNeedAddToList = true;

                    foreach (var signature in signatures)
                    {
                        var signatureId = signature.Split('[')[1].Split(']')[0];

                        if (coordinateId == signatureId)
                        {
                            isNeedAddToList = false;
                        }
                    }

                    if (isNeedAddToList)
                    {
                        listLocationBookmarks.Items.Add(coordinate);
                    }
                }

                foreach (var signature in signatures)
                {
                    var signatureId = signature.Split('[')[1].Split(']')[0];


                    var isNeedAddToList = true;

                    foreach (var coordinate in coordinates)
                    {
                        var coordinateId = coordinate.Split('[')[1].Split(']')[0];

                        if (coordinateId == signatureId)
                        {
                            isNeedAddToList = false;
                        }
                    }

                    if (isNeedAddToList)
                    {
                        listCosmicSifnatures.Items.Add(signature);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("[WindowMonitoring.EventExecuteClearBookmarksAndSignatures] Critical error = {0}", ex);
            }

            
        }

        private void cmbPilots_SelectedValueChanged(object sender, EventArgs e)
        {
            Global.Pilots.Activate(cmbPilots.Text);

            OpenAuthorizationPanel();
        }

        private void Event_WindowResizeEnd(object sender, EventArgs e)
        {
            if (_windowIsMinimaze) return;

            if (ContainerTabs.Active().Name == "WebBrowser")
            {
                ResizeWebBrowser();
            }
            else
            {
                ResizeWindow();
            }

            ResizeButtonsPanelLocation();
        }

        private void Event_OpenSignaturesPanel(object sender, EventArgs e)
        {
            OpenSignaturesPanel();
        }

        private void Event_PasteSignatures(object sender, EventArgs e)
        {
            listSignaturesInCurrentSolarSystem.Items.Clear();

            var txtInClip = Clipboard.GetText();

            if (string.IsNullOrEmpty(txtInClip))
            {
                return;
            }

            try
            {
                var parts = txtInClip.Split('\n');

                foreach (var signature in parts)
                {
                    var code = signature.Split('\t')[0];

                    listSignaturesInCurrentSolarSystem.Items.Add(code);
                }
            }
            catch (Exception ex)
            {
                Log.Error("[WindowMonitoring.Event_PasteSignatures] Critical error = " + ex);
            }

            listSignaturesInCurrentSolarSystem.Refresh();

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

        private void Event_PanelDrawBorder(object sender, PaintEventArgs e)
        {
            //ControlPaint.DrawBorder(e.Graphics, this.panel1.ClientRectangle, Color.OrangeRed, ButtonBorderStyle.Solid);
        }

        private void Event_WebBrowserEnterUrl(object sender, KeyEventArgs e)
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


    }
}
