#pragma warning disable CS8600
using System;
using System.Drawing;
using System.Windows.Forms;

namespace loginpage1
{
    public class HeaderControl : UserControl
    {
        private PictureBox pbLogo;
        private Button btnToggleTheme;

        public HeaderControl()
        {
            this.Size = new Size(300, 70);
            this.BackColor = Color.Transparent;

            pbLogo = new PictureBox
            {
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Transparent,
                Width = 200,
                Height = 70,
                Location = new Point(100, 0),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            try
            {
                object logo = Properties.Resources.ResourceManager.GetObject("MegaLogo");
                if (logo is Image img) pbLogo.Image = img;
            }
            catch { }

            btnToggleTheme = new Button
            {
                Text = ThemeManager.IsDarkMode ? "☀️ Light" : "🌙 Dark",
                Size = new Size(80, 40),
                Location = new Point(10, 15),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnToggleTheme.FlatAppearance.BorderSize = 0;
            btnToggleTheme.Click += BtnToggleTheme_Click;

            this.Controls.Add(btnToggleTheme);
            this.Controls.Add(pbLogo);

            ThemeManager.ThemeChanged += OnThemeChanged;
            UpdateThemeButton();
        }

        private void BtnToggleTheme_Click(object? sender, EventArgs e)
        {
            ThemeManager.ToggleTheme();
        }

        private void OnThemeChanged(object? sender, EventArgs e)
        {
            UpdateThemeButton();
        }

        private void UpdateThemeButton()
        {
            btnToggleTheme.Text = ThemeManager.IsDarkMode ? "☀️ Light" : "🌙 Dark";
            btnToggleTheme.BackColor = ThemeManager.IsDarkMode ? Color.FromArgb(60, 60, 70) : Color.FromArgb(200, 210, 220);
            btnToggleTheme.ForeColor = ThemeManager.IsDarkMode ? Color.White : Color.Black;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ThemeManager.ThemeChanged -= OnThemeChanged;
            }
            base.Dispose(disposing);
        }
    }
}
