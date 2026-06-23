using System;
using System.Drawing;
using System.Windows.Forms;

namespace MegaEngineeringSuite
{
    public static class CompanyBrandingService
    {
        public static void ApplyBranding(Form form)
        {
            // Update the form title
            if (!form.Text.StartsWith("MEGA ENGINEERING - "))
            {
                form.Text = "MEGA ENGINEERING - " + form.Text;
            }

            // Add HeaderControl (Logo + Theme Toggle)
            HeaderControl header = new HeaderControl();
            int margin = 10;
            header.Location = new Point(form.ClientSize.Width - header.Width - margin, margin);
            header.Anchor = AnchorStyles.Top | AnchorStyles.Right;

            form.Controls.Add(header);
            header.BringToFront();

            // Apply Theme
            ThemeManager.ApplyTheme(form);

            // Subscribe to theme changes and ensure cleanup on form close to prevent memory leaks
            EventHandler themeHandler = (s, e) => ThemeManager.ApplyTheme(form);
            ThemeManager.ThemeChanged += themeHandler;

            form.FormClosed += (s, e) => 
            {

                ThemeManager.ThemeChanged -= themeHandler;
            };
        }
    }
}
