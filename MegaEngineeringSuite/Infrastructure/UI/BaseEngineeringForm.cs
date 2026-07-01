using System;
using System.Drawing;
using System.Windows.Forms;

namespace MegaEngineeringSuite.Infrastructure.UI
{
    public partial class BaseEngineeringForm : Form
    {
        public BaseEngineeringForm()
        {
            InitializeComponent();
            ApplyTheming();
        }

        private void ApplyTheming()
        {
            ThemeManager.ApplyTheme(this);
            // Ensure left panel stays white per requirements
            pnlLeftFixed.BackColor = Color.White;
        }

        protected void SetModuleHeader(string title, string subtitle)
        {
            lblModuleTitle.Text = title;
            lblModuleSubtitle.Text = subtitle;
        }

        protected void SetStatus(string text, bool isWorking)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => SetStatus(text, isWorking)));
                return;
            }
            
            tsStatusLabel.Text = text;
            tsProgressBar.Visible = isWorking;
            if (isWorking)
            {
                tsProgressBar.Style = ProgressBarStyle.Marquee;
            }
            else
            {
                tsProgressBar.Style = ProgressBarStyle.Continuous;
                tsProgressBar.Value = 0;
            }
        }
    }
}
