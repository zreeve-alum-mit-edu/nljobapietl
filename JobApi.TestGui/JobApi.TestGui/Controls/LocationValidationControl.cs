using System.Text.Json;

namespace JobApi.TestGui.Controls;

public partial class LocationValidationControl : UserControl
{
    private const string LocationApiUrl = "https://42r7s00kck.execute-api.us-east-2.amazonaws.com/locations/validate";
    private const string ApiKey = "HHTnWCCgx2uCP7Ia3ZVB80SI6lviPPK0gR7eG8Ne";

    public LocationValidationControl()
    {
        InitializeComponent();
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

    private void btnClearLocation_Click(object sender, EventArgs e)
    {
        txtCity.Clear();
        txtState.Clear();
        txtCountry.Text = "US";
        txtLocationResults.Clear();
    }
}
