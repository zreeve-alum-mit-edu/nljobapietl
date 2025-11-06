namespace JobApi.TestGui.Controls
{
    partial class SearchControl
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
            this.lblPrompt = new System.Windows.Forms.Label();
            this.txtPrompt = new System.Windows.Forms.TextBox();
            this.lblNumJobs = new System.Windows.Forms.Label();
            this.txtNumJobs = new System.Windows.Forms.TextBox();
            this.lblDays = new System.Windows.Forms.Label();
            this.txtDays = new System.Windows.Forms.TextBox();
            this.lblVersion = new System.Windows.Forms.Label();
            this.cmbVersion = new System.Windows.Forms.ComboBox();
            this.grpFilter = new System.Windows.Forms.GroupBox();
            this.chkHybrid = new System.Windows.Forms.CheckBox();
            this.chkOnsite = new System.Windows.Forms.CheckBox();
            this.txtMiles = new System.Windows.Forms.TextBox();
            this.lblMiles = new System.Windows.Forms.Label();
            this.txtSearchState = new System.Windows.Forms.TextBox();
            this.lblSearchState = new System.Windows.Forms.Label();
            this.txtSearchCity = new System.Windows.Forms.TextBox();
            this.lblSearchCity = new System.Windows.Forms.Label();
            this.btnSearch = new System.Windows.Forms.Button();
            this.btnClear = new System.Windows.Forms.Button();
            this.txtResults = new System.Windows.Forms.TextBox();
            this.lblResults = new System.Windows.Forms.Label();
            this.grpFilter.SuspendLayout();
            this.SuspendLayout();
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
            // lblVersion
            //
            this.lblVersion.AutoSize = true;
            this.lblVersion.Location = new System.Drawing.Point(130, 106);
            this.lblVersion.Name = "lblVersion";
            this.lblVersion.Size = new System.Drawing.Size(68, 15);
            this.lblVersion.TabIndex = 7;
            this.lblVersion.Text = "API Version:";
            //
            // cmbVersion
            //
            this.cmbVersion.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbVersion.FormattingEnabled = true;
            this.cmbVersion.Items.AddRange(new object[] { "1", "2" });
            this.cmbVersion.Location = new System.Drawing.Point(130, 124);
            this.cmbVersion.Name = "cmbVersion";
            this.cmbVersion.Size = new System.Drawing.Size(100, 23);
            this.cmbVersion.TabIndex = 8;
            this.cmbVersion.SelectedIndex = 1;
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
            // SearchControl
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lblResults);
            this.Controls.Add(this.txtResults);
            this.Controls.Add(this.btnClear);
            this.Controls.Add(this.btnSearch);
            this.Controls.Add(this.grpFilter);
            this.Controls.Add(this.cmbVersion);
            this.Controls.Add(this.lblVersion);
            this.Controls.Add(this.txtDays);
            this.Controls.Add(this.lblDays);
            this.Controls.Add(this.txtNumJobs);
            this.Controls.Add(this.lblNumJobs);
            this.Controls.Add(this.txtPrompt);
            this.Controls.Add(this.lblPrompt);
            this.Name = "SearchControl";
            this.Size = new System.Drawing.Size(792, 652);
            this.grpFilter.ResumeLayout(false);
            this.grpFilter.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Label lblPrompt;
        private System.Windows.Forms.TextBox txtPrompt;
        private System.Windows.Forms.Label lblNumJobs;
        private System.Windows.Forms.TextBox txtNumJobs;
        private System.Windows.Forms.Label lblDays;
        private System.Windows.Forms.TextBox txtDays;
        private System.Windows.Forms.Label lblVersion;
        private System.Windows.Forms.ComboBox cmbVersion;
        private System.Windows.Forms.GroupBox grpFilter;
        private System.Windows.Forms.CheckBox chkHybrid;
        private System.Windows.Forms.CheckBox chkOnsite;
        private System.Windows.Forms.TextBox txtMiles;
        private System.Windows.Forms.Label lblMiles;
        private System.Windows.Forms.TextBox txtSearchState;
        private System.Windows.Forms.Label lblSearchState;
        private System.Windows.Forms.TextBox txtSearchCity;
        private System.Windows.Forms.Label lblSearchCity;
        private System.Windows.Forms.Button btnSearch;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.TextBox txtResults;
        private System.Windows.Forms.Label lblResults;
    }
}
