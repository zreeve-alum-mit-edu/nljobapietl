using System.Text.Json;

namespace JobApi.TestGui.Controls;

public partial class HealthCheckControl : UserControl
{
    private const string HealthCheckUrl = "https://42r7s00kck.execute-api.us-east-2.amazonaws.com/health";
    private System.Windows.Forms.Timer? _autoRefreshTimer;

    public HealthCheckControl()
    {
        InitializeComponent();
    }

    private async void btnCheckHealth_Click(object sender, EventArgs e)
    {
        await CheckHealth();
    }

    private async Task CheckHealth()
    {
        try
        {
            btnCheckHealth.Enabled = false;
            txtResponse.Text = "Checking API health...";
            lblStatus.Text = "Status: Checking...";
            lblStatus.ForeColor = Color.Gray;
            lblLastChecked.Text = "";

            var startTime = DateTime.Now;

            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(10);

            var response = await httpClient.GetAsync(HealthCheckUrl);
            var content = await response.Content.ReadAsStringAsync();

            var elapsed = DateTime.Now - startTime;

            // Parse and pretty-print JSON
            var jsonDoc = JsonDocument.Parse(content);
            var formattedJson = JsonSerializer.Serialize(jsonDoc, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            txtResponse.Text = formattedJson;

            // Extract status from JSON
            var statusElement = jsonDoc.RootElement.GetProperty("status");
            var status = statusElement.GetString();

            if (response.IsSuccessStatusCode && status == "healthy")
            {
                lblStatus.Text = "Status: HEALTHY ✓";
                lblStatus.ForeColor = Color.Green;
                lblStatusCode.Text = $"HTTP {(int)response.StatusCode}";
                lblStatusCode.ForeColor = Color.Green;
            }
            else
            {
                lblStatus.Text = "Status: UNHEALTHY ✗";
                lblStatus.ForeColor = Color.Red;
                lblStatusCode.Text = $"HTTP {(int)response.StatusCode}";
                lblStatusCode.ForeColor = Color.Red;
            }

            lblResponseTime.Text = $"Response Time: {elapsed.TotalMilliseconds:F0}ms";
            lblLastChecked.Text = $"Last Checked: {DateTime.Now:yyyy-MM-dd HH:mm:ss}";

            // Extract database status if present
            if (jsonDoc.RootElement.TryGetProperty("database", out var dbElement))
            {
                var dbStatus = dbElement.GetString();
                lblDatabaseStatus.Text = $"Database: {dbStatus}";
                lblDatabaseStatus.ForeColor = dbStatus == "connected" ? Color.Green : Color.Red;
            }
        }
        catch (HttpRequestException ex)
        {
            txtResponse.Text = $"HTTP Error: {ex.Message}";
            lblStatus.Text = "Status: ERROR ✗";
            lblStatus.ForeColor = Color.Red;
            lblStatusCode.Text = "Connection Failed";
            lblStatusCode.ForeColor = Color.Red;
            lblLastChecked.Text = $"Last Checked: {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
        }
        catch (TaskCanceledException)
        {
            txtResponse.Text = "Request timed out after 10 seconds";
            lblStatus.Text = "Status: TIMEOUT ✗";
            lblStatus.ForeColor = Color.Red;
            lblStatusCode.Text = "Timeout";
            lblStatusCode.ForeColor = Color.Red;
            lblLastChecked.Text = $"Last Checked: {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
        }
        catch (Exception ex)
        {
            txtResponse.Text = $"Error: {ex.Message}\n\n{ex.StackTrace}";
            lblStatus.Text = "Status: ERROR ✗";
            lblStatus.ForeColor = Color.Red;
            lblStatusCode.Text = "Error";
            lblStatusCode.ForeColor = Color.Red;
            lblLastChecked.Text = $"Last Checked: {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
        }
        finally
        {
            btnCheckHealth.Enabled = true;
        }
    }

    private void chkAutoRefresh_CheckedChanged(object sender, EventArgs e)
    {
        if (chkAutoRefresh.Checked)
        {
            // Get refresh interval from NumericUpDown
            var intervalSeconds = (int)numRefreshInterval.Value;

            _autoRefreshTimer = new System.Windows.Forms.Timer();
            _autoRefreshTimer.Interval = intervalSeconds * 1000;
            _autoRefreshTimer.Tick += async (s, e) => await CheckHealth();
            _autoRefreshTimer.Start();

            // Do initial check
            _ = CheckHealth();
        }
        else
        {
            if (_autoRefreshTimer != null)
            {
                _autoRefreshTimer.Stop();
                _autoRefreshTimer.Dispose();
                _autoRefreshTimer = null;
            }
        }
    }

    private void btnCopyUrl_Click(object sender, EventArgs e)
    {
        Clipboard.SetText(HealthCheckUrl);
        MessageBox.Show("Health check URL copied to clipboard", "Copied", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
}
