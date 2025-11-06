using System.Data;
using Npgsql;

namespace JobApi.TestGui.Controls;

public partial class CountsControl : UserControl
{
    private const string DbConnectionString = "Host=nljobsearchapi.c74ioigi2bn4.us-east-2.rds.amazonaws.com;Database=nljobsearch;Username=JSadmin;Password=mxofoyLVkiV2aQACxIbJ;Port=5432";

    public event EventHandler<Guid>? CentroidSelected;

    public CountsControl()
    {
        InitializeComponent();
    }

    public void LoadData()
    {
        LoadCounts();
    }

    private async void LoadCounts()
    {
        try
        {
            btnRefreshCounts.Enabled = false;
            btnRefreshCounts.Text = "Loading...";
            Application.DoEvents();

            await using var connection = new NpgsqlConnection(DbConnectionString);
            await connection.OpenAsync();

            // Get Job Status Counts
            var statusSql = @"
                SELECT status, COUNT(*) as count
                FROM jobs
                GROUP BY status
                ORDER BY count DESC";

            await using var statusCmd = new NpgsqlCommand(statusSql, connection);
            var statusTable = new DataTable();
            await using var statusReader = await statusCmd.ExecuteReaderAsync();
            statusTable.Load(statusReader);
            dgvJobStatusCounts.DataSource = statusTable;

            // Get Centroid Counts
            var centroidSql = @"
                SELECT centroid_id, COUNT(*) as count
                FROM centroid_assignments
                GROUP BY centroid_id
                ORDER BY count DESC";

            await using var centroidCmd = new NpgsqlCommand(centroidSql, connection);
            var centroidTable = new DataTable();
            await using var centroidReader = await centroidCmd.ExecuteReaderAsync();
            centroidTable.Load(centroidReader);
            dgvCentroidCounts.DataSource = centroidTable;

            // Get Total Jobs Count
            var jobsCountSql = "SELECT COUNT(*) FROM jobs";
            await using var jobsCountCmd = new NpgsqlCommand(jobsCountSql, connection);
            var totalJobs = Convert.ToInt64(await jobsCountCmd.ExecuteScalarAsync());
            lblTotalJobsValue.Text = totalJobs.ToString("N0");

            // Get Total Locations Count
            var locationsCountSql = "SELECT COUNT(*) FROM job_locations";
            await using var locationsCountCmd = new NpgsqlCommand(locationsCountSql, connection);
            var totalLocations = Convert.ToInt64(await locationsCountCmd.ExecuteScalarAsync());
            lblTotalLocationsValue.Text = totalLocations.ToString("N0");

            // Get Total URLs Count
            var urlsCountSql = "SELECT COUNT(*) FROM job_location_urls";
            await using var urlsCountCmd = new NpgsqlCommand(urlsCountSql, connection);
            var totalUrls = Convert.ToInt64(await urlsCountCmd.ExecuteScalarAsync());
            lblTotalUrlsValue.Text = totalUrls.ToString("N0");

            // Get Invalid Locations Count
            var invalidLocationsCountSql = "SELECT COUNT(*) FROM locationerrors";
            await using var invalidLocationsCountCmd = new NpgsqlCommand(invalidLocationsCountSql, connection);
            var invalidLocations = Convert.ToInt64(await invalidLocationsCountCmd.ExecuteScalarAsync());
            lblInvalidLocationsValue.Text = invalidLocations.ToString("N0");

            btnRefreshCounts.Text = "Refresh";
            btnRefreshCounts.Enabled = true;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading counts: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            btnRefreshCounts.Text = "Refresh";
            btnRefreshCounts.Enabled = true;
        }
    }

    private void btnRefreshCounts_Click(object sender, EventArgs e)
    {
        LoadCounts();
    }

    private void dgvCentroidCounts_CellClick(object sender, DataGridViewCellEventArgs e)
    {
        // Ignore header row clicks
        if (e.RowIndex < 0)
            return;

        try
        {
            // Get the selected centroid_id from the clicked row
            var centroidIdValue = dgvCentroidCounts.Rows[e.RowIndex].Cells["centroid_id"].Value;

            if (centroidIdValue != null && Guid.TryParse(centroidIdValue.ToString(), out Guid centroidId))
            {
                // Raise event to notify Form1 to switch tabs
                CentroidSelected?.Invoke(this, centroidId);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading centroid jobs: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
