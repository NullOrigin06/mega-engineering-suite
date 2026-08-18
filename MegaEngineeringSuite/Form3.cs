#pragma warning disable CS8618
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

using MegaEngineeringSuite.Infrastructure.Logging;

namespace MegaEngineeringSuite
{
    public partial class Form3 : Form
    {
        private bool isNavigating = false;
        private ErrorProvider errorProvider;

        // User Inputs
        private ComboBox cmbHTA;
        private ComboBox cmbTubeOD;
        private ComboBox cmbTubeLength;
        private ComboBox cmbTubeTHK;
        private ComboBox cmbNoOfPass;
        private ComboBox cmbBaffleQty;

        // Calculated Values
        private TextBox txtTubeQty;
        private TextBox txtShellID;
        // Drawing Information
        private ComboBox cmbCustomerName;
        private ComboBox cmbTitle;
        private ComboBox cmbProjectNo;
        private ComboBox cmbDrawingNo;
        private ComboBox cmbRevision;
        private ComboBox cmbDate;
        private ComboBox cmbPreparedBy;
        private ComboBox cmbCheckedBy;
        private ComboBox cmbApprovedBy;

        private DataGridView dgvProperties;
        private DataGridView dgvExtras;
        private Label lblValidationStatus;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel lblStatusReady;
        private ToolStripStatusLabel lblStatusExcel;
        private ToolStripStatusLabel lblStatusCAD;
        private ToolStripStatusLabel lblStatusTime;
        private ToolStripStatusLabel lblStatusGenerated;

        private readonly HashSet<Control> invalidControls = new HashSet<Control>();
        private static readonly string[] EngineeringPropertyNames =
        {
            "Shell I.D.",
            "Tube Sheet Finish THK",
            "Body Flange Finish THK",
            "Partition Plate THK",
            "Baffle THK",
            "Bolt Size",
            "Bolt Length",
            "No Of Bolts",
            "Flange I.D.",
            "Tube Sheet Finish O.D.",
            "Tie Rod Qty."
        };

        private static readonly string[] ExtrasPropertyNames =
        {
            "Bonnet Shell FS Length",
            "Bonnet Shell RS Length",
            "Dishend THK"
        };

        // Services
        private ExcelLookupService lookupService;
        private DrawingAutomationService drawingService;
        private GeometryCalculationService geometryService;
        private EngineeringDataModel currentData;
        private GeometryModel currentGeometry;
        private string lastGeneratedLispPath = string.Empty;
        private string lastGeneratedScrPath = string.Empty;
        private static readonly System.Threading.SemaphoreSlim _generationLock = new System.Threading.SemaphoreSlim(1, 1);

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
            this.MinimumSize = new Size(1400, 800);
            this.BackColor = Color.FromArgb(240, 244, 248);

            Font titleFont = new Font("Segoe UI", 20, FontStyle.Bold);
            Font sectionFont = new Font("Segoe UI Semibold", 11);
            Font labelFont = new Font("Segoe UI", 10);
            Font inputFont = new Font("Segoe UI", 10);
            Font smallStatusFont = new Font("Segoe UI", 9, FontStyle.Bold);

            // 1. MAIN LAYOUT
            TableLayoutPanel mainTable = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 3,
                Padding = new Padding(20, 0, 20, 16)
            };
            
            // 25 / 35 / 40 Split
            mainTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            mainTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F));
            mainTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));

            mainTable.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Header
            mainTable.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // Content
            mainTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 70F)); // Buttons

            Label lblTitle = new Label { Text = "TUBE SHEET DESIGN MODULE", Font = titleFont, AutoSize = true, ForeColor = Color.FromArgb(20, 40, 80), Margin = new Padding(0, 10, 0, 15) };
            mainTable.Controls.Add(lblTitle, 0, 0);
            mainTable.SetColumnSpan(lblTitle, 3);

            // 2. LEFT PANEL: USER INPUTS
            GroupBox grpInputs = new GroupBox
            {
                Text = "USER INPUTS",
                Font = sectionFont,
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 10, 0),
                Padding = new Padding(15)
            };

            TableLayoutPanel inputGrid = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                ColumnCount = 2,
                AutoSize = true,
                Padding = new Padding(0)
            };
            inputGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45F));
            inputGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55F));

            cmbHTA = AddComboRow(inputGrid, "HTA", labelFont, inputFont, 0, false);
            cmbTubeOD = AddComboRow(inputGrid, "Tube OD (mm)", labelFont, inputFont, 1, false);
            cmbTubeLength = AddComboRow(inputGrid, "Tube Length (mm)", labelFont, inputFont, 2, false);
            cmbTubeTHK = AddComboRow(inputGrid, "Tube THK (mm)", labelFont, inputFont, 3, false);
            cmbNoOfPass = AddComboRow(inputGrid, "No Of Pass", labelFont, inputFont, 4, false);
            cmbBaffleQty = AddComboRow(inputGrid, "Baffle Qty", labelFont, inputFont, 5, false);

            lblValidationStatus = new Label
            {
                Text = string.Empty,
                Font = smallStatusFont,
                AutoSize = true,
                ForeColor = Color.FromArgb(248, 113, 113),
                Margin = new Padding(0, 15, 0, 0)
            };
            inputGrid.Controls.Add(lblValidationStatus, 0, 6);
            inputGrid.SetColumnSpan(lblValidationStatus, 2);

            grpInputs.Controls.Add(inputGrid);
            mainTable.Controls.Add(grpInputs, 0, 1);

            // 3. CENTER PANEL: DRAWING INFO + SUMMARY
            TableLayoutPanel centerLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Margin = new Padding(5, 0, 5, 0)
            };
            centerLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 65F));
            centerLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 35F));

            GroupBox grpDrawingInfo = new GroupBox
            {
                Text = "PROJECT INFORMATION",
                Font = sectionFont,
                Dock = DockStyle.Fill,
                Padding = new Padding(15)
            };

            TableLayoutPanel pnlDrawInfo = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                ColumnCount = 2,
                AutoSize = true,
                Padding = new Padding(0)
            };
            pnlDrawInfo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F));
            pnlDrawInfo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65F));

            cmbCustomerName = AddComboRow(pnlDrawInfo, "Customer Name", labelFont, inputFont, 0, false);
            cmbTitle = AddComboRow(pnlDrawInfo, "Drawing Title", labelFont, inputFont, 1, false);
            cmbProjectNo = AddComboRow(pnlDrawInfo, "Project No", labelFont, inputFont, 2, false);
            cmbDrawingNo = AddComboRow(pnlDrawInfo, "Drawing No", labelFont, inputFont, 3, false);
            cmbRevision = AddComboRow(pnlDrawInfo, "Revision", labelFont, inputFont, 4, false);
            cmbDate = AddComboRow(pnlDrawInfo, "Date", labelFont, inputFont, 5, false);
            cmbPreparedBy = AddComboRow(pnlDrawInfo, "Prepared By", labelFont, inputFont, 6, false);
            cmbCheckedBy = AddComboRow(pnlDrawInfo, "Checked By", labelFont, inputFont, 7, false);
            cmbApprovedBy = AddComboRow(pnlDrawInfo, "Approved By", labelFont, inputFont, 8, false);

            // Default values
            cmbCustomerName.Items.AddRange(AppConfigManager.Current.CustomerHistory.ToArray());
            cmbDrawingNo.Items.AddRange(AppConfigManager.Current.DrawingNoHistory.ToArray());
            cmbTitle.Items.AddRange(AppConfigManager.Current.DrawingTitleHistory.ToArray());
            cmbProjectNo.Items.AddRange(AppConfigManager.Current.ProjectNoHistory.ToArray());
            cmbRevision.Items.AddRange(AppConfigManager.Current.RevisionHistory.ToArray());
            cmbDate.Items.AddRange(AppConfigManager.Current.DateHistory.ToArray());
            cmbPreparedBy.Items.AddRange(AppConfigManager.Current.PreparedByHistory.ToArray());
            cmbCheckedBy.Items.AddRange(AppConfigManager.Current.CheckedByHistory.ToArray());
            cmbApprovedBy.Items.AddRange(AppConfigManager.Current.ApprovedByHistory.ToArray());

            cmbHTA.Items.AddRange(AppConfigManager.Current.HTAHistory.ToArray());
            cmbTubeOD.Items.AddRange(AppConfigManager.Current.TubeODHistory.ToArray());
            cmbTubeLength.Items.AddRange(AppConfigManager.Current.TubeLengthHistory.ToArray());
            cmbTubeTHK.Items.AddRange(AppConfigManager.Current.TubeTHKHistory.ToArray());
            cmbNoOfPass.Items.AddRange(AppConfigManager.Current.NoOfPassHistory.ToArray());
            cmbBaffleQty.Items.AddRange(AppConfigManager.Current.BaffleQtyHistory.ToArray());
            
            cmbTitle.Text = "product for";
            cmbCustomerName.Text = "parth";
            cmbProjectNo.Text = "25-005";
            cmbDrawingNo.Text = "25-005-FLG-EX-1405";
            cmbPreparedBy.Text = "NSS";
            cmbCheckedBy.Text = "ASK";
            cmbApprovedBy.Text = "ASK";
            cmbRevision.Text = "0";
            cmbDate.Text = DateTime.Today.ToString("dd-MM-yyyy");
            cmbNoOfPass.Text = "4";

            Action<ComboBox, Action<string>> setupComboLearning = (cmb, addHistoryCallback) =>
            {
                cmb.Leave += (s, e) => AddComboValueIfNew(cmb, addHistoryCallback);
                cmb.KeyDown += (s, e) => 
                {
                    if (e.KeyCode == Keys.Enter)
                    {
                        AddComboValueIfNew(cmb, addHistoryCallback);
                        e.Handled = true;
                        e.SuppressKeyPress = true;
                    }
                };
            };

            setupComboLearning(cmbCustomerName, val => { if (!AppConfigManager.Current.CustomerHistory.Contains(val)) { AppConfigManager.Current.CustomerHistory.Add(val); AppConfigManager.Save(); } });
            setupComboLearning(cmbDrawingNo, val => { if (!AppConfigManager.Current.DrawingNoHistory.Contains(val)) { AppConfigManager.Current.DrawingNoHistory.Add(val); AppConfigManager.Save(); } });
            setupComboLearning(cmbTitle, val => { if (!AppConfigManager.Current.DrawingTitleHistory.Contains(val)) { AppConfigManager.Current.DrawingTitleHistory.Add(val); AppConfigManager.Save(); } });
            
            setupComboLearning(cmbProjectNo, val => { if (!AppConfigManager.Current.ProjectNoHistory.Contains(val)) { AppConfigManager.Current.ProjectNoHistory.Add(val); AppConfigManager.Save(); } });
            setupComboLearning(cmbRevision, val => { if (!AppConfigManager.Current.RevisionHistory.Contains(val)) { AppConfigManager.Current.RevisionHistory.Add(val); AppConfigManager.Save(); } });
            setupComboLearning(cmbDate, val => { if (!AppConfigManager.Current.DateHistory.Contains(val)) { AppConfigManager.Current.DateHistory.Add(val); AppConfigManager.Save(); } });
            setupComboLearning(cmbPreparedBy, val => { if (!AppConfigManager.Current.PreparedByHistory.Contains(val)) { AppConfigManager.Current.PreparedByHistory.Add(val); AppConfigManager.Save(); } });
            setupComboLearning(cmbCheckedBy, val => { if (!AppConfigManager.Current.CheckedByHistory.Contains(val)) { AppConfigManager.Current.CheckedByHistory.Add(val); AppConfigManager.Save(); } });
            setupComboLearning(cmbApprovedBy, val => { if (!AppConfigManager.Current.ApprovedByHistory.Contains(val)) { AppConfigManager.Current.ApprovedByHistory.Add(val); AppConfigManager.Save(); } });

            setupComboLearning(cmbHTA, val => { if (!AppConfigManager.Current.HTAHistory.Contains(val)) { AppConfigManager.Current.HTAHistory.Add(val); AppConfigManager.Save(); } });
            setupComboLearning(cmbTubeOD, val => { if (!AppConfigManager.Current.TubeODHistory.Contains(val)) { AppConfigManager.Current.TubeODHistory.Add(val); AppConfigManager.Save(); } });
            setupComboLearning(cmbTubeLength, val => { if (!AppConfigManager.Current.TubeLengthHistory.Contains(val)) { AppConfigManager.Current.TubeLengthHistory.Add(val); AppConfigManager.Save(); } });
            setupComboLearning(cmbTubeTHK, val => { if (!AppConfigManager.Current.TubeTHKHistory.Contains(val)) { AppConfigManager.Current.TubeTHKHistory.Add(val); AppConfigManager.Save(); } });
            setupComboLearning(cmbNoOfPass, val => { if (!AppConfigManager.Current.NoOfPassHistory.Contains(val)) { AppConfigManager.Current.NoOfPassHistory.Add(val); AppConfigManager.Save(); } });
            setupComboLearning(cmbBaffleQty, val => { if (!AppConfigManager.Current.BaffleQtyHistory.Contains(val)) { AppConfigManager.Current.BaffleQtyHistory.Add(val); AppConfigManager.Save(); } });

            grpDrawingInfo.Controls.Add(pnlDrawInfo);
            centerLayout.Controls.Add(grpDrawingInfo, 0, 0);

            // CALCULATED SUMMARY
            GroupBox grpSummary = new GroupBox
            {
                Text = "CALCULATED SUMMARY",
                Font = sectionFont,
                Dock = DockStyle.Fill,
                Padding = new Padding(15),
                Margin = new Padding(0, 10, 0, 0)
            };

            TableLayoutPanel pnlSummary = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                ColumnCount = 2,
                AutoSize = true
            };
            pnlSummary.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            pnlSummary.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));

            Label lblTubeQty = new Label { Text = "Tube Qty", Font = labelFont, AutoSize = true, Margin = new Padding(3, 5, 3, 0) };
            txtTubeQty = new TextBox { Font = inputFont, ReadOnly = true, BackColor = Color.WhiteSmoke, Anchor = AnchorStyles.Left | AnchorStyles.Right, Margin = new Padding(3, 5, 3, 5) };
            pnlSummary.Controls.Add(lblTubeQty, 0, 0);
            pnlSummary.Controls.Add(txtTubeQty, 1, 0);

            Label lblShellID = new Label { Text = "Shell ID", Font = labelFont, AutoSize = true, Margin = new Padding(3, 5, 3, 0) };
            txtShellID = new TextBox { Font = inputFont, ReadOnly = true, BackColor = Color.WhiteSmoke, Anchor = AnchorStyles.Left | AnchorStyles.Right, Margin = new Padding(3, 5, 3, 5) };
            pnlSummary.Controls.Add(lblShellID, 0, 1);
            pnlSummary.Controls.Add(txtShellID, 1, 1);

            grpSummary.Controls.Add(pnlSummary);
            centerLayout.Controls.Add(grpSummary, 0, 1);

            mainTable.Controls.Add(centerLayout, 1, 1);

            // 4. RIGHT PANEL: ENGINEERING DATA & EXTRAS
            TableLayoutPanel rightLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Margin = new Padding(10, 0, 0, 0)
            };
            rightLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 65F));
            rightLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 35F));

            GroupBox grpData = new GroupBox
            {
                Text = "ENGINEERING PARAMETERS",
                Font = sectionFont,
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            dgvProperties = CreateStyledDataGridView();
            grpData.Controls.Add(dgvProperties);
            rightLayout.Controls.Add(grpData, 0, 0);

            GroupBox grpExtras = new GroupBox
            {
                Text = "EXTRAS",
                Font = sectionFont,
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
                Margin = new Padding(0, 5, 0, 0)
            };

            dgvExtras = CreateStyledDataGridView();
            grpExtras.Controls.Add(dgvExtras);
            rightLayout.Controls.Add(grpExtras, 0, 1);

            PopulateEmptyEngineeringProperties();

            dgvProperties.CellEndEdit += (s, e) => SyncUiToDataModel();
            dgvExtras.CellEndEdit += (s, e) => SyncUiToDataModel();

            mainTable.Controls.Add(rightLayout, 2, 1);

            // 5. BOTTOM PANEL: BUTTONS
            TableLayoutPanel pnlButtons = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 6,
                RowCount = 1,
                Padding = new Padding(0, 10, 0, 10),
                Margin = new Padding(0)
            };
            
            for (int i = 0; i < 6; i++)
            {
                pnlButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16.66F));
            }

            Font btnFont = new Font("Segoe UI Semibold", 10);
            
            Button btnCalculate = new Button { Name = "btnCalculate", Text = "Calculate", Tag = ThemeManager.PositiveActionButtonTag, Font = btnFont, Dock = DockStyle.Fill, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Margin = new Padding(0, 0, 10, 0) };
            btnCalculate.Click += BtnCalculate_Click;
            
            Button btnGenerateTubeSheet = new Button { Name = "btnGenerateTubeSheet", Text = "Generate Tube Sheet", Font = btnFont, Dock = DockStyle.Fill, Margin = new Padding(0, 0, 10, 0) };
            btnGenerateTubeSheet.Click += BtnGenerateTubeSheet_Click;
            
            Button btnGenerateBodyFlange = new Button { Name = "btnGenerateBodyFlange", Text = "Generate Body Flange", Font = btnFont, Dock = DockStyle.Fill, Margin = new Padding(0, 0, 10, 0) };
            btnGenerateBodyFlange.Click += BtnGenerateBodyFlange_Click;

            Button btnGenerateHeatExchanger = new Button { Name = "btnGenerateHeatExchanger", Text = "Generate Heat Exchanger", Font = btnFont, Dock = DockStyle.Fill, Margin = new Padding(0, 0, 10, 0) };
            btnGenerateHeatExchanger.Click += BtnGenerateHeatExchanger_Click;
            
            Button btnExport = new Button { Name = "btnExport", Text = "Export Data", Font = btnFont, Dock = DockStyle.Fill, Margin = new Padding(0, 0, 10, 0) };
            btnExport.Click += BtnExport_Click;
            
            Button btnBack = new Button { Name = "btnBack", Text = "Back", Tag = ThemeManager.DangerActionButtonTag, Font = btnFont, Dock = DockStyle.Fill, Margin = new Padding(0) };
            btnBack.Click += BtnBack_Click;

            pnlButtons.Controls.Add(btnCalculate, 0, 0);
            pnlButtons.Controls.Add(btnGenerateTubeSheet, 1, 0);
            pnlButtons.Controls.Add(btnGenerateBodyFlange, 2, 0);
            pnlButtons.Controls.Add(btnGenerateHeatExchanger, 3, 0);
            pnlButtons.Controls.Add(btnExport, 4, 0);
            pnlButtons.Controls.Add(btnBack, 5, 0);

            mainTable.Controls.Add(pnlButtons, 0, 2);
            mainTable.SetColumnSpan(pnlButtons, 3);

            if (Program.StartupValidation != null && !Program.StartupValidation.TemplatesValid)
            {
                btnGenerateTubeSheet.Enabled = false;
                btnGenerateBodyFlange.Enabled = false;
                // We'll leave btnCalculate and btnExport enabled as they don't depend on DWG templates
            }

            Controls.Add(mainTable);
            
            // 6. STATUS STRIP
            statusStrip = new StatusStrip();
            statusStrip.BackColor = Color.White;
            
            lblStatusReady = new ToolStripStatusLabel("Ready") { Margin = new Padding(10, 3, 20, 3) };
            lblStatusExcel = new ToolStripStatusLabel("Excel ✔") { Margin = new Padding(0, 3, 20, 3) };
            lblStatusCAD = new ToolStripStatusLabel("CAD ✔") { Margin = new Padding(0, 3, 20, 3) };
            lblStatusTime = new ToolStripStatusLabel("Time : -") { Margin = new Padding(0, 3, 20, 3) };
            lblStatusGenerated = new ToolStripStatusLabel("Generated : -") { Margin = new Padding(0, 3, 20, 3) };
            
            statusStrip.Items.Add(lblStatusReady);
            statusStrip.Items.Add(new ToolStripSeparator());
            statusStrip.Items.Add(lblStatusExcel);
            statusStrip.Items.Add(new ToolStripSeparator());
            statusStrip.Items.Add(lblStatusCAD);
            statusStrip.Items.Add(new ToolStripSeparator());
            statusStrip.Items.Add(lblStatusTime);
            statusStrip.Items.Add(new ToolStripSeparator());
            statusStrip.Items.Add(lblStatusGenerated);
            
            Controls.Add(statusStrip);

            // Apply Mega Engineering Branding
            CompanyBrandingService.ApplyBranding(this);
        }

        private ComboBox AddComboRow(TableLayoutPanel panel, string labelText, Font lblFont, Font inputFont, int row, bool dropdownList)
        {
            Label lbl = CreateInputLabel(labelText, lblFont);
            ComboBox cmb = new ComboBox 
            { 
                Font = inputFont, 
                Anchor = AnchorStyles.Left | AnchorStyles.Right, 
                Margin = new Padding(3, 5, 3, 5),
                DropDownStyle = dropdownList ? ComboBoxStyle.DropDownList : ComboBoxStyle.DropDown
            };
            if (!dropdownList)
            {
                cmb.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                cmb.AutoCompleteSource = AutoCompleteSource.ListItems;
            }
            cmb.TextChanged += (s, e) => ClearInputValidation(cmb);
            
            panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            panel.Controls.Add(lbl, 0, row);
            panel.Controls.Add(cmb, 1, row);
            
            return cmb;
        }

        private void AddComboValueIfNew(ComboBox comboBox, Action<string> addHistoryCallback)
        {
            string value = comboBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(value))
                return;

            if (!comboBox.Items.Contains(value))
            {
                comboBox.Items.Add(value);
                addHistoryCallback(value);
            }

            comboBox.SelectedItem = value;
        }

        private DrawingInformation GetDrawingInformation()
        {
            DateTime parsedDate = DateTime.Today;
            if (!string.IsNullOrWhiteSpace(cmbDate.Text) && DateTime.TryParseExact(cmbDate.Text, "dd-MM-yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime tempDate))
            {
                parsedDate = tempDate;
            }

            return new DrawingInformation
            {
                CustomerName = cmbCustomerName.Text.Trim(),
                Title = cmbTitle.Text.Trim(),
                ProjectNo = cmbProjectNo.Text.Trim(),
                DrawingNo = cmbDrawingNo.Text.Trim(),
                Revision = cmbRevision.Text.Trim(),
                PreparedBy = cmbPreparedBy.Text.Trim(),
                CheckedBy = cmbCheckedBy.Text.Trim(),
                ApprovedBy = cmbApprovedBy.Text.Trim(),
                Date = parsedDate
            };
        }

        private static Label CreateInputLabel(string text, Font font)
        {
            return new Label
            {
                Text = text,
                Font = font,
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(0, 4, 12, 4),
                MinimumSize = new Size(160, 34)
            };
        }

        private bool ValidateInputs()
        {
            bool isValid = true;
            errorProvider.Clear();
            ClearValidationStyles();

            if (string.IsNullOrWhiteSpace(cmbHTA.Text)) AddError(cmbHTA, "Required");
            if (string.IsNullOrWhiteSpace(cmbTubeOD.Text)) AddError(cmbTubeOD, "Required");
            if (string.IsNullOrWhiteSpace(cmbTubeLength.Text)) AddError(cmbTubeLength, "Required");
            if (string.IsNullOrWhiteSpace(cmbTubeTHK.Text)) AddError(cmbTubeTHK, "Required");
            if (string.IsNullOrWhiteSpace(cmbBaffleQty.Text)) AddError(cmbBaffleQty, "Required");
            
            if (!ValidateNumericGreaterThanZero(cmbHTA, "HTA must be a numeric value greater than 0.")) isValid = false;
            if (!ValidateNumericGreaterThanZero(cmbTubeOD, "Tube OD must be a numeric value greater than 0.")) isValid = false;
            if (!ValidateNumericGreaterThanZero(cmbTubeLength, "Tube Length must be a numeric value greater than 0.")) isValid = false;
            if (!ValidateNumericGreaterThanZero(cmbTubeTHK, "Tube THK must be a numeric value greater than 0.")) isValid = false;
            
            int baffleQty;
            if (!int.TryParse(cmbBaffleQty.Text, out baffleQty) || baffleQty <= 0)
            {
                errorProvider.SetError(cmbBaffleQty, "Baffle Qty must be an integer greater than 0.");
                SetInvalidInput(cmbBaffleQty);
                isValid = false;
            }

            lblValidationStatus.ForeColor = Color.FromArgb(248, 113, 113);
            lblValidationStatus.Text = isValid ? string.Empty : "Highlighted inputs need valid positive numeric values.";
            return isValid;
        }

        private void AddError(Control c, string msg) { errorProvider.SetError(c, msg); }

        private bool ValidateNumericGreaterThanZero(ComboBox cmb, string errorMessage)
        {
            double val;
            if (!double.TryParse(cmb.Text, out val) || val <= 0)
            {
                errorProvider.SetError(cmb, errorMessage);
                SetInvalidInput(cmb);
                return false;
            }
            return true;
        }

        private void SetInvalidInput(Control control)
        {
            invalidControls.Add(control);
            control.BackColor = ThemeManager.IsDarkMode ? Color.FromArgb(82, 32, 38) : Color.FromArgb(255, 235, 238);
            control.ForeColor = ThemeManager.IsDarkMode ? Color.White : Color.FromArgb(120, 20, 30);
        }

        private void ClearInputValidation(Control control)
        {
            if (!invalidControls.Remove(control))
            {
                return;
            }

            errorProvider.SetError(control, string.Empty);
            RestoreInputTheme(control);

            if (invalidControls.Count == 0)
            {
                lblValidationStatus.Text = string.Empty;
            }
        }

        private void ClearValidationStyles()
        {
            foreach (Control control in invalidControls.ToList())
            {
                RestoreInputTheme(control);
            }

            invalidControls.Clear();
            lblValidationStatus.Text = string.Empty;
        }

        private static void RestoreInputTheme(Control control)
        {
            control.BackColor = ThemeManager.IsDarkMode ? Color.FromArgb(50, 50, 55) : Color.White;
            control.ForeColor = ThemeManager.IsDarkMode ? Color.White : Color.FromArgb(20, 40, 80);
        }

        private void BtnCalculate_Click(object? sender, EventArgs e)
        {
            if (!ValidateInputs())
            {
                MessageBox.Show("Please correct the validation errors before proceeding.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var sw = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                double hta = double.Parse(cmbHTA.Text);
                double tubeOD = double.Parse(cmbTubeOD.Text);
                double tubeLength = double.Parse(cmbTubeLength.Text);
                int noOfPass = int.Parse(cmbNoOfPass.Text);

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
                
                if (int.TryParse(cmbBaffleQty.Text, out int bQty))
                    currentData.BaffleQty = bQty;

                PopulateGrid(currentData);

                // Phase 2: Geometry Engine & Validation
                currentGeometry = geometryService.CalculateGeometry(currentData);
                
                sw.Stop();
                lblStatusTime.Text = $"Time : {sw.Elapsed.TotalSeconds:F1} sec";
                lblStatusExcel.Text = "Excel ✔";
                lblStatusReady.Text = "Calculated";
            }

            catch (ArgumentException aex)
            {
                MessageBox.Show(aex.Message, "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (System.IO.FileNotFoundException fnfEx)
            {
                if (fnfEx.Message.Contains("engineering data file"))
                {
                    DialogResult res = MessageBox.Show("Engineering data file not found. Would you like to locate it?", "File Missing", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (res == DialogResult.Yes)
                    {
                        using (OpenFileDialog ofd = new OpenFileDialog())
                        {
                            ofd.Filter = "Excel Files|*.xlsx;*.xls";
                            ofd.Title = "Select Excel Data File";
                            if (ofd.ShowDialog() == DialogResult.OK)
                            {
                                AppConfigManager.Current.ExcelTemplatePath = ofd.FileName;
                                AppConfigManager.Save();
                                BtnCalculate_Click(sender, e); // Retry calculation
                            }
                        }
                    }
                }
                else
                {
                    MessageBox.Show(fnfEx.Message, "File Missing", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred during calculation: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static DataGridView CreateStyledDataGridView()
        {
            var dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeColumns = false,
                AllowUserToResizeRows = false,
                ReadOnly = false,
                RowHeadersVisible = false,
                BackgroundColor = Color.FromArgb(240, 244, 248),
                SelectionMode = DataGridViewSelectionMode.CellSelect,
                MultiSelect = false,
                Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                EnableHeadersVisualStyles = false,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
                RowTemplate = { Height = 32 },
                AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCellsExceptHeaders,
                BorderStyle = BorderStyle.FixedSingle,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                GridColor = Color.LightGray,
                EditMode = DataGridViewEditMode.EditOnEnter,
                Margin = new Padding(0)
            };

            dgv.DefaultCellStyle.WrapMode = DataGridViewTriState.False;
            dgv.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dgv.DefaultCellStyle.Padding = new Padding(5, 3, 5, 3);
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(220, 235, 252);
            dgv.DefaultCellStyle.SelectionForeColor = Color.Black;

            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.WhiteSmoke;
            dgv.AlternatingRowsDefaultCellStyle.SelectionBackColor = Color.FromArgb(220, 235, 252);
            dgv.AlternatingRowsDefaultCellStyle.SelectionForeColor = Color.Black;

            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(20, 40, 80);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 10F);
            dgv.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgv.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.False;
            dgv.ColumnHeadersDefaultCellStyle.Padding = new Padding(8, 4, 8, 4);

            var parameterColumn = new DataGridViewTextBoxColumn
            {
                Name = "Parameter",
                HeaderText = "Parameter Name",
                FillWeight = 65F,
                ReadOnly = true
            };
            parameterColumn.DefaultCellStyle.Font = new Font("Segoe UI Semibold", 10F);
            parameterColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

            var valueColumn = new DataGridViewTextBoxColumn
            {
                Name = "Value",
                HeaderText = "Value",
                FillWeight = 35F,
                ReadOnly = false
            };
            valueColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            valueColumn.DefaultCellStyle.BackColor = Color.White;

            dgv.Columns.Add(parameterColumn);
            dgv.Columns.Add(valueColumn);

            return dgv;
        }

        private void SyncUiToDataModel()
        {
            if (currentData == null) return;

            if (dgvProperties.IsCurrentCellInEditMode) dgvProperties.EndEdit();
            if (dgvExtras != null && dgvExtras.IsCurrentCellInEditMode) dgvExtras.EndEdit();

            foreach (DataGridViewRow row in dgvProperties.Rows)
            {
                if (row.IsNewRow) continue;
                string? name = row.Cells["Parameter"]?.Value?.ToString();
                string? val = row.Cells["Value"]?.Value?.ToString()?.Trim();
                if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(val)) continue;

                switch (name)
                {
                    case "Shell I.D.":
                        if (int.TryParse(val, out int sId) && sId > 0)
                        {
                            currentData.ShellID = sId;
                            txtShellID.Text = sId.ToString();
                        }
                        break;
                    case "Tube Sheet Finish THK":
                        if (double.TryParse(val, out double tsfThk) && tsfThk > 0) currentData.TubeSheetFinishTHK = tsfThk;
                        break;
                    case "Body Flange Finish THK":
                        if (double.TryParse(val, out double bffThk) && bffThk > 0) currentData.BodyFlangeFinishTHK = bffThk;
                        break;
                    case "Partition Plate THK":
                        if (double.TryParse(val, out double ppThk) && ppThk > 0) currentData.PartitionPlateTHK = ppThk;
                        break;
                    case "Baffle THK":
                        if (double.TryParse(val, out double bThk) && bThk > 0) currentData.BaffleTHK = bThk;
                        break;
                    case "Bolt Size":
                        currentData.BoltSize = val;
                        break;
                    case "Bolt Length":
                        if (double.TryParse(val, out double bLen) && bLen > 0) currentData.BoltLength = bLen;
                        break;
                    case "No Of Bolts":
                        if (int.TryParse(val, out int numBolts) && numBolts > 0) currentData.NoOfBolts = numBolts;
                        break;
                    case "Flange I.D.":
                        if (double.TryParse(val, out double fId) && fId > 0) currentData.FlangeID = fId;
                        break;
                    case "Tube Sheet Finish O.D.":
                        if (double.TryParse(val, out double tsfOd) && tsfOd > 0) currentData.TubeSheetFinishOD = tsfOd;
                        break;
                    case "Tie Rod Qty.":
                        if (double.TryParse(val, out double trQty) && trQty > 0) currentData.TieRodQty = trQty;
                        break;
                }
            }

            if (dgvExtras != null)
            {
                foreach (DataGridViewRow row in dgvExtras.Rows)
                {
                    if (row.IsNewRow) continue;
                    string? name = row.Cells["Parameter"]?.Value?.ToString();
                    string? val = row.Cells["Value"]?.Value?.ToString()?.Trim();
                    if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(val)) continue;

                    switch (name)
                    {
                        case "Bonnet Shell FS Length":
                            if (double.TryParse(val, out double fsLen)) currentData.BonnetShellFSLength = fsLen;
                            break;
                        case "Bonnet Shell RS Length":
                            if (double.TryParse(val, out double rsLen)) currentData.BonnetShellRSLength = rsLen;
                            break;
                        case "Dishend THK":
                            if (double.TryParse(val, out double dThk) && dThk > 0) currentData.DishendTHK = dThk;
                            break;
                    }
                }
            }
        }

        private void PopulateGrid(EngineeringDataModel data)
        {
            dgvProperties.Rows.Clear();
            var dict = data.ToDisplayDictionary();
            foreach (string parameterName in EngineeringPropertyNames)
            {
                dict.TryGetValue(parameterName, out string? value);
                dgvProperties.Rows.Add(parameterName, value ?? string.Empty);
            }

            if (dgvExtras != null)
            {
                dgvExtras.Rows.Clear();
                dgvExtras.Rows.Add("Bonnet Shell FS Length", data.BonnetShellFSLength > 0 ? data.BonnetShellFSLength.ToString() : "500");
                dgvExtras.Rows.Add("Bonnet Shell RS Length", data.BonnetShellRSLength > 0 ? data.BonnetShellRSLength.ToString() : "500");
                dgvExtras.Rows.Add("Dishend THK", data.DishendTHK > 0 ? data.DishendTHK.ToString() : "5");
            }

            ClearPropertiesSelection();
        }

        private void PopulateEmptyEngineeringProperties()
        {
            dgvProperties.Rows.Clear();
            foreach (string parameterName in EngineeringPropertyNames)
            {
                dgvProperties.Rows.Add(parameterName, string.Empty);
            }

            if (dgvExtras != null)
            {
                dgvExtras.Rows.Clear();
                foreach (string parameterName in ExtrasPropertyNames)
                {
                    dgvExtras.Rows.Add(parameterName, string.Empty);
                }
            }

            ClearPropertiesSelection();
        }

        private void ClearPropertiesSelection()
        {
            dgvProperties.ClearSelection();
            dgvProperties.CurrentCell = null;
            if (dgvExtras != null)
            {
                dgvExtras.ClearSelection();
                dgvExtras.CurrentCell = null;
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

        private async void BtnGenerateTubeSheet_Click(object? sender, EventArgs e)
        {
            if (currentData == null || currentGeometry == null)
            {
                MessageBox.Show("Please click Calculate first.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!_generationLock.Wait(0))
            {
                MessageBox.Show("Drawing generation is already in progress. Please wait.", "Generation In Progress", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var sw = System.Diagnostics.Stopwatch.StartNew();
            var btn = sender as Button;
            var originalCursor = Cursor.Current;

            try
            {
                if (btn != null) btn.Enabled = false;
                Cursor.Current = Cursors.WaitCursor;

                RunContext.GenerateNewRunId();
                SyncUiToDataModel();
                lblStatusReady.Text = "Generating...";
                statusStrip.Refresh();
                SimpleLogger.LogGeneration("TubeSheet", "Tube Sheet Generation Started");
                currentGeometry = geometryService.CalculateGeometry(currentData);
                var tempService = new TemplateDrawingService();
                var groupedViews = tempService.GenerateTemplateViews(currentGeometry, currentData);

                string templatePath = AppConfigManager.Current.DwgTemplatePath;
                
                if (!System.IO.File.Exists(templatePath))
                {
                    DialogResult res = MessageBox.Show("DWG Template file not found. Would you like to locate it?", "File Missing", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (res == DialogResult.Yes)
                    {
                        using (OpenFileDialog ofd = new OpenFileDialog())
                        {
                            ofd.Filter = "DWG Files|*.dwg";
                            ofd.Title = "Select DWG Template";
                            if (ofd.ShowDialog() == DialogResult.OK)
                            {
                                AppConfigManager.Current.DwgTemplatePath = ofd.FileName;
                                AppConfigManager.Save();
                                templatePath = ofd.FileName;
                            }
                            else
                            {
                                return;
                            }
                        }
                    }
                    else
                    {
                        return;
                    }
                }
                
                DrawingAutomationResult result = drawingService.GenerateTemplateLispAndLaunchCAD(groupedViews, currentData, currentGeometry, templatePath);
                
                lastGeneratedLispPath = result.BackupPath;
                lastGeneratedScrPath = result.BackupScrPath;

                if (AppConfigManager.Current.UsePipelineV2)
                {
                    lblStatusReady.Text = "Generating V2...";
                    var tsData = new MegaEngineeringSuite.TubeSheet.TubeSheetData
                    {
                        OutsideDiameter = currentData.TubeSheetFinishOD,
                        InsideDiameter = currentData.ShellID,
                        StepOutsideDiameter = currentData.TubeSheetFinishOD, // Placeholder
                        Thickness = currentData.TubeSheetFinishTHK,
                        
                        // BOM Properties
                        TubeSheetFinishOD = currentData.TubeSheetFinishOD,
                        TubeSheetFinishTHK = currentData.TubeSheetFinishTHK,
                        TubeSheetWeight = MegaEngineeringSuite.Calculations.EngineeringCalculator.CalculateTubeSheetWeight(currentData),

                    };
                    
                    var drawInfo = GetDrawingInformation();
                    var orchestrator = new MegaEngineeringSuite.TubeSheet.PipelineOrchestrator();
                    bool success = await orchestrator.RunV2PipelineAsync(result, tsData, drawInfo, MegaEngineeringSuite.TubeSheet.PipelineExecutionMode.Interactive);
                    if (!success)
                    {
                        MessageBox.Show("Pipeline V2 failed or timed out.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }

#if DEBUG
                Debug.WriteLine("AutoLISP and SCR scripts generated.");
                Debug.WriteLine($"Generated LSP: {result.ScriptPath}");
                Debug.WriteLine($"Generated SCR: {result.ScrPath}");
                Debug.WriteLine($"Process Arguments: \"{result.CadExecutable}\" {result.Arguments}");
                Debug.WriteLine(result.ScrContent);
#endif
                sw.Stop();
                lblStatusTime.Text = $"Time : {sw.Elapsed.TotalSeconds:F1} sec";
                lblStatusCAD.Text = "CAD ✔";
                lblStatusGenerated.Text = "Generated : Tube Sheet";
                lblStatusReady.Text = "Ready";
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                ShowStructuredError("Failed to generate Tube Sheet Drawing.", "GstarCAD may not be installed or is unresponsive, or the DWG template is invalid.", "Please verify that GstarCAD is installed and the template paths are correct.", "CAD-001", ex);
                lblStatusReady.Text = "Error";
            }
            finally
            {
                if (btn != null) btn.Enabled = true;
                Cursor.Current = originalCursor;
                _generationLock.Release();
            }
        }

        private async void BtnGenerateBodyFlange_Click(object? sender, EventArgs e)
        {
            if (currentData == null)
            {
                MessageBox.Show("Please calculate engineering data first.", "Missing Data", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!_generationLock.Wait(0))
            {
                MessageBox.Show("Drawing generation is already in progress. Please wait.", "Generation In Progress", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var sw = System.Diagnostics.Stopwatch.StartNew();
            var btn = sender as Button;
            var originalCursor = Cursor.Current;

            try
            {
                if (btn != null) btn.Enabled = false;
                Cursor.Current = Cursors.WaitCursor;

                RunContext.GenerateNewRunId();
                SyncUiToDataModel();
                lblStatusReady.Text = "Generating...";
                SimpleLogger.LogGeneration("BodyFlange", "Body Flange Generation Started");
                
                // Map the data (includes validation)
                MegaEngineeringSuite.BonnetFlange.BonnetFlangeData mappedData = MegaEngineeringSuite.BonnetFlange.BonnetFlangeDataMapper.Map(currentData);
                
                // Extract drawing information
                DrawingInformation drawInfo = GetDrawingInformation();

                // Run generation asynchronously to prevent UI freeze
                string outputPath = await System.Threading.Tasks.Task.Run(() =>
                {
                    var generator = new MegaEngineeringSuite.BonnetFlange.BonnetFlangeGenerator();
                    return generator.Generate(mappedData, drawInfo);
                });

                SimpleLogger.LogGeneration("BodyFlange", "Body Flange Generation Completed");

                if (!string.IsNullOrEmpty(outputPath) && System.IO.File.Exists(outputPath))
                {
                    SimpleLogger.LogCad("BodyFlange", "Opening Drawing");
                    string cadExe = AppConfigManager.Current.CadPath;
                    
                    if (!string.IsNullOrEmpty(cadExe) && System.IO.File.Exists(cadExe))
                    {
                        Process.Start(new ProcessStartInfo { FileName = cadExe, Arguments = $"\"{outputPath}\"" });
                    }
                    else
                    {
                        Process.Start(new ProcessStartInfo { FileName = outputPath, UseShellExecute = true });
                    }
                    SimpleLogger.LogCad("BodyFlange", "Drawing Opened");
                    
                    sw.Stop();
                    lblStatusTime.Text = $"Time : {sw.Elapsed.TotalSeconds:F1} sec";
                    lblStatusCAD.Text = "CAD ✔";
                    lblStatusGenerated.Text = "Generated : Body Flange";
                    lblStatusReady.Text = "Ready";
                }
            }
            catch (Exception ex)
            {
                ShowStructuredError("Failed to generate Body Flange Drawing.", "The COM interface to GstarCAD failed or the DWG output path was inaccessible.", "Ensure CAD is running properly and no drawings are locked.", "GEN-001", ex);
                lblStatusReady.Text = "Error";
            }
            finally
            {
                if (btn != null) btn.Enabled = true;
                Cursor.Current = originalCursor;
                _generationLock.Release();
            }
        }

        private async void BtnGenerateHeatExchanger_Click(object? sender, EventArgs e)
        {
            if (currentData == null)
            {
                MessageBox.Show("Please calculate engineering data first.", "Missing Data", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!_generationLock.Wait(0))
            {
                MessageBox.Show("Drawing generation is already in progress. Please wait.", "Generation In Progress", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var sw = System.Diagnostics.Stopwatch.StartNew();
            var btn = sender as Button;
            var originalCursor = Cursor.Current;

            try
            {
                RunContext.GenerateNewRunId();
                SyncUiToDataModel();
                if (btn != null) btn.Enabled = false;
                Cursor.Current = Cursors.WaitCursor;

                lblStatusReady.Text = "Generating...";
                statusStrip.Refresh();
                SimpleLogger.LogGeneration("HeatExchanger", "Heat Exchanger Fabrication Generation Started");
                
                MegaEngineeringSuite.HeatExchangerFab.HeatExchangerFabData mappedData = MegaEngineeringSuite.HeatExchangerFab.HeatExchangerFabDataMapper.Map(currentData);
                DrawingInformation drawInfo = GetDrawingInformation();

                string outputPath = await System.Threading.Tasks.Task.Run(() =>
                {
                    var generator = new MegaEngineeringSuite.HeatExchangerFab.HeatExchangerFabGenerator();
                    return generator.Generate(mappedData, drawInfo);
                });

                SimpleLogger.LogGeneration("HeatExchanger", "Heat Exchanger Fabrication Generation Completed");

                if (!string.IsNullOrEmpty(outputPath) && System.IO.File.Exists(outputPath))
                {
                    SimpleLogger.LogCad("HeatExchanger", "Drawing Generated, Activated and Visible in GstarCAD Session");
                    
                    sw.Stop();
                    lblStatusTime.Text = $"Time : {sw.Elapsed.TotalSeconds:F1} sec";
                    lblStatusCAD.Text = "CAD ✔";
                    lblStatusGenerated.Text = "Generated : Heat Exchanger";
                    lblStatusReady.Text = "Ready";
                }
            }
            catch (Exception ex)
            {
                ShowStructuredError("Failed to generate Heat Exchanger Fabrication Drawing.", "GstarCAD COM objects became unresponsive or the pipeline encountered invalid placeholders.", "Restart GstarCAD and try again. If the issue persists, review the diagnostic logs.", "GEN-002", ex);
                lblStatusReady.Text = "Error";
            }
            finally
            {
                if (btn != null) btn.Enabled = true;
                Cursor.Current = originalCursor;
                _generationLock.Release();
            }
        }

        private void BtnExport_Click(object? sender, EventArgs e)
        {
            if (currentData == null)
            {
                MessageBox.Show("Please calculate the engineering parameters first.", "Missing Data", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SyncUiToDataModel();

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
                        ShowStructuredError("Failed to export data to Excel.", "The target file may be open in Excel or you do not have write permissions.", "Close the Excel file and try exporting again.", "EXC-001", ex);
                    }
                }
            }
        }

        private void ShowStructuredError(string whatHappened, string why, string whatToDo, string errorCode, Exception ex)
        {
            string runId = RunContext.CurrentRunId;
            string msg = $"WHAT HAPPENED:\n{whatHappened}\n\nWHY IT MAY HAVE HAPPENED:\n{why}\n\nWHAT TO DO:\n{whatToDo}\n\nERROR CODE:\n{errorCode}\n\nRUN ID:\n{runId}";
            MessageBox.Show(msg, "Application Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            SimpleLogger.LogError("UI", errorCode, whatHappened, ex);
        }
    }
}
