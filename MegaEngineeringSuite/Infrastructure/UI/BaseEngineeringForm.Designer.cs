using System;

namespace MegaEngineeringSuite.Infrastructure.UI
{
    partial class BaseEngineeringForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.lblModuleTitle = new System.Windows.Forms.Label();
            this.lblModuleSubtitle = new System.Windows.Forms.Label();
            
            this.tlpMain = new System.Windows.Forms.TableLayoutPanel();
            this.pnlLeftFixed = new System.Windows.Forms.Panel();
            this.pnlRightPreview = new System.Windows.Forms.Panel();
            this.lblPreviewMessage = new System.Windows.Forms.Label();

            this.statusStripMain = new System.Windows.Forms.StatusStrip();
            this.tsStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.tsProgressBar = new System.Windows.Forms.ToolStripProgressBar();

            this.pnlHeader.SuspendLayout();
            this.tlpMain.SuspendLayout();
            this.pnlRightPreview.SuspendLayout();
            this.statusStripMain.SuspendLayout();
            this.SuspendLayout();

            // 
            // pnlHeader
            // 
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Height = 80;
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(20, 40, 80);
            this.pnlHeader.ForeColor = System.Drawing.Color.White;
            
            this.lblModuleTitle.AutoSize = true;
            this.lblModuleTitle.Font = new System.Drawing.Font("Segoe UI", 24F, System.Drawing.FontStyle.Bold);
            this.lblModuleTitle.Location = new System.Drawing.Point(15, 10);
            this.lblModuleTitle.Name = "lblModuleTitle";
            this.lblModuleTitle.Text = "ENGINEERING MODULE";

            this.lblModuleSubtitle.AutoSize = true;
            this.lblModuleSubtitle.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblModuleSubtitle.Location = new System.Drawing.Point(22, 55);
            this.lblModuleSubtitle.Name = "lblModuleSubtitle";
            this.lblModuleSubtitle.Text = "Subtitle goes here";

            this.pnlHeader.Controls.Add(this.lblModuleTitle);
            this.pnlHeader.Controls.Add(this.lblModuleSubtitle);

            // 
            // tlpMain
            // 
            this.tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMain.ColumnCount = 2;
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 380F));
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.RowCount = 1;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMain.Controls.Add(this.pnlLeftFixed, 0, 0);
            this.tlpMain.Controls.Add(this.pnlRightPreview, 1, 0);

            // 
            // pnlLeftFixed
            // 
            this.pnlLeftFixed.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlLeftFixed.AutoScroll = true;
            this.pnlLeftFixed.Padding = new System.Windows.Forms.Padding(15);
            this.pnlLeftFixed.BackColor = System.Drawing.Color.White;

            // 
            // pnlRightPreview
            // 
            this.pnlRightPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlRightPreview.BackColor = System.Drawing.Color.WhiteSmoke;
            this.pnlRightPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            
            this.lblPreviewMessage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblPreviewMessage.Text = "No Preview Available\n\nPreview functionality coming soon.";
            this.lblPreviewMessage.Font = new System.Drawing.Font("Segoe UI", 16F);
            this.lblPreviewMessage.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.pnlRightPreview.Controls.Add(this.lblPreviewMessage);

            // 
            // statusStripMain
            // 
            this.statusStripMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsStatusLabel,
            this.tsProgressBar});
            this.statusStripMain.Location = new System.Drawing.Point(0, 878);
            this.statusStripMain.Name = "statusStripMain";
            this.statusStripMain.Size = new System.Drawing.Size(1384, 22);
            this.statusStripMain.TabIndex = 2;

            this.tsStatusLabel.Name = "tsStatusLabel";
            this.tsStatusLabel.Size = new System.Drawing.Size(39, 17);
            this.tsStatusLabel.Text = "Ready";

            this.tsProgressBar.Name = "tsProgressBar";
            this.tsProgressBar.Size = new System.Drawing.Size(200, 16);
            this.tsProgressBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.tsProgressBar.Visible = false;

            // 
            // BaseEngineeringForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1384, 900);
            this.MinimumSize = new System.Drawing.Size(1400, 900);
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            
            this.Controls.Add(this.tlpMain);
            this.Controls.Add(this.pnlHeader);
            this.Controls.Add(this.statusStripMain);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.Name = "BaseEngineeringForm";
            this.Text = "Mega Engineering Suite";

            this.pnlHeader.ResumeLayout(false);
            this.pnlHeader.PerformLayout();
            this.tlpMain.ResumeLayout(false);
            this.pnlRightPreview.ResumeLayout(false);
            this.statusStripMain.ResumeLayout(false);
            this.statusStripMain.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        protected System.Windows.Forms.Panel pnlHeader;
        protected System.Windows.Forms.Label lblModuleTitle;
        protected System.Windows.Forms.Label lblModuleSubtitle;
        
        protected System.Windows.Forms.TableLayoutPanel tlpMain;
        protected System.Windows.Forms.Panel pnlLeftFixed;
        protected System.Windows.Forms.Panel pnlRightPreview;
        protected System.Windows.Forms.Label lblPreviewMessage;

        protected System.Windows.Forms.StatusStrip statusStripMain;
        protected System.Windows.Forms.ToolStripStatusLabel tsStatusLabel;
        protected System.Windows.Forms.ToolStripProgressBar tsProgressBar;
    }
}
