namespace JobApi.TestGui.Controls
{
    partial class LocationLookupsControl
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
            this.btnRefreshLookups = new System.Windows.Forms.Button();
            this.btnSaveLookups = new System.Windows.Forms.Button();
            this.lblLocationLookups = new System.Windows.Forms.Label();
            this.dgvLocationLookups = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.dgvLocationLookups)).BeginInit();
            this.SuspendLayout();
            //
            // btnRefreshLookups
            //
            this.btnRefreshLookups.Location = new System.Drawing.Point(12, 15);
            this.btnRefreshLookups.Name = "btnRefreshLookups";
            this.btnRefreshLookups.Size = new System.Drawing.Size(100, 30);
            this.btnRefreshLookups.TabIndex = 0;
            this.btnRefreshLookups.Text = "Refresh";
            this.btnRefreshLookups.UseVisualStyleBackColor = true;
            this.btnRefreshLookups.Click += new System.EventHandler(this.btnRefreshLookups_Click);
            //
            // btnSaveLookups
            //
            this.btnSaveLookups.Location = new System.Drawing.Point(120, 15);
            this.btnSaveLookups.Name = "btnSaveLookups";
            this.btnSaveLookups.Size = new System.Drawing.Size(120, 30);
            this.btnSaveLookups.TabIndex = 1;
            this.btnSaveLookups.Text = "Save Changes";
            this.btnSaveLookups.UseVisualStyleBackColor = true;
            this.btnSaveLookups.Click += new System.EventHandler(this.btnSaveLookups_Click);
            //
            // lblLocationLookups
            //
            this.lblLocationLookups.AutoSize = true;
            this.lblLocationLookups.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblLocationLookups.Location = new System.Drawing.Point(12, 60);
            this.lblLocationLookups.Name = "lblLocationLookups";
            this.lblLocationLookups.Size = new System.Drawing.Size(357, 19);
            this.lblLocationLookups.TabIndex = 1;
            this.lblLocationLookups.Text = "Location Lookups (Sorted by Confidence):";
            //
            // dgvLocationLookups
            //
            this.dgvLocationLookups.AllowUserToAddRows = false;
            this.dgvLocationLookups.AllowUserToDeleteRows = false;
            this.dgvLocationLookups.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvLocationLookups.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvLocationLookups.Location = new System.Drawing.Point(12, 85);
            this.dgvLocationLookups.Name = "dgvLocationLookups";
            this.dgvLocationLookups.ReadOnly = true;
            this.dgvLocationLookups.RowHeadersVisible = false;
            this.dgvLocationLookups.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.dgvLocationLookups.Size = new System.Drawing.Size(860, 450);
            this.dgvLocationLookups.TabIndex = 2;
            //
            // LocationLookupsControl
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.dgvLocationLookups);
            this.Controls.Add(this.lblLocationLookups);
            this.Controls.Add(this.btnSaveLookups);
            this.Controls.Add(this.btnRefreshLookups);
            this.Name = "LocationLookupsControl";
            this.Size = new System.Drawing.Size(884, 561);
            ((System.ComponentModel.ISupportInitialize)(this.dgvLocationLookups)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Button btnRefreshLookups;
        private System.Windows.Forms.Button btnSaveLookups;
        private System.Windows.Forms.Label lblLocationLookups;
        private System.Windows.Forms.DataGridView dgvLocationLookups;
    }
}
