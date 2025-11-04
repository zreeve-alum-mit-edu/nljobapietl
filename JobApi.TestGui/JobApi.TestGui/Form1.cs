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
}
