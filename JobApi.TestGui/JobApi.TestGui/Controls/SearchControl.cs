using System.Text;
using System.Text.Json;

namespace JobApi.TestGui.Controls;

public partial class SearchControl : UserControl
{
    private const string SearchApiUrl = "https://42r7s00kck.execute-api.us-east-2.amazonaws.com/search";
    private const string ApiKey = "HHTnWCCgx2uCP7Ia3ZVB80SI6lviPPK0gR7eG8Ne";

    private int _totalPages = 0;
    private int _totalCount = 0;

    public SearchControl()
    {
        InitializeComponent();
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
                daysSincePosting = string.IsNullOrWhiteSpace(txtDays.Text) ? (int?)null : int.Parse(txtDays.Text),
                page = (int)numPage.Value
            };

            // Send request
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("x-api-key", ApiKey);

            // Add API version header
            var apiVersion = cmbVersion.SelectedItem?.ToString() ?? "2";
            client.DefaultRequestHeaders.Add("X-API-Version", apiVersion);

            client.Timeout = TimeSpan.FromSeconds(60);

            var json = JsonSerializer.Serialize(request, new JsonSerializerOptions { WriteIndented = true });
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            txtResults.Text = $"Request (API v{apiVersion}):\n{json}\n\nSending...";
            Application.DoEvents();

            var response = await client.PostAsync(SearchApiUrl, content);
            var responseBody = await response.Content.ReadAsStringAsync();

            // Parse pagination metadata
            if (response.IsSuccessStatusCode)
            {
                try
                {
                    var doc = JsonDocument.Parse(responseBody);
                    if (doc.RootElement.TryGetProperty("totalPages", out var totalPagesElement))
                    {
                        _totalPages = totalPagesElement.GetInt32();
                    }
                    if (doc.RootElement.TryGetProperty("totalCount", out var totalCountElement))
                    {
                        _totalCount = totalCountElement.GetInt32();
                    }
                    if (doc.RootElement.TryGetProperty("page", out var pageElement))
                    {
                        var currentPage = pageElement.GetInt32();
                        lblPageInfo.Text = $"Page {currentPage} of {_totalPages} - {_totalCount} total results";
                    }

                    // Update button states
                    bool hasNextPage = false;
                    bool hasPreviousPage = false;
                    if (doc.RootElement.TryGetProperty("hasNextPage", out var hasNextElement))
                    {
                        hasNextPage = hasNextElement.GetBoolean();
                    }
                    if (doc.RootElement.TryGetProperty("hasPreviousPage", out var hasPrevElement))
                    {
                        hasPreviousPage = hasPrevElement.GetBoolean();
                    }

                    btnNextPage.Enabled = hasNextPage;
                    btnPrevPage.Enabled = hasPreviousPage;
                }
                catch
                {
                    // If parsing fails, just show the response
                    lblPageInfo.Text = "";
                }
            }

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
            return JsonSerializer.Serialize(doc.RootElement, new JsonSerializerOptions { WriteIndented = true });
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
        numPage.Value = 1;
        lblPageInfo.Text = "";
        btnPrevPage.Enabled = false;
        btnNextPage.Enabled = false;
    }

    private void numPage_ValueChanged(object sender, EventArgs e)
    {
        // Trigger search with new page number
        if (_totalPages > 0 && numPage.Value <= _totalPages)
        {
            btnSearch_Click(sender, e);
        }
    }

    private void btnPrevPage_Click(object sender, EventArgs e)
    {
        if (numPage.Value > 1)
        {
            numPage.Value--;
        }
    }

    private void btnNextPage_Click(object sender, EventArgs e)
    {
        if (numPage.Value < _totalPages)
        {
            numPage.Value++;
        }
    }
}
