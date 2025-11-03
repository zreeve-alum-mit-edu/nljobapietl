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
            this.lblPrompt = new System.Windows.Forms.Label();
            this.txtPrompt = new System.Windows.Forms.TextBox();
            this.lblNumJobs = new System.Windows.Forms.Label();
            this.txtNumJobs = new System.Windows.Forms.TextBox();
            this.chkRemote = new System.Windows.Forms.CheckBox();
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
            this.txtLocation = new System.Windows.Forms.TextBox();
            this.lblLocation = new System.Windows.Forms.Label();
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
            this.tabControl.SuspendLayout();
            this.tabSearch.SuspendLayout();
            this.grpFilter.SuspendLayout();
            this.tabRemoteSearch.SuspendLayout();
            this.tabLocation.SuspendLayout();
            this.SuspendLayout();
            //
            // tabControl
            //
            this.tabControl.Controls.Add(this.tabSearch);
            this.tabControl.Controls.Add(this.tabRemoteSearch);
            this.tabControl.Controls.Add(this.tabLocation);
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
            this.tabSearch.Controls.Add(this.chkRemote);
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
            // chkRemote
            //
            this.chkRemote.AutoSize = true;
            this.chkRemote.Checked = true;
            this.chkRemote.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkRemote.Location = new System.Drawing.Point(130, 126);
            this.chkRemote.Name = "chkRemote";
            this.chkRemote.Size = new System.Drawing.Size(111, 19);
            this.chkRemote.TabIndex = 4;
            this.chkRemote.Text = "Include Remote";
            this.chkRemote.UseVisualStyleBackColor = true;
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
            this.grpFilter.Controls.Add(this.txtLocation);
            this.grpFilter.Controls.Add(this.lblLocation);
            this.grpFilter.Location = new System.Drawing.Point(12, 163);
            this.grpFilter.Name = "grpFilter";
            this.grpFilter.Size = new System.Drawing.Size(760, 100);
            this.grpFilter.TabIndex = 7;
            this.grpFilter.TabStop = false;
            this.grpFilter.Text = "Location Filter (optional)";
            //
            // chkHybrid
            //
            this.chkHybrid.AutoSize = true;
            this.chkHybrid.Checked = true;
            this.chkHybrid.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkHybrid.Location = new System.Drawing.Point(520, 54);
            this.chkHybrid.Name = "chkHybrid";
            this.chkHybrid.Size = new System.Drawing.Size(106, 19);
            this.chkHybrid.TabIndex = 5;
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
            this.chkOnsite.TabIndex = 4;
            this.chkOnsite.Text = "Include Onsite";
            this.chkOnsite.UseVisualStyleBackColor = true;
            //
            // txtMiles
            //
            this.txtMiles.Location = new System.Drawing.Point(380, 52);
            this.txtMiles.Name = "txtMiles";
            this.txtMiles.Size = new System.Drawing.Size(100, 23);
            this.txtMiles.TabIndex = 3;
            this.txtMiles.Text = "50";
            //
            // lblMiles
            //
            this.lblMiles.AutoSize = true;
            this.lblMiles.Location = new System.Drawing.Point(380, 34);
            this.lblMiles.Name = "lblMiles";
            this.lblMiles.Size = new System.Drawing.Size(38, 15);
            this.lblMiles.TabIndex = 2;
            this.lblMiles.Text = "Miles:";
            //
            // txtLocation
            //
            this.txtLocation.Location = new System.Drawing.Point(18, 52);
            this.txtLocation.Name = "txtLocation";
            this.txtLocation.Size = new System.Drawing.Size(340, 23);
            this.txtLocation.TabIndex = 1;
            this.txtLocation.Text = "";
            //
            // lblLocation
            //
            this.lblLocation.AutoSize = true;
            this.lblLocation.Location = new System.Drawing.Point(18, 34);
            this.lblLocation.Name = "lblLocation";
            this.lblLocation.Size = new System.Drawing.Size(165, 15);
            this.lblLocation.TabIndex = 0;
            this.lblLocation.Text = "Location (e.g., \"Austin,TX\"):";
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
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabSearch;
        private System.Windows.Forms.Label lblPrompt;
        private System.Windows.Forms.TextBox txtPrompt;
        private System.Windows.Forms.Label lblNumJobs;
        private System.Windows.Forms.TextBox txtNumJobs;
        private System.Windows.Forms.CheckBox chkRemote;
        private System.Windows.Forms.Label lblDays;
        private System.Windows.Forms.TextBox txtDays;
        private System.Windows.Forms.GroupBox grpFilter;
        private System.Windows.Forms.Label lblLocation;
        private System.Windows.Forms.TextBox txtLocation;
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
    }
}
