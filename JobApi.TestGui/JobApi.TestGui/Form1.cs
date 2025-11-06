namespace JobApi.TestGui;

public partial class Form1 : Form
{
    private bool auditLogsLoaded = false;
    private bool countsLoaded = false;
    private bool locationErrorsLoaded = false;

    public Form1()
    {
        InitializeComponent();

        // Wire up event for centroid selection
        countsControl.CentroidSelected += CountsControl_CentroidSelected;

        // Subscribe to tab selection event to load data on first view
        tabControl.SelectedIndexChanged += TabControl_SelectedIndexChanged;
    }

    private void TabControl_SelectedIndexChanged(object? sender, EventArgs e)
    {
        // Load audit logs when the audit logs tab is selected for the first time
        if (tabControl.SelectedTab == tabAuditLogs && !auditLogsLoaded)
        {
            auditLogsLoaded = true;
            auditLogsControl.LoadData();
        }

        // Load counts when the counts tab is selected for the first time
        if (tabControl.SelectedTab == tabCounts && !countsLoaded)
        {
            countsLoaded = true;
            countsControl.LoadData();
        }

        // Load location errors when the location errors tab is selected for the first time
        if (tabControl.SelectedTab == tabLocationErrors && !locationErrorsLoaded)
        {
            locationErrorsLoaded = true;
            locationErrorsControl.LoadData();
        }
    }

    private void CountsControl_CentroidSelected(object? sender, Guid centroidId)
    {
        // Switch to the Centroid Jobs tab
        tabControl.SelectedTab = tabCentroidJobs;

        // Load the jobs for this centroid
        centroidJobsControl.SetCentroid(centroidId);
    }
}
