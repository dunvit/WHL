using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace WHL
{
    public partial class WindowAbout : Form
    {
        #region WinAPI

        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        #endregion
        
        public WindowAbout()
        {
            InitializeComponent();
        }


        private void WindowAbout_Load(object sender, EventArgs e)
        {
            ControlBox = false;
            Text = String.Empty;
            FormBorderStyle = FormBorderStyle.FixedSingle;
        }

        protected override void WndProc(ref Message m)
        {

            if (isResizible == false)
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

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void Event_TitleBarMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && e.Clicks==1)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void Event_WindowResize(object sender, MouseEventArgs e)
        {
            Size = new Size(this.PointToClient(MousePosition).X, this.PointToClient(MousePosition).Y);
        }

        private void Event_WindowResizeEnd(object sender, EventArgs e)
        {
            string a = ";";

            
        }

        private void Event_WindowResize(object sender, EventArgs e)
        {
            TitleBar.Width = Width;
        }

        private bool isMinimazed;
        private int lastHeight;

        private void Event_TitleBarDoubleClick(object sender, EventArgs e)
        {
            if (isMinimazed)
            {
                isMinimazed = false;
                Height = lastHeight;
                Refresh();
            }
            else
            {
                isMinimazed = true;
                lastHeight = Height;
                Height = 12;
                Refresh();
            }
        }

        private void WindowAbout_DoubleClick(object sender, EventArgs e)
        {
            if (isMinimazed)
            {
                isMinimazed = false;
                Height = lastHeight;
                Refresh();
            }
            else
            {
                isMinimazed = true;
                lastHeight = Height;
                Height = 28;
                Refresh();
            }
        }

        private bool isResizible;

        private void button1_Click(object sender, EventArgs e)
        {
            isResizible = !isResizible;
        }
    }
}
