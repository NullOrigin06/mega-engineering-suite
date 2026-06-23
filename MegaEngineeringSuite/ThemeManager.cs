using System;
using System.Drawing;
using System.Windows.Forms;

namespace MegaEngineeringSuite
{
    public static class ThemeManager
    {
        public static bool IsDarkMode { get; private set; } = true;
        
        public static event EventHandler ThemeChanged = delegate { };

        public static void ToggleTheme()
        {
            IsDarkMode = !IsDarkMode;
            ThemeChanged.Invoke(null, EventArgs.Empty);
        }

        public static void ApplyTheme(Control control)
        {
            Color backColor = IsDarkMode ? Color.FromArgb(30, 30, 35) : Color.FromArgb(240, 244, 248);
            Color foreColor = IsDarkMode ? Color.White : Color.FromArgb(20, 40, 80);
            Color buttonBackColor = IsDarkMode ? Color.FromArgb(60, 60, 70) : Color.FromArgb(0, 120, 215);
            Color buttonForeColor = Color.White;
            Color panelBackColor = IsDarkMode ? Color.FromArgb(40, 40, 45) : Color.FromArgb(220, 230, 240);
            Color gridBackColor = IsDarkMode ? Color.FromArgb(40, 40, 45) : Color.White;
            Color gridTextColor = IsDarkMode ? Color.White : Color.Black;
            Color textBoxBackColor = IsDarkMode ? Color.FromArgb(50, 50, 55) : Color.White;

            ApplyThemeRecursive(control, backColor, foreColor, buttonBackColor, buttonForeColor, panelBackColor, gridBackColor, gridTextColor, textBoxBackColor);
        }

        private static void ApplyThemeRecursive(Control control, Color backColor, Color foreColor, Color buttonBackColor, Color buttonForeColor, Color panelBackColor, Color gridBackColor, Color gridTextColor, Color textBoxBackColor)
        {
            if (control is Form form)
            {
                form.BackColor = backColor;
                form.ForeColor = foreColor;
            }
            else if (control is Panel || control is TableLayoutPanel || control is FlowLayoutPanel)
            {
                if (control.BackColor != Color.Transparent)
                {
                    control.BackColor = panelBackColor;
                }
                control.ForeColor = foreColor;
            }
            else if (control is Button btn)
            {
                btn.BackColor = buttonBackColor;
                btn.ForeColor = buttonForeColor;
                btn.FlatStyle = FlatStyle.Flat;
                btn.FlatAppearance.BorderSize = 0;
            }
            else if (control is TextBox txt)
            {
                txt.BackColor = textBoxBackColor;
                txt.ForeColor = foreColor;
                txt.BorderStyle = BorderStyle.FixedSingle;
            }
            else if (control is ComboBox cmb)
            {
                cmb.BackColor = textBoxBackColor;
                cmb.ForeColor = foreColor;
                cmb.FlatStyle = FlatStyle.Flat;
            }
            else if (control is DataGridView dgv)
            {
                dgv.BackgroundColor = gridBackColor;
                dgv.DefaultCellStyle.BackColor = gridBackColor;
                dgv.DefaultCellStyle.ForeColor = gridTextColor;
                dgv.ColumnHeadersDefaultCellStyle.BackColor = panelBackColor;
                dgv.ColumnHeadersDefaultCellStyle.ForeColor = foreColor;
                dgv.RowHeadersDefaultCellStyle.BackColor = panelBackColor;
                dgv.EnableHeadersVisualStyles = false;
            }
            else if (control is Label lbl)
            {
                lbl.ForeColor = foreColor;
            }

            foreach (Control child in control.Controls)
            {
                // EngineeringDrawingCanvas has its own theme management if needed, but we can skip it or let it inherit.
                if (child.GetType().Name == "EngineeringDrawingCanvas")
                {
                    child.BackColor = IsDarkMode ? Color.FromArgb(20, 20, 30) : Color.FromArgb(245, 245, 250);
                    child.Invalidate();
                    continue; // Skip recursive for canvas to protect its children (like toolbar) or we can apply it.
                }

                // If HeaderControl, its buttons will be themed normally
                ApplyThemeRecursive(child, backColor, foreColor, buttonBackColor, buttonForeColor, panelBackColor, gridBackColor, gridTextColor, textBoxBackColor);
            }
        }
    }
}
