namespace JobApi.TestGui.Controls
{
    partial class HealthCheckControl
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
            this.btnCheckHealth = new System.Windows.Forms.Button();
            this.lblResponse = new System.Windows.Forms.Label();
            this.txtResponse = new System.Windows.Forms.TextBox();
            this.lblStatus = new System.Windows.Forms.Label();
            this.lblStatusCode = new System.Windows.Forms.Label();
            this.lblResponseTime = new System.Windows.Forms.Label();
            this.lblLastChecked = new System.Windows.Forms.Label();
            this.lblDatabaseStatus = new System.Windows.Forms.Label();
            this.chkAutoRefresh = new System.Windows.Forms.CheckBox();
            this.numRefreshInterval = new System.Windows.Forms.NumericUpDown();
            this.lblRefreshInterval = new System.Windows.Forms.Label();
            this.lblEndpoint = new System.Windows.Forms.Label();
            this.txtEndpoint = new System.Windows.Forms.TextBox();
            this.btnCopyUrl = new System.Windows.Forms.Button();
            this.panelStatus = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.numRefreshInterval)).BeginInit();
            this.panelStatus.SuspendLayout();
            this.SuspendLayout();
            //
            // lblTitle
            //
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblTitle.Location = new System.Drawing.Point(12, 10);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(149, 21);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "API Health Check";
            //
            // lblEndpoint
            //
            this.lblEndpoint.AutoSize = true;
            this.lblEndpoint.Location = new System.Drawing.Point(12, 45);
            this.lblEndpoint.Name = "lblEndpoint";
            this.lblEndpoint.Size = new System.Drawing.Size(61, 15);
            this.lblEndpoint.TabIndex = 1;
            this.lblEndpoint.Text = "Endpoint:";
            //
            // txtEndpoint
            //
            this.txtEndpoint.Location = new System.Drawing.Point(12, 63);
            this.txtEndpoint.Name = "txtEndpoint";
            this.txtEndpoint.ReadOnly = true;
            this.txtEndpoint.Size = new System.Drawing.Size(950, 23);
            this.txtEndpoint.TabIndex = 2;
            this.txtEndpoint.Text = "https://42r7s00kck.execute-api.us-east-2.amazonaws.com/health";
            //
            // btnCopyUrl
            //
            this.btnCopyUrl.Location = new System.Drawing.Point(972, 62);
            this.btnCopyUrl.Name = "btnCopyUrl";
            this.btnCopyUrl.Size = new System.Drawing.Size(100, 25);
            this.btnCopyUrl.TabIndex = 3;
            this.btnCopyUrl.Text = "Copy URL";
            this.btnCopyUrl.UseVisualStyleBackColor = true;
            this.btnCopyUrl.Click += new System.EventHandler(this.btnCopyUrl_Click);
            //
            // btnCheckHealth
            //
            this.btnCheckHealth.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.btnCheckHealth.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCheckHealth.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnCheckHealth.ForeColor = System.Drawing.Color.White;
            this.btnCheckHealth.Location = new System.Drawing.Point(12, 100);
            this.btnCheckHealth.Name = "btnCheckHealth";
            this.btnCheckHealth.Size = new System.Drawing.Size(150, 35);
            this.btnCheckHealth.TabIndex = 4;
            this.btnCheckHealth.Text = "Check Health Now";
            this.btnCheckHealth.UseVisualStyleBackColor = false;
            this.btnCheckHealth.Click += new System.EventHandler(this.btnCheckHealth_Click);
            //
            // chkAutoRefresh
            //
            this.chkAutoRefresh.AutoSize = true;
            this.chkAutoRefresh.Location = new System.Drawing.Point(180, 110);
            this.chkAutoRefresh.Name = "chkAutoRefresh";
            this.chkAutoRefresh.Size = new System.Drawing.Size(95, 19);
            this.chkAutoRefresh.TabIndex = 5;
            this.chkAutoRefresh.Text = "Auto Refresh";
            this.chkAutoRefresh.UseVisualStyleBackColor = true;
            this.chkAutoRefresh.CheckedChanged += new System.EventHandler(this.chkAutoRefresh_CheckedChanged);
            //
            // lblRefreshInterval
            //
            this.lblRefreshInterval.AutoSize = true;
            this.lblRefreshInterval.Location = new System.Drawing.Point(285, 111);
            this.lblRefreshInterval.Name = "lblRefreshInterval";
            this.lblRefreshInterval.Size = new System.Drawing.Size(43, 15);
            this.lblRefreshInterval.TabIndex = 6;
            this.lblRefreshInterval.Text = "Every:";
            //
            // numRefreshInterval
            //
            this.numRefreshInterval.Location = new System.Drawing.Point(334, 109);
            this.numRefreshInterval.Maximum = new decimal(new int[] { 300, 0, 0, 0 });
            this.numRefreshInterval.Minimum = new decimal(new int[] { 5, 0, 0, 0 });
            this.numRefreshInterval.Name = "numRefreshInterval";
            this.numRefreshInterval.Size = new System.Drawing.Size(60, 23);
            this.numRefreshInterval.TabIndex = 7;
            this.numRefreshInterval.Value = new decimal(new int[] { 30, 0, 0, 0 });
            //
            // lblSeconds
            //
            var lblSeconds = new System.Windows.Forms.Label();
            lblSeconds.AutoSize = true;
            lblSeconds.Location = new System.Drawing.Point(400, 111);
            lblSeconds.Name = "lblSeconds";
            lblSeconds.Size = new System.Drawing.Size(53, 15);
            lblSeconds.TabIndex = 8;
            lblSeconds.Text = "seconds";
            //
            // panelStatus
            //
            this.panelStatus.BackColor = System.Drawing.Color.WhiteSmoke;
            this.panelStatus.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelStatus.Controls.Add(this.lblStatus);
            this.panelStatus.Controls.Add(this.lblStatusCode);
            this.panelStatus.Controls.Add(this.lblResponseTime);
            this.panelStatus.Controls.Add(this.lblDatabaseStatus);
            this.panelStatus.Controls.Add(this.lblLastChecked);
            this.panelStatus.Location = new System.Drawing.Point(12, 150);
            this.panelStatus.Name = "panelStatus";
            this.panelStatus.Size = new System.Drawing.Size(1160, 80);
            this.panelStatus.TabIndex = 9;
            //
            // lblStatus
            //
            this.lblStatus.AutoSize = true;
            this.lblStatus.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblStatus.Location = new System.Drawing.Point(10, 10);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(54, 19);
            this.lblStatus.TabIndex = 0;
            this.lblStatus.Text = "Status:";
            //
            // lblStatusCode
            //
            this.lblStatusCode.AutoSize = true;
            this.lblStatusCode.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblStatusCode.Location = new System.Drawing.Point(10, 35);
            this.lblStatusCode.Name = "lblStatusCode";
            this.lblStatusCode.Size = new System.Drawing.Size(0, 15);
            this.lblStatusCode.TabIndex = 1;
            //
            // lblResponseTime
            //
            this.lblResponseTime.AutoSize = true;
            this.lblResponseTime.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblResponseTime.Location = new System.Drawing.Point(250, 35);
            this.lblResponseTime.Name = "lblResponseTime";
            this.lblResponseTime.Size = new System.Drawing.Size(0, 15);
            this.lblResponseTime.TabIndex = 2;
            //
            // lblDatabaseStatus
            //
            this.lblDatabaseStatus.AutoSize = true;
            this.lblDatabaseStatus.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblDatabaseStatus.Location = new System.Drawing.Point(500, 35);
            this.lblDatabaseStatus.Name = "lblDatabaseStatus";
            this.lblDatabaseStatus.Size = new System.Drawing.Size(0, 15);
            this.lblDatabaseStatus.TabIndex = 3;
            //
            // lblLastChecked
            //
            this.lblLastChecked.AutoSize = true;
            this.lblLastChecked.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblLastChecked.ForeColor = System.Drawing.Color.Gray;
            this.lblLastChecked.Location = new System.Drawing.Point(10, 55);
            this.lblLastChecked.Name = "lblLastChecked";
            this.lblLastChecked.Size = new System.Drawing.Size(0, 13);
            this.lblLastChecked.TabIndex = 4;
            //
            // lblResponse
            //
            this.lblResponse.AutoSize = true;
            this.lblResponse.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblResponse.Location = new System.Drawing.Point(12, 245);
            this.lblResponse.Name = "lblResponse";
            this.lblResponse.Size = new System.Drawing.Size(97, 15);
            this.lblResponse.TabIndex = 10;
            this.lblResponse.Text = "JSON Response:";
            //
            // txtResponse
            //
            this.txtResponse.BackColor = System.Drawing.Color.White;
            this.txtResponse.Font = new System.Drawing.Font("Consolas", 9F);
            this.txtResponse.Location = new System.Drawing.Point(12, 263);
            this.txtResponse.Multiline = true;
            this.txtResponse.Name = "txtResponse";
            this.txtResponse.ReadOnly = true;
            this.txtResponse.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtResponse.Size = new System.Drawing.Size(1160, 377);
            this.txtResponse.TabIndex = 11;
            this.txtResponse.WordWrap = false;
            //
            // HealthCheckControl
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.txtResponse);
            this.Controls.Add(this.lblResponse);
            this.Controls.Add(this.panelStatus);
            this.Controls.Add(lblSeconds);
            this.Controls.Add(this.numRefreshInterval);
            this.Controls.Add(this.lblRefreshInterval);
            this.Controls.Add(this.chkAutoRefresh);
            this.Controls.Add(this.btnCheckHealth);
            this.Controls.Add(this.btnCopyUrl);
            this.Controls.Add(this.txtEndpoint);
            this.Controls.Add(this.lblEndpoint);
            this.Controls.Add(this.lblTitle);
            this.Name = "HealthCheckControl";
            this.Size = new System.Drawing.Size(1192, 652);
            ((System.ComponentModel.ISupportInitialize)(this.numRefreshInterval)).EndInit();
            this.panelStatus.ResumeLayout(false);
            this.panelStatus.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblEndpoint;
        private System.Windows.Forms.TextBox txtEndpoint;
        private System.Windows.Forms.Button btnCopyUrl;
        private System.Windows.Forms.Button btnCheckHealth;
        private System.Windows.Forms.CheckBox chkAutoRefresh;
        private System.Windows.Forms.Label lblRefreshInterval;
        private System.Windows.Forms.NumericUpDown numRefreshInterval;
        private System.Windows.Forms.Panel panelStatus;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Label lblStatusCode;
        private System.Windows.Forms.Label lblResponseTime;
        private System.Windows.Forms.Label lblDatabaseStatus;
        private System.Windows.Forms.Label lblLastChecked;
        private System.Windows.Forms.Label lblResponse;
        private System.Windows.Forms.TextBox txtResponse;
    }
}
