#pragma warning disable CS8600
using System;
using System.Drawing;
using System.Windows.Forms;

namespace MegaEngineeringSuite
{
    public class HeaderControl : UserControl
    {
        private Button btnToggleTheme;

        public HeaderControl()
        {
            this.Size = new Size(90, 40);
            this.BackColor = Color.Transparent;

            btnToggleTheme = new Button
            {
                Text = ThemeManager.IsDarkMode ? "☀️ Light" : "🌙 Dark",
                Dock = DockStyle.Fill,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnToggleTheme.FlatAppearance.BorderSize = 0;
            btnToggleTheme.Click += BtnToggleTheme_Click;

            this.Controls.Add(btnToggleTheme);

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
