using System;
using System.Windows.Forms;

namespace EveJimaCore
{
    public partial class WindowLoadingError : Form
    {
        public WindowLoadingError()
        {
            InitializeComponent();
        }

        private void Event_EndApplication(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
