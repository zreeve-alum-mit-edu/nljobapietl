namespace JobApi.TestGui.Controls
{
    partial class AuditLogsControl
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
            this.dgvAuditLogs = new System.Windows.Forms.DataGridView();
            this.btnFirstPage = new System.Windows.Forms.Button();
            this.btnPreviousPage = new System.Windows.Forms.Button();
            this.btnNextPage = new System.Windows.Forms.Button();
            this.btnLastPage = new System.Windows.Forms.Button();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.lblPageInfo = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dgvAuditLogs)).BeginInit();
            this.SuspendLayout();
            //
            // dgvAuditLogs
            //
            this.dgvAuditLogs.AllowUserToAddRows = false;
            this.dgvAuditLogs.AllowUserToDeleteRows = false;
            this.dgvAuditLogs.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dgvAuditLogs.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvAuditLogs.Location = new System.Drawing.Point(12, 50);
            this.dgvAuditLogs.Name = "dgvAuditLogs";
            this.dgvAuditLogs.ReadOnly = true;
            this.dgvAuditLogs.RowHeadersVisible = false;
            this.dgvAuditLogs.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvAuditLogs.Size = new System.Drawing.Size(768, 550);
            this.dgvAuditLogs.TabIndex = 0;
            //
            // btnFirstPage
            //
            this.btnFirstPage.Location = new System.Drawing.Point(12, 15);
            this.btnFirstPage.Name = "btnFirstPage";
            this.btnFirstPage.Size = new System.Drawing.Size(75, 25);
            this.btnFirstPage.TabIndex = 1;
            this.btnFirstPage.Text = "First";
            this.btnFirstPage.UseVisualStyleBackColor = true;
            this.btnFirstPage.Click += new System.EventHandler(this.btnFirstPage_Click);
            //
            // btnPreviousPage
            //
            this.btnPreviousPage.Location = new System.Drawing.Point(93, 15);
            this.btnPreviousPage.Name = "btnPreviousPage";
            this.btnPreviousPage.Size = new System.Drawing.Size(75, 25);
            this.btnPreviousPage.TabIndex = 2;
            this.btnPreviousPage.Text = "Previous";
            this.btnPreviousPage.UseVisualStyleBackColor = true;
            this.btnPreviousPage.Click += new System.EventHandler(this.btnPreviousPage_Click);
            //
            // btnNextPage
            //
            this.btnNextPage.Location = new System.Drawing.Point(174, 15);
            this.btnNextPage.Name = "btnNextPage";
            this.btnNextPage.Size = new System.Drawing.Size(75, 25);
            this.btnNextPage.TabIndex = 3;
            this.btnNextPage.Text = "Next";
            this.btnNextPage.UseVisualStyleBackColor = true;
            this.btnNextPage.Click += new System.EventHandler(this.btnNextPage_Click);
            //
            // btnLastPage
            //
            this.btnLastPage.Location = new System.Drawing.Point(255, 15);
            this.btnLastPage.Name = "btnLastPage";
            this.btnLastPage.Size = new System.Drawing.Size(75, 25);
            this.btnLastPage.TabIndex = 4;
            this.btnLastPage.Text = "Last";
            this.btnLastPage.UseVisualStyleBackColor = true;
            this.btnLastPage.Click += new System.EventHandler(this.btnLastPage_Click);
            //
            // btnRefresh
            //
            this.btnRefresh.Location = new System.Drawing.Point(350, 15);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(75, 25);
            this.btnRefresh.TabIndex = 5;
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            //
            // lblPageInfo
            //
            this.lblPageInfo.AutoSize = true;
            this.lblPageInfo.Location = new System.Drawing.Point(450, 20);
            this.lblPageInfo.Name = "lblPageInfo";
            this.lblPageInfo.Size = new System.Drawing.Size(0, 15);
            this.lblPageInfo.TabIndex = 6;
            //
            // AuditLogsControl
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lblPageInfo);
            this.Controls.Add(this.btnRefresh);
            this.Controls.Add(this.btnLastPage);
            this.Controls.Add(this.btnNextPage);
            this.Controls.Add(this.btnPreviousPage);
            this.Controls.Add(this.btnFirstPage);
            this.Controls.Add(this.dgvAuditLogs);
            this.Name = "AuditLogsControl";
            this.Size = new System.Drawing.Size(792, 610);
            ((System.ComponentModel.ISupportInitialize)(this.dgvAuditLogs)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.DataGridView dgvAuditLogs;
        private System.Windows.Forms.Button btnFirstPage;
        private System.Windows.Forms.Button btnPreviousPage;
        private System.Windows.Forms.Button btnNextPage;
        private System.Windows.Forms.Button btnLastPage;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Label lblPageInfo;
    }
}
