#pragma warning disable CS8618
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace MegaEngineeringSuite
{
    public partial class Form3 : Form
    {
        private bool isNavigating = false;
        private ErrorProvider errorProvider;

        // User Inputs
        private TextBox txtHTA;
        private TextBox txtTubeOD;
        private TextBox txtTubeLength;
        private TextBox txtTubeTHK;
        private ComboBox cmbNoOfPass;
        private TextBox txtBaffleQty;

        // Calculated Values
        private TextBox txtTubeQty;
        private TextBox txtShellID;
        private DataGridView dgvProperties;
        private EngineeringDrawingCanvas drawingCanvas;

        // Services
        private ExcelLookupService lookupService;
        private DrawingAutomationService drawingService;
        private GeometryCalculationService geometryService;
        private EngineeringDataModel currentData;
        private GeometryModel currentGeometry;
        private string lastGeneratedLispPath = string.Empty;
        private string lastGeneratedScrPath = string.Empty;

        public Form3()
        {
            InitializeComponent();
            errorProvider = new ErrorProvider();
            errorProvider.BlinkStyle = ErrorBlinkStyle.NeverBlink;
            lookupService = new ExcelLookupService();
            drawingService = new DrawingAutomationService();
            geometryService = new GeometryCalculationService();
            CreateProfessionalUI();
        }

        private void Form3_Load(object? sender, EventArgs e)
        {
        }

        private void CreateProfessionalUI()
        {
            this.Text = "TubeSheet Design Module";
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.FromArgb(240, 244, 248); // Subtle professional background

            Font titleFont = new Font("Segoe UI", 20, FontStyle.Bold);
            Font sectionFont = new Font("Segoe UI", 14, FontStyle.Bold);
            Font labelFont = new Font("Segoe UI", 11);
            Font inputFont = new Font("Segoe UI", 11);

            // 1. MAIN LAYOUT
            TableLayoutPanel mainTable = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 2,
                Padding = new Padding(20)
            };
            
            mainTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            mainTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            mainTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.34F));

            mainTable.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            mainTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 80F));

            // 2. LEFT PANEL: INPUTS
            TableLayoutPanel pnlInputs = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(10)
            };
            pnlInputs.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            pnlInputs.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            pnlInputs.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            
            Label lblTitle = new Label { Text = "3 PHASE TUBESHEET(1/2/4 PASS)", Font = titleFont, AutoSize = true, ForeColor = Color.FromArgb(20, 40, 80), Margin = new Padding(0, 0, 0, 20) };
            pnlInputs.Controls.Add(lblTitle, 0, 0);

            Label lblInputsHeader = new Label { Text = "User Inputs", Font = sectionFont, AutoSize = true, Margin = new Padding(0, 0, 0, 10) };
            pnlInputs.Controls.Add(lblInputsHeader, 0, 1);

            TableLayoutPanel inputGrid = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                ColumnCount = 2,
                AutoSize = true
            };
            inputGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45F));
            inputGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55F));

            txtHTA = AddInputRow(inputGrid, "HTA", labelFont, inputFont, 0);
            txtTubeOD = AddInputRow(inputGrid, "Tube OD (mm)", labelFont, inputFont, 1);
            txtTubeLength = AddInputRow(inputGrid, "Tube Length (mm)", labelFont, inputFont, 2);
            txtTubeTHK = AddInputRow(inputGrid, "Tube THK (mm)", labelFont, inputFont, 3);
            
            Label lblPass = new Label { Text = "No Of Pass", Font = labelFont, Anchor = AnchorStyles.Left, AutoSize = true };
            cmbNoOfPass = new ComboBox { Font = inputFont, DropDownStyle = ComboBoxStyle.DropDownList, Anchor = AnchorStyles.Left | AnchorStyles.Right, Margin = new Padding(3, 8, 3, 8) };
            cmbNoOfPass.Items.AddRange(new object[] { "1", "2", "4" });
            cmbNoOfPass.SelectedIndex = 2; 
            inputGrid.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            inputGrid.Controls.Add(lblPass, 0, 4);
            inputGrid.Controls.Add(cmbNoOfPass, 1, 4);

            txtBaffleQty = AddInputRow(inputGrid, "Baffle Qty", labelFont, inputFont, 5);

            pnlInputs.Controls.Add(inputGrid, 0, 2);
            mainTable.Controls.Add(pnlInputs, 0, 0);

            // 3. CENTER PANEL: CALCULATED VALUES
            TableLayoutPanel pnlCalculated = new TableLayoutPanel 
            { 
                Dock = DockStyle.Fill, 
                ColumnCount = 1,
                RowCount = 6,
                Padding = new Padding(20), 
                BackColor = Color.FromArgb(220, 230, 240) 
            }; 
            pnlCalculated.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            pnlCalculated.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            pnlCalculated.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            pnlCalculated.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            pnlCalculated.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            pnlCalculated.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            
            Label lblCalcHeader = new Label { Text = "Calculated Values", Font = sectionFont, AutoSize = true, Margin = new Padding(0, 0, 0, 20) };
            pnlCalculated.Controls.Add(lblCalcHeader, 0, 0);

            Label lblTubeQty = new Label { Text = "Tube Qty", Font = labelFont, AutoSize = true, Margin = new Padding(0, 10, 0, 5) };
            txtTubeQty = new TextBox { Font = new Font("Segoe UI", 16, FontStyle.Bold), ReadOnly = true, BackColor = Color.White, Anchor = AnchorStyles.Left | AnchorStyles.Right };
            pnlCalculated.Controls.Add(lblTubeQty, 0, 1);
            pnlCalculated.Controls.Add(txtTubeQty, 0, 2);

            Label lblShellID = new Label { Text = "Shell ID", Font = labelFont, AutoSize = true, Margin = new Padding(0, 20, 0, 5) };
            txtShellID = new TextBox { Font = new Font("Segoe UI", 16, FontStyle.Bold), ReadOnly = true, BackColor = Color.White, Anchor = AnchorStyles.Left | AnchorStyles.Right };
            pnlCalculated.Controls.Add(lblShellID, 0, 3);
            pnlCalculated.Controls.Add(txtShellID, 0, 4);

            Panel canvasScrollPanel = new Panel 
            { 
                Dock = DockStyle.Fill, 
                AutoScroll = true, 
                Margin = new Padding(0, 20, 0, 0), 
                BorderStyle = BorderStyle.FixedSingle 
            };
            
            drawingCanvas = new EngineeringDrawingCanvas { Margin = new Padding(0) };
            canvasScrollPanel.Controls.Add(drawingCanvas);
            pnlCalculated.Controls.Add(canvasScrollPanel, 0, 5);

            mainTable.Controls.Add(pnlCalculated, 1, 0);

            // 4. RIGHT PANEL: PROPERTY GRID
            TableLayoutPanel pnlGrid = new TableLayoutPanel 
            { 
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(10) 
            };
            pnlGrid.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            pnlGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            
            Label lblGridHeader = new Label { Text = "Engineering Properties (Excel Lookup)", Font = sectionFont, AutoSize = true, Margin = new Padding(0, 0, 0, 10) };
            pnlGrid.Controls.Add(lblGridHeader, 0, 0);

            dgvProperties = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                BackgroundColor = Color.White,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                Font = labelFont
            };
            
            dgvProperties.Columns.Add("Parameter", "Parameter Name");
            dgvProperties.Columns.Add("Value", "Value");

            pnlGrid.Controls.Add(dgvProperties, 0, 1);
            mainTable.Controls.Add(pnlGrid, 2, 0);

            // 5. BOTTOM PANEL: BUTTONS
            FlowLayoutPanel pnlButtons = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(0),
                WrapContents = false
            };
            mainTable.SetColumnSpan(pnlButtons, 3);

            Button btnCalculate = new Button { Text = "Calculate", Font = sectionFont, Size = new Size(180, 50), BackColor = Color.FromArgb(0, 120, 215), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Margin = new Padding(0, 10, 15, 10) };
            btnCalculate.Click += BtnCalculate_Click;
            
            Button btnGenerate = new Button { Text = "Generate Drawing", Font = sectionFont, Size = new Size(220, 50), Margin = new Padding(0, 10, 15, 10) };
            btnGenerate.Click += BtnGenerate_Click;
            
            Button btnOpenLisp = new Button { Text = "Open Generated LISP", Font = sectionFont, Size = new Size(250, 50), Margin = new Padding(0, 10, 15, 10) };
            btnOpenLisp.Click += BtnOpenLisp_Click;


            
            Button btnOpenScr = new Button { Text = "Open Generated SCR", Font = sectionFont, Size = new Size(250, 50), Margin = new Padding(0, 10, 15, 10) };
            btnOpenScr.Click += BtnOpenScr_Click;
            
            Button btnExport = new Button { Text = "Export Data", Font = sectionFont, Size = new Size(180, 50), Margin = new Padding(0, 10, 15, 10) };
            btnExport.Click += BtnExport_Click;
            
            Button btnBack = new Button { Text = "Back", Font = sectionFont, Size = new Size(150, 50), Margin = new Padding(0, 10, 0, 10) };
            btnBack.Click += BtnBack_Click;

            pnlButtons.Controls.Add(btnCalculate);
            pnlButtons.Controls.Add(btnGenerate);
            pnlButtons.Controls.Add(btnOpenLisp);
            pnlButtons.Controls.Add(btnOpenScr);
            pnlButtons.Controls.Add(btnExport);
            pnlButtons.Controls.Add(btnBack);

            mainTable.Controls.Add(pnlButtons, 0, 1);

            Controls.Add(mainTable);
            
            // Apply Mega Engineering Branding
            CompanyBrandingService.ApplyBranding(this);
        }

        private TextBox AddInputRow(TableLayoutPanel panel, string labelText, Font lblFont, Font txtFont, int row)
        {
            Label lbl = new Label { Text = labelText, Font = lblFont, Anchor = AnchorStyles.Left, AutoSize = true };
            TextBox txt = new TextBox { Font = txtFont, Anchor = AnchorStyles.Left | AnchorStyles.Right, Margin = new Padding(3, 8, 3, 8) };
            
            panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            panel.Controls.Add(lbl, 0, row);
            panel.Controls.Add(txt, 1, row);
            
            return txt;
        }

        private bool ValidateInputs()
        {
            bool isValid = true;
            errorProvider.Clear();

            if (!ValidateNumericGreaterThanZero(txtHTA, "HTA must be a numeric value greater than 0.")) isValid = false;
            if (!ValidateNumericGreaterThanZero(txtTubeOD, "Tube OD must be a numeric value greater than 0.")) isValid = false;
            if (!ValidateNumericGreaterThanZero(txtTubeLength, "Tube Length must be a numeric value greater than 0.")) isValid = false;
            if (!ValidateNumericGreaterThanZero(txtTubeTHK, "Tube THK must be a numeric value greater than 0.")) isValid = false;
            
            int baffleQty;
            if (!int.TryParse(txtBaffleQty.Text, out baffleQty) || baffleQty <= 0)
            {
                errorProvider.SetError(txtBaffleQty, "Baffle Qty must be an integer greater than 0.");
                isValid = false;
            }

            return isValid;
        }

        private bool ValidateNumericGreaterThanZero(TextBox txt, string errorMessage)
        {
            double val;
            if (!double.TryParse(txt.Text, out val) || val <= 0)
            {
                errorProvider.SetError(txt, errorMessage);
                return false;
            }
            return true;
        }

        private void BtnCalculate_Click(object? sender, EventArgs e)
        {
            if (!ValidateInputs())
            {
                MessageBox.Show("Please correct the validation errors before proceeding.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                double hta = double.Parse(txtHTA.Text);
                double tubeOD = double.Parse(txtTubeOD.Text);
                double tubeLength = double.Parse(txtTubeLength.Text);
                int noOfPass = int.Parse(cmbNoOfPass.SelectedItem?.ToString() ?? "4");

                // Formula 1: Tube Quantity (using the Tubes-Per-Pass rounding logic from Excel Analysis)
                double rawTubeQty = hta / ((tubeOD / 1000.0) * Math.PI * (tubeLength / 1000.0));
                int tubesPerPass = (int)Math.Ceiling(rawTubeQty / noOfPass);
                int tubeQty = tubesPerPass * noOfPass;

                txtTubeQty.Text = tubeQty.ToString();

                // Formula 2: Shell ID
                double shellIdRaw = ((Math.Sqrt(tubeQty) + 1.25 * Math.Sqrt(noOfPass)) * 1.25 * 1.05 * tubeOD + 25);
                int shellId = (int)(Math.Ceiling(shellIdRaw / 10.0) * 10);

                txtShellID.Text = shellId.ToString();

                // Phase 4: Excel Lookup
                currentData = lookupService.LoadByShellId(shellId);
                
                // Assign User Inputs to the Engineering Data Model
                currentData.TubeOD = tubeOD;
                currentData.TubeQty = tubeQty;
                currentData.NoOfPass = noOfPass;
                currentData.HTA = hta;
                currentData.TubeLength = tubeLength;
                
                if (int.TryParse(txtBaffleQty.Text, out int bQty))
                    currentData.BaffleQty = bQty;

                PopulateGrid(currentData);

                // Phase 2: Geometry Engine & Validation
                currentGeometry = geometryService.CalculateGeometry(currentData);
                drawingCanvas.LoadDrawing(currentGeometry, currentData); // Load data and trigger redraw
            }

            catch (ArgumentException aex)
            {
                MessageBox.Show(aex.Message, "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred during calculation: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PopulateGrid(EngineeringDataModel data)
        {
            dgvProperties.Rows.Clear();
            var dict = data.ToDisplayDictionary();
            foreach (var kvp in dict)
            {
                dgvProperties.Rows.Add(kvp.Key, kvp.Value);
            }
        }

        private void BtnBack_Click(object? sender, EventArgs e)
        {
            isNavigating = true;
            if (this.Owner != null)
            {
                this.Owner.Show();
            }
            else
            {
                Form2 form = new Form2();
                form.Show();
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

        private void BtnGenerate_Click(object? sender, EventArgs e)
        {
            if (currentData == null || currentGeometry == null)
            {
                MessageBox.Show("Please click Calculate first.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var tempService = new TemplateDrawingService();
                var groupedViews = tempService.GenerateTemplateViews(currentGeometry, currentData);

                string templatePath = @"C:\MEGA_SUITE_DATA\FINAL TUBESHEET.dwg";
                DrawingAutomationResult result = drawingService.GenerateTemplateLispAndLaunchCAD(groupedViews, currentData, templatePath);
                
                lastGeneratedLispPath = result.BackupPath;
                lastGeneratedScrPath = result.BackupScrPath;

                string escapedPath = result.ScriptPath.Replace("\\", "\\\\");
                string msg = $"AutoLISP and SCR scripts generated!\n\n" +
                             $"Generated LSP:\n{escapedPath}\n\n" +
                             $"Generated SCR:\n{result.ScrPath}\n\n" +
                             $"SCR Content:\n{result.ScrContent}\n" +
                             $"Process Arguments:\n\"{result.CadExecutable}\" {result.Arguments}";

                MessageBox.Show(msg, "CAD Launch Debug", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "CAD Launch Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnOpenLisp_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(lastGeneratedLispPath) || !File.Exists(lastGeneratedLispPath))
            {
                MessageBox.Show("No generated LISP file found. Please generate the drawing first.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            try
            {
                Process.Start(new ProcessStartInfo("notepad.exe", $"\"{lastGeneratedLispPath}\"") { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open LISP file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnOpenScr_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(lastGeneratedScrPath) || !File.Exists(lastGeneratedScrPath))
            {
                MessageBox.Show("No generated SCR file found. Please generate the drawing first.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            try
            {
                Process.Start(new ProcessStartInfo("notepad.exe", $"\"{lastGeneratedScrPath}\"") { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open SCR file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnExport_Click(object? sender, EventArgs e)
        {
            if (currentData == null)
            {
                MessageBox.Show("Please calculate the engineering parameters first.", "Missing Data", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "Excel Workbook|*.xlsx";
                sfd.Title = "Save Engineering Data";
                sfd.FileName = $"TubeSheet_Data_{currentData.ShellID}_{DateTime.Now:yyyyMMdd}.xlsx";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        using (var workbook = new ClosedXML.Excel.XLWorkbook())
                        {
                            var ws = workbook.Worksheets.Add("Engineering Data");
                            ws.Cell(1, 1).Value = "Parameter";
                            ws.Cell(1, 2).Value = "Value";
                            ws.Range("A1:B1").Style.Font.Bold = true;

                            int row = 2;
                            foreach (var kvp in currentData.ToDisplayDictionary())
                            {
                                ws.Cell(row, 1).Value = kvp.Key;
                                ws.Cell(row, 2).Value = kvp.Value;
                                row++;
                            }

                            ws.Columns().AdjustToContents();
                            workbook.SaveAs(sfd.FileName);
                        }
                        MessageBox.Show("Data exported successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to export data: {ex.Message}", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }


    }
}