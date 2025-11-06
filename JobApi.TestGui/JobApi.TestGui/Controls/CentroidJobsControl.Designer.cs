namespace JobApi.TestGui.Controls
{
    partial class CentroidJobsControl
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
            this.dgvCentroidJobs = new System.Windows.Forms.DataGridView();
            this.btnCentroidFirstPage = new System.Windows.Forms.Button();
            this.btnCentroidPreviousPage = new System.Windows.Forms.Button();
            this.btnCentroidNextPage = new System.Windows.Forms.Button();
            this.btnCentroidLastPage = new System.Windows.Forms.Button();
            this.lblCentroidPageInfo = new System.Windows.Forms.Label();
            this.lblSelectedCentroid = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dgvCentroidJobs)).BeginInit();
            this.SuspendLayout();
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
            // CentroidJobsControl
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lblSelectedCentroid);
            this.Controls.Add(this.lblCentroidPageInfo);
            this.Controls.Add(this.btnCentroidLastPage);
            this.Controls.Add(this.btnCentroidNextPage);
            this.Controls.Add(this.btnCentroidPreviousPage);
            this.Controls.Add(this.btnCentroidFirstPage);
            this.Controls.Add(this.dgvCentroidJobs);
            this.Name = "CentroidJobsControl";
            this.Size = new System.Drawing.Size(792, 610);
            ((System.ComponentModel.ISupportInitialize)(this.dgvCentroidJobs)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.DataGridView dgvCentroidJobs;
        private System.Windows.Forms.Button btnCentroidFirstPage;
        private System.Windows.Forms.Button btnCentroidPreviousPage;
        private System.Windows.Forms.Button btnCentroidNextPage;
        private System.Windows.Forms.Button btnCentroidLastPage;
        private System.Windows.Forms.Label lblCentroidPageInfo;
        private System.Windows.Forms.Label lblSelectedCentroid;
    }
}
