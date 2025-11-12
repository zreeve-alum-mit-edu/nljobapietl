namespace JobApi.TestGui.Controls
{
    partial class LoadTestControl
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
            this.lblTestSuite = new System.Windows.Forms.Label();
            this.dgvPrompts = new System.Windows.Forms.DataGridView();
            this.colName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colPrompt = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colNumJobs = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDays = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colCity = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colState = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colMiles = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colOnsite = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.colHybrid = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.btnAdd = new System.Windows.Forms.Button();
            this.btnRemove = new System.Windows.Forms.Button();
            this.btnLoadCsv = new System.Windows.Forms.Button();
            this.btnSaveCsv = new System.Windows.Forms.Button();
            this.lblEndpointType = new System.Windows.Forms.Label();
            this.cmbEndpointType = new System.Windows.Forms.ComboBox();
            this.lblConcurrency = new System.Windows.Forms.Label();
            this.cmbConcurrency = new System.Windows.Forms.ComboBox();
            this.lblIterations = new System.Windows.Forms.Label();
            this.cmbIterations = new System.Windows.Forms.ComboBox();
            this.btnRunTest = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.btnClearResults = new System.Windows.Forms.Button();
            this.lblResults = new System.Windows.Forms.Label();
            this.dgvResults = new System.Windows.Forms.DataGridView();
            this.colResultName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colResultPrompt = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colResultStatus = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colResultTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colResultCount = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colResultError = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.lblStats = new System.Windows.Forms.Label();
            this.txtStats = new System.Windows.Forms.TextBox();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.lblProgress = new System.Windows.Forms.Label();
            this.btnExportResults = new System.Windows.Forms.Button();
            this.lblApiVersion = new System.Windows.Forms.Label();
            this.cmbApiVersion = new System.Windows.Forms.ComboBox();
            ((System.ComponentModel.ISupportInitialize)(this.dgvPrompts)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvResults)).BeginInit();
            this.SuspendLayout();
            //
            // lblTestSuite
            //
            this.lblTestSuite.AutoSize = true;
            this.lblTestSuite.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblTestSuite.Location = new System.Drawing.Point(12, 10);
            this.lblTestSuite.Name = "lblTestSuite";
            this.lblTestSuite.Size = new System.Drawing.Size(129, 15);
            this.lblTestSuite.TabIndex = 0;
            this.lblTestSuite.Text = "Test Suite - Prompts:";
            //
            // dgvPrompts
            //
            this.dgvPrompts.AllowUserToAddRows = false;
            this.dgvPrompts.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvPrompts.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colName,
            this.colPrompt,
            this.colNumJobs,
            this.colDays,
            this.colCity,
            this.colState,
            this.colMiles,
            this.colOnsite,
            this.colHybrid});
            this.dgvPrompts.Location = new System.Drawing.Point(12, 30);
            this.dgvPrompts.Name = "dgvPrompts";
            this.dgvPrompts.RowHeadersWidth = 25;
            this.dgvPrompts.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvPrompts.Size = new System.Drawing.Size(1160, 150);
            this.dgvPrompts.TabIndex = 1;
            //
            // colName
            //
            this.colName.HeaderText = "Name";
            this.colName.Name = "colName";
            this.colName.Width = 100;
            //
            // colPrompt
            //
            this.colPrompt.HeaderText = "Prompt";
            this.colPrompt.Name = "colPrompt";
            this.colPrompt.Width = 250;
            //
            // colNumJobs
            //
            this.colNumJobs.HeaderText = "NumJobs";
            this.colNumJobs.Name = "colNumJobs";
            this.colNumJobs.Width = 70;
            //
            // colDays
            //
            this.colDays.HeaderText = "Days";
            this.colDays.Name = "colDays";
            this.colDays.Width = 50;
            //
            // colCity
            //
            this.colCity.HeaderText = "City";
            this.colCity.Name = "colCity";
            this.colCity.Width = 80;
            //
            // colState
            //
            this.colState.HeaderText = "State";
            this.colState.Name = "colState";
            this.colState.Width = 50;
            //
            // colMiles
            //
            this.colMiles.HeaderText = "Miles";
            this.colMiles.Name = "colMiles";
            this.colMiles.Width = 50;
            //
            // colOnsite
            //
            this.colOnsite.HeaderText = "Onsite";
            this.colOnsite.Name = "colOnsite";
            this.colOnsite.Width = 60;
            //
            // colHybrid
            //
            this.colHybrid.HeaderText = "Hybrid";
            this.colHybrid.Name = "colHybrid";
            this.colHybrid.Width = 60;
            //
            // btnAdd
            //
            this.btnAdd.Location = new System.Drawing.Point(12, 186);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(80, 30);
            this.btnAdd.TabIndex = 2;
            this.btnAdd.Text = "Add";
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            //
            // btnRemove
            //
            this.btnRemove.Location = new System.Drawing.Point(98, 186);
            this.btnRemove.Name = "btnRemove";
            this.btnRemove.Size = new System.Drawing.Size(80, 30);
            this.btnRemove.TabIndex = 3;
            this.btnRemove.Text = "Remove";
            this.btnRemove.UseVisualStyleBackColor = true;
            this.btnRemove.Click += new System.EventHandler(this.btnRemove_Click);
            //
            // btnLoadCsv
            //
            this.btnLoadCsv.Location = new System.Drawing.Point(184, 186);
            this.btnLoadCsv.Name = "btnLoadCsv";
            this.btnLoadCsv.Size = new System.Drawing.Size(90, 30);
            this.btnLoadCsv.TabIndex = 4;
            this.btnLoadCsv.Text = "Load CSV";
            this.btnLoadCsv.UseVisualStyleBackColor = true;
            this.btnLoadCsv.Click += new System.EventHandler(this.btnLoadCsv_Click);
            //
            // btnSaveCsv
            //
            this.btnSaveCsv.Location = new System.Drawing.Point(280, 186);
            this.btnSaveCsv.Name = "btnSaveCsv";
            this.btnSaveCsv.Size = new System.Drawing.Size(90, 30);
            this.btnSaveCsv.TabIndex = 5;
            this.btnSaveCsv.Text = "Save CSV";
            this.btnSaveCsv.UseVisualStyleBackColor = true;
            this.btnSaveCsv.Click += new System.EventHandler(this.btnSaveCsv_Click);
            //
            // lblEndpointType
            //
            this.lblEndpointType.AutoSize = true;
            this.lblEndpointType.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblEndpointType.Location = new System.Drawing.Point(400, 191);
            this.lblEndpointType.Name = "lblEndpointType";
            this.lblEndpointType.Size = new System.Drawing.Size(90, 15);
            this.lblEndpointType.TabIndex = 20;
            this.lblEndpointType.Text = "Endpoint Type:";
            //
            // cmbEndpointType
            //
            this.cmbEndpointType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbEndpointType.FormattingEnabled = true;
            this.cmbEndpointType.Items.AddRange(new object[] {
            "Remote Only",
            "Location-Based"});
            this.cmbEndpointType.Location = new System.Drawing.Point(496, 188);
            this.cmbEndpointType.Name = "cmbEndpointType";
            this.cmbEndpointType.Size = new System.Drawing.Size(150, 23);
            this.cmbEndpointType.TabIndex = 21;
            this.cmbEndpointType.SelectedIndex = 0;
            this.cmbEndpointType.SelectedIndexChanged += new System.EventHandler(this.cmbEndpointType_SelectedIndexChanged);
            //
            // lblConcurrency
            //
            this.lblConcurrency.AutoSize = true;
            this.lblConcurrency.Location = new System.Drawing.Point(12, 230);
            this.lblConcurrency.Name = "lblConcurrency";
            this.lblConcurrency.Size = new System.Drawing.Size(78, 15);
            this.lblConcurrency.TabIndex = 6;
            this.lblConcurrency.Text = "Concurrency:";
            //
            // cmbConcurrency
            //
            this.cmbConcurrency.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbConcurrency.FormattingEnabled = true;
            this.cmbConcurrency.Items.AddRange(new object[] {
            "1",
            "2",
            "5",
            "10",
            "20",
            "50"});
            this.cmbConcurrency.Location = new System.Drawing.Point(12, 248);
            this.cmbConcurrency.Name = "cmbConcurrency";
            this.cmbConcurrency.Size = new System.Drawing.Size(100, 23);
            this.cmbConcurrency.TabIndex = 7;
            this.cmbConcurrency.SelectedIndex = 3;
            //
            // lblIterations
            //
            this.lblIterations.AutoSize = true;
            this.lblIterations.Location = new System.Drawing.Point(130, 230);
            this.lblIterations.Name = "lblIterations";
            this.lblIterations.Size = new System.Drawing.Size(61, 15);
            this.lblIterations.TabIndex = 8;
            this.lblIterations.Text = "Iterations:";
            //
            // cmbIterations
            //
            this.cmbIterations.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbIterations.FormattingEnabled = true;
            this.cmbIterations.Items.AddRange(new object[] {
            "1",
            "2",
            "3",
            "5",
            "10"});
            this.cmbIterations.Location = new System.Drawing.Point(130, 248);
            this.cmbIterations.Name = "cmbIterations";
            this.cmbIterations.Size = new System.Drawing.Size(100, 23);
            this.cmbIterations.TabIndex = 9;
            this.cmbIterations.SelectedIndex = 0;
            //
            // lblApiVersion
            //
            this.lblApiVersion.AutoSize = true;
            this.lblApiVersion.Location = new System.Drawing.Point(250, 230);
            this.lblApiVersion.Name = "lblApiVersion";
            this.lblApiVersion.Size = new System.Drawing.Size(68, 15);
            this.lblApiVersion.TabIndex = 10;
            this.lblApiVersion.Text = "API Version:";
            //
            // cmbApiVersion
            //
            this.cmbApiVersion.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbApiVersion.FormattingEnabled = true;
            this.cmbApiVersion.Items.AddRange(new object[] { "1", "2" });
            this.cmbApiVersion.Location = new System.Drawing.Point(250, 248);
            this.cmbApiVersion.Name = "cmbApiVersion";
            this.cmbApiVersion.Size = new System.Drawing.Size(100, 23);
            this.cmbApiVersion.TabIndex = 11;
            this.cmbApiVersion.SelectedIndex = 1;
            //
            // btnRunTest
            //
            this.btnRunTest.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.btnRunTest.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRunTest.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnRunTest.ForeColor = System.Drawing.Color.White;
            this.btnRunTest.Location = new System.Drawing.Point(12, 285);
            this.btnRunTest.Name = "btnRunTest";
            this.btnRunTest.Size = new System.Drawing.Size(120, 35);
            this.btnRunTest.TabIndex = 10;
            this.btnRunTest.Text = "Run Load Test";
            this.btnRunTest.UseVisualStyleBackColor = false;
            this.btnRunTest.Click += new System.EventHandler(this.btnRunTest_Click);
            //
            // btnStop
            //
            this.btnStop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.btnStop.Enabled = false;
            this.btnStop.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStop.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnStop.ForeColor = System.Drawing.Color.White;
            this.btnStop.Location = new System.Drawing.Point(148, 285);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(120, 35);
            this.btnStop.TabIndex = 11;
            this.btnStop.Text = "Stop";
            this.btnStop.UseVisualStyleBackColor = false;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            //
            // btnClearResults
            //
            this.btnClearResults.Location = new System.Drawing.Point(284, 285);
            this.btnClearResults.Name = "btnClearResults";
            this.btnClearResults.Size = new System.Drawing.Size(120, 35);
            this.btnClearResults.TabIndex = 12;
            this.btnClearResults.Text = "Clear Results";
            this.btnClearResults.UseVisualStyleBackColor = true;
            this.btnClearResults.Click += new System.EventHandler(this.btnClearResults_Click);
            //
            // lblProgress
            //
            this.lblProgress.AutoSize = true;
            this.lblProgress.Location = new System.Drawing.Point(12, 333);
            this.lblProgress.Name = "lblProgress";
            this.lblProgress.Size = new System.Drawing.Size(55, 15);
            this.lblProgress.TabIndex = 13;
            this.lblProgress.Text = "Progress:";
            //
            // progressBar
            //
            this.progressBar.Location = new System.Drawing.Point(12, 351);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(1160, 23);
            this.progressBar.TabIndex = 14;
            //
            // lblResults
            //
            this.lblResults.AutoSize = true;
            this.lblResults.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblResults.Location = new System.Drawing.Point(12, 384);
            this.lblResults.Name = "lblResults";
            this.lblResults.Size = new System.Drawing.Size(51, 15);
            this.lblResults.TabIndex = 15;
            this.lblResults.Text = "Results:";
            //
            // dgvResults
            //
            this.dgvResults.AllowUserToAddRows = false;
            this.dgvResults.AllowUserToDeleteRows = false;
            this.dgvResults.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvResults.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colResultName,
            this.colResultPrompt,
            this.colResultStatus,
            this.colResultTime,
            this.colResultCount,
            this.colResultError});
            this.dgvResults.Location = new System.Drawing.Point(12, 402);
            this.dgvResults.Name = "dgvResults";
            this.dgvResults.ReadOnly = true;
            this.dgvResults.RowHeadersWidth = 25;
            this.dgvResults.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvResults.Size = new System.Drawing.Size(1160, 150);
            this.dgvResults.TabIndex = 16;
            //
            // colResultName
            //
            this.colResultName.HeaderText = "Name";
            this.colResultName.Name = "colResultName";
            this.colResultName.ReadOnly = true;
            this.colResultName.Width = 100;
            //
            // colResultPrompt
            //
            this.colResultPrompt.HeaderText = "Prompt";
            this.colResultPrompt.Name = "colResultPrompt";
            this.colResultPrompt.ReadOnly = true;
            this.colResultPrompt.Width = 200;
            //
            // colResultStatus
            //
            this.colResultStatus.HeaderText = "Status";
            this.colResultStatus.Name = "colResultStatus";
            this.colResultStatus.ReadOnly = true;
            this.colResultStatus.Width = 70;
            //
            // colResultTime
            //
            this.colResultTime.HeaderText = "Time (ms)";
            this.colResultTime.Name = "colResultTime";
            this.colResultTime.ReadOnly = true;
            this.colResultTime.Width = 90;
            //
            // colResultCount
            //
            this.colResultCount.HeaderText = "Count";
            this.colResultCount.Name = "colResultCount";
            this.colResultCount.ReadOnly = true;
            this.colResultCount.Width = 60;
            //
            // colResultError
            //
            this.colResultError.HeaderText = "Error";
            this.colResultError.Name = "colResultError";
            this.colResultError.ReadOnly = true;
            this.colResultError.Width = 200;
            //
            // lblStats
            //
            this.lblStats.AutoSize = true;
            this.lblStats.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblStats.Location = new System.Drawing.Point(12, 563);
            this.lblStats.Name = "lblStats";
            this.lblStats.Size = new System.Drawing.Size(66, 15);
            this.lblStats.TabIndex = 17;
            this.lblStats.Text = "Summary:";
            //
            // txtStats
            //
            this.txtStats.Font = new System.Drawing.Font("Consolas", 9F);
            this.txtStats.Location = new System.Drawing.Point(12, 581);
            this.txtStats.Multiline = true;
            this.txtStats.Name = "txtStats";
            this.txtStats.ReadOnly = true;
            this.txtStats.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtStats.Size = new System.Drawing.Size(1020, 59);
            this.txtStats.TabIndex = 18;
            //
            // btnExportResults
            //
            this.btnExportResults.Location = new System.Drawing.Point(1042, 581);
            this.btnExportResults.Name = "btnExportResults";
            this.btnExportResults.Size = new System.Drawing.Size(130, 35);
            this.btnExportResults.TabIndex = 19;
            this.btnExportResults.Text = "Export Results CSV";
            this.btnExportResults.UseVisualStyleBackColor = true;
            this.btnExportResults.Click += new System.EventHandler(this.btnExportResults_Click);
            //
            // LoadTestControl
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btnExportResults);
            this.Controls.Add(this.txtStats);
            this.Controls.Add(this.lblStats);
            this.Controls.Add(this.dgvResults);
            this.Controls.Add(this.lblResults);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.lblProgress);
            this.Controls.Add(this.btnClearResults);
            this.Controls.Add(this.btnStop);
            this.Controls.Add(this.btnRunTest);
            this.Controls.Add(this.cmbApiVersion);
            this.Controls.Add(this.lblApiVersion);
            this.Controls.Add(this.cmbIterations);
            this.Controls.Add(this.lblIterations);
            this.Controls.Add(this.cmbConcurrency);
            this.Controls.Add(this.lblConcurrency);
            this.Controls.Add(this.cmbEndpointType);
            this.Controls.Add(this.lblEndpointType);
            this.Controls.Add(this.btnSaveCsv);
            this.Controls.Add(this.btnLoadCsv);
            this.Controls.Add(this.btnRemove);
            this.Controls.Add(this.btnAdd);
            this.Controls.Add(this.dgvPrompts);
            this.Controls.Add(this.lblTestSuite);
            this.Name = "LoadTestControl";
            this.Size = new System.Drawing.Size(1192, 652);
            ((System.ComponentModel.ISupportInitialize)(this.dgvPrompts)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvResults)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Label lblTestSuite;
        private System.Windows.Forms.DataGridView dgvPrompts;
        private System.Windows.Forms.DataGridViewTextBoxColumn colName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colPrompt;
        private System.Windows.Forms.DataGridViewTextBoxColumn colNumJobs;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDays;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCity;
        private System.Windows.Forms.DataGridViewTextBoxColumn colState;
        private System.Windows.Forms.DataGridViewTextBoxColumn colMiles;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colOnsite;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colHybrid;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.Button btnRemove;
        private System.Windows.Forms.Button btnLoadCsv;
        private System.Windows.Forms.Button btnSaveCsv;
        private System.Windows.Forms.Label lblEndpointType;
        private System.Windows.Forms.ComboBox cmbEndpointType;
        private System.Windows.Forms.Label lblConcurrency;
        private System.Windows.Forms.ComboBox cmbConcurrency;
        private System.Windows.Forms.Label lblIterations;
        private System.Windows.Forms.ComboBox cmbIterations;
        private System.Windows.Forms.Button btnRunTest;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Button btnClearResults;
        private System.Windows.Forms.Label lblResults;
        private System.Windows.Forms.DataGridView dgvResults;
        private System.Windows.Forms.DataGridViewTextBoxColumn colResultName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colResultPrompt;
        private System.Windows.Forms.DataGridViewTextBoxColumn colResultStatus;
        private System.Windows.Forms.DataGridViewTextBoxColumn colResultTime;
        private System.Windows.Forms.DataGridViewTextBoxColumn colResultCount;
        private System.Windows.Forms.DataGridViewTextBoxColumn colResultError;
        private System.Windows.Forms.Label lblStats;
        private System.Windows.Forms.TextBox txtStats;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Label lblProgress;
        private System.Windows.Forms.Button btnExportResults;
        private System.Windows.Forms.Label lblApiVersion;
        private System.Windows.Forms.ComboBox cmbApiVersion;
    }
}
