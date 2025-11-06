namespace JobApi.TestGui.Controls
{
    partial class RemoteSearchControl
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
            this.lblRemotePrompt = new System.Windows.Forms.Label();
            this.txtRemotePrompt = new System.Windows.Forms.TextBox();
            this.lblRemoteNumJobs = new System.Windows.Forms.Label();
            this.txtRemoteNumJobs = new System.Windows.Forms.TextBox();
            this.lblRemoteDays = new System.Windows.Forms.Label();
            this.txtRemoteDays = new System.Windows.Forms.TextBox();
            this.lblRemoteVersion = new System.Windows.Forms.Label();
            this.cmbRemoteVersion = new System.Windows.Forms.ComboBox();
            this.btnSearchRemote = new System.Windows.Forms.Button();
            this.btnClearRemote = new System.Windows.Forms.Button();
            this.txtRemoteResults = new System.Windows.Forms.TextBox();
            this.lblRemoteResults = new System.Windows.Forms.Label();
            this.SuspendLayout();
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
            // lblRemoteVersion
            //
            this.lblRemoteVersion.AutoSize = true;
            this.lblRemoteVersion.Location = new System.Drawing.Point(250, 106);
            this.lblRemoteVersion.Name = "lblRemoteVersion";
            this.lblRemoteVersion.Size = new System.Drawing.Size(68, 15);
            this.lblRemoteVersion.TabIndex = 6;
            this.lblRemoteVersion.Text = "API Version:";
            //
            // cmbRemoteVersion
            //
            this.cmbRemoteVersion.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbRemoteVersion.FormattingEnabled = true;
            this.cmbRemoteVersion.Items.AddRange(new object[] { "1", "2" });
            this.cmbRemoteVersion.Location = new System.Drawing.Point(250, 124);
            this.cmbRemoteVersion.Name = "cmbRemoteVersion";
            this.cmbRemoteVersion.Size = new System.Drawing.Size(100, 23);
            this.cmbRemoteVersion.TabIndex = 7;
            this.cmbRemoteVersion.SelectedIndex = 1;
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
            // RemoteSearchControl
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lblRemoteResults);
            this.Controls.Add(this.txtRemoteResults);
            this.Controls.Add(this.btnClearRemote);
            this.Controls.Add(this.btnSearchRemote);
            this.Controls.Add(this.cmbRemoteVersion);
            this.Controls.Add(this.lblRemoteVersion);
            this.Controls.Add(this.txtRemoteDays);
            this.Controls.Add(this.lblRemoteDays);
            this.Controls.Add(this.txtRemoteNumJobs);
            this.Controls.Add(this.lblRemoteNumJobs);
            this.Controls.Add(this.txtRemotePrompt);
            this.Controls.Add(this.lblRemotePrompt);
            this.Name = "RemoteSearchControl";
            this.Size = new System.Drawing.Size(792, 652);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Label lblRemotePrompt;
        private System.Windows.Forms.TextBox txtRemotePrompt;
        private System.Windows.Forms.Label lblRemoteNumJobs;
        private System.Windows.Forms.TextBox txtRemoteNumJobs;
        private System.Windows.Forms.Label lblRemoteDays;
        private System.Windows.Forms.TextBox txtRemoteDays;
        private System.Windows.Forms.Label lblRemoteVersion;
        private System.Windows.Forms.ComboBox cmbRemoteVersion;
        private System.Windows.Forms.Button btnSearchRemote;
        private System.Windows.Forms.Button btnClearRemote;
        private System.Windows.Forms.TextBox txtRemoteResults;
        private System.Windows.Forms.Label lblRemoteResults;
    }
}
