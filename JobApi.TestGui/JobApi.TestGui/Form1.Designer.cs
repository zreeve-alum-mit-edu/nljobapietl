using JobApi.TestGui.Controls;

namespace JobApi.TestGui
{
    partial class Form1
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
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabSearch = new System.Windows.Forms.TabPage();
            this.tabRemoteSearch = new System.Windows.Forms.TabPage();
            this.tabLoadTest = new System.Windows.Forms.TabPage();
            this.tabQueryAnalyzer = new System.Windows.Forms.TabPage();
            this.tabLocation = new System.Windows.Forms.TabPage();
            this.tabAuditLogs = new System.Windows.Forms.TabPage();
            this.tabCounts = new System.Windows.Forms.TabPage();
            this.tabCentroidJobs = new System.Windows.Forms.TabPage();
            this.tabLocationErrors = new System.Windows.Forms.TabPage();
            this.tabLocationLookups = new System.Windows.Forms.TabPage();
            this.tabHealthCheck = new System.Windows.Forms.TabPage();
            this.searchControl = new SearchControl();
            this.remoteSearchControl = new RemoteSearchControl();
            this.loadTestControl = new LoadTestControl();
            this.queryAnalyzerControl = new QueryAnalyzerControl();
            this.locationValidationControl = new LocationValidationControl();
            this.auditLogsControl = new AuditLogsControl();
            this.countsControl = new CountsControl();
            this.centroidJobsControl = new CentroidJobsControl();
            this.locationErrorsControl = new LocationErrorsControl();
            this.locationLookupsControl = new LocationLookupsControl();
            this.healthCheckControl = new HealthCheckControl();
            this.tabControl.SuspendLayout();
            this.tabSearch.SuspendLayout();
            this.tabRemoteSearch.SuspendLayout();
            this.tabLoadTest.SuspendLayout();
            this.tabQueryAnalyzer.SuspendLayout();
            this.tabLocation.SuspendLayout();
            this.tabAuditLogs.SuspendLayout();
            this.tabCounts.SuspendLayout();
            this.tabCentroidJobs.SuspendLayout();
            this.tabLocationErrors.SuspendLayout();
            this.tabLocationLookups.SuspendLayout();
            this.tabHealthCheck.SuspendLayout();
            this.SuspendLayout();
            //
            // tabControl
            //
            this.tabControl.Controls.Add(this.tabSearch);
            this.tabControl.Controls.Add(this.tabRemoteSearch);
            this.tabControl.Controls.Add(this.tabLoadTest);
            this.tabControl.Controls.Add(this.tabQueryAnalyzer);
            this.tabControl.Controls.Add(this.tabHealthCheck);
            this.tabControl.Controls.Add(this.tabLocation);
            this.tabControl.Controls.Add(this.tabAuditLogs);
            this.tabControl.Controls.Add(this.tabCounts);
            this.tabControl.Controls.Add(this.tabCentroidJobs);
            this.tabControl.Controls.Add(this.tabLocationErrors);
            this.tabControl.Controls.Add(this.tabLocationLookups);
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.Location = new System.Drawing.Point(0, 0);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(1200, 680);
            this.tabControl.TabIndex = 0;
            this.tabControl.SelectedIndexChanged += new System.EventHandler(this.TabControl_SelectedIndexChanged);
            //
            // tabSearch
            //
            this.tabSearch.BackColor = System.Drawing.SystemColors.Control;
            this.tabSearch.Controls.Add(this.searchControl);
            this.tabSearch.Location = new System.Drawing.Point(4, 24);
            this.tabSearch.Name = "tabSearch";
            this.tabSearch.Padding = new System.Windows.Forms.Padding(3);
            this.tabSearch.Size = new System.Drawing.Size(1192, 652);
            this.tabSearch.TabIndex = 0;
            this.tabSearch.Text = "Job Search";
            //
            // searchControl
            //
            this.searchControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.searchControl.Location = new System.Drawing.Point(3, 3);
            this.searchControl.Name = "searchControl";
            this.searchControl.Size = new System.Drawing.Size(1186, 646);
            this.searchControl.TabIndex = 0;
            //
            // tabRemoteSearch
            //
            this.tabRemoteSearch.BackColor = System.Drawing.SystemColors.Control;
            this.tabRemoteSearch.Controls.Add(this.remoteSearchControl);
            this.tabRemoteSearch.Location = new System.Drawing.Point(4, 24);
            this.tabRemoteSearch.Name = "tabRemoteSearch";
            this.tabRemoteSearch.Padding = new System.Windows.Forms.Padding(3);
            this.tabRemoteSearch.Size = new System.Drawing.Size(1192, 652);
            this.tabRemoteSearch.TabIndex = 1;
            this.tabRemoteSearch.Text = "Remote Jobs Only";
            //
            // remoteSearchControl
            //
            this.remoteSearchControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.remoteSearchControl.Location = new System.Drawing.Point(3, 3);
            this.remoteSearchControl.Name = "remoteSearchControl";
            this.remoteSearchControl.Size = new System.Drawing.Size(1186, 646);
            this.remoteSearchControl.TabIndex = 0;
            //
            // tabLoadTest
            //
            this.tabLoadTest.BackColor = System.Drawing.SystemColors.Control;
            this.tabLoadTest.Controls.Add(this.loadTestControl);
            this.tabLoadTest.Location = new System.Drawing.Point(4, 24);
            this.tabLoadTest.Name = "tabLoadTest";
            this.tabLoadTest.Padding = new System.Windows.Forms.Padding(3);
            this.tabLoadTest.Size = new System.Drawing.Size(1192, 652);
            this.tabLoadTest.TabIndex = 2;
            this.tabLoadTest.Text = "Load Test";
            //
            // loadTestControl
            //
            this.loadTestControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.loadTestControl.Location = new System.Drawing.Point(3, 3);
            this.loadTestControl.Name = "loadTestControl";
            this.loadTestControl.Size = new System.Drawing.Size(1186, 646);
            this.loadTestControl.TabIndex = 0;
            //
            // tabQueryAnalyzer
            //
            this.tabQueryAnalyzer.BackColor = System.Drawing.SystemColors.Control;
            this.tabQueryAnalyzer.Controls.Add(this.queryAnalyzerControl);
            this.tabQueryAnalyzer.Location = new System.Drawing.Point(4, 24);
            this.tabQueryAnalyzer.Name = "tabQueryAnalyzer";
            this.tabQueryAnalyzer.Padding = new System.Windows.Forms.Padding(3);
            this.tabQueryAnalyzer.Size = new System.Drawing.Size(1192, 652);
            this.tabQueryAnalyzer.TabIndex = 3;
            this.tabQueryAnalyzer.Text = "Query Analyzer";
            //
            // queryAnalyzerControl
            //
            this.queryAnalyzerControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.queryAnalyzerControl.Location = new System.Drawing.Point(3, 3);
            this.queryAnalyzerControl.Name = "queryAnalyzerControl";
            this.queryAnalyzerControl.Size = new System.Drawing.Size(1186, 646);
            this.queryAnalyzerControl.TabIndex = 0;
            //
            // tabLocation
            //
            this.tabLocation.BackColor = System.Drawing.SystemColors.Control;
            this.tabLocation.Controls.Add(this.locationValidationControl);
            this.tabLocation.Location = new System.Drawing.Point(4, 24);
            this.tabLocation.Name = "tabLocation";
            this.tabLocation.Padding = new System.Windows.Forms.Padding(3);
            this.tabLocation.Size = new System.Drawing.Size(1192, 652);
            this.tabLocation.TabIndex = 4;
            this.tabLocation.Text = "Location Validation";
            //
            // locationValidationControl
            //
            this.locationValidationControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.locationValidationControl.Location = new System.Drawing.Point(3, 3);
            this.locationValidationControl.Name = "locationValidationControl";
            this.locationValidationControl.Size = new System.Drawing.Size(1186, 646);
            this.locationValidationControl.TabIndex = 0;
            //
            // tabAuditLogs
            //
            this.tabAuditLogs.BackColor = System.Drawing.SystemColors.Control;
            this.tabAuditLogs.Controls.Add(this.auditLogsControl);
            this.tabAuditLogs.Location = new System.Drawing.Point(4, 24);
            this.tabAuditLogs.Name = "tabAuditLogs";
            this.tabAuditLogs.Padding = new System.Windows.Forms.Padding(3);
            this.tabAuditLogs.Size = new System.Drawing.Size(1192, 652);
            this.tabAuditLogs.TabIndex = 4;
            this.tabAuditLogs.Text = "Audit Logs";
            //
            // auditLogsControl
            //
            this.auditLogsControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.auditLogsControl.Location = new System.Drawing.Point(3, 3);
            this.auditLogsControl.Name = "auditLogsControl";
            this.auditLogsControl.Size = new System.Drawing.Size(1186, 646);
            this.auditLogsControl.TabIndex = 0;
            //
            // tabCounts
            //
            this.tabCounts.BackColor = System.Drawing.SystemColors.Control;
            this.tabCounts.Controls.Add(this.countsControl);
            this.tabCounts.Location = new System.Drawing.Point(4, 24);
            this.tabCounts.Name = "tabCounts";
            this.tabCounts.Padding = new System.Windows.Forms.Padding(3);
            this.tabCounts.Size = new System.Drawing.Size(1192, 652);
            this.tabCounts.TabIndex = 5;
            this.tabCounts.Text = "Counts";
            //
            // countsControl
            //
            this.countsControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.countsControl.Location = new System.Drawing.Point(3, 3);
            this.countsControl.Name = "countsControl";
            this.countsControl.Size = new System.Drawing.Size(1186, 646);
            this.countsControl.TabIndex = 0;
            //
            // tabCentroidJobs
            //
            this.tabCentroidJobs.BackColor = System.Drawing.SystemColors.Control;
            this.tabCentroidJobs.Controls.Add(this.centroidJobsControl);
            this.tabCentroidJobs.Location = new System.Drawing.Point(4, 24);
            this.tabCentroidJobs.Name = "tabCentroidJobs";
            this.tabCentroidJobs.Padding = new System.Windows.Forms.Padding(3);
            this.tabCentroidJobs.Size = new System.Drawing.Size(1192, 652);
            this.tabCentroidJobs.TabIndex = 6;
            this.tabCentroidJobs.Text = "Centroid Jobs";
            //
            // centroidJobsControl
            //
            this.centroidJobsControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.centroidJobsControl.Location = new System.Drawing.Point(3, 3);
            this.centroidJobsControl.Name = "centroidJobsControl";
            this.centroidJobsControl.Size = new System.Drawing.Size(1186, 646);
            this.centroidJobsControl.TabIndex = 0;
            //
            // tabLocationErrors
            //
            this.tabLocationErrors.BackColor = System.Drawing.SystemColors.Control;
            this.tabLocationErrors.Controls.Add(this.locationErrorsControl);
            this.tabLocationErrors.Location = new System.Drawing.Point(4, 24);
            this.tabLocationErrors.Name = "tabLocationErrors";
            this.tabLocationErrors.Padding = new System.Windows.Forms.Padding(3);
            this.tabLocationErrors.Size = new System.Drawing.Size(1192, 652);
            this.tabLocationErrors.TabIndex = 7;
            this.tabLocationErrors.Text = "Location Errors";
            //
            // locationErrorsControl
            //
            this.locationErrorsControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.locationErrorsControl.Location = new System.Drawing.Point(3, 3);
            this.locationErrorsControl.Name = "locationErrorsControl";
            this.locationErrorsControl.Size = new System.Drawing.Size(1186, 646);
            this.locationErrorsControl.TabIndex = 0;
            //
            // tabLocationLookups
            //
            this.tabLocationLookups.BackColor = System.Drawing.SystemColors.Control;
            this.tabLocationLookups.Controls.Add(this.locationLookupsControl);
            this.tabLocationLookups.Location = new System.Drawing.Point(4, 24);
            this.tabLocationLookups.Name = "tabLocationLookups";
            this.tabLocationLookups.Padding = new System.Windows.Forms.Padding(3);
            this.tabLocationLookups.Size = new System.Drawing.Size(1192, 652);
            this.tabLocationLookups.TabIndex = 8;
            this.tabLocationLookups.Text = "Location Lookups";
            //
            // locationLookupsControl
            //
            this.locationLookupsControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.locationLookupsControl.Location = new System.Drawing.Point(3, 3);
            this.locationLookupsControl.Name = "locationLookupsControl";
            this.locationLookupsControl.Size = new System.Drawing.Size(1186, 646);
            this.locationLookupsControl.TabIndex = 0;
            //
            // tabHealthCheck
            //
            this.tabHealthCheck.BackColor = System.Drawing.SystemColors.Control;
            this.tabHealthCheck.Controls.Add(this.healthCheckControl);
            this.tabHealthCheck.Location = new System.Drawing.Point(4, 24);
            this.tabHealthCheck.Name = "tabHealthCheck";
            this.tabHealthCheck.Padding = new System.Windows.Forms.Padding(3);
            this.tabHealthCheck.Size = new System.Drawing.Size(1192, 652);
            this.tabHealthCheck.TabIndex = 9;
            this.tabHealthCheck.Text = "Health Check";
            //
            // healthCheckControl
            //
            this.healthCheckControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.healthCheckControl.Location = new System.Drawing.Point(3, 3);
            this.healthCheckControl.Name = "healthCheckControl";
            this.healthCheckControl.Size = new System.Drawing.Size(1186, 646);
            this.healthCheckControl.TabIndex = 0;
            //
            // Form1
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1200, 680);
            this.Controls.Add(this.tabControl);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Job Search API Tester";
            this.tabControl.ResumeLayout(false);
            this.tabSearch.ResumeLayout(false);
            this.tabRemoteSearch.ResumeLayout(false);
            this.tabLoadTest.ResumeLayout(false);
            this.tabQueryAnalyzer.ResumeLayout(false);
            this.tabLocation.ResumeLayout(false);
            this.tabAuditLogs.ResumeLayout(false);
            this.tabCounts.ResumeLayout(false);
            this.tabCentroidJobs.ResumeLayout(false);
            this.tabLocationErrors.ResumeLayout(false);
            this.tabLocationLookups.ResumeLayout(false);
            this.tabHealthCheck.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabSearch;
        private System.Windows.Forms.TabPage tabRemoteSearch;
        private System.Windows.Forms.TabPage tabLoadTest;
        private System.Windows.Forms.TabPage tabQueryAnalyzer;
        private System.Windows.Forms.TabPage tabLocation;
        private System.Windows.Forms.TabPage tabAuditLogs;
        private System.Windows.Forms.TabPage tabCounts;
        private System.Windows.Forms.TabPage tabCentroidJobs;
        private System.Windows.Forms.TabPage tabLocationErrors;
        private System.Windows.Forms.TabPage tabLocationLookups;
        private System.Windows.Forms.TabPage tabHealthCheck;
        private SearchControl searchControl;
        private RemoteSearchControl remoteSearchControl;
        private LoadTestControl loadTestControl;
        private QueryAnalyzerControl queryAnalyzerControl;
        private LocationValidationControl locationValidationControl;
        private AuditLogsControl auditLogsControl;
        private CountsControl countsControl;
        private CentroidJobsControl centroidJobsControl;
        private LocationErrorsControl locationErrorsControl;
        private LocationLookupsControl locationLookupsControl;
        private HealthCheckControl healthCheckControl;
    }
}
