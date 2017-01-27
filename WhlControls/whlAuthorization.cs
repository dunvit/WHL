using System;
using System.Diagnostics;
using System.Net;
using System.Windows.Forms;
using log4net;
using WHL.BLL;

namespace WHL.WhlControls
{
    public partial class whlAuthorization : UserControl
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(whlAuthorization));

        private const string TextAuthorizationInfo =
            "To login you will need to press the button and go to the  CCP SSO (single sign-on) site. All your private data will remain on the CCP's website.";

        private const string TextAfterAuthorizationInfo =
            "You have successfully logged into the system and the WHL (WormHoleLocator) can now keep track of your current position. You can log in again with another character.";

        public whlAuthorization()
        {
            InitializeComponent();

            lblAuthorizationInfo.Text = TextAuthorizationInfo;
        }

        private void Event_GoToCCPSSO(object sender, EventArgs e)
        {
            var data = WebUtility.UrlEncode(@"http://localhost:8080/WormholeLocator");
            Process.Start("https://login.eveonline.com/oauth/authorize?response_type=code&redirect_uri=" + data +
                          "&client_id=8f1e2ac9d4aa467c88b12674926dc5e6&scope=characterLocationRead&state=75c68f04aec80589a157fd13");
        }

        public void PilotAuthorizeFlow(string code)
        {
            Log.DebugFormat("[whlAuthorization.PilotAuthorizeFlow] starting for token = {0}", code);

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

        }

        public void RefreshPilotInfo()
        {
            crlPilotPortrait.Image = Global.Pilots.Selected.Portrait;
            crlPilotPortrait.Refresh();

            crlPilotPortrait.Visible = true;

            lblAuthorizationInfo.Text = TextAfterAuthorizationInfo + Environment.NewLine + Environment.NewLine + TextAuthorizationInfo;
        }

        private void cmbPilots_TextChanged(object sender, EventArgs e)
        {
            Global.Pilots.Activate(cmbPilots.Text);

            RefreshPilotInfo();
        }


    }
}
