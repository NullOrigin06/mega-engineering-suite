using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;
using MegaEngineeringSuite.Infrastructure.UI;
using MegaEngineeringSuite.Infrastructure.Logging;

namespace MegaEngineeringSuite.BonnetFlange
{
    public class BonnetFlangeForm : BaseEngineeringForm
    {
        private NumericUpDown numOD;
        private NumericUpDown numID;
        private NumericUpDown numThickness;
        private NumericUpDown numLinerOD;
        private NumericUpDown numLinerID;
        
        private NumericUpDown numBoltQty;
        private NumericUpDown numHoleDia;
        private NumericUpDown numPCD;

        private CheckBox chkAutoOpen;
        private CheckBox chkOpenFolder;

        private Label lblSummaryText;

        private Button btnGenerate;
        private Button btnOpenDrawing;
        private Button btnOpenFolder;
        private Button btnReset;
        private Button btnBack;

        private string _currentOutputPath = "";
        private readonly Form _previousForm;

        public BonnetFlangeForm(Form previousForm)
        {
            _previousForm = previousForm;
            
            InitializeBonnetFlangeUI();
            
            btnGenerate.Click += BtnGenerate_Click;
            btnOpenDrawing.Click += BtnOpenDrawing_Click;
            btnOpenFolder.Click += BtnOpenFolder_Click;
            btnReset.Click += BtnReset_Click;
            btnBack.Click += BtnBack_Click;

            UpdateSummary();
        }

        private void InitializeBonnetFlangeUI()
        {
            string templateName = Path.GetFileName(AppConfigManager.Current.BonnetTemplatePath);
            SetModuleHeader("BONNET FLANGE GENERATOR", $"Generate engineering drawings from template\nTemplate: {templateName} | Output: {AppConfigManager.Current.BonnetOutputFolder}");

            // The Left panel needs to hold multiple GroupBoxes, docked Top, so they stack.
            // We must add them in reverse order so they dock correctly, OR bring them to front in order.
            // A safer way is to create a layout helper.
            Control[] groups = new Control[]
            {
                CreateGeneralDimensionsGroup(),
                CreateLinerDetailsGroup(),
                CreateBoltDetailsGroup(),
                CreateOutputSettingsGroup(),
                CreateSummaryGroup(),
                CreateActionsGroup()
            };

            foreach (var grp in groups)
            {
                pnlLeftFixed.Controls.Add(grp);
                grp.BringToFront(); // Ensures it goes below the previously added control
            }
        }

        private GroupBox CreateGeneralDimensionsGroup()
        {
            GroupBox gb = new GroupBox();
            gb.Text = "GENERAL DIMENSIONS";
            gb.Dock = DockStyle.Top;
            gb.AutoSize = true;
            gb.Padding = new Padding(10);
            gb.Margin = new Padding(0, 0, 0, 12);

            TableLayoutPanel tlp = CreateGroupTable();
            numOD = AddNumericRow(tlp, 0, "OD (mm)", 1070m);
            numID = AddNumericRow(tlp, 1, "ID (mm)", 932m);
            numThickness = AddNumericRow(tlp, 2, "Thickness (mm)", 36m);

            gb.Controls.Add(tlp);
            return gb;
        }

        private GroupBox CreateLinerDetailsGroup()
        {
            GroupBox gb = new GroupBox();
            gb.Text = "LINER DETAILS";
            gb.Dock = DockStyle.Top;
            gb.AutoSize = true;
            gb.Padding = new Padding(10);
            gb.Margin = new Padding(0, 0, 0, 12);

            TableLayoutPanel tlp = CreateGroupTable();
            numLinerOD = AddNumericRow(tlp, 0, "Liner OD (mm)", 984m);
            numLinerID = AddNumericRow(tlp, 1, "Liner ID (mm)", 920m);

            gb.Controls.Add(tlp);
            return gb;
        }

        private GroupBox CreateBoltDetailsGroup()
        {
            GroupBox gb = new GroupBox();
            gb.Text = "BOLT DETAILS";
            gb.Dock = DockStyle.Top;
            gb.AutoSize = true;
            gb.Padding = new Padding(10);
            gb.Margin = new Padding(0, 0, 0, 12);

            TableLayoutPanel tlp = CreateGroupTable();
            numBoltQty = AddNumericRow(tlp, 0, "Bolt Qty", 0m);
            numBoltQty.DecimalPlaces = 0;
            numHoleDia = AddNumericRow(tlp, 1, "Bolt Hole Dia (mm)", 0m);
            numPCD = AddNumericRow(tlp, 2, "PCD (mm)", 0m);

            gb.Controls.Add(tlp);
            return gb;
        }

        private GroupBox CreateOutputSettingsGroup()
        {
            GroupBox gb = new GroupBox();
            gb.Text = "OUTPUT SETTINGS";
            gb.Dock = DockStyle.Top;
            gb.AutoSize = true;
            gb.Padding = new Padding(10);
            gb.Margin = new Padding(0, 0, 0, 12);

            TableLayoutPanel tlp = CreateGroupTable();
            
            chkAutoOpen = new CheckBox();
            chkAutoOpen.Text = "Open drawing after generation";
            chkAutoOpen.AutoSize = true;
            chkAutoOpen.Dock = DockStyle.Fill;
            tlp.Controls.Add(chkAutoOpen, 0, 0);
            tlp.SetColumnSpan(chkAutoOpen, 2);

            chkOpenFolder = new CheckBox();
            chkOpenFolder.Text = "Open output folder";
            chkOpenFolder.AutoSize = true;
            chkOpenFolder.Dock = DockStyle.Fill;
            tlp.Controls.Add(chkOpenFolder, 0, 1);
            tlp.SetColumnSpan(chkOpenFolder, 2);

            gb.Controls.Add(tlp);
            return gb;
        }

        private GroupBox CreateSummaryGroup()
        {
            GroupBox gb = new GroupBox();
            gb.Text = "CURRENT PARAMETERS";
            gb.Dock = DockStyle.Top;
            gb.AutoSize = true;
            gb.Padding = new Padding(10);
            gb.Margin = new Padding(0, 0, 0, 12);

            lblSummaryText = new Label();
            lblSummaryText.Dock = DockStyle.Fill;
            lblSummaryText.AutoSize = true;
            lblSummaryText.Font = new Font("Consolas", 10F);
            lblSummaryText.Margin = new Padding(5);
            
            gb.Controls.Add(lblSummaryText);
            return gb;
        }

        private Panel CreateActionsGroup()
        {
            Panel pnl = new Panel();
            pnl.Dock = DockStyle.Top;
            pnl.AutoSize = true;
            pnl.Padding = new Padding(0, 20, 0, 20);

            TableLayoutPanel tlp = new TableLayoutPanel();
            tlp.Dock = DockStyle.Top;
            tlp.AutoSize = true;
            tlp.ColumnCount = 1;
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            
            btnGenerate = CreateButton("📄 Generate", Color.FromArgb(0, 120, 215), Color.White);
            btnOpenDrawing = CreateButton("🖼 Open Drawing", Color.WhiteSmoke, Color.Black);
            btnOpenFolder = CreateButton("📁 Open Folder", Color.WhiteSmoke, Color.Black);
            btnReset = CreateButton("↺ Reset", Color.WhiteSmoke, Color.Black);
            btnBack = CreateButton("⬅ Back", Color.FromArgb(220, 220, 220), Color.Black);

            tlp.RowCount = 5;
            tlp.Controls.Add(btnGenerate, 0, 0);
            tlp.Controls.Add(btnOpenDrawing, 0, 1);
            tlp.Controls.Add(btnOpenFolder, 0, 2);
            tlp.Controls.Add(btnReset, 0, 3);
            tlp.Controls.Add(btnBack, 0, 4);

            pnl.Controls.Add(tlp);
            return pnl;
        }

        private Button CreateButton(string text, Color back, Color fore)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.Dock = DockStyle.Fill;
            btn.Height = 45;
            btn.Margin = new Padding(0, 0, 0, 10);
            btn.FlatStyle = FlatStyle.Flat;
            btn.BackColor = back;
            btn.ForeColor = fore;
            btn.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btn.Cursor = Cursors.Hand;
            return btn;
        }

        private TableLayoutPanel CreateGroupTable()
        {
            TableLayoutPanel tlp = new TableLayoutPanel();
            tlp.Dock = DockStyle.Top;
            tlp.AutoSize = true;
            tlp.ColumnCount = 2;
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45F));
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55F));
            return tlp;
        }

        private NumericUpDown AddNumericRow(TableLayoutPanel tlp, int rowIndex, string labelText, decimal defaultValue)
        {
            tlp.RowCount = Math.Max(tlp.RowCount, rowIndex + 1);
            tlp.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            Label lbl = new Label();
            lbl.Text = labelText;
            lbl.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            lbl.AutoSize = true;

            NumericUpDown num = new NumericUpDown();
            num.Width = 120; // 120px minimum requested
            num.Minimum = 0;
            num.Maximum = 100000;
            num.DecimalPlaces = 2;
            num.Value = defaultValue;
            num.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            num.ValueChanged += (s, e) => UpdateSummary();

            tlp.Controls.Add(lbl, 0, rowIndex);
            tlp.Controls.Add(num, 1, rowIndex);

            return num;
        }

        private void UpdateSummary()
        {
            if (lblSummaryText == null) return;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"OD...............{numOD.Value} mm");
            sb.AppendLine($"ID...............{numID.Value} mm");
            sb.AppendLine($"Thickness........{numThickness.Value} mm");
            sb.AppendLine($"Liner OD.........{numLinerOD.Value} mm");
            sb.AppendLine($"Liner ID.........{numLinerID.Value} mm");
            
            lblSummaryText.Text = sb.ToString();
        }

        private bool ValidateInputs()
        {
            StringBuilder errors = new StringBuilder();

            if (numThickness.Value <= 0)
                errors.AppendLine("• Thickness must be greater than 0");
            
            if (numOD.Value <= numID.Value)
                errors.AppendLine("• OD must be greater than ID");

            if (numLinerOD.Value <= numLinerID.Value && (numLinerOD.Value > 0 || numLinerID.Value > 0))
                errors.AppendLine("• Liner OD must be greater than Liner ID");

            if (errors.Length > 0)
            {
                MessageBox.Show($"Missing / Invalid Inputs\n\n{errors.ToString()}", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        private void SetInputsEnabled(bool enabled)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => SetInputsEnabled(enabled)));
                return;
            }

            pnlLeftFixed.Enabled = enabled;
        }

        private async void BtnGenerate_Click(object? sender, EventArgs e)
        {
            if (!ValidateInputs()) return;

            try
            {
                SetInputsEnabled(false);
                SetStatus("Generating", true);

                var data = new BonnetFlangeData
                {
                    OD = (double)numOD.Value,
                    ID = (double)numID.Value,
                    Thickness = (double)numThickness.Value,
                    LinerOD = (double)numLinerOD.Value,
                    LinerID = (double)numLinerID.Value
                };

                SetStatus("Opening CAD and Replacing...", true);

                string outputPath = await Task.Run(() =>
                {
                    var generator = new BonnetFlangeGenerator();
                    return generator.Generate(data);
                });

                _currentOutputPath = outputPath;
                SetStatus("Finished", false);

                if (chkAutoOpen.Checked)
                    BtnOpenDrawing_Click(null, EventArgs.Empty);
                
                if (chkOpenFolder != null && chkOpenFolder.Checked)
                    BtnOpenFolder_Click(null, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                SetStatus("Error", false);
                MessageBox.Show($"An error occurred:\n{ex.Message}", "Generation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SetInputsEnabled(true);
            }
        }

        private void BtnOpenDrawing_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_currentOutputPath) || !File.Exists(_currentOutputPath)) return;

            string cadExe = AppConfigManager.Current.CadPath;
            if (!string.IsNullOrEmpty(cadExe) && File.Exists(cadExe))
            {
                Process.Start(new ProcessStartInfo { FileName = cadExe, Arguments = $"\"{_currentOutputPath}\"" });
            }
            else
            {
                Process.Start(new ProcessStartInfo { FileName = _currentOutputPath, UseShellExecute = true });
            }
        }

        private void BtnOpenFolder_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_currentOutputPath)) return;
            string folderPath = Path.GetDirectoryName(_currentOutputPath) ?? string.Empty;
            if (Directory.Exists(folderPath))
            {
                Process.Start(new ProcessStartInfo { FileName = folderPath, UseShellExecute = true });
            }
        }

        private void BtnReset_Click(object? sender, EventArgs e)
        {
            numOD.Value = 1070m;
            numID.Value = 932m;
            numThickness.Value = 36m;
            numLinerOD.Value = 984m;
            numLinerID.Value = 920m;
            numBoltQty.Value = 0m;
            numHoleDia.Value = 0m;
            numPCD.Value = 0m;
            chkAutoOpen.Checked = false;
            chkOpenFolder.Checked = false;
        }

        private void BtnBack_Click(object? sender, EventArgs e)
        {
            this.Hide();
            _previousForm.Show();
        }
    }
}
