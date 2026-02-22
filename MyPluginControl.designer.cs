namespace GM.XrmToolBox.DataVerseSearchFieldsMatrix
{
    partial class MyPluginControl
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

        #region Component Designer generated code

        private void InitializeComponent()
        {
            this.toolStripMenu = new System.Windows.Forms.ToolStrip();
            this.tsbClose = new System.Windows.Forms.ToolStripButton();
            this.tssSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.tsbLoadData = new System.Windows.Forms.ToolStripButton();
            this.tssSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.tsbExportCsv = new System.Windows.Forms.ToolStripButton();
            this.tssSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.tsbReadme = new System.Windows.Forms.ToolStripButton();
            this.tsbAbout = new System.Windows.Forms.ToolStripButton();
            this.panelTop = new System.Windows.Forms.Panel();
            this.lblRecordCount = new System.Windows.Forms.Label();
            this.txtSearch = new System.Windows.Forms.TextBox();
            this.lblSearch = new System.Windows.Forms.Label();
            this.lblSearchStatus = new System.Windows.Forms.Label();
            this.dgvMatrix = new System.Windows.Forms.DataGridView();
            this.chkMatchPpac = new System.Windows.Forms.CheckBox();
            this.panelDiagnostics = new System.Windows.Forms.Panel();
            this.lblDiagnostics = new System.Windows.Forms.Label();
            this.dgvDiagnostics = new System.Windows.Forms.DataGridView();
            this.splitterDiagnostics = new System.Windows.Forms.Splitter();
            this.toolStripMenu.SuspendLayout();
            this.panelTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvMatrix)).BeginInit();
            this.SuspendLayout();
            // 
            // toolStripMenu
            // 
            this.toolStripMenu.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.toolStripMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.tsbClose,
                this.tssSeparator1,
                this.tsbLoadData,
                this.tssSeparator2,
                this.tsbExportCsv,
                this.tssSeparator3,
                this.tsbReadme,
                this.tsbAbout});
            this.toolStripMenu.Location = new System.Drawing.Point(0, 0);
            this.toolStripMenu.Name = "toolStripMenu";
            this.toolStripMenu.Padding = new System.Windows.Forms.Padding(0, 0, 2, 0);
            this.toolStripMenu.Size = new System.Drawing.Size(950, 31);
            this.toolStripMenu.TabIndex = 0;
            // 
            // tsbClose
            // 
            this.tsbClose.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsbClose.Name = "tsbClose";
            this.tsbClose.Size = new System.Drawing.Size(46, 28);
            this.tsbClose.Text = "Close";
            this.tsbClose.Click += new System.EventHandler(this.tsbClose_Click);
            // 
            // tssSeparator1
            // 
            this.tssSeparator1.Name = "tssSeparator1";
            this.tssSeparator1.Size = new System.Drawing.Size(6, 31);
            // 
            // tsbLoadData
            // 
            this.tsbLoadData.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsbLoadData.Name = "tsbLoadData";
            this.tsbLoadData.Size = new System.Drawing.Size(76, 28);
            this.tsbLoadData.Text = "Load Data";
            this.tsbLoadData.ToolTipText = "Load Dataverse Search Fields Matrix";
            this.tsbLoadData.Click += new System.EventHandler(this.tsbLoadData_Click);
            // 
            // tssSeparator2
            // 
            this.tssSeparator2.Name = "tssSeparator2";
            this.tssSeparator2.Size = new System.Drawing.Size(6, 31);
            // 
            // tsbExportCsv
            // 
            this.tsbExportCsv.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsbExportCsv.Name = "tsbExportCsv";
            this.tsbExportCsv.Size = new System.Drawing.Size(80, 28);
            this.tsbExportCsv.Text = "Export CSV";
            this.tsbExportCsv.ToolTipText = "Export current view to CSV file";
            this.tsbExportCsv.Click += new System.EventHandler(this.tsbExportCsv_Click);
            // 
            // tssSeparator3
            // 
            this.tssSeparator3.Name = "tssSeparator3";
            this.tssSeparator3.Size = new System.Drawing.Size(6, 31);
            // tsbReadme
            this.tsbReadme.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsbReadme.Name = "tsbReadme";
            this.tsbReadme.Size = new System.Drawing.Size(67, 28);
            this.tsbReadme.Text = "Readme";
            this.tsbReadme.Click += new System.EventHandler(this.tsbReadme_Click);
            // 
            // tsbAbout
            // 
            this.tsbAbout.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsbAbout.Name = "tsbAbout";
            this.tsbAbout.Size = new System.Drawing.Size(52, 28);
            this.tsbAbout.Text = "About";
            this.tsbAbout.ToolTipText = "About this plugin";
            this.tsbAbout.Click += new System.EventHandler(this.tsbAbout_Click);
            this.panelTop.Controls.Add(this.chkMatchPpac);
            // 
            // panelTop — three rows: status banner, search bar, record count
            // 
            this.panelTop.Controls.Add(this.lblRecordCount);
            this.panelTop.Controls.Add(this.txtSearch);
            this.panelTop.Controls.Add(this.lblSearch);
            this.panelTop.Controls.Add(this.lblSearchStatus);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 31);
            this.panelTop.Name = "panelTop";
            this.panelTop.Padding = new System.Windows.Forms.Padding(10, 5, 10, 5);
            this.panelTop.Size = new System.Drawing.Size(950, 105);
            this.panelTop.TabIndex = 1;
            // 
            // lblSearchStatus — Row 1: Dataverse Search enabled/disabled banner
            // 
            this.lblSearchStatus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSearchStatus.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblSearchStatus.ForeColor = System.Drawing.Color.Gray;
            this.lblSearchStatus.Location = new System.Drawing.Point(10, 5);
            this.lblSearchStatus.Name = "lblSearchStatus";
            this.lblSearchStatus.Size = new System.Drawing.Size(930, 30);
            this.lblSearchStatus.TabIndex = 0;
            this.lblSearchStatus.Text = "Connect and click \'Load Data\' to check Dataverse Search configuration.";
            this.lblSearchStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblSearch — Row 2: search label
            // 
            this.lblSearch.AutoSize = true;
            this.lblSearch.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblSearch.Location = new System.Drawing.Point(10, 45);
            this.lblSearch.Name = "lblSearch";
            this.lblSearch.Size = new System.Drawing.Size(52, 20);
            this.lblSearch.TabIndex = 1;
            this.lblSearch.Text = "Search:";
            // 
            // txtSearch — Row 2: search text box (stretches with window)
            // 
            this.txtSearch.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSearch.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.txtSearch.Location = new System.Drawing.Point(75, 42);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new System.Drawing.Size(865, 27);
            this.txtSearch.TabIndex = 2;
            this.txtSearch.TextChanged += new System.EventHandler(this.txtSearch_TextChanged);
            // 
            // lblRecordCount — Row 3: full-width label for raw + weighted counts
            // 
            this.lblRecordCount.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblRecordCount.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblRecordCount.ForeColor = System.Drawing.Color.DimGray;
            this.lblRecordCount.Location = new System.Drawing.Point(10, 78);
            this.lblRecordCount.Name = "lblRecordCount";
            this.lblRecordCount.Size = new System.Drawing.Size(750, 20);
            this.lblRecordCount.TabIndex = 3;
            this.lblRecordCount.Text = "";
            this.lblRecordCount.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // chkMatchPpac
            this.chkMatchPpac.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.chkMatchPpac.AutoSize = true;
            this.chkMatchPpac.Location = new System.Drawing.Point(770, 78);
            this.chkMatchPpac.Name = "chkMatchPpac";
            this.chkMatchPpac.Size = new System.Drawing.Size(170, 21);
            this.chkMatchPpac.TabIndex = 4;
            this.chkMatchPpac.Text = "Match PPAC counting";
            this.chkMatchPpac.UseVisualStyleBackColor = true;
            this.chkMatchPpac.CheckedChanged += new System.EventHandler(this.chkMatchPpac_CheckedChanged);
            // 
            // dgvMatrix
            // 
            this.dgvMatrix.AllowUserToAddRows = false;
            this.dgvMatrix.AllowUserToDeleteRows = false;
            this.dgvMatrix.BackgroundColor = System.Drawing.SystemColors.Window;
            this.dgvMatrix.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.dgvMatrix.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvMatrix.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvMatrix.Location = new System.Drawing.Point(0, 136);
            this.dgvMatrix.Name = "dgvMatrix";
            this.dgvMatrix.ReadOnly = true;
            this.dgvMatrix.RowHeadersVisible = false;
            this.dgvMatrix.RowHeadersWidth = 51;
            this.dgvMatrix.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvMatrix.Size = new System.Drawing.Size(950, 414);
            this.dgvMatrix.TabIndex = 2;
            // panelDiagnostics
            this.panelDiagnostics.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelDiagnostics.Height = 170;
            this.panelDiagnostics.Name = "panelDiagnostics";
            this.panelDiagnostics.Padding = new System.Windows.Forms.Padding(10);
            this.panelDiagnostics.Visible = false;
            // splitterDiagnostics
            this.splitterDiagnostics.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.splitterDiagnostics.Height = 6;
            this.splitterDiagnostics.Name = "splitterDiagnostics";
            this.splitterDiagnostics.TabStop = false;
            this.splitterDiagnostics.Cursor = System.Windows.Forms.Cursors.HSplit;
            this.splitterDiagnostics.Visible = false;
            // this.splitterDiagnostics.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.splitterDiagnostics_SplitterMoved);
            // lblDiagnostics
            this.lblDiagnostics.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblDiagnostics.Height = 20;
            this.lblDiagnostics.Name = "lblDiagnostics";
            this.lblDiagnostics.Text = "Diagnostics (fields excluded/deduplicated in PPAC-like count)";
            this.lblDiagnostics.ForeColor = System.Drawing.Color.DimGray;

            // dgvDiagnostics
            this.dgvDiagnostics.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvDiagnostics.Name = "dgvDiagnostics";
            this.dgvDiagnostics.ReadOnly = true;
            this.dgvDiagnostics.RowHeadersVisible = false;
            this.dgvDiagnostics.AllowUserToAddRows = false;
            this.dgvDiagnostics.AllowUserToDeleteRows = false;
            this.dgvDiagnostics.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;

            // compose diagnostics panel
            this.panelDiagnostics.Controls.Add(this.dgvDiagnostics);
            this.panelDiagnostics.Controls.Add(this.lblDiagnostics);
            // 
            // MyPluginControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.dgvMatrix);
            this.Controls.Add(this.splitterDiagnostics);
            this.Controls.Add(this.panelDiagnostics);
            this.Controls.Add(this.panelTop);
            this.Controls.Add(this.toolStripMenu);
            this.Name = "MyPluginControl";
            this.Size = new System.Drawing.Size(950, 550);
            this.Load += new System.EventHandler(this.MyPluginControl_Load);
            this.toolStripMenu.ResumeLayout(false);
            this.toolStripMenu.PerformLayout();
            this.panelTop.ResumeLayout(false);
            this.panelTop.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvMatrix)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStripMenu;
        private System.Windows.Forms.ToolStripButton tsbClose;
        private System.Windows.Forms.ToolStripSeparator tssSeparator1;
        private System.Windows.Forms.ToolStripButton tsbLoadData;
        private System.Windows.Forms.ToolStripSeparator tssSeparator2;
        private System.Windows.Forms.ToolStripButton tsbExportCsv;
        private System.Windows.Forms.ToolStripSeparator tssSeparator3;
        private System.Windows.Forms.ToolStripButton tsbAbout;
        private System.Windows.Forms.ToolStripButton tsbReadme;

        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.Label lblSearchStatus;
        private System.Windows.Forms.Label lblSearch;
        private System.Windows.Forms.TextBox txtSearch;
        private System.Windows.Forms.Label lblRecordCount;
        private System.Windows.Forms.DataGridView dgvMatrix;

        private System.Windows.Forms.CheckBox chkMatchPpac;
        private System.Windows.Forms.Panel panelDiagnostics;
        private System.Windows.Forms.Label lblDiagnostics;
        private System.Windows.Forms.DataGridView dgvDiagnostics;
        private System.Windows.Forms.Splitter splitterDiagnostics;

    }
}
