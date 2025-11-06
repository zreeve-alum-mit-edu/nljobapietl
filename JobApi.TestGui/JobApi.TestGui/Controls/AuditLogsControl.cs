using System.Data;
using Npgsql;

namespace JobApi.TestGui.Controls;

public partial class AuditLogsControl : UserControl
{
    private const string DbConnectionString = "Host=nljobsearchapi.c74ioigi2bn4.us-east-2.rds.amazonaws.com;Database=nljobsearch;Username=JSadmin;Password=mxofoyLVkiV2aQACxIbJ;Port=5432";

    // Pagination fields
    private int currentPage = 1;
    private const int pageSize = 50;
    private int totalRecords = 0;
    private int totalPages = 0;

    public AuditLogsControl()
    {
        InitializeComponent();
    }

    public void LoadData()
    {
        LoadAuditLogs();
    }

    private async void LoadAuditLogs()
    {
        try
        {
            // Disable pagination buttons while loading
            btnFirstPage.Enabled = false;
            btnPreviousPage.Enabled = false;
            btnNextPage.Enabled = false;
            btnLastPage.Enabled = false;
            btnRefresh.Enabled = false;

            lblPageInfo.Text = "Loading...";
            Application.DoEvents();

            await using var connection = new NpgsqlConnection(DbConnectionString);
            await connection.OpenAsync();

            // Get total count first
            var countSql = "SELECT COUNT(*) FROM api_audit_logs";
            await using var countCmd = new NpgsqlCommand(countSql, connection);
            totalRecords = Convert.ToInt32(await countCmd.ExecuteScalarAsync());
            totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

            // Ensure current page is valid
            if (currentPage > totalPages && totalPages > 0)
            {
                currentPage = totalPages;
            }
            if (currentPage < 1)
            {
                currentPage = 1;
            }

            // Get paginated data
            var offset = (currentPage - 1) * pageSize;
            var sql = @"
                SELECT
                    id,
                    endpoint,
                    start_time,
                    total_duration_ms,
                    embedding_duration_ms,
                    database_duration_ms,
                    prompt,
                    num_jobs_requested,
                    city,
                    state,
                    miles,
                    include_onsite,
                    include_hybrid,
                    days_since_posting,
                    num_results_returned,
                    num_jobs_filtered,
                    status_code,
                    error_message,
                    lambda_request_id,
                    created_at
                FROM api_audit_logs
                ORDER BY created_at DESC
                LIMIT @limit OFFSET @offset";

            await using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("limit", pageSize);
            cmd.Parameters.AddWithValue("offset", offset);

            var dataTable = new DataTable();
            await using var reader = await cmd.ExecuteReaderAsync();
            dataTable.Load(reader);

            // Bind to DataGridView
            dgvAuditLogs.DataSource = dataTable;

            // Format columns
            if (dgvAuditLogs.Columns.Count > 0)
            {
                dgvAuditLogs.Columns["id"]!.HeaderText = "ID";
                dgvAuditLogs.Columns["endpoint"]!.HeaderText = "Endpoint";
                dgvAuditLogs.Columns["start_time"]!.HeaderText = "Start Time";
                dgvAuditLogs.Columns["total_duration_ms"]!.HeaderText = "Total (ms)";
                dgvAuditLogs.Columns["embedding_duration_ms"]!.HeaderText = "Embedding (ms)";
                dgvAuditLogs.Columns["database_duration_ms"]!.HeaderText = "DB (ms)";
                dgvAuditLogs.Columns["prompt"]!.HeaderText = "Prompt";
                dgvAuditLogs.Columns["num_jobs_requested"]!.HeaderText = "Jobs Req";
                dgvAuditLogs.Columns["city"]!.HeaderText = "City";
                dgvAuditLogs.Columns["state"]!.HeaderText = "State";
                dgvAuditLogs.Columns["miles"]!.HeaderText = "Miles";
                dgvAuditLogs.Columns["include_onsite"]!.HeaderText = "Onsite";
                dgvAuditLogs.Columns["include_hybrid"]!.HeaderText = "Hybrid";
                dgvAuditLogs.Columns["days_since_posting"]!.HeaderText = "Days";
                dgvAuditLogs.Columns["num_results_returned"]!.HeaderText = "Results";
                dgvAuditLogs.Columns["num_jobs_filtered"]!.HeaderText = "Filtered";
                dgvAuditLogs.Columns["status_code"]!.HeaderText = "Status";
                dgvAuditLogs.Columns["error_message"]!.HeaderText = "Error";
                dgvAuditLogs.Columns["lambda_request_id"]!.HeaderText = "Lambda ID";
                dgvAuditLogs.Columns["created_at"]!.HeaderText = "Created";

                // Set specific column widths for better readability
                dgvAuditLogs.Columns["endpoint"]!.Width = 120;
                dgvAuditLogs.Columns["start_time"]!.Width = 150;
                dgvAuditLogs.Columns["prompt"]!.Width = 200;
                dgvAuditLogs.Columns["error_message"]!.Width = 200;
            }

            // Update page info
            lblPageInfo.Text = $"Page {currentPage} of {totalPages} ({totalRecords} total records)";

            // Enable/disable pagination buttons based on current page
            btnFirstPage.Enabled = currentPage > 1;
            btnPreviousPage.Enabled = currentPage > 1;
            btnNextPage.Enabled = currentPage < totalPages;
            btnLastPage.Enabled = currentPage < totalPages;
            btnRefresh.Enabled = true;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading audit logs: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            lblPageInfo.Text = "Error loading data";

            // Re-enable buttons
            btnFirstPage.Enabled = true;
            btnPreviousPage.Enabled = true;
            btnNextPage.Enabled = true;
            btnLastPage.Enabled = true;
            btnRefresh.Enabled = true;
        }
    }

    private void btnFirstPage_Click(object sender, EventArgs e)
    {
        currentPage = 1;
        LoadAuditLogs();
    }

    private void btnPreviousPage_Click(object sender, EventArgs e)
    {
        if (currentPage > 1)
        {
            currentPage--;
            LoadAuditLogs();
        }
    }

    private void btnNextPage_Click(object sender, EventArgs e)
    {
        if (currentPage < totalPages)
        {
            currentPage++;
            LoadAuditLogs();
        }
    }

    private void btnLastPage_Click(object sender, EventArgs e)
    {
        currentPage = totalPages;
        LoadAuditLogs();
    }

    private void btnRefresh_Click(object sender, EventArgs e)
    {
        LoadAuditLogs();
    }
}
