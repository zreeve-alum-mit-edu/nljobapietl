using System.Text;
using System.Text.Json;

namespace JobApi.TestGui.Controls;

public partial class RemoteSearchControl : UserControl
{
    private const string RemoteSearchApiUrl = "https://42r7s00kck.execute-api.us-east-2.amazonaws.com/search/remote";
    private const string ApiKey = "HHTnWCCgx2uCP7Ia3ZVB80SI6lviPPK0gR7eG8Ne";

    private int _totalPages = 0;
    private int _totalCount = 0;

    public RemoteSearchControl()
    {
        InitializeComponent();
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
                daysSincePosting = daysSincePosting,
                page = (int)numRemotePage.Value
            };

            // Send request
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("x-api-key", ApiKey);

            // Add API version header
            var apiVersion = cmbRemoteVersion.SelectedItem?.ToString() ?? "2";
            client.DefaultRequestHeaders.Add("X-API-Version", apiVersion);

            client.Timeout = TimeSpan.FromSeconds(60);

            var json = JsonSerializer.Serialize(request, new JsonSerializerOptions { WriteIndented = true });
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            txtRemoteResults.Text = $"Request (API v{apiVersion}):\n{json}\n\nSending to /search/remote...";
            Application.DoEvents();

            var response = await client.PostAsync(RemoteSearchApiUrl, content);
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
                        lblRemotePageInfo.Text = $"Page {currentPage} of {_totalPages} - {_totalCount} total results";
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

                    btnRemoteNextPage.Enabled = hasNextPage;
                    btnRemotePrevPage.Enabled = hasPreviousPage;
                }
                catch
                {
                    // If parsing fails, just show the response
                    lblRemotePageInfo.Text = "";
                }
            }

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

    private void btnClearRemote_Click(object sender, EventArgs e)
    {
        txtRemotePrompt.Text = "senior software engineer with Python and AWS experience";
        txtRemoteNumJobs.Text = "10";
        txtRemoteDays.Clear();
        txtRemoteResults.Clear();
        numRemotePage.Value = 1;
        lblRemotePageInfo.Text = "";
        btnRemotePrevPage.Enabled = false;
        btnRemoteNextPage.Enabled = false;
    }

    private void numRemotePage_ValueChanged(object sender, EventArgs e)
    {
        // Trigger search with new page number
        if (_totalPages > 0 && numRemotePage.Value <= _totalPages)
        {
            btnSearchRemote_Click(sender, e);
        }
    }

    private void btnRemotePrevPage_Click(object sender, EventArgs e)
    {
        if (numRemotePage.Value > 1)
        {
            numRemotePage.Value--;
        }
    }

    private void btnRemoteNextPage_Click(object sender, EventArgs e)
    {
        if (numRemotePage.Value < _totalPages)
        {
            numRemotePage.Value++;
        }
    }
}
