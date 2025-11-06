namespace JobApi.TestGui
{
    partial class Form1
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
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabSearch = new System.Windows.Forms.TabPage();
            this.tabRemoteSearch = new System.Windows.Forms.TabPage();
            this.tabAuditLogs = new System.Windows.Forms.TabPage();
            this.tabCounts = new System.Windows.Forms.TabPage();
            this.tabCentroidJobs = new System.Windows.Forms.TabPage();
            this.lblPrompt = new System.Windows.Forms.Label();
            this.txtPrompt = new System.Windows.Forms.TextBox();
            this.lblNumJobs = new System.Windows.Forms.Label();
            this.txtNumJobs = new System.Windows.Forms.TextBox();
            this.lblDays = new System.Windows.Forms.Label();
            this.txtDays = new System.Windows.Forms.TextBox();
            this.lblRemotePrompt = new System.Windows.Forms.Label();
            this.txtRemotePrompt = new System.Windows.Forms.TextBox();
            this.lblRemoteNumJobs = new System.Windows.Forms.Label();
            this.txtRemoteNumJobs = new System.Windows.Forms.TextBox();
            this.lblRemoteDays = new System.Windows.Forms.Label();
            this.txtRemoteDays = new System.Windows.Forms.TextBox();
            this.btnSearchRemote = new System.Windows.Forms.Button();
            this.btnClearRemote = new System.Windows.Forms.Button();
            this.txtRemoteResults = new System.Windows.Forms.TextBox();
            this.lblRemoteResults = new System.Windows.Forms.Label();
            this.grpFilter = new System.Windows.Forms.GroupBox();
            this.chkHybrid = new System.Windows.Forms.CheckBox();
            this.chkOnsite = new System.Windows.Forms.CheckBox();
            this.txtMiles = new System.Windows.Forms.TextBox();
            this.lblMiles = new System.Windows.Forms.Label();
            this.txtSearchCity = new System.Windows.Forms.TextBox();
            this.lblSearchCity = new System.Windows.Forms.Label();
            this.txtSearchState = new System.Windows.Forms.TextBox();
            this.lblSearchState = new System.Windows.Forms.Label();
            this.btnSearch = new System.Windows.Forms.Button();
            this.btnClear = new System.Windows.Forms.Button();
            this.txtResults = new System.Windows.Forms.TextBox();
            this.lblResults = new System.Windows.Forms.Label();
            this.tabLocation = new System.Windows.Forms.TabPage();
            this.lblCity = new System.Windows.Forms.Label();
            this.txtCity = new System.Windows.Forms.TextBox();
            this.lblState = new System.Windows.Forms.Label();
            this.txtState = new System.Windows.Forms.TextBox();
            this.lblCountry = new System.Windows.Forms.Label();
            this.txtCountry = new System.Windows.Forms.TextBox();
            this.btnValidateLocation = new System.Windows.Forms.Button();
            this.btnClearLocation = new System.Windows.Forms.Button();
            this.lblLocationResults = new System.Windows.Forms.Label();
            this.txtLocationResults = new System.Windows.Forms.TextBox();
            this.dgvAuditLogs = new System.Windows.Forms.DataGridView();
            this.btnFirstPage = new System.Windows.Forms.Button();
            this.btnPreviousPage = new System.Windows.Forms.Button();
            this.btnNextPage = new System.Windows.Forms.Button();
            this.btnLastPage = new System.Windows.Forms.Button();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.lblPageInfo = new System.Windows.Forms.Label();
            this.dgvJobStatusCounts = new System.Windows.Forms.DataGridView();
            this.dgvCentroidCounts = new System.Windows.Forms.DataGridView();
            this.lblJobStatusCounts = new System.Windows.Forms.Label();
            this.lblCentroidCounts = new System.Windows.Forms.Label();
            this.lblTotalJobs = new System.Windows.Forms.Label();
            this.lblTotalLocations = new System.Windows.Forms.Label();
            this.lblTotalUrls = new System.Windows.Forms.Label();
            this.lblInvalidLocations = new System.Windows.Forms.Label();
            this.lblTotalJobsValue = new System.Windows.Forms.Label();
            this.lblTotalLocationsValue = new System.Windows.Forms.Label();
            this.lblTotalUrlsValue = new System.Windows.Forms.Label();
            this.lblInvalidLocationsValue = new System.Windows.Forms.Label();
            this.btnRefreshCounts = new System.Windows.Forms.Button();
            this.dgvCentroidJobs = new System.Windows.Forms.DataGridView();
            this.btnCentroidFirstPage = new System.Windows.Forms.Button();
            this.btnCentroidPreviousPage = new System.Windows.Forms.Button();
            this.btnCentroidNextPage = new System.Windows.Forms.Button();
            this.btnCentroidLastPage = new System.Windows.Forms.Button();
            this.lblCentroidPageInfo = new System.Windows.Forms.Label();
            this.lblSelectedCentroid = new System.Windows.Forms.Label();
            this.tabControl.SuspendLayout();
            this.tabSearch.SuspendLayout();
            this.grpFilter.SuspendLayout();
            this.tabRemoteSearch.SuspendLayout();
            this.tabLocation.SuspendLayout();
            this.tabAuditLogs.SuspendLayout();
            this.tabCounts.SuspendLayout();
            this.tabCentroidJobs.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvAuditLogs)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvJobStatusCounts)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvCentroidCounts)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvCentroidJobs)).BeginInit();
            this.SuspendLayout();
            //
            // tabControl
            //
            this.tabControl.Controls.Add(this.tabSearch);
            this.tabControl.Controls.Add(this.tabRemoteSearch);
            this.tabControl.Controls.Add(this.tabLocation);
            this.tabControl.Controls.Add(this.tabAuditLogs);
            this.tabControl.Controls.Add(this.tabCounts);
            this.tabControl.Controls.Add(this.tabCentroidJobs);
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.Location = new System.Drawing.Point(0, 0);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(800, 680);
            this.tabControl.TabIndex = 0;
            //
            // tabSearch
            //
            this.tabSearch.BackColor = System.Drawing.SystemColors.Control;
            this.tabSearch.Controls.Add(this.lblResults);
            this.tabSearch.Controls.Add(this.txtResults);
            this.tabSearch.Controls.Add(this.btnClear);
            this.tabSearch.Controls.Add(this.btnSearch);
            this.tabSearch.Controls.Add(this.grpFilter);
            this.tabSearch.Controls.Add(this.txtDays);
            this.tabSearch.Controls.Add(this.lblDays);
            this.tabSearch.Controls.Add(this.txtNumJobs);
            this.tabSearch.Controls.Add(this.lblNumJobs);
            this.tabSearch.Controls.Add(this.txtPrompt);
            this.tabSearch.Controls.Add(this.lblPrompt);
            this.tabSearch.Location = new System.Drawing.Point(4, 24);
            this.tabSearch.Name = "tabSearch";
            this.tabSearch.Padding = new System.Windows.Forms.Padding(3);
            this.tabSearch.Size = new System.Drawing.Size(792, 652);
            this.tabSearch.TabIndex = 0;
            this.tabSearch.Text = "Job Search";
            //
            // lblPrompt
            //
            this.lblPrompt.AutoSize = true;
            this.lblPrompt.Location = new System.Drawing.Point(12, 15);
            this.lblPrompt.Name = "lblPrompt";
            this.lblPrompt.Size = new System.Drawing.Size(107, 15);
            this.lblPrompt.TabIndex = 0;
            this.lblPrompt.Text = "Prompt (required):";
            //
            // txtPrompt
            //
            this.txtPrompt.Location = new System.Drawing.Point(12, 33);
            this.txtPrompt.Multiline = true;
            this.txtPrompt.Name = "txtPrompt";
            this.txtPrompt.Size = new System.Drawing.Size(760, 60);
            this.txtPrompt.TabIndex = 1;
            this.txtPrompt.Text = "senior software engineer with Python and AWS experience";
            //
            // lblNumJobs
            //
            this.lblNumJobs.AutoSize = true;
            this.lblNumJobs.Location = new System.Drawing.Point(12, 106);
            this.lblNumJobs.Name = "lblNumJobs";
            this.lblNumJobs.Size = new System.Drawing.Size(59, 15);
            this.lblNumJobs.TabIndex = 2;
            this.lblNumJobs.Text = "NumJobs:";
            //
            // txtNumJobs
            //
            this.txtNumJobs.Location = new System.Drawing.Point(12, 124);
            this.txtNumJobs.Name = "txtNumJobs";
            this.txtNumJobs.Size = new System.Drawing.Size(100, 23);
            this.txtNumJobs.TabIndex = 3;
            this.txtNumJobs.Text = "10";
            //
            // lblDays
            //
            this.lblDays.AutoSize = true;
            this.lblDays.Location = new System.Drawing.Point(260, 106);
            this.lblDays.Name = "lblDays";
            this.lblDays.Size = new System.Drawing.Size(143, 15);
            this.lblDays.TabIndex = 5;
            this.lblDays.Text = "Days Since Posting (opt):";
            //
            // txtDays
            //
            this.txtDays.Location = new System.Drawing.Point(260, 124);
            this.txtDays.Name = "txtDays";
            this.txtDays.Size = new System.Drawing.Size(100, 23);
            this.txtDays.TabIndex = 6;
            //
            // grpFilter
            //
            this.grpFilter.Controls.Add(this.chkHybrid);
            this.grpFilter.Controls.Add(this.chkOnsite);
            this.grpFilter.Controls.Add(this.txtMiles);
            this.grpFilter.Controls.Add(this.lblMiles);
            this.grpFilter.Controls.Add(this.txtSearchState);
            this.grpFilter.Controls.Add(this.lblSearchState);
            this.grpFilter.Controls.Add(this.txtSearchCity);
            this.grpFilter.Controls.Add(this.lblSearchCity);
            this.grpFilter.Location = new System.Drawing.Point(12, 163);
            this.grpFilter.Name = "grpFilter";
            this.grpFilter.Size = new System.Drawing.Size(760, 100);
            this.grpFilter.TabIndex = 7;
            this.grpFilter.TabStop = false;
            this.grpFilter.Text = "Location Filter (required)";
            //
            // chkHybrid
            //
            this.chkHybrid.AutoSize = true;
            this.chkHybrid.Checked = true;
            this.chkHybrid.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkHybrid.Location = new System.Drawing.Point(520, 54);
            this.chkHybrid.Name = "chkHybrid";
            this.chkHybrid.Size = new System.Drawing.Size(106, 19);
            this.chkHybrid.TabIndex = 7;
            this.chkHybrid.Text = "Include Hybrid";
            this.chkHybrid.UseVisualStyleBackColor = true;
            //
            // chkOnsite
            //
            this.chkOnsite.AutoSize = true;
            this.chkOnsite.Checked = true;
            this.chkOnsite.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkOnsite.Location = new System.Drawing.Point(520, 29);
            this.chkOnsite.Name = "chkOnsite";
            this.chkOnsite.Size = new System.Drawing.Size(106, 19);
            this.chkOnsite.TabIndex = 6;
            this.chkOnsite.Text = "Include Onsite";
            this.chkOnsite.UseVisualStyleBackColor = true;
            //
            // txtMiles
            //
            this.txtMiles.Location = new System.Drawing.Point(380, 52);
            this.txtMiles.Name = "txtMiles";
            this.txtMiles.Size = new System.Drawing.Size(100, 23);
            this.txtMiles.TabIndex = 5;
            this.txtMiles.Text = "20";
            //
            // lblMiles
            //
            this.lblMiles.AutoSize = true;
            this.lblMiles.Location = new System.Drawing.Point(380, 34);
            this.lblMiles.Name = "lblMiles";
            this.lblMiles.Size = new System.Drawing.Size(38, 15);
            this.lblMiles.TabIndex = 4;
            this.lblMiles.Text = "Miles:";
            //
            // txtSearchState
            //
            this.txtSearchState.Location = new System.Drawing.Point(250, 52);
            this.txtSearchState.Name = "txtSearchState";
            this.txtSearchState.Size = new System.Drawing.Size(100, 23);
            this.txtSearchState.TabIndex = 3;
            this.txtSearchState.Text = "TX";
            //
            // lblSearchState
            //
            this.lblSearchState.AutoSize = true;
            this.lblSearchState.Location = new System.Drawing.Point(250, 34);
            this.lblSearchState.Name = "lblSearchState";
            this.lblSearchState.Size = new System.Drawing.Size(36, 15);
            this.lblSearchState.TabIndex = 2;
            this.lblSearchState.Text = "State:";
            //
            // txtSearchCity
            //
            this.txtSearchCity.Location = new System.Drawing.Point(18, 52);
            this.txtSearchCity.Name = "txtSearchCity";
            this.txtSearchCity.Size = new System.Drawing.Size(210, 23);
            this.txtSearchCity.TabIndex = 1;
            this.txtSearchCity.Text = "Austin";
            //
            // lblSearchCity
            //
            this.lblSearchCity.AutoSize = true;
            this.lblSearchCity.Location = new System.Drawing.Point(18, 34);
            this.lblSearchCity.Name = "lblSearchCity";
            this.lblSearchCity.Size = new System.Drawing.Size(31, 15);
            this.lblSearchCity.TabIndex = 0;
            this.lblSearchCity.Text = "City:";
            //
            // btnSearch
            //
            this.btnSearch.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.btnSearch.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSearch.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnSearch.ForeColor = System.Drawing.Color.White;
            this.btnSearch.Location = new System.Drawing.Point(12, 279);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new System.Drawing.Size(120, 35);
            this.btnSearch.TabIndex = 8;
            this.btnSearch.Text = "Search";
            this.btnSearch.UseVisualStyleBackColor = false;
            this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);
            //
            // btnClear
            //
            this.btnClear.Location = new System.Drawing.Point(148, 279);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(120, 35);
            this.btnClear.TabIndex = 9;
            this.btnClear.Text = "Clear";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            //
            // txtResults
            //
            this.txtResults.Font = new System.Drawing.Font("Consolas", 9F);
            this.txtResults.Location = new System.Drawing.Point(12, 345);
            this.txtResults.Multiline = true;
            this.txtResults.Name = "txtResults";
            this.txtResults.ReadOnly = true;
            this.txtResults.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtResults.Size = new System.Drawing.Size(760, 295);
            this.txtResults.TabIndex = 10;
            this.txtResults.WordWrap = false;
            //
            // lblResults
            //
            this.lblResults.AutoSize = true;
            this.lblResults.Location = new System.Drawing.Point(12, 327);
            this.lblResults.Name = "lblResults";
            this.lblResults.Size = new System.Drawing.Size(48, 15);
            this.lblResults.TabIndex = 11;
            this.lblResults.Text = "Results:";
            //
            // tabLocation
            //
            this.tabLocation.BackColor = System.Drawing.SystemColors.Control;
            this.tabLocation.Controls.Add(this.txtLocationResults);
            this.tabLocation.Controls.Add(this.lblLocationResults);
            this.tabLocation.Controls.Add(this.btnClearLocation);
            this.tabLocation.Controls.Add(this.btnValidateLocation);
            this.tabLocation.Controls.Add(this.txtCountry);
            this.tabLocation.Controls.Add(this.lblCountry);
            this.tabLocation.Controls.Add(this.txtState);
            this.tabLocation.Controls.Add(this.lblState);
            this.tabLocation.Controls.Add(this.txtCity);
            this.tabLocation.Controls.Add(this.lblCity);
            this.tabLocation.Location = new System.Drawing.Point(4, 24);
            this.tabLocation.Name = "tabLocation";
            this.tabLocation.Padding = new System.Windows.Forms.Padding(3);
            this.tabLocation.Size = new System.Drawing.Size(792, 652);
            this.tabLocation.TabIndex = 1;
            this.tabLocation.Text = "Location Validation";
            //
            // lblCity
            //
            this.lblCity.AutoSize = true;
            this.lblCity.Location = new System.Drawing.Point(12, 15);
            this.lblCity.Name = "lblCity";
            this.lblCity.Size = new System.Drawing.Size(31, 15);
            this.lblCity.TabIndex = 0;
            this.lblCity.Text = "City:";
            //
            // txtCity
            //
            this.txtCity.Location = new System.Drawing.Point(12, 33);
            this.txtCity.Name = "txtCity";
            this.txtCity.Size = new System.Drawing.Size(300, 23);
            this.txtCity.TabIndex = 1;
            //
            // lblState
            //
            this.lblState.AutoSize = true;
            this.lblState.Location = new System.Drawing.Point(330, 15);
            this.lblState.Name = "lblState";
            this.lblState.Size = new System.Drawing.Size(36, 15);
            this.lblState.TabIndex = 2;
            this.lblState.Text = "State:";
            //
            // txtState
            //
            this.txtState.Location = new System.Drawing.Point(330, 33);
            this.txtState.Name = "txtState";
            this.txtState.Size = new System.Drawing.Size(100, 23);
            this.txtState.TabIndex = 3;
            //
            // lblCountry
            //
            this.lblCountry.AutoSize = true;
            this.lblCountry.Location = new System.Drawing.Point(450, 15);
            this.lblCountry.Name = "lblCountry";
            this.lblCountry.Size = new System.Drawing.Size(56, 15);
            this.lblCountry.TabIndex = 4;
            this.lblCountry.Text = "Country:";
            //
            // txtCountry
            //
            this.txtCountry.Location = new System.Drawing.Point(450, 33);
            this.txtCountry.Name = "txtCountry";
            this.txtCountry.Size = new System.Drawing.Size(100, 23);
            this.txtCountry.TabIndex = 5;
            this.txtCountry.Text = "US";
            //
            // btnValidateLocation
            //
            this.btnValidateLocation.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.btnValidateLocation.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnValidateLocation.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnValidateLocation.ForeColor = System.Drawing.Color.White;
            this.btnValidateLocation.Location = new System.Drawing.Point(12, 75);
            this.btnValidateLocation.Name = "btnValidateLocation";
            this.btnValidateLocation.Size = new System.Drawing.Size(120, 35);
            this.btnValidateLocation.TabIndex = 6;
            this.btnValidateLocation.Text = "Validate";
            this.btnValidateLocation.UseVisualStyleBackColor = false;
            this.btnValidateLocation.Click += new System.EventHandler(this.btnValidateLocation_Click);
            //
            // btnClearLocation
            //
            this.btnClearLocation.Location = new System.Drawing.Point(148, 75);
            this.btnClearLocation.Name = "btnClearLocation";
            this.btnClearLocation.Size = new System.Drawing.Size(120, 35);
            this.btnClearLocation.TabIndex = 7;
            this.btnClearLocation.Text = "Clear";
            this.btnClearLocation.UseVisualStyleBackColor = true;
            this.btnClearLocation.Click += new System.EventHandler(this.btnClearLocation_Click);
            //
            // lblLocationResults
            //
            this.lblLocationResults.AutoSize = true;
            this.lblLocationResults.Location = new System.Drawing.Point(12, 123);
            this.lblLocationResults.Name = "lblLocationResults";
            this.lblLocationResults.Size = new System.Drawing.Size(48, 15);
            this.lblLocationResults.TabIndex = 8;
            this.lblLocationResults.Text = "Results:";
            //
            // txtLocationResults
            //
            this.txtLocationResults.Font = new System.Drawing.Font("Consolas", 9F);
            this.txtLocationResults.Location = new System.Drawing.Point(12, 141);
            this.txtLocationResults.Multiline = true;
            this.txtLocationResults.Name = "txtLocationResults";
            this.txtLocationResults.ReadOnly = true;
            this.txtLocationResults.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtLocationResults.Size = new System.Drawing.Size(760, 499);
            this.txtLocationResults.TabIndex = 9;
            this.txtLocationResults.WordWrap = false;
            //
            // tabRemoteSearch
            //
            this.tabRemoteSearch.BackColor = System.Drawing.SystemColors.Control;
            this.tabRemoteSearch.Controls.Add(this.lblRemoteResults);
            this.tabRemoteSearch.Controls.Add(this.txtRemoteResults);
            this.tabRemoteSearch.Controls.Add(this.btnClearRemote);
            this.tabRemoteSearch.Controls.Add(this.btnSearchRemote);
            this.tabRemoteSearch.Controls.Add(this.txtRemoteDays);
            this.tabRemoteSearch.Controls.Add(this.lblRemoteDays);
            this.tabRemoteSearch.Controls.Add(this.txtRemoteNumJobs);
            this.tabRemoteSearch.Controls.Add(this.lblRemoteNumJobs);
            this.tabRemoteSearch.Controls.Add(this.txtRemotePrompt);
            this.tabRemoteSearch.Controls.Add(this.lblRemotePrompt);
            this.tabRemoteSearch.Location = new System.Drawing.Point(4, 24);
            this.tabRemoteSearch.Name = "tabRemoteSearch";
            this.tabRemoteSearch.Padding = new System.Windows.Forms.Padding(3);
            this.tabRemoteSearch.Size = new System.Drawing.Size(792, 652);
            this.tabRemoteSearch.TabIndex = 2;
            this.tabRemoteSearch.Text = "Remote Jobs Only";
            //
            // lblRemotePrompt
            //
            this.lblRemotePrompt.AutoSize = true;
            this.lblRemotePrompt.Location = new System.Drawing.Point(12, 15);
            this.lblRemotePrompt.Name = "lblRemotePrompt";
            this.lblRemotePrompt.Size = new System.Drawing.Size(107, 15);
            this.lblRemotePrompt.TabIndex = 0;
            this.lblRemotePrompt.Text = "Prompt (required):";
            //
            // txtRemotePrompt
            //
            this.txtRemotePrompt.Location = new System.Drawing.Point(12, 33);
            this.txtRemotePrompt.Multiline = true;
            this.txtRemotePrompt.Name = "txtRemotePrompt";
            this.txtRemotePrompt.Size = new System.Drawing.Size(760, 60);
            this.txtRemotePrompt.TabIndex = 1;
            this.txtRemotePrompt.Text = "senior software engineer with Python and AWS experience";
            //
            // lblRemoteNumJobs
            //
            this.lblRemoteNumJobs.AutoSize = true;
            this.lblRemoteNumJobs.Location = new System.Drawing.Point(12, 106);
            this.lblRemoteNumJobs.Name = "lblRemoteNumJobs";
            this.lblRemoteNumJobs.Size = new System.Drawing.Size(59, 15);
            this.lblRemoteNumJobs.TabIndex = 2;
            this.lblRemoteNumJobs.Text = "NumJobs:";
            //
            // txtRemoteNumJobs
            //
            this.txtRemoteNumJobs.Location = new System.Drawing.Point(12, 124);
            this.txtRemoteNumJobs.Name = "txtRemoteNumJobs";
            this.txtRemoteNumJobs.Size = new System.Drawing.Size(100, 23);
            this.txtRemoteNumJobs.TabIndex = 3;
            this.txtRemoteNumJobs.Text = "10";
            //
            // lblRemoteDays
            //
            this.lblRemoteDays.AutoSize = true;
            this.lblRemoteDays.Location = new System.Drawing.Point(130, 106);
            this.lblRemoteDays.Name = "lblRemoteDays";
            this.lblRemoteDays.Size = new System.Drawing.Size(143, 15);
            this.lblRemoteDays.TabIndex = 4;
            this.lblRemoteDays.Text = "Days Since Posting (opt):";
            //
            // txtRemoteDays
            //
            this.txtRemoteDays.Location = new System.Drawing.Point(130, 124);
            this.txtRemoteDays.Name = "txtRemoteDays";
            this.txtRemoteDays.Size = new System.Drawing.Size(100, 23);
            this.txtRemoteDays.TabIndex = 5;
            //
            // btnSearchRemote
            //
            this.btnSearchRemote.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.btnSearchRemote.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSearchRemote.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnSearchRemote.ForeColor = System.Drawing.Color.White;
            this.btnSearchRemote.Location = new System.Drawing.Point(12, 163);
            this.btnSearchRemote.Name = "btnSearchRemote";
            this.btnSearchRemote.Size = new System.Drawing.Size(120, 35);
            this.btnSearchRemote.TabIndex = 6;
            this.btnSearchRemote.Text = "Search Remote";
            this.btnSearchRemote.UseVisualStyleBackColor = false;
            this.btnSearchRemote.Click += new System.EventHandler(this.btnSearchRemote_Click);
            //
            // btnClearRemote
            //
            this.btnClearRemote.Location = new System.Drawing.Point(148, 163);
            this.btnClearRemote.Name = "btnClearRemote";
            this.btnClearRemote.Size = new System.Drawing.Size(120, 35);
            this.btnClearRemote.TabIndex = 7;
            this.btnClearRemote.Text = "Clear";
            this.btnClearRemote.UseVisualStyleBackColor = true;
            this.btnClearRemote.Click += new System.EventHandler(this.btnClearRemote_Click);
            //
            // txtRemoteResults
            //
            this.txtRemoteResults.Font = new System.Drawing.Font("Consolas", 9F);
            this.txtRemoteResults.Location = new System.Drawing.Point(12, 229);
            this.txtRemoteResults.Multiline = true;
            this.txtRemoteResults.Name = "txtRemoteResults";
            this.txtRemoteResults.ReadOnly = true;
            this.txtRemoteResults.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtRemoteResults.Size = new System.Drawing.Size(760, 411);
            this.txtRemoteResults.TabIndex = 8;
            this.txtRemoteResults.WordWrap = false;
            //
            // lblRemoteResults
            //
            this.lblRemoteResults.AutoSize = true;
            this.lblRemoteResults.Location = new System.Drawing.Point(12, 211);
            this.lblRemoteResults.Name = "lblRemoteResults";
            this.lblRemoteResults.Size = new System.Drawing.Size(48, 15);
            this.lblRemoteResults.TabIndex = 9;
            this.lblRemoteResults.Text = "Results:";
            //
            // tabAuditLogs
            //
            this.tabAuditLogs.BackColor = System.Drawing.SystemColors.Control;
            this.tabAuditLogs.Controls.Add(this.lblPageInfo);
            this.tabAuditLogs.Controls.Add(this.btnRefresh);
            this.tabAuditLogs.Controls.Add(this.btnLastPage);
            this.tabAuditLogs.Controls.Add(this.btnNextPage);
            this.tabAuditLogs.Controls.Add(this.btnPreviousPage);
            this.tabAuditLogs.Controls.Add(this.btnFirstPage);
            this.tabAuditLogs.Controls.Add(this.dgvAuditLogs);
            this.tabAuditLogs.Location = new System.Drawing.Point(4, 24);
            this.tabAuditLogs.Name = "tabAuditLogs";
            this.tabAuditLogs.Padding = new System.Windows.Forms.Padding(3);
            this.tabAuditLogs.Size = new System.Drawing.Size(792, 652);
            this.tabAuditLogs.TabIndex = 3;
            this.tabAuditLogs.Text = "Audit Logs";
            //
            // dgvAuditLogs
            //
            this.dgvAuditLogs.AllowUserToAddRows = false;
            this.dgvAuditLogs.AllowUserToDeleteRows = false;
            this.dgvAuditLogs.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dgvAuditLogs.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvAuditLogs.Location = new System.Drawing.Point(12, 50);
            this.dgvAuditLogs.Name = "dgvAuditLogs";
            this.dgvAuditLogs.ReadOnly = true;
            this.dgvAuditLogs.RowHeadersVisible = false;
            this.dgvAuditLogs.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvAuditLogs.Size = new System.Drawing.Size(768, 550);
            this.dgvAuditLogs.TabIndex = 0;
            //
            // btnFirstPage
            //
            this.btnFirstPage.Location = new System.Drawing.Point(12, 15);
            this.btnFirstPage.Name = "btnFirstPage";
            this.btnFirstPage.Size = new System.Drawing.Size(75, 25);
            this.btnFirstPage.TabIndex = 1;
            this.btnFirstPage.Text = "First";
            this.btnFirstPage.UseVisualStyleBackColor = true;
            this.btnFirstPage.Click += new System.EventHandler(this.btnFirstPage_Click);
            //
            // btnPreviousPage
            //
            this.btnPreviousPage.Location = new System.Drawing.Point(93, 15);
            this.btnPreviousPage.Name = "btnPreviousPage";
            this.btnPreviousPage.Size = new System.Drawing.Size(75, 25);
            this.btnPreviousPage.TabIndex = 2;
            this.btnPreviousPage.Text = "Previous";
            this.btnPreviousPage.UseVisualStyleBackColor = true;
            this.btnPreviousPage.Click += new System.EventHandler(this.btnPreviousPage_Click);
            //
            // btnNextPage
            //
            this.btnNextPage.Location = new System.Drawing.Point(174, 15);
            this.btnNextPage.Name = "btnNextPage";
            this.btnNextPage.Size = new System.Drawing.Size(75, 25);
            this.btnNextPage.TabIndex = 3;
            this.btnNextPage.Text = "Next";
            this.btnNextPage.UseVisualStyleBackColor = true;
            this.btnNextPage.Click += new System.EventHandler(this.btnNextPage_Click);
            //
            // btnLastPage
            //
            this.btnLastPage.Location = new System.Drawing.Point(255, 15);
            this.btnLastPage.Name = "btnLastPage";
            this.btnLastPage.Size = new System.Drawing.Size(75, 25);
            this.btnLastPage.TabIndex = 4;
            this.btnLastPage.Text = "Last";
            this.btnLastPage.UseVisualStyleBackColor = true;
            this.btnLastPage.Click += new System.EventHandler(this.btnLastPage_Click);
            //
            // btnRefresh
            //
            this.btnRefresh.Location = new System.Drawing.Point(350, 15);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(75, 25);
            this.btnRefresh.TabIndex = 5;
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            //
            // lblPageInfo
            //
            this.lblPageInfo.AutoSize = true;
            this.lblPageInfo.Location = new System.Drawing.Point(450, 20);
            this.lblPageInfo.Name = "lblPageInfo";
            this.lblPageInfo.Size = new System.Drawing.Size(0, 15);
            this.lblPageInfo.TabIndex = 6;
            //
            // tabCounts
            //
            this.tabCounts.BackColor = System.Drawing.SystemColors.Control;
            this.tabCounts.Controls.Add(this.btnRefreshCounts);
            this.tabCounts.Controls.Add(this.lblJobStatusCounts);
            this.tabCounts.Controls.Add(this.dgvJobStatusCounts);
            this.tabCounts.Controls.Add(this.lblCentroidCounts);
            this.tabCounts.Controls.Add(this.dgvCentroidCounts);
            this.tabCounts.Controls.Add(this.lblTotalJobs);
            this.tabCounts.Controls.Add(this.lblTotalJobsValue);
            this.tabCounts.Controls.Add(this.lblTotalLocations);
            this.tabCounts.Controls.Add(this.lblTotalLocationsValue);
            this.tabCounts.Controls.Add(this.lblTotalUrls);
            this.tabCounts.Controls.Add(this.lblTotalUrlsValue);
            this.tabCounts.Controls.Add(this.lblInvalidLocations);
            this.tabCounts.Controls.Add(this.lblInvalidLocationsValue);
            this.tabCounts.Location = new System.Drawing.Point(4, 24);
            this.tabCounts.Name = "tabCounts";
            this.tabCounts.Padding = new System.Windows.Forms.Padding(3);
            this.tabCounts.Size = new System.Drawing.Size(792, 652);
            this.tabCounts.TabIndex = 4;
            this.tabCounts.Text = "Counts";
            //
            // btnRefreshCounts
            //
            this.btnRefreshCounts.Location = new System.Drawing.Point(12, 15);
            this.btnRefreshCounts.Name = "btnRefreshCounts";
            this.btnRefreshCounts.Size = new System.Drawing.Size(100, 30);
            this.btnRefreshCounts.TabIndex = 0;
            this.btnRefreshCounts.Text = "Refresh";
            this.btnRefreshCounts.UseVisualStyleBackColor = true;
            this.btnRefreshCounts.Click += new System.EventHandler(this.btnRefreshCounts_Click);
            //
            // lblJobStatusCounts
            //
            this.lblJobStatusCounts.AutoSize = true;
            this.lblJobStatusCounts.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblJobStatusCounts.Location = new System.Drawing.Point(12, 60);
            this.lblJobStatusCounts.Name = "lblJobStatusCounts";
            this.lblJobStatusCounts.Size = new System.Drawing.Size(133, 19);
            this.lblJobStatusCounts.TabIndex = 1;
            this.lblJobStatusCounts.Text = "Job Status Counts:";
            //
            // dgvJobStatusCounts
            //
            this.dgvJobStatusCounts.AllowUserToAddRows = false;
            this.dgvJobStatusCounts.AllowUserToDeleteRows = false;
            this.dgvJobStatusCounts.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvJobStatusCounts.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvJobStatusCounts.Location = new System.Drawing.Point(12, 85);
            this.dgvJobStatusCounts.Name = "dgvJobStatusCounts";
            this.dgvJobStatusCounts.ReadOnly = true;
            this.dgvJobStatusCounts.RowHeadersVisible = false;
            this.dgvJobStatusCounts.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvJobStatusCounts.Size = new System.Drawing.Size(365, 250);
            this.dgvJobStatusCounts.TabIndex = 2;
            //
            // lblCentroidCounts
            //
            this.lblCentroidCounts.AutoSize = true;
            this.lblCentroidCounts.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblCentroidCounts.Location = new System.Drawing.Point(400, 60);
            this.lblCentroidCounts.Name = "lblCentroidCounts";
            this.lblCentroidCounts.Size = new System.Drawing.Size(126, 19);
            this.lblCentroidCounts.TabIndex = 3;
            this.lblCentroidCounts.Text = "Centroid Counts:";
            //
            // dgvCentroidCounts
            //
            this.dgvCentroidCounts.AllowUserToAddRows = false;
            this.dgvCentroidCounts.AllowUserToDeleteRows = false;
            this.dgvCentroidCounts.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvCentroidCounts.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvCentroidCounts.Location = new System.Drawing.Point(400, 85);
            this.dgvCentroidCounts.Name = "dgvCentroidCounts";
            this.dgvCentroidCounts.ReadOnly = true;
            this.dgvCentroidCounts.RowHeadersVisible = false;
            this.dgvCentroidCounts.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvCentroidCounts.Size = new System.Drawing.Size(365, 250);
            this.dgvCentroidCounts.TabIndex = 4;
            this.dgvCentroidCounts.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvCentroidCounts_CellClick);
            //
            // lblTotalJobs
            //
            this.lblTotalJobs.AutoSize = true;
            this.lblTotalJobs.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblTotalJobs.Location = new System.Drawing.Point(12, 360);
            this.lblTotalJobs.Name = "lblTotalJobs";
            this.lblTotalJobs.Size = new System.Drawing.Size(108, 15);
            this.lblTotalJobs.TabIndex = 5;
            this.lblTotalJobs.Text = "Total Job Records:";
            //
            // lblTotalJobsValue
            //
            this.lblTotalJobsValue.AutoSize = true;
            this.lblTotalJobsValue.Location = new System.Drawing.Point(130, 360);
            this.lblTotalJobsValue.Name = "lblTotalJobsValue";
            this.lblTotalJobsValue.Size = new System.Drawing.Size(13, 15);
            this.lblTotalJobsValue.TabIndex = 6;
            this.lblTotalJobsValue.Text = "0";
            //
            // lblTotalLocations
            //
            this.lblTotalLocations.AutoSize = true;
            this.lblTotalLocations.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblTotalLocations.Location = new System.Drawing.Point(12, 390);
            this.lblTotalLocations.Name = "lblTotalLocations";
            this.lblTotalLocations.Size = new System.Drawing.Size(135, 15);
            this.lblTotalLocations.TabIndex = 7;
            this.lblTotalLocations.Text = "Total Location Records:";
            //
            // lblTotalLocationsValue
            //
            this.lblTotalLocationsValue.AutoSize = true;
            this.lblTotalLocationsValue.Location = new System.Drawing.Point(155, 390);
            this.lblTotalLocationsValue.Name = "lblTotalLocationsValue";
            this.lblTotalLocationsValue.Size = new System.Drawing.Size(13, 15);
            this.lblTotalLocationsValue.TabIndex = 8;
            this.lblTotalLocationsValue.Text = "0";
            //
            // lblTotalUrls
            //
            this.lblTotalUrls.AutoSize = true;
            this.lblTotalUrls.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblTotalUrls.Location = new System.Drawing.Point(12, 420);
            this.lblTotalUrls.Name = "lblTotalUrls";
            this.lblTotalUrls.Size = new System.Drawing.Size(107, 15);
            this.lblTotalUrls.TabIndex = 9;
            this.lblTotalUrls.Text = "Total URL Records:";
            //
            // lblTotalUrlsValue
            //
            this.lblTotalUrlsValue.AutoSize = true;
            this.lblTotalUrlsValue.Location = new System.Drawing.Point(130, 420);
            this.lblTotalUrlsValue.Name = "lblTotalUrlsValue";
            this.lblTotalUrlsValue.Size = new System.Drawing.Size(13, 15);
            this.lblTotalUrlsValue.TabIndex = 10;
            this.lblTotalUrlsValue.Text = "0";
            //
            // lblInvalidLocations
            //
            this.lblInvalidLocations.AutoSize = true;
            this.lblInvalidLocations.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblInvalidLocations.Location = new System.Drawing.Point(12, 450);
            this.lblInvalidLocations.Name = "lblInvalidLocations";
            this.lblInvalidLocations.Size = new System.Drawing.Size(146, 15);
            this.lblInvalidLocations.TabIndex = 11;
            this.lblInvalidLocations.Text = "Invalid Location Records:";
            //
            // lblInvalidLocationsValue
            //
            this.lblInvalidLocationsValue.AutoSize = true;
            this.lblInvalidLocationsValue.Location = new System.Drawing.Point(165, 450);
            this.lblInvalidLocationsValue.Name = "lblInvalidLocationsValue";
            this.lblInvalidLocationsValue.Size = new System.Drawing.Size(13, 15);
            this.lblInvalidLocationsValue.TabIndex = 12;
            this.lblInvalidLocationsValue.Text = "0";
            //
            // tabCentroidJobs
            //
            this.tabCentroidJobs.BackColor = System.Drawing.SystemColors.Control;
            this.tabCentroidJobs.Controls.Add(this.lblSelectedCentroid);
            this.tabCentroidJobs.Controls.Add(this.lblCentroidPageInfo);
            this.tabCentroidJobs.Controls.Add(this.btnCentroidLastPage);
            this.tabCentroidJobs.Controls.Add(this.btnCentroidNextPage);
            this.tabCentroidJobs.Controls.Add(this.btnCentroidPreviousPage);
            this.tabCentroidJobs.Controls.Add(this.btnCentroidFirstPage);
            this.tabCentroidJobs.Controls.Add(this.dgvCentroidJobs);
            this.tabCentroidJobs.Location = new System.Drawing.Point(4, 24);
            this.tabCentroidJobs.Name = "tabCentroidJobs";
            this.tabCentroidJobs.Padding = new System.Windows.Forms.Padding(3);
            this.tabCentroidJobs.Size = new System.Drawing.Size(792, 652);
            this.tabCentroidJobs.TabIndex = 5;
            this.tabCentroidJobs.Text = "Centroid Jobs";
            //
            // dgvCentroidJobs
            //
            this.dgvCentroidJobs.AllowUserToAddRows = false;
            this.dgvCentroidJobs.AllowUserToDeleteRows = false;
            this.dgvCentroidJobs.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dgvCentroidJobs.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvCentroidJobs.Location = new System.Drawing.Point(12, 80);
            this.dgvCentroidJobs.Name = "dgvCentroidJobs";
            this.dgvCentroidJobs.ReadOnly = true;
            this.dgvCentroidJobs.RowHeadersVisible = false;
            this.dgvCentroidJobs.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvCentroidJobs.Size = new System.Drawing.Size(768, 520);
            this.dgvCentroidJobs.TabIndex = 0;
            //
            // btnCentroidFirstPage
            //
            this.btnCentroidFirstPage.Enabled = false;
            this.btnCentroidFirstPage.Location = new System.Drawing.Point(12, 45);
            this.btnCentroidFirstPage.Name = "btnCentroidFirstPage";
            this.btnCentroidFirstPage.Size = new System.Drawing.Size(75, 25);
            this.btnCentroidFirstPage.TabIndex = 1;
            this.btnCentroidFirstPage.Text = "First";
            this.btnCentroidFirstPage.UseVisualStyleBackColor = true;
            this.btnCentroidFirstPage.Click += new System.EventHandler(this.btnCentroidFirstPage_Click);
            //
            // btnCentroidPreviousPage
            //
            this.btnCentroidPreviousPage.Enabled = false;
            this.btnCentroidPreviousPage.Location = new System.Drawing.Point(93, 45);
            this.btnCentroidPreviousPage.Name = "btnCentroidPreviousPage";
            this.btnCentroidPreviousPage.Size = new System.Drawing.Size(75, 25);
            this.btnCentroidPreviousPage.TabIndex = 2;
            this.btnCentroidPreviousPage.Text = "Previous";
            this.btnCentroidPreviousPage.UseVisualStyleBackColor = true;
            this.btnCentroidPreviousPage.Click += new System.EventHandler(this.btnCentroidPreviousPage_Click);
            //
            // btnCentroidNextPage
            //
            this.btnCentroidNextPage.Enabled = false;
            this.btnCentroidNextPage.Location = new System.Drawing.Point(174, 45);
            this.btnCentroidNextPage.Name = "btnCentroidNextPage";
            this.btnCentroidNextPage.Size = new System.Drawing.Size(75, 25);
            this.btnCentroidNextPage.TabIndex = 3;
            this.btnCentroidNextPage.Text = "Next";
            this.btnCentroidNextPage.UseVisualStyleBackColor = true;
            this.btnCentroidNextPage.Click += new System.EventHandler(this.btnCentroidNextPage_Click);
            //
            // btnCentroidLastPage
            //
            this.btnCentroidLastPage.Enabled = false;
            this.btnCentroidLastPage.Location = new System.Drawing.Point(255, 45);
            this.btnCentroidLastPage.Name = "btnCentroidLastPage";
            this.btnCentroidLastPage.Size = new System.Drawing.Size(75, 25);
            this.btnCentroidLastPage.TabIndex = 4;
            this.btnCentroidLastPage.Text = "Last";
            this.btnCentroidLastPage.UseVisualStyleBackColor = true;
            this.btnCentroidLastPage.Click += new System.EventHandler(this.btnCentroidLastPage_Click);
            //
            // lblCentroidPageInfo
            //
            this.lblCentroidPageInfo.AutoSize = true;
            this.lblCentroidPageInfo.Location = new System.Drawing.Point(350, 50);
            this.lblCentroidPageInfo.Name = "lblCentroidPageInfo";
            this.lblCentroidPageInfo.Size = new System.Drawing.Size(0, 15);
            this.lblCentroidPageInfo.TabIndex = 5;
            //
            // lblSelectedCentroid
            //
            this.lblSelectedCentroid.AutoSize = true;
            this.lblSelectedCentroid.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblSelectedCentroid.Location = new System.Drawing.Point(12, 15);
            this.lblSelectedCentroid.Name = "lblSelectedCentroid";
            this.lblSelectedCentroid.Size = new System.Drawing.Size(293, 19);
            this.lblSelectedCentroid.TabIndex = 6;
            this.lblSelectedCentroid.Text = "Select a centroid from the Counts tab first";
            //
            // Form1
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 680);
            this.Controls.Add(this.tabControl);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Job Search API Tester";
            this.tabControl.ResumeLayout(false);
            this.tabSearch.ResumeLayout(false);
            this.tabSearch.PerformLayout();
            this.grpFilter.ResumeLayout(false);
            this.grpFilter.PerformLayout();
            this.tabRemoteSearch.ResumeLayout(false);
            this.tabRemoteSearch.PerformLayout();
            this.tabLocation.ResumeLayout(false);
            this.tabLocation.PerformLayout();
            this.tabAuditLogs.ResumeLayout(false);
            this.tabAuditLogs.PerformLayout();
            this.tabCounts.ResumeLayout(false);
            this.tabCounts.PerformLayout();
            this.tabCentroidJobs.ResumeLayout(false);
            this.tabCentroidJobs.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvAuditLogs)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvJobStatusCounts)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvCentroidCounts)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvCentroidJobs)).EndInit();
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabSearch;
        private System.Windows.Forms.Label lblPrompt;
        private System.Windows.Forms.TextBox txtPrompt;
        private System.Windows.Forms.Label lblNumJobs;
        private System.Windows.Forms.TextBox txtNumJobs;
        private System.Windows.Forms.Label lblDays;
        private System.Windows.Forms.TextBox txtDays;
        private System.Windows.Forms.GroupBox grpFilter;
        private System.Windows.Forms.Label lblSearchCity;
        private System.Windows.Forms.TextBox txtSearchCity;
        private System.Windows.Forms.Label lblSearchState;
        private System.Windows.Forms.TextBox txtSearchState;
        private System.Windows.Forms.Label lblMiles;
        private System.Windows.Forms.TextBox txtMiles;
        private System.Windows.Forms.CheckBox chkOnsite;
        private System.Windows.Forms.CheckBox chkHybrid;
        private System.Windows.Forms.Button btnSearch;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.TextBox txtResults;
        private System.Windows.Forms.Label lblResults;
        private System.Windows.Forms.TabPage tabLocation;
        private System.Windows.Forms.Label lblCity;
        private System.Windows.Forms.TextBox txtCity;
        private System.Windows.Forms.Label lblState;
        private System.Windows.Forms.TextBox txtState;
        private System.Windows.Forms.Label lblCountry;
        private System.Windows.Forms.TextBox txtCountry;
        private System.Windows.Forms.Button btnValidateLocation;
        private System.Windows.Forms.Button btnClearLocation;
        private System.Windows.Forms.Label lblLocationResults;
        private System.Windows.Forms.TextBox txtLocationResults;
        private System.Windows.Forms.TabPage tabRemoteSearch;
        private System.Windows.Forms.Label lblRemotePrompt;
        private System.Windows.Forms.TextBox txtRemotePrompt;
        private System.Windows.Forms.Label lblRemoteNumJobs;
        private System.Windows.Forms.TextBox txtRemoteNumJobs;
        private System.Windows.Forms.Label lblRemoteDays;
        private System.Windows.Forms.TextBox txtRemoteDays;
        private System.Windows.Forms.Button btnSearchRemote;
        private System.Windows.Forms.Button btnClearRemote;
        private System.Windows.Forms.TextBox txtRemoteResults;
        private System.Windows.Forms.Label lblRemoteResults;
        private System.Windows.Forms.TabPage tabAuditLogs;
        private System.Windows.Forms.DataGridView dgvAuditLogs;
        private System.Windows.Forms.Button btnFirstPage;
        private System.Windows.Forms.Button btnPreviousPage;
        private System.Windows.Forms.Button btnNextPage;
        private System.Windows.Forms.Button btnLastPage;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Label lblPageInfo;
        private System.Windows.Forms.TabPage tabCounts;
        private System.Windows.Forms.DataGridView dgvJobStatusCounts;
        private System.Windows.Forms.DataGridView dgvCentroidCounts;
        private System.Windows.Forms.Label lblJobStatusCounts;
        private System.Windows.Forms.Label lblCentroidCounts;
        private System.Windows.Forms.Label lblTotalJobs;
        private System.Windows.Forms.Label lblTotalLocations;
        private System.Windows.Forms.Label lblTotalUrls;
        private System.Windows.Forms.Label lblInvalidLocations;
        private System.Windows.Forms.Label lblTotalJobsValue;
        private System.Windows.Forms.Label lblTotalLocationsValue;
        private System.Windows.Forms.Label lblTotalUrlsValue;
        private System.Windows.Forms.Label lblInvalidLocationsValue;
        private System.Windows.Forms.Button btnRefreshCounts;
        private System.Windows.Forms.TabPage tabCentroidJobs;
        private System.Windows.Forms.DataGridView dgvCentroidJobs;
        private System.Windows.Forms.Button btnCentroidFirstPage;
        private System.Windows.Forms.Button btnCentroidPreviousPage;
        private System.Windows.Forms.Button btnCentroidNextPage;
        private System.Windows.Forms.Button btnCentroidLastPage;
        private System.Windows.Forms.Label lblCentroidPageInfo;
        private System.Windows.Forms.Label lblSelectedCentroid;
    }
}
