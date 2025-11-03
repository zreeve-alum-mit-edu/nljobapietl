using System.Text;
using System.Text.Json;

namespace JobApi.TestGui;

public partial class Form1 : Form
{
    private const string SearchApiUrl = "https://42r7s00kck.execute-api.us-east-2.amazonaws.com/search";
    private const string RemoteSearchApiUrl = "https://42r7s00kck.execute-api.us-east-2.amazonaws.com/search/remote";
    private const string LocationApiUrl = "https://42r7s00kck.execute-api.us-east-2.amazonaws.com/locations/validate";
    private const string ApiKey = "HHTnWCCgx2uCP7Ia3ZVB80SI6lviPPK0gR7eG8Ne";

    public Form1()
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

            // Build filters
            var filters = new List<object>();
            if (!string.IsNullOrWhiteSpace(txtLocation.Text))
            {
                if (!int.TryParse(txtMiles.Text, out int miles) || miles < 1)
                {
                    MessageBox.Show("Miles must be at least 1", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                filters.Add(new
                {
                    includeOnsite = chkOnsite.Checked,
                    includeHybrid = chkHybrid.Checked,
                    location = txtLocation.Text,
                    miles = miles
                });
            }

            // Build request
            var request = new
            {
                prompt = txtPrompt.Text,
                numJobs = numJobs,
                includeRemote = chkRemote.Checked,
                daysSincePosting = string.IsNullOrWhiteSpace(txtDays.Text) ? (int?)null : int.Parse(txtDays.Text),
                filters = filters
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
        chkRemote.Checked = true;
        txtDays.Clear();
        txtLocation.Clear();
        txtMiles.Text = "50";
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

            // Validate inputs
            if (string.IsNullOrWhiteSpace(txtRemotePrompt.Text))
            {
                MessageBox.Show("Prompt is required", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!int.TryParse(txtRemoteNumJobs.Text, out int numJobs) || numJobs < 1 || numJobs > 100)
            {
                MessageBox.Show("NumJobs must be between 1 and 100", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Build request (only remote jobs, no location filters needed)
            var request = new
            {
                prompt = txtRemotePrompt.Text,
                numJobs = numJobs,
                daysSincePosting = string.IsNullOrWhiteSpace(txtRemoteDays.Text) ? (int?)null : int.Parse(txtRemoteDays.Text)
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
}
