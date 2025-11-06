namespace JobApi.TestGui.Controls
{
    partial class LocationErrorsControl
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
            this.dgvLocationErrors = new System.Windows.Forms.DataGridView();
            this.btnRefreshLocationErrors = new System.Windows.Forms.Button();
            this.lblLocationErrors = new System.Windows.Forms.Label();
            this.grpLocationValidation = new System.Windows.Forms.GroupBox();
            this.btnAddOverride = new System.Windows.Forms.Button();
            this.lstSuggestions = new System.Windows.Forms.ListBox();
            this.lblSuggestions = new System.Windows.Forms.Label();
            this.btnValidateErrorLocation = new System.Windows.Forms.Button();
            this.lblSelectedLocation = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dgvLocationErrors)).BeginInit();
            this.grpLocationValidation.SuspendLayout();
            this.SuspendLayout();
            //
            // dgvLocationErrors
            //
            this.dgvLocationErrors.AllowUserToAddRows = false;
            this.dgvLocationErrors.AllowUserToDeleteRows = false;
            this.dgvLocationErrors.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvLocationErrors.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvLocationErrors.Location = new System.Drawing.Point(12, 85);
            this.dgvLocationErrors.Name = "dgvLocationErrors";
            this.dgvLocationErrors.ReadOnly = true;
            this.dgvLocationErrors.RowHeadersVisible = false;
            this.dgvLocationErrors.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvLocationErrors.Size = new System.Drawing.Size(400, 520);
            this.dgvLocationErrors.TabIndex = 0;
            this.dgvLocationErrors.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvLocationErrors_CellClick);
            //
            // btnRefreshLocationErrors
            //
            this.btnRefreshLocationErrors.Location = new System.Drawing.Point(12, 15);
            this.btnRefreshLocationErrors.Name = "btnRefreshLocationErrors";
            this.btnRefreshLocationErrors.Size = new System.Drawing.Size(100, 30);
            this.btnRefreshLocationErrors.TabIndex = 1;
            this.btnRefreshLocationErrors.Text = "Refresh";
            this.btnRefreshLocationErrors.UseVisualStyleBackColor = true;
            this.btnRefreshLocationErrors.Click += new System.EventHandler(this.btnRefreshLocationErrors_Click);
            //
            // lblLocationErrors
            //
            this.lblLocationErrors.AutoSize = true;
            this.lblLocationErrors.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblLocationErrors.Location = new System.Drawing.Point(12, 60);
            this.lblLocationErrors.Name = "lblLocationErrors";
            this.lblLocationErrors.Size = new System.Drawing.Size(175, 19);
            this.lblLocationErrors.TabIndex = 2;
            this.lblLocationErrors.Text = "Location Errors by Tuple:";
            //
            // grpLocationValidation
            //
            this.grpLocationValidation.Controls.Add(this.btnAddOverride);
            this.grpLocationValidation.Controls.Add(this.lstSuggestions);
            this.grpLocationValidation.Controls.Add(this.lblSuggestions);
            this.grpLocationValidation.Controls.Add(this.btnValidateErrorLocation);
            this.grpLocationValidation.Controls.Add(this.lblSelectedLocation);
            this.grpLocationValidation.Location = new System.Drawing.Point(420, 85);
            this.grpLocationValidation.Name = "grpLocationValidation";
            this.grpLocationValidation.Size = new System.Drawing.Size(360, 520);
            this.grpLocationValidation.TabIndex = 3;
            this.grpLocationValidation.TabStop = false;
            this.grpLocationValidation.Text = "Location Validation";
            //
            // btnAddOverride
            //
            this.btnAddOverride.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.btnAddOverride.Enabled = false;
            this.btnAddOverride.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAddOverride.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnAddOverride.ForeColor = System.Drawing.Color.White;
            this.btnAddOverride.Location = new System.Drawing.Point(12, 465);
            this.btnAddOverride.Name = "btnAddOverride";
            this.btnAddOverride.Size = new System.Drawing.Size(160, 35);
            this.btnAddOverride.TabIndex = 4;
            this.btnAddOverride.Text = "Add Override";
            this.btnAddOverride.UseVisualStyleBackColor = false;
            this.btnAddOverride.Click += new System.EventHandler(this.btnAddOverride_Click);
            //
            // lstSuggestions
            //
            this.lstSuggestions.FormattingEnabled = true;
            this.lstSuggestions.ItemHeight = 15;
            this.lstSuggestions.Location = new System.Drawing.Point(12, 115);
            this.lstSuggestions.Name = "lstSuggestions";
            this.lstSuggestions.Size = new System.Drawing.Size(336, 334);
            this.lstSuggestions.TabIndex = 3;
            //
            // lblSuggestions
            //
            this.lblSuggestions.AutoSize = true;
            this.lblSuggestions.Location = new System.Drawing.Point(12, 95);
            this.lblSuggestions.Name = "lblSuggestions";
            this.lblSuggestions.Size = new System.Drawing.Size(127, 15);
            this.lblSuggestions.TabIndex = 2;
            this.lblSuggestions.Text = "Suggested Corrections:";
            //
            // btnValidateErrorLocation
            //
            this.btnValidateErrorLocation.Enabled = false;
            this.btnValidateErrorLocation.Location = new System.Drawing.Point(12, 50);
            this.btnValidateErrorLocation.Name = "btnValidateErrorLocation";
            this.btnValidateErrorLocation.Size = new System.Drawing.Size(120, 30);
            this.btnValidateErrorLocation.TabIndex = 1;
            this.btnValidateErrorLocation.Text = "Validate Location";
            this.btnValidateErrorLocation.UseVisualStyleBackColor = true;
            this.btnValidateErrorLocation.Click += new System.EventHandler(this.btnValidateErrorLocation_Click);
            //
            // lblSelectedLocation
            //
            this.lblSelectedLocation.AutoSize = true;
            this.lblSelectedLocation.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblSelectedLocation.Location = new System.Drawing.Point(12, 25);
            this.lblSelectedLocation.Name = "lblSelectedLocation";
            this.lblSelectedLocation.Size = new System.Drawing.Size(221, 15);
            this.lblSelectedLocation.TabIndex = 0;
            this.lblSelectedLocation.Text = "Select a location error to validate";
            //
            // LocationErrorsControl
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.grpLocationValidation);
            this.Controls.Add(this.lblLocationErrors);
            this.Controls.Add(this.btnRefreshLocationErrors);
            this.Controls.Add(this.dgvLocationErrors);
            this.Name = "LocationErrorsControl";
            this.Size = new System.Drawing.Size(792, 652);
            ((System.ComponentModel.ISupportInitialize)(this.dgvLocationErrors)).EndInit();
            this.grpLocationValidation.ResumeLayout(false);
            this.grpLocationValidation.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.DataGridView dgvLocationErrors;
        private System.Windows.Forms.Button btnRefreshLocationErrors;
        private System.Windows.Forms.Label lblLocationErrors;
        private System.Windows.Forms.GroupBox grpLocationValidation;
        private System.Windows.Forms.Label lblSelectedLocation;
        private System.Windows.Forms.Button btnValidateErrorLocation;
        private System.Windows.Forms.Label lblSuggestions;
        private System.Windows.Forms.ListBox lstSuggestions;
        private System.Windows.Forms.Button btnAddOverride;
    }
}
