namespace JobApi.TestGui.Controls
{
    partial class LocationValidationControl
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
            this.SuspendLayout();
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
            // LocationValidationControl
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.txtLocationResults);
            this.Controls.Add(this.lblLocationResults);
            this.Controls.Add(this.btnClearLocation);
            this.Controls.Add(this.btnValidateLocation);
            this.Controls.Add(this.txtCountry);
            this.Controls.Add(this.lblCountry);
            this.Controls.Add(this.txtState);
            this.Controls.Add(this.lblState);
            this.Controls.Add(this.txtCity);
            this.Controls.Add(this.lblCity);
            this.Name = "LocationValidationControl";
            this.Size = new System.Drawing.Size(792, 652);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

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
    }
}
