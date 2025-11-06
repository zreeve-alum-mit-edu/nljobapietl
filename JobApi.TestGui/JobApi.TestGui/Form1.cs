using System.Text;
using System.Text.Json;
using Npgsql;
using System.Data;

namespace JobApi.TestGui;

public partial class Form1 : Form
{
    private const string SearchApiUrl = "https://42r7s00kck.execute-api.us-east-2.amazonaws.com/search";
    private const string RemoteSearchApiUrl = "https://42r7s00kck.execute-api.us-east-2.amazonaws.com/search/remote";
    private const string LocationApiUrl = "https://42r7s00kck.execute-api.us-east-2.amazonaws.com/locations/validate";
    private const string ApiKey = "HHTnWCCgx2uCP7Ia3ZVB80SI6lviPPK0gR7eG8Ne";
    private const string DbConnectionString = "Host=nljobsearchapi.c74ioigi2bn4.us-east-2.rds.amazonaws.com;Database=nljobsearch;Username=JSadmin;Password=mxofoyLVkiV2aQACxIbJ;Port=5432";

    // Pagination fields
    private int currentPage = 1;
    private const int pageSize = 50;
    private int totalRecords = 0;
    private int totalPages = 0;
    private bool auditLogsLoaded = false;
    private bool countsLoaded = false;

    // Centroid jobs pagination fields
    private int centroidCurrentPage = 1;
    private const int centroidPageSize = 50;
    private int centroidTotalRecords = 0;
    private int centroidTotalPages = 0;
    private Guid? selectedCentroidId = null;

    public Form1()
    {
        InitializeComponent();

        // Subscribe to tab selection event to load audit logs on first view
        tabControl.SelectedIndexChanged += TabControl_SelectedIndexChanged;
    }

    private void TabControl_SelectedIndexChanged(object? sender, EventArgs e)
    {
        // Load audit logs when the audit logs tab is selected for the first time
        if (tabControl.SelectedTab == tabAuditLogs && !auditLogsLoaded)
        {
            auditLogsLoaded = true;
            LoadAuditLogs();
        }

        // Load counts when the counts tab is selected for the first time
        if (tabControl.SelectedTab == tabCounts && !countsLoaded)
        {
            countsLoaded = true;
            LoadCounts();
        }
    }

    private async void btnSearch_Click(object sender, EventArgs e)
    {
        try
        {
            btnSearch.Enabled = false;
            txtResults.Text = "Searching...";

            // Validate inputs
            if (string.IsNullOrWhiteSpace(txtPrompt.Text))
            {
                MessageBox.Show("Prompt is required", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!int.TryParse(txtNumJobs.Text, out int numJobs) || numJobs < 1 || numJobs > 100)
            {
                MessageBox.Show("NumJobs must be between 1 and 100", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Validate city input
            if (string.IsNullOrWhiteSpace(txtSearchCity.Text))
            {
                MessageBox.Show("City is required (e.g., Austin)", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Validate state input
            if (string.IsNullOrWhiteSpace(txtSearchState.Text))
            {
                MessageBox.Show("State is required (e.g., TX)", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var city = txtSearchCity.Text.Trim();
            var state = txtSearchState.Text.Trim();

            // Validate miles
            if (!int.TryParse(txtMiles.Text, out int miles) || miles < 1 || miles > 20)
            {
                MessageBox.Show("Miles must be between 1 and 20", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Validate at least one workplace type is selected
            if (!chkOnsite.Checked && !chkHybrid.Checked)
            {
                MessageBox.Show("At least one of Onsite or Hybrid must be checked", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Build request with flattened structure
            var request = new
            {
                prompt = txtPrompt.Text,
                numJobs = numJobs,
                city = city,
                state = state,
                miles = miles,
                includeOnsite = chkOnsite.Checked,
                includeHybrid = chkHybrid.Checked,
                daysSincePosting = string.IsNullOrWhiteSpace(txtDays.Text) ? (int?)null : int.Parse(txtDays.Text)
            };

            // Send request
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("x-api-key", ApiKey);
            client.Timeout = TimeSpan.FromSeconds(60);

            var json = JsonSerializer.Serialize(request, new JsonSerializerOptions { WriteIndented = true });
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            txtResults.Text = $"Request:\n{json}\n\nSending...";
            Application.DoEvents();

            var response = await client.PostAsync(SearchApiUrl, content);
            var responseBody = await response.Content.ReadAsStringAsync();

            // Format response
            var formattedResponse = FormatJsonResponse(responseBody);
            txtResults.Text = $"Request:\n{json}\n\n---\n\nResponse ({response.StatusCode}):\n{formattedResponse}";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            txtResults.Text += $"\n\nError: {ex.Message}";
        }
        finally
        {
            btnSearch.Enabled = true;
        }
    }

    private string FormatJsonResponse(string json)
    {
        try
        {
            var doc = JsonDocument.Parse(json);
            return JsonSerializer.Serialize(doc, new JsonSerializerOptions { WriteIndented = true });
        }
        catch
        {
            return json;
        }
    }

    private void btnClear_Click(object sender, EventArgs e)
    {
        txtPrompt.Clear();
        txtNumJobs.Text = "10";
        txtDays.Clear();
        txtSearchCity.Text = "Austin";
        txtSearchState.Text = "TX";
        txtMiles.Text = "20";
        chkOnsite.Checked = true;
        chkHybrid.Checked = true;
        txtResults.Clear();
    }

    private async void btnValidateLocation_Click(object sender, EventArgs e)
    {
        try
        {
            btnValidateLocation.Enabled = false;
            txtLocationResults.Text = "Validating...";

            // Validate inputs
            if (string.IsNullOrWhiteSpace(txtCity.Text))
            {
                MessageBox.Show("City is required", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtState.Text))
            {
                MessageBox.Show("State is required", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Build URL with query parameters
            var country = string.IsNullOrWhiteSpace(txtCountry.Text) ? "US" : txtCountry.Text;
            var url = $"{LocationApiUrl}?city={Uri.EscapeDataString(txtCity.Text)}&state={Uri.EscapeDataString(txtState.Text)}&country={Uri.EscapeDataString(country)}";

            // Send request
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("x-api-key", ApiKey);
            client.Timeout = TimeSpan.FromSeconds(30);

            txtLocationResults.Text = $"Request:\nGET {url}\n\nSending...";
            Application.DoEvents();

            var response = await client.GetAsync(url);
            var responseBody = await response.Content.ReadAsStringAsync();

            // Format response
            var formattedResponse = FormatJsonResponse(responseBody);
            txtLocationResults.Text = $"Request:\nGET {url}\n\n---\n\nResponse ({response.StatusCode}):\n{formattedResponse}";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            txtLocationResults.Text += $"\n\nError: {ex.Message}";
        }
        finally
        {
            btnValidateLocation.Enabled = true;
        }
    }

    private void btnClearLocation_Click(object sender, EventArgs e)
    {
        txtCity.Clear();
        txtState.Clear();
        txtCountry.Text = "US";
        txtLocationResults.Clear();
    }

    private async void btnSearchRemote_Click(object sender, EventArgs e)
    {
        try
        {
            btnSearchRemote.Enabled = false;
            txtRemoteResults.Text = "Searching...";

            // No validation - let the API validate
            int.TryParse(txtRemoteNumJobs.Text, out int numJobs);
            int? daysSincePosting = null;
            if (!string.IsNullOrWhiteSpace(txtRemoteDays.Text) && int.TryParse(txtRemoteDays.Text, out int days))
            {
                daysSincePosting = days;
            }

            // Build request (only remote jobs, no location filters needed)
            var request = new
            {
                prompt = txtRemotePrompt.Text,
                numJobs = numJobs,
                daysSincePosting = daysSincePosting
            };

            // Send request
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("x-api-key", ApiKey);
            client.Timeout = TimeSpan.FromSeconds(60);

            var json = JsonSerializer.Serialize(request, new JsonSerializerOptions { WriteIndented = true });
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            txtRemoteResults.Text = $"Request:\n{json}\n\nSending to /search/remote...";
            Application.DoEvents();

            var response = await client.PostAsync(RemoteSearchApiUrl, content);
            var responseBody = await response.Content.ReadAsStringAsync();

            // Format response
            var formattedResponse = FormatJsonResponse(responseBody);
            txtRemoteResults.Text = $"Request:\n{json}\n\n---\n\nResponse ({response.StatusCode}):\n{formattedResponse}";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            txtRemoteResults.Text += $"\n\nError: {ex.Message}";
        }
        finally
        {
            btnSearchRemote.Enabled = true;
        }
    }

    private void btnClearRemote_Click(object sender, EventArgs e)
    {
        txtRemotePrompt.Text = "senior software engineer with Python and AWS experience";
        txtRemoteNumJobs.Text = "10";
        txtRemoteDays.Clear();
        txtRemoteResults.Clear();
    }

    // ========== AUDIT LOGS METHODS ==========

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

    // ========== COUNTS TAB METHODS ==========

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

    // ========== CENTROID JOBS TAB METHODS ==========

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
                selectedCentroidId = centroidId;
                centroidCurrentPage = 1;

                // Switch to the Centroid Jobs tab
                tabControl.SelectedTab = tabCentroidJobs;

                // Load the jobs for this centroid
                LoadCentroidJobs();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading centroid jobs: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
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
