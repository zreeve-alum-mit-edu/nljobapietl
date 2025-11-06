using System.Data;
using Npgsql;

namespace JobApi.TestGui.Controls;

public partial class CentroidJobsControl : UserControl
{
    private const string DbConnectionString = "Host=nljobsearchapi.c74ioigi2bn4.us-east-2.rds.amazonaws.com;Database=nljobsearch;Username=JSadmin;Password=mxofoyLVkiV2aQACxIbJ;Port=5432";

    // Centroid jobs pagination fields
    private int centroidCurrentPage = 1;
    private const int centroidPageSize = 50;
    private int centroidTotalRecords = 0;
    private int centroidTotalPages = 0;
    private Guid? selectedCentroidId = null;

    public CentroidJobsControl()
    {
        InitializeComponent();
    }

    public void SetCentroid(Guid centroidId)
    {
        selectedCentroidId = centroidId;
        centroidCurrentPage = 1;
        LoadCentroidJobs();
    }

    private async void LoadCentroidJobs()
    {
        if (!selectedCentroidId.HasValue)
        {
            lblSelectedCentroid.Text = "Select a centroid from the Counts tab first";
            lblCentroidPageInfo.Text = "";
            dgvCentroidJobs.DataSource = null;
            return;
        }

        try
        {
            // Disable pagination buttons while loading
            btnCentroidFirstPage.Enabled = false;
            btnCentroidPreviousPage.Enabled = false;
            btnCentroidNextPage.Enabled = false;
            btnCentroidLastPage.Enabled = false;

            lblCentroidPageInfo.Text = "Loading...";
            Application.DoEvents();

            await using var connection = new NpgsqlConnection(DbConnectionString);
            await connection.OpenAsync();

            // Get total count first
            var countSql = @"
                SELECT COUNT(*)
                FROM centroid_assignments
                WHERE centroid_id = @centroidId";

            await using var countCmd = new NpgsqlCommand(countSql, connection);
            countCmd.Parameters.AddWithValue("centroidId", selectedCentroidId.Value);
            centroidTotalRecords = Convert.ToInt32(await countCmd.ExecuteScalarAsync());
            centroidTotalPages = (int)Math.Ceiling(centroidTotalRecords / (double)centroidPageSize);

            // Ensure current page is valid
            if (centroidCurrentPage > centroidTotalPages && centroidTotalPages > 0)
            {
                centroidCurrentPage = centroidTotalPages;
            }
            if (centroidCurrentPage < 1)
            {
                centroidCurrentPage = 1;
            }

            // Update selected centroid label
            lblSelectedCentroid.Text = $"Jobs for Centroid: {selectedCentroidId.Value}";

            // Get paginated data with job title and description, sorted by cosine distance to centroid
            var offset = (centroidCurrentPage - 1) * centroidPageSize;
            var sql = @"
                SELECT
                    (je.embedding <=> c.centroid) as cosine_distance,
                    j.job_title,
                    j.job_description,
                    j.id
                FROM centroid_assignments ca
                INNER JOIN jobs j ON ca.job_id = j.id
                INNER JOIN job_embeddings je ON j.id = je.job_id
                INNER JOIN centroids c ON ca.centroid_id = c.id
                WHERE ca.centroid_id = @centroidId
                ORDER BY je.embedding <=> c.centroid
                LIMIT @limit OFFSET @offset";

            await using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("centroidId", selectedCentroidId.Value);
            cmd.Parameters.AddWithValue("limit", centroidPageSize);
            cmd.Parameters.AddWithValue("offset", offset);

            var dataTable = new DataTable();
            await using var reader = await cmd.ExecuteReaderAsync();
            dataTable.Load(reader);

            // Bind to DataGridView
            dgvCentroidJobs.DataSource = dataTable;

            // Format columns
            if (dgvCentroidJobs.Columns.Count > 0)
            {
                dgvCentroidJobs.Columns["cosine_distance"]!.HeaderText = "Distance";
                dgvCentroidJobs.Columns["job_title"]!.HeaderText = "Job Title";
                dgvCentroidJobs.Columns["job_description"]!.HeaderText = "Job Description";
                dgvCentroidJobs.Columns["id"]!.HeaderText = "Job ID";

                // Set specific column widths for better readability
                dgvCentroidJobs.Columns["cosine_distance"]!.Width = 100;
                dgvCentroidJobs.Columns["job_title"]!.Width = 200;
                dgvCentroidJobs.Columns["job_description"]!.Width = 300;
                dgvCentroidJobs.Columns["id"]!.Width = 250;

                // Format distance as a number with 4 decimal places
                dgvCentroidJobs.Columns["cosine_distance"]!.DefaultCellStyle.Format = "N4";
            }

            // Update page info
            lblCentroidPageInfo.Text = $"Page {centroidCurrentPage} of {centroidTotalPages} ({centroidTotalRecords} total jobs)";

            // Enable/disable pagination buttons based on current page
            btnCentroidFirstPage.Enabled = centroidCurrentPage > 1;
            btnCentroidPreviousPage.Enabled = centroidCurrentPage > 1;
            btnCentroidNextPage.Enabled = centroidCurrentPage < centroidTotalPages;
            btnCentroidLastPage.Enabled = centroidCurrentPage < centroidTotalPages;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading centroid jobs: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            lblCentroidPageInfo.Text = "Error loading data";

            // Re-enable buttons
            btnCentroidFirstPage.Enabled = true;
            btnCentroidPreviousPage.Enabled = true;
            btnCentroidNextPage.Enabled = true;
            btnCentroidLastPage.Enabled = true;
        }
    }

    private void btnCentroidFirstPage_Click(object sender, EventArgs e)
    {
        centroidCurrentPage = 1;
        LoadCentroidJobs();
    }

    private void btnCentroidPreviousPage_Click(object sender, EventArgs e)
    {
        if (centroidCurrentPage > 1)
        {
            centroidCurrentPage--;
            LoadCentroidJobs();
        }
    }

    private void btnCentroidNextPage_Click(object sender, EventArgs e)
    {
        if (centroidCurrentPage < centroidTotalPages)
        {
            centroidCurrentPage++;
            LoadCentroidJobs();
        }
    }

    private void btnCentroidLastPage_Click(object sender, EventArgs e)
    {
        centroidCurrentPage = centroidTotalPages;
        LoadCentroidJobs();
    }
}
