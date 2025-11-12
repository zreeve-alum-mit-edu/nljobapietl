namespace JobApi.TestGui.Controls
{
    partial class QueryAnalyzerControl
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
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblQueryTemplate = new System.Windows.Forms.Label();
            this.cmbQueryTemplate = new System.Windows.Forms.ComboBox();
            this.lblQuery = new System.Windows.Forms.Label();
            this.txtQuery = new System.Windows.Forms.TextBox();
            this.btnRunExplain = new System.Windows.Forms.Button();
            this.btnRunExplainAnalyze = new System.Windows.Forms.Button();
            this.btnClear = new System.Windows.Forms.Button();
            this.lblResults = new System.Windows.Forms.Label();
            this.txtResults = new System.Windows.Forms.TextBox();
            this.lblStats = new System.Windows.Forms.Label();
            this.txtStats = new System.Windows.Forms.TextBox();
            this.chkBuffers = new System.Windows.Forms.CheckBox();
            this.chkTiming = new System.Windows.Forms.CheckBox();
            this.lblLimit = new System.Windows.Forms.Label();
            this.txtLimit = new System.Windows.Forms.TextBox();
            this.lblEfSearch = new System.Windows.Forms.Label();
            this.txtEfSearch = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            //
            // lblTitle
            //
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblTitle.Location = new System.Drawing.Point(12, 10);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(275, 21);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "PostgreSQL Query Performance Analyzer";
            //
            // lblQueryTemplate
            //
            this.lblQueryTemplate.AutoSize = true;
            this.lblQueryTemplate.Location = new System.Drawing.Point(12, 45);
            this.lblQueryTemplate.Name = "lblQueryTemplate";
            this.lblQueryTemplate.Size = new System.Drawing.Size(96, 15);
            this.lblQueryTemplate.TabIndex = 1;
            this.lblQueryTemplate.Text = "Query Template:";
            //
            // cmbQueryTemplate
            //
            this.cmbQueryTemplate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbQueryTemplate.FormattingEnabled = true;
            this.cmbQueryTemplate.Location = new System.Drawing.Point(12, 63);
            this.cmbQueryTemplate.Name = "cmbQueryTemplate";
            this.cmbQueryTemplate.Size = new System.Drawing.Size(400, 23);
            this.cmbQueryTemplate.TabIndex = 2;
            this.cmbQueryTemplate.SelectedIndexChanged += new System.EventHandler(this.cmbQueryTemplate_SelectedIndexChanged);
            //
            // lblLimit
            //
            this.lblLimit.AutoSize = true;
            this.lblLimit.Location = new System.Drawing.Point(430, 45);
            this.lblLimit.Name = "lblLimit";
            this.lblLimit.Size = new System.Drawing.Size(37, 15);
            this.lblLimit.TabIndex = 3;
            this.lblLimit.Text = "Limit:";
            //
            // txtLimit
            //
            this.txtLimit.Location = new System.Drawing.Point(430, 63);
            this.txtLimit.Name = "txtLimit";
            this.txtLimit.Size = new System.Drawing.Size(60, 23);
            this.txtLimit.TabIndex = 4;
            this.txtLimit.Text = "10";
            //
            // lblEfSearch
            //
            this.lblEfSearch.AutoSize = true;
            this.lblEfSearch.Location = new System.Drawing.Point(510, 45);
            this.lblEfSearch.Name = "lblEfSearch";
            this.lblEfSearch.Size = new System.Drawing.Size(61, 15);
            this.lblEfSearch.TabIndex = 5;
            this.lblEfSearch.Text = "ef_search:";
            //
            // txtEfSearch
            //
            this.txtEfSearch.Location = new System.Drawing.Point(510, 63);
            this.txtEfSearch.Name = "txtEfSearch";
            this.txtEfSearch.Size = new System.Drawing.Size(60, 23);
            this.txtEfSearch.TabIndex = 6;
            this.txtEfSearch.Text = "40";
            //
            // lblQuery
            //
            this.lblQuery.AutoSize = true;
            this.lblQuery.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblQuery.Location = new System.Drawing.Point(12, 100);
            this.lblQuery.Name = "lblQuery";
            this.lblQuery.Size = new System.Drawing.Size(70, 15);
            this.lblQuery.TabIndex = 7;
            this.lblQuery.Text = "SQL Query:";
            //
            // txtQuery
            //
            this.txtQuery.Font = new System.Drawing.Font("Consolas", 9F);
            this.txtQuery.Location = new System.Drawing.Point(12, 118);
            this.txtQuery.Multiline = true;
            this.txtQuery.Name = "txtQuery";
            this.txtQuery.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtQuery.Size = new System.Drawing.Size(1160, 150);
            this.txtQuery.TabIndex = 8;
            this.txtQuery.WordWrap = false;
            //
            // chkBuffers
            //
            this.chkBuffers.AutoSize = true;
            this.chkBuffers.Checked = true;
            this.chkBuffers.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkBuffers.Location = new System.Drawing.Point(12, 280);
            this.chkBuffers.Name = "chkBuffers";
            this.chkBuffers.Size = new System.Drawing.Size(123, 19);
            this.chkBuffers.TabIndex = 9;
            this.chkBuffers.Text = "Show Buffer Stats";
            this.chkBuffers.UseVisualStyleBackColor = true;
            //
            // chkTiming
            //
            this.chkTiming.AutoSize = true;
            this.chkTiming.Checked = true;
            this.chkTiming.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkTiming.Location = new System.Drawing.Point(160, 280);
            this.chkTiming.Name = "chkTiming";
            this.chkTiming.Size = new System.Drawing.Size(107, 19);
            this.chkTiming.TabIndex = 10;
            this.chkTiming.Text = "Show I/O Timing";
            this.chkTiming.UseVisualStyleBackColor = true;
            //
            // btnRunExplain
            //
            this.btnRunExplain.Location = new System.Drawing.Point(12, 310);
            this.btnRunExplain.Name = "btnRunExplain";
            this.btnRunExplain.Size = new System.Drawing.Size(120, 35);
            this.btnRunExplain.TabIndex = 11;
            this.btnRunExplain.Text = "Run EXPLAIN";
            this.btnRunExplain.UseVisualStyleBackColor = true;
            this.btnRunExplain.Click += new System.EventHandler(this.btnRunExplain_Click);
            //
            // btnRunExplainAnalyze
            //
            this.btnRunExplainAnalyze.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.btnRunExplainAnalyze.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRunExplainAnalyze.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnRunExplainAnalyze.ForeColor = System.Drawing.Color.White;
            this.btnRunExplainAnalyze.Location = new System.Drawing.Point(148, 310);
            this.btnRunExplainAnalyze.Name = "btnRunExplainAnalyze";
            this.btnRunExplainAnalyze.Size = new System.Drawing.Size(160, 35);
            this.btnRunExplainAnalyze.TabIndex = 12;
            this.btnRunExplainAnalyze.Text = "Run EXPLAIN ANALYZE";
            this.btnRunExplainAnalyze.UseVisualStyleBackColor = false;
            this.btnRunExplainAnalyze.Click += new System.EventHandler(this.btnRunExplainAnalyze_Click);
            //
            // btnClear
            //
            this.btnClear.Location = new System.Drawing.Point(324, 310);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(100, 35);
            this.btnClear.TabIndex = 13;
            this.btnClear.Text = "Clear";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            //
            // lblStats
            //
            this.lblStats.AutoSize = true;
            this.lblStats.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblStats.Location = new System.Drawing.Point(12, 360);
            this.lblStats.Name = "lblStats";
            this.lblStats.Size = new System.Drawing.Size(124, 15);
            this.lblStats.TabIndex = 14;
            this.lblStats.Text = "Performance Summary:";
            //
            // txtStats
            //
            this.txtStats.BackColor = System.Drawing.Color.LightYellow;
            this.txtStats.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Bold);
            this.txtStats.Location = new System.Drawing.Point(12, 378);
            this.txtStats.Multiline = true;
            this.txtStats.Name = "txtStats";
            this.txtStats.ReadOnly = true;
            this.txtStats.Size = new System.Drawing.Size(1160, 80);
            this.txtStats.TabIndex = 15;
            //
            // lblResults
            //
            this.lblResults.AutoSize = true;
            this.lblResults.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblResults.Location = new System.Drawing.Point(12, 470);
            this.lblResults.Name = "lblResults";
            this.lblResults.Size = new System.Drawing.Size(102, 15);
            this.lblResults.TabIndex = 16;
            this.lblResults.Text = "EXPLAIN Output:";
            //
            // txtResults
            //
            this.txtResults.Font = new System.Drawing.Font("Consolas", 8.25F);
            this.txtResults.Location = new System.Drawing.Point(12, 488);
            this.txtResults.Multiline = true;
            this.txtResults.Name = "txtResults";
            this.txtResults.ReadOnly = true;
            this.txtResults.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtResults.Size = new System.Drawing.Size(1160, 152);
            this.txtResults.TabIndex = 17;
            this.txtResults.WordWrap = false;
            //
            // QueryAnalyzerControl
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.txtResults);
            this.Controls.Add(this.lblResults);
            this.Controls.Add(this.txtStats);
            this.Controls.Add(this.lblStats);
            this.Controls.Add(this.btnClear);
            this.Controls.Add(this.btnRunExplainAnalyze);
            this.Controls.Add(this.btnRunExplain);
            this.Controls.Add(this.chkTiming);
            this.Controls.Add(this.chkBuffers);
            this.Controls.Add(this.txtQuery);
            this.Controls.Add(this.lblQuery);
            this.Controls.Add(this.txtEfSearch);
            this.Controls.Add(this.lblEfSearch);
            this.Controls.Add(this.txtLimit);
            this.Controls.Add(this.lblLimit);
            this.Controls.Add(this.cmbQueryTemplate);
            this.Controls.Add(this.lblQueryTemplate);
            this.Controls.Add(this.lblTitle);
            this.Name = "QueryAnalyzerControl";
            this.Size = new System.Drawing.Size(1192, 652);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblQueryTemplate;
        private System.Windows.Forms.ComboBox cmbQueryTemplate;
        private System.Windows.Forms.Label lblQuery;
        private System.Windows.Forms.TextBox txtQuery;
        private System.Windows.Forms.Button btnRunExplain;
        private System.Windows.Forms.Button btnRunExplainAnalyze;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Label lblResults;
        private System.Windows.Forms.TextBox txtResults;
        private System.Windows.Forms.Label lblStats;
        private System.Windows.Forms.TextBox txtStats;
        private System.Windows.Forms.CheckBox chkBuffers;
        private System.Windows.Forms.CheckBox chkTiming;
        private System.Windows.Forms.Label lblLimit;
        private System.Windows.Forms.TextBox txtLimit;
        private System.Windows.Forms.Label lblEfSearch;
        private System.Windows.Forms.TextBox txtEfSearch;
    }
}
