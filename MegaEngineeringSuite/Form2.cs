using System;
using System.Drawing;
using System.Windows.Forms;

namespace MegaEngineeringSuite
{
    public partial class Form2 : Form
    {
        private bool isNavigating = false;
        private Label lblTitle = null!;

        private Button btnTubeSheet = null!;
        private Button btnHeatChamber = null!;
        private Button btnCylinder = null!;
        private Button btnLogout = null!;

        public Form2()
        {
            InitializeComponent();
            CreateUI();
        }

        private void Form2_Load(object? sender, EventArgs e)
        {

        }

        private void CreateUI()
        {
            this.Text = "Structure Selection";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized;
            this.AutoScroll = true;

            Font titleFont =
                new Font("Segoe UI", 28, FontStyle.Bold);

            Font buttonFont =
                new Font("Segoe UI", 14, FontStyle.Bold);

            // Title

            lblTitle = new Label();

            lblTitle.Text =
                "SELECT STRUCTURE";

            lblTitle.Font =
                titleFont;

            lblTitle.AutoSize = true;

            lblTitle.Location =
                new Point(550, 50);

            // Tube Sheet Button

            btnTubeSheet = new Button();

            btnTubeSheet.Text =
                "Tube Sheet";

            btnTubeSheet.Font =
                buttonFont;

            btnTubeSheet.Size =
                new Size(350, 100);

            btnTubeSheet.Location =
                new Point(550, 180);

            btnTubeSheet.Anchor =
                AnchorStyles.Top;

            btnTubeSheet.Click +=
                BtnTubeSheet_Click;

            // Heat Chamber Button

            btnHeatChamber = new Button();

            btnHeatChamber.Text =
                "Heat Chamber";

            btnHeatChamber.Font =
                buttonFont;

            btnHeatChamber.Size =
                new Size(350, 100);

            btnHeatChamber.Location =
                new Point(550, 320);

            btnHeatChamber.Anchor =
                AnchorStyles.Top;

            btnHeatChamber.Click +=
                BtnHeatChamber_Click;

            // Cylinder Button

            btnCylinder = new Button();

            btnCylinder.Text =
                "Cylinder";

            btnCylinder.Font =
                buttonFont;

            btnCylinder.Size =
                new Size(350, 100);

            btnCylinder.Location =
                new Point(550, 460);

            btnCylinder.Anchor =
                AnchorStyles.Top;

            btnCylinder.Click +=
                BtnCylinder_Click;

            // Logout Button

            btnLogout = new Button();

            btnLogout.Text =
                "Logout";

            btnLogout.Tag =
                ThemeManager.DangerActionButtonTag;

            btnLogout.Font =
                buttonFont;

            btnLogout.Size =
                new Size(180, 60);

            btnLogout.Location =
                new Point(630, 620);

            btnLogout.Anchor =
                AnchorStyles.Top;

            btnLogout.Click +=
                BtnLogout_Click;

            // Add Controls

            Controls.Add(lblTitle);

            Controls.Add(btnTubeSheet);
            Controls.Add(btnHeatChamber);
            Controls.Add(btnCylinder);

            Controls.Add(btnLogout);
            
            // Apply Mega Engineering Branding
            CompanyBrandingService.ApplyBranding(this);
        }

        private void BtnTubeSheet_Click(
            object? sender,
            EventArgs e)
        {
            isNavigating = true;
            Form3 form = new Form3();
            form.Owner = this.Owner;

            form.Show();

            this.Close();
        }

        private void BtnHeatChamber_Click(
            object? sender,
            EventArgs e)
        {
            MessageBox.Show(
                "Heat Chamber Module Coming Soon",
                "Information",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        
        private void BtnCylinder_Click(
            object? sender,
            EventArgs e)
        {
            MessageBox.Show(
                "Cylinder Module Coming Soon",
                "Information",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void BtnLogout_Click(
            object? sender,
            EventArgs e)
        {
            isNavigating = true;
            if (this.Owner != null)
            {
                this.Owner.Show();
            }
            else
            {
                Form1 loginForm = new Form1();
                loginForm.Show();
            }

            this.Close();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            if (!isNavigating)
            {
                Application.Exit();
            }
        }
    }
}
