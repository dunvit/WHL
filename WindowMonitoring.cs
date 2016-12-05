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
using WHL.UiTools;
using WHLocator.Infrastructure;
using WHLocator.UiTools;

namespace WHLocator
{
    public partial class WindowMonitoring : Form
    {
        //TODO: webBrowser1.Url = new Uri("http://www.ellatha.com/eve/wormholelist.asp");

        public delegate void DelegateStartProcess(string value);

        private const string TextAuthorizationInfo = "To login you will need to press the button and go to the  CCP SSO (single sign-on) site. All your private data will remain on the CCP's website.";
        private const string TextAfterAuthorizationInfo = "You have successfully logged into the system and the WHL (WormHoleLocator) can now keep track of your current position. You can log in again with another character.";
        #region private variables
        private Wormholes _wormholes = new Wormholes();

        private CcpXmlApi _ccpXmlApi = new CcpXmlApi();

        private Pilotes _pilotes = new Pilotes();

        //private Pilot _currentPilot;

        private static readonly ILog Log = LogManager.GetLogger(typeof(CrestApiListener));

        private bool _windowIsPinned = false;
        private bool _windowIsMinimaze = false;

        private Size _sizeCompact = new Size(160, 29);

        private Size _sizeOpen = new Size(564, 325);

        private Size _sizeWithZkillboard = new Size(896, 591);

        #endregion

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

            
        }

        private void WindowMonitoring_Load(object sender, EventArgs e)
        {

            lblVersionID.Text = @"1.22"; //System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

            Log.DebugFormat("[WindowMonitoring] Version: {0}", lblVersionID.Text);

            lblAuthorizationInfo.Text = TextAuthorizationInfo;

            OpenAuthorizationPanel();

            Size = _sizeOpen;
            Resize();
            CreateTooltipsForStatics();

            System.Security.Principal.WindowsIdentity identity = System.Security.Principal.WindowsIdentity.GetCurrent();
            System.Security.Principal.WindowsPrincipal principal = new System.Security.Principal.WindowsPrincipal(identity);

            if (principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator))
            {
                MessageBox.Show("For use return tokens from EVE CREST we need run WHL as administrator");
            }

            DelegateStartProcess startProcessFunction = StartPilotAuthorizeFlow;

            new Thread(() => new CrestApiListener().ListenLocalhost(startProcessFunction)) { IsBackground = true }.Start();

            
        }

        ToolTip toolTip1 = new ToolTip();
        ToolTip toolTip2 = new ToolTip();

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

            OpenAuthorizationPanel();
        }

        private void PilotAuthorizeFlow(string code)
        {
            Log.DebugFormat("[WindowMonitoring.PilotAuthorizeFlow] starting for token = {0}", code);

            var _currentPilot = new Pilot();

            _currentPilot.Initialization(code);

            

            if (_pilotes.IsExist(_currentPilot.Id) == false)
            {
                _pilotes.Add(_currentPilot);

                cmbPilots.Items.Add(_currentPilot.Name.Trim());
                cmbPilots.Text = _currentPilot.Name.Trim();
            }

            cmbPilots.Visible = true;

            _pilotes.Selected = _currentPilot;

            DrawPilotPanel(_pilotes.Selected);
        }

        private void DrawPilotPanel(Pilot pilot)
        {
            lblSolarSystemInformation.ForeColor = Color.LightGray;
            lblPilotsInformation.ForeColor = Color.LightGray;
            lblCoordinatesSignatures.ForeColor = Color.LightGray;

            lblPilotName.Text = @"Log in as " + pilot.Name;
            lblPilotName.Visible = true;

            lblSolarSystemName.Text = pilot.Location.Name;
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

        private void Resize()
        {
            pnlControls.Location = new Point(Size.Width - pnlControls.Size.Width - 3, 0 + 3);

            Refresh();
        }

        private void WindowMonitoring_DoubleClick(object sender, EventArgs e)
        {
            Size = _sizeOpen;
            Resize();
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
                Size = _sizeOpen;
            }
            else
            {
                _windowIsMinimaze = true;
                cmdMinimazeRestore.Image = Resources.restore;
                lblSolarSystemName.Visible = true;
                lblPilotName.Visible = false;
                Size = _sizeCompact;
            }

            Resize();
        }

        private void lblSolarSystemName_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void WindowMonitoring_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void OpenAuthorizationPanel()
        {
            pnlContainer.BringToFront();
            pnlPilotsInformation.BringToFront();
            pnlSolarSystemInformation.BringToFront();
            pnlBookmarksAndSignatures.BringToFront();

            if (_pilotes.Count() > 0)
            {
                crlPilotPortrait.Image = _pilotes.Selected.Portrait;
                crlPilotPortrait.Refresh();

                crlPilotPortrait.Visible = true;

                lblAuthorizationInfo.Text = TextAfterAuthorizationInfo + Environment.NewLine + Environment.NewLine + TextAuthorizationInfo;
            }

            containerAuthirization.BackColor = Color.Black;
            containerAuthirization.Location = new Point(pnlContainer.Location.X + 6, pnlContainer.Location.Y + 6);
            containerAuthirization.BringToFront();
        }

        private void OpenSolarSystemPanel()
        {
            if (_pilotes.Count() == 0)
            {
                return;
            }
            pnlContainer.BringToFront();
            pnlAuthirization.BringToFront();
            pnlPilotsInformation.BringToFront();
            pnlBookmarksAndSignatures.BringToFront();

            RefreshSolarSystemInformation();
            

            containerSolarSystemInformation.BackColor = Color.Black;
            containerSolarSystemInformation.Location = new Point(pnlContainer.Location.X + 6, pnlContainer.Location.Y + 6);
            containerSolarSystemInformation.BringToFront();

        }

        private void RefreshSolarSystemInformation()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(RefreshSolarSystemInformation));
            }

            if (_pilotes.Count() > 0)
            {
                txtSolarSystemName.Text = _pilotes.Selected.Location.Name;
                txtSolarSystemClass.Text = _pilotes.Selected.Location.Class;
                txtSolarSystemEffect.Text = _pilotes.Selected.Location.Effect;
                txtSolarSystemRegion.Text = _pilotes.Selected.Location.Region;
                txtSolarSystemConstellation.Text = _pilotes.Selected.Location.Constellation;

                Wormhole wormholeI;
                Wormhole wormholeII;
                switch (_pilotes.Selected.Location.StaticSystems.Count)
                {
                    case 0:
                        txtSolarSystemStaticI.Visible = false;
                        txtSolarSystemStaticII.Visible = false;
                        txtSolarSystemStaticIData.Visible = false;
                        txtSolarSystemStaticIIData.Visible = false;
                        break;
                    case 1:
                        wormholeI = _wormholes.GetWormhole(_pilotes.Selected.Location.StaticSystems[0]);

                        txtSolarSystemStaticII.Visible = false;
                        txtSolarSystemStaticIIData.Visible = false;
                        txtSolarSystemStaticI.Text = _pilotes.Selected.Location.StaticSystems[0];
                        txtSolarSystemStaticI.Visible = true;
                        txtSolarSystemStaticI.ForeColor = Tools.GetColorBySolarSystem(_wormholes.GetWormhole(_pilotes.Selected.Location.StaticSystems[0]).Name);
                        txtSolarSystemStaticIData.Text = _wormholes.GetWormhole(_pilotes.Selected.Location.StaticSystems[0]).Name;
                        txtSolarSystemStaticIData.Visible = true;

                        // Set up the ToolTip text for the Button and Checkbox.
                        toolTip1.SetToolTip(txtSolarSystemStaticI, "Max Stable Mass=" + wormholeI.MaxStableMass + "\r\nMax Jump  Mass=" + wormholeI.MaxJumpMass);

                        break;
                    case 2:

                        try
                        {
                            wormholeI = _wormholes.GetWormhole(_pilotes.Selected.Location.StaticSystems[0]);
                            wormholeII = _wormholes.GetWormhole(_pilotes.Selected.Location.StaticSystems[1]);

                            txtSolarSystemStaticI.Text = _pilotes.Selected.Location.StaticSystems[0];
                            txtSolarSystemStaticI.Visible = true;

                            txtSolarSystemStaticII.Text = _pilotes.Selected.Location.StaticSystems[1];
                            txtSolarSystemStaticII.Visible = true;

                            txtSolarSystemStaticIData.Text = _wormholes.GetWormhole(_pilotes.Selected.Location.StaticSystems[0]).Name;
                            txtSolarSystemStaticIData.Visible = true;
                            txtSolarSystemStaticI.ForeColor = Tools.GetColorBySolarSystem(_wormholes.GetWormhole(_pilotes.Selected.Location.StaticSystems[0]).Name);

                            txtSolarSystemStaticIIData.Text = _wormholes.GetWormhole(_pilotes.Selected.Location.StaticSystems[1]).Name;
                            txtSolarSystemStaticIIData.Visible = true;
                            txtSolarSystemStaticII.ForeColor = Tools.GetColorBySolarSystem(_wormholes.GetWormhole(_pilotes.Selected.Location.StaticSystems[1]).Name);
                            
                            toolTip1.SetToolTip(txtSolarSystemStaticI, "Max Stable Mass=" + wormholeI.MaxStableMass + " Max Jump Mass=" + wormholeI.MaxJumpMass);
                            toolTip2.SetToolTip(txtSolarSystemStaticII, "Max Stable Mass=" + wormholeII.MaxStableMass + " Max Jump Mass=" + wormholeII.MaxJumpMass);

                        }
                        catch (Exception ex)
                        {
                            Log.Error("Critical error/ Exception = " + ex);
                        }

                        break;
                }
            }
        }

        private void OpenPilotInformationPanel()
        {
            pnlContainer.BringToFront();
            pnlAuthirization.BringToFront();
            pnlSolarSystemInformation.BringToFront();
            pnlBookmarksAndSignatures.BringToFront();

            containerPilotInfo.BackColor = Color.Black;
            containerPilotInfo.Location = new Point(pnlContainer.Location.X + 6, pnlContainer.Location.Y + 6);
            containerPilotInfo.BringToFront();
        }



        private void OpenCoordinatesSignaturesPanel(object sender, EventArgs e)
        {
            pnlContainer.BringToFront();
            pnlAuthirization.BringToFront();
            pnlSolarSystemInformation.BringToFront();
            pnlPilotsInformation.BringToFront();

            containerBookmarksAndSignatures.BackColor = Color.Black;
            containerBookmarksAndSignatures.Location = new Point(pnlContainer.Location.X + 6, pnlContainer.Location.Y + 6);
            containerBookmarksAndSignatures.BringToFront();
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
            Process.Start("https://login.eveonline.com/oauth/authorize?response_type=code&redirect_uri=" + data + "&client_id=8f1e2ac9d4aa467c88b12674926dc5e6&scope=characterLocationRead&state=75c68f04aec80589a157fd13");

        }

        #endregion

        private void RefreshTokenTimer_Tick(object sender, EventArgs e)
        {
            if (_pilotes.Count() > 0)
            {
                foreach (var pilot in _pilotes)
                {
                    Task.Run(() =>
                    {
                        var locationId = pilot.Location.Id;

                        Log.DebugFormat("[WindowMonitoring.RefreshTokenTimer_Tick] starting get location info for pilot = {0}", pilot.Name);
                        pilot.RefreshInfo();

                        

                        if (locationId != pilot.Location.Id && pilot.Id == _pilotes.Selected.Id)
                        {
                            RefreshSolarSystemInformation();
                        }
                    });
                }

                lblSolarSystemName.Text = _pilotes.Selected.Location.Name;
            }
        }

        private void cmdShowZkillboardLabel_Click(object sender, EventArgs e)
        {
            ShowZkillboard();
        }

        private void ShowZkillboard()
        {
            if (_pilotes.Selected != null && _pilotes.Selected.Location != null && _pilotes.Selected.Location.Id > 0)
            {
                webBrowser1.Url = new Uri("https://zkillboard.com/system/" + _pilotes.Selected.Location.Id + "/");
                webBrowser1.Visible = true;
                OpenWebBrowserPanel();
                Resize();
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
            if (_pilotes.Selected != null && _pilotes.Selected.Location != null && _pilotes.Selected.Location.Id > 0)
            {
                webBrowser1.Url = new Uri("http://superpute.com/system/" + _pilotes.Selected.Location.Name + "");
                webBrowser1.Visible = true;
                OpenWebBrowserPanel();
                Resize();
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

        private void ShowEllatha()
        {
            if (_pilotes.Selected != null && _pilotes.Selected.Location != null && _pilotes.Selected.Location.Id > 0)
            {

                if (_pilotes.Selected.Location.Name.Contains("J") == false)
                {
                    MessageBox.Show("Ellatha only for W-Space systems");
                    return;
                }

                webBrowser1.Url = new Uri("http://www.ellatha.com/eve/WormholeSystemview.asp?key=" + _pilotes.Selected.Location.Name.Replace("J", "") + "");
                webBrowser1.Visible = true;
                OpenWebBrowserPanel();
                Resize();
            }
        }

        private void cmdShowDotlanPanel_Click(object sender, EventArgs e)
        {
            ShowDotlan();
        }

        private void ShowDotlan()
        {
            if (_pilotes.Selected != null && _pilotes.Selected.Location != null && _pilotes.Selected.Location.Id > 0)
            {

                webBrowser1.Url = new Uri("http://evemaps.dotlan.net/system/" + _pilotes.Selected.Location.Name + "");
                webBrowser1.Visible = true;
                OpenWebBrowserPanel();
                Resize();
            }
        }

        private void OpenWebBrowserPanel()
        {
            containerWebBrowserPanel.Location = new Point(7,37);
            containerWebBrowserPanel.BringToFront();
            panel1.BringToFront();
            Size = _sizeWithZkillboard;
            containerWebBrowserPanel.Visible = true;
        }

        private void panel2_Click(object sender, EventArgs e)
        {
            containerWebBrowserPanel.Visible = false;
            Size = _sizeOpen;
            Resize();
        }

        private void lblPilotName_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
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

        private void label11_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtSelectedPilotName.Text))
            {
                return;
            }

            var characterId = _ccpXmlApi.GetPilotIdByName(txtSelectedPilotName.Text.Trim());

            webBrowser1.Url = new Uri("https://zkillboard.com/character/" + characterId + "/");

            if (crlPilotsHistory.Items.Contains(txtSelectedPilotName.Text.Trim()) == false)
            {
                crlPilotsHistory.Items.Add(txtSelectedPilotName.Text.Trim());
            }

            OpenWebBrowserPanel();

            Resize();
        }

        private void label10_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtSelectedPilotName.Text))
            {
                return;
            }

            webBrowser1.Url = new Uri("http://eve-hunt.net/hunt/" + txtSelectedPilotName.Text + "/");

            if (crlPilotsHistory.Items.Contains(txtSelectedPilotName.Text.Trim()) == false)
            {
                crlPilotsHistory.Items.Add(txtSelectedPilotName.Text.Trim());
            }

            OpenWebBrowserPanel();

            Resize();
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

        private void button1_Click(object sender, EventArgs e)
        {
            var txtInClip = Clipboard.GetText();

            if (txtInClip.StartsWith("http"))
            {
                if (webBrowser1.Url != null)
                {
                    if (txtInClip != txtUrl.Text)
                    {
                        webBrowser1.Url = new Uri(txtInClip);
                        txtUrl.Text = txtInClip;
                    }
                }
                else
                {
                    webBrowser1.Url = new Uri(txtInClip);
                    txtUrl.Text = txtInClip;
                }

                
            }

            
            webBrowser1.Visible = true;
            OpenWebBrowserPanel();
            Resize();

            txtUrl.Focus();

        }

        private void Event_ExecuteUrlInWhlBrowser(object sender, EventArgs e)
        {
            if (txtUrl.Text.StartsWith("http"))
            {
                webBrowser1.Url = new Uri(txtUrl.Text);
            }
            else
            {
                webBrowser1.Url = new Uri("http://" + txtUrl.Text);
            }
        }

        private void cmbPilots_SelectedValueChanged(object sender, EventArgs e)
        {
            _pilotes.SetSelected(cmbPilots.Text);

            OpenAuthorizationPanel();
        }

        






    }
}
