namespace JobApi.TestGui.Controls
{
    partial class CountsControl
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
            this.btnRefreshCounts = new System.Windows.Forms.Button();
            this.lblJobStatusCounts = new System.Windows.Forms.Label();
            this.dgvJobStatusCounts = new System.Windows.Forms.DataGridView();
            this.lblCentroidCounts = new System.Windows.Forms.Label();
            this.dgvCentroidCounts = new System.Windows.Forms.DataGridView();
            this.lblTotalJobs = new System.Windows.Forms.Label();
            this.lblTotalJobsValue = new System.Windows.Forms.Label();
            this.lblTotalLocations = new System.Windows.Forms.Label();
            this.lblTotalLocationsValue = new System.Windows.Forms.Label();
            this.lblTotalUrls = new System.Windows.Forms.Label();
            this.lblTotalUrlsValue = new System.Windows.Forms.Label();
            this.lblInvalidLocations = new System.Windows.Forms.Label();
            this.lblInvalidLocationsValue = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dgvJobStatusCounts)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvCentroidCounts)).BeginInit();
            this.SuspendLayout();
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
            // CountsControl
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lblInvalidLocationsValue);
            this.Controls.Add(this.lblInvalidLocations);
            this.Controls.Add(this.lblTotalUrlsValue);
            this.Controls.Add(this.lblTotalUrls);
            this.Controls.Add(this.lblTotalLocationsValue);
            this.Controls.Add(this.lblTotalLocations);
            this.Controls.Add(this.lblTotalJobsValue);
            this.Controls.Add(this.lblTotalJobs);
            this.Controls.Add(this.dgvCentroidCounts);
            this.Controls.Add(this.lblCentroidCounts);
            this.Controls.Add(this.dgvJobStatusCounts);
            this.Controls.Add(this.lblJobStatusCounts);
            this.Controls.Add(this.btnRefreshCounts);
            this.Name = "CountsControl";
            this.Size = new System.Drawing.Size(780, 600);
            ((System.ComponentModel.ISupportInitialize)(this.dgvJobStatusCounts)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvCentroidCounts)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Button btnRefreshCounts;
        private System.Windows.Forms.Label lblJobStatusCounts;
        private System.Windows.Forms.DataGridView dgvJobStatusCounts;
        private System.Windows.Forms.Label lblCentroidCounts;
        private System.Windows.Forms.DataGridView dgvCentroidCounts;
        private System.Windows.Forms.Label lblTotalJobs;
        private System.Windows.Forms.Label lblTotalJobsValue;
        private System.Windows.Forms.Label lblTotalLocations;
        private System.Windows.Forms.Label lblTotalLocationsValue;
        private System.Windows.Forms.Label lblTotalUrls;
        private System.Windows.Forms.Label lblTotalUrlsValue;
        private System.Windows.Forms.Label lblInvalidLocations;
        private System.Windows.Forms.Label lblInvalidLocationsValue;
    }
}
