using System;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace MegaEngineeringSuite
{
    public partial class Form1 : Form
    {
        private Label lblTitle = null!;
        private Label lblUser = null!;
        private Label lblPass = null!;
        private Label lblStatus = null!;

        private TextBox txtUser = null!;
        private TextBox txtPass = null!;

        private CheckBox chkShow = null!;

        private Button btnLogin = null!;
        private Button btnClear = null!;
        private Button btnExit = null!;

        private ErrorProvider errorProvider = null!;

        public Form1()
        {
            InitializeComponent();
            CreateUI();
        }

        private void CreateUI()
        {
            this.Text = "Mega Engineering Login";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized;
            this.AutoScroll = true;

            Font normalFont = new Font("Segoe UI", 12);
            Font titleFont = new Font("Segoe UI", 28, FontStyle.Bold);

            errorProvider = new ErrorProvider();

            // Title
            lblTitle = new Label();
            lblTitle.Text = "MEGA ENGINEERING";
            lblTitle.Font = titleFont;
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(550, 40);

            // Username Label
            lblUser = new Label();
            lblUser.Text = "Username";
            lblUser.Font = normalFont;
            lblUser.AutoSize = true;
            lblUser.Location = new Point(450, 180);

            // Username TextBox
            txtUser = new TextBox();
            txtUser.Font = normalFont;
            txtUser.Location = new Point(650, 175);
            txtUser.Size = new Size(400, 35);

            txtUser.Anchor =
                AnchorStyles.Top |
                AnchorStyles.Left |
                AnchorStyles.Right;

            // Password Label
            lblPass = new Label();
            lblPass.Text = "Password";
            lblPass.Font = normalFont;
            lblPass.AutoSize = true;
            lblPass.Location = new Point(450, 260);

            // Password TextBox
            txtPass = new TextBox();
            txtPass.Font = normalFont;
            txtPass.Location = new Point(650, 255);
            txtPass.Size = new Size(400, 35);
            txtPass.UseSystemPasswordChar = true;

            txtPass.Anchor =
                AnchorStyles.Top |
                AnchorStyles.Left |
                AnchorStyles.Right;

            // Show Password
            chkShow = new CheckBox();
            chkShow.Text = "Show Password";
            chkShow.Font = normalFont;
            chkShow.AutoSize = true;
            chkShow.Location = new Point(650, 310);

            chkShow.CheckedChanged += (s, e) =>
            {
                txtPass.UseSystemPasswordChar = !chkShow.Checked;
            };

            // Login Button
            btnLogin = new Button();
            btnLogin.Text = "Login";
            btnLogin.Font = normalFont;
            btnLogin.Size = new Size(130, 50);
            btnLogin.Location = new Point(500, 420);

            btnLogin.Click += BtnLogin_Click;

            // Clear Button
            btnClear = new Button();
            btnClear.Text = "Clear";
            btnClear.Font = normalFont;
            btnClear.Size = new Size(130, 50);
            btnClear.Location = new Point(670, 420);

            btnClear.Click += (s, e) =>
            {
                txtUser.Clear();
                txtPass.Clear();
                errorProvider.Clear();
                lblStatus.Text = "Status : Cleared";
            };

            // Exit Button
            btnExit = new Button();
            btnExit.Text = "Exit";
            btnExit.Font = normalFont;
            btnExit.Size = new Size(130, 50);
            btnExit.Location = new Point(840, 420);

            btnExit.Click += (s, e) =>
            {
                Application.Exit();
            };

            // Status Label
            lblStatus = new Label();
            lblStatus.Text = "Status : Not Logged In";
            lblStatus.Font = normalFont;
            lblStatus.AutoSize = true;
            lblStatus.Location = new Point(650, 520);

            Controls.Add(lblTitle);

            Controls.Add(lblUser);
            Controls.Add(txtUser);

            Controls.Add(lblPass);
            Controls.Add(txtPass);

            Controls.Add(chkShow);

            Controls.Add(btnLogin);
            Controls.Add(btnClear);
            Controls.Add(btnExit);

            Controls.Add(lblStatus);

            // Apply Mega Engineering Branding
            CompanyBrandingService.ApplyBranding(this);
        }

        private void BtnLogin_Click(object? sender, EventArgs e)
        {
            errorProvider.Clear();

            string username = txtUser.Text.Trim();
            string password = txtPass.Text;

            // Username Validation
            if (string.IsNullOrWhiteSpace(username))
            {
                errorProvider.SetError(txtUser,
                    "Username is required");
                txtUser.Focus();
                return;
            }

            if (!Regex.IsMatch(username,
                @"^[A-Za-z][A-Za-z0-9_]*$"))
            {
                errorProvider.SetError(txtUser,
                    "Invalid username format");
                txtUser.Focus();
                return;
            }

            // Password Validation
            if (string.IsNullOrWhiteSpace(password))
            {
                errorProvider.SetError(txtPass,
                    "Password is required");
                txtPass.Focus();
                return;
            }

            if (password.Length < 8)
            {
                errorProvider.SetError(txtPass,
                    "Minimum 8 characters");
                txtPass.Focus();
                return;
            }

            // Login Check
            if (username == "MMane" &&
                password == "Mega@2026")
            {
                lblStatus.Text =
                    "Status : Login Successful";

                MessageBox.Show(
                    "Welcome MMane",
                    "Login Successful",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                Form2 form = new Form2();
                form.Owner = this;

                form.Show();

                this.Hide();
            }
            else
            {
                lblStatus.Text =
                    "Status : Invalid Login";

                MessageBox.Show(
                    "Invalid Username or Password",
                    "Login Failed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                txtPass.Clear();
                txtPass.Focus();
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            Application.Exit();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}