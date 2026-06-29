using System;
using System.Drawing;
using System.Windows.Forms;

namespace MegaEngineeringSuite
{
    public static class CompanyBrandingService
    {
        private const string BrandLogoName = "pbMegaEngineeringLogo";
        private const string HeaderControlName = "megaHeaderControl";
        private const int BrandMargin = 12;
        private const int HeaderGap = 12;
        private static readonly Size BrandLogoSize = new Size(180, 80);

        public static int ReservedTopMargin => BrandMargin + BrandLogoSize.Height + HeaderGap;

        public static void ApplyBranding(Form form)
        {
            // Update the form title
            if (!form.Text.StartsWith("MEGA ENGINEERING - ", StringComparison.Ordinal))
            {
                form.Text = "MEGA ENGINEERING - " + form.Text;
            }

            ReserveBrandingArea(form);
            AddBrandLogo(form);
            AddThemeToggle(form);

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

        private static void ReserveBrandingArea(Form form)
        {
            if (!HasDockedUserControls(form))
            {
                return;
            }

            form.Padding = new Padding(
                form.Padding.Left,
                Math.Max(form.Padding.Top, ReservedTopMargin),
                form.Padding.Right,
                form.Padding.Bottom);
        }

        private static bool HasDockedUserControls(Form form)
        {
            foreach (Control control in form.Controls)
            {
                if (control.Dock != DockStyle.None)
                {
                    return true;
                }
            }

            return false;
        }

        private static void AddBrandLogo(Form form)
        {
            PictureBox? logo = form.Controls[BrandLogoName] as PictureBox;

            if (logo == null)
            {
                logo = new PictureBox
                {
                    Name = BrandLogoName,
                    SizeMode = PictureBoxSizeMode.Zoom,
                    BackColor = Color.Transparent,
                    TabStop = false
                };

                form.Controls.Add(logo);
            }

            logo.Image = Properties.Resources.MegaLogo;
            logo.Size = BrandLogoSize;
            logo.Location = new Point(BrandMargin, BrandMargin);
            logo.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            logo.BringToFront();
        }

        private static void AddThemeToggle(Form form)
        {
            HeaderControl? header = form.Controls[HeaderControlName] as HeaderControl;

            if (header == null)
            {
                header = new HeaderControl
                {
                    Name = HeaderControlName
                };

                form.Controls.Add(header);
            }

            header.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            PositionThemeToggle(form, header);

            header.BringToFront();

            EventHandler repositionHeader = (s, e) => PositionThemeToggle(form, header);
            form.SizeChanged += repositionHeader;
            form.Shown += repositionHeader;
            form.FormClosed += (s, e) =>
            {
                form.SizeChanged -= repositionHeader;
                form.Shown -= repositionHeader;
            };
        }

        private static void PositionThemeToggle(Form form, HeaderControl header)
        {
            header.Location = new Point(
                Math.Max(BrandMargin, form.ClientSize.Width - header.Width - BrandMargin),
                BrandMargin);
        }
    }
}
