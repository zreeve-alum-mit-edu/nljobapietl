using System.Data;
using System.Text.Json;
using Npgsql;

namespace JobApi.TestGui.Controls;

public partial class LocationErrorsControl : UserControl
{
    private const string ApiKey = "HHTnWCCgx2uCP7Ia3ZVB80SI6lviPPK0gR7eG8Ne";
    private const string LocationApiUrl = "https://42r7s00kck.execute-api.us-east-2.amazonaws.com/locations/validate";
    private const string DbConnectionString = "Host=nljobsearchapi.c74ioigi2bn4.us-east-2.rds.amazonaws.com;Database=nljobsearch;Username=JSadmin;Password=mxofoyLVkiV2aQACxIbJ;Port=5432";

    private string? selectedLocationCity = null;
    private string? selectedLocationState = null;
    private string? selectedLocationCountry = null;
    private List<LocationSuggestion> currentSuggestions = new List<LocationSuggestion>();

    public LocationErrorsControl()
    {
        InitializeComponent();
    }

    public void LoadData()
    {
        LoadLocationErrors();
    }

    private async void LoadLocationErrors()
    {
        try
        {
            btnRefreshLocationErrors.Enabled = false;
            btnRefreshLocationErrors.Text = "Loading...";
            Application.DoEvents();

            await using var connection = new NpgsqlConnection(DbConnectionString);
            await connection.OpenAsync();

            // Get distinct city/state/country tuples with counts
            var sql = @"
                SELECT
                    generated_city as city,
                    generated_state as state,
                    generated_country as country,
                    COUNT(*) as count
                FROM locationerrors
                GROUP BY generated_city, generated_state, generated_country
                ORDER BY count DESC";

            await using var cmd = new NpgsqlCommand(sql, connection);
            var dataTable = new DataTable();
            await using var reader = await cmd.ExecuteReaderAsync();
            dataTable.Load(reader);

            // Bind to DataGridView
            dgvLocationErrors.DataSource = dataTable;

            // Format columns
            if (dgvLocationErrors.Columns.Count > 0)
            {
                dgvLocationErrors.Columns["city"]!.HeaderText = "City";
                dgvLocationErrors.Columns["state"]!.HeaderText = "State";
                dgvLocationErrors.Columns["country"]!.HeaderText = "Country";
                dgvLocationErrors.Columns["count"]!.HeaderText = "Count";

                // Set specific column widths for better readability
                dgvLocationErrors.Columns["city"]!.Width = 200;
                dgvLocationErrors.Columns["state"]!.Width = 100;
                dgvLocationErrors.Columns["country"]!.Width = 100;
                dgvLocationErrors.Columns["count"]!.Width = 100;
            }

            btnRefreshLocationErrors.Text = "Refresh";
            btnRefreshLocationErrors.Enabled = true;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading location errors: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            btnRefreshLocationErrors.Text = "Refresh";
            btnRefreshLocationErrors.Enabled = true;
        }
    }

    private void btnRefreshLocationErrors_Click(object sender, EventArgs e)
    {
        LoadLocationErrors();
    }

    private void dgvLocationErrors_CellClick(object sender, DataGridViewCellEventArgs e)
    {
        // Ignore header row clicks
        if (e.RowIndex < 0)
            return;

        try
        {
            // Get the selected location from the clicked row
            var cityValue = dgvLocationErrors.Rows[e.RowIndex].Cells["city"].Value;
            var stateValue = dgvLocationErrors.Rows[e.RowIndex].Cells["state"].Value;
            var countryValue = dgvLocationErrors.Rows[e.RowIndex].Cells["country"].Value;

            selectedLocationCity = cityValue?.ToString();
            selectedLocationState = stateValue?.ToString();
            selectedLocationCountry = countryValue?.ToString();

            // Update UI
            lblSelectedLocation.Text = $"Selected: {selectedLocationCity}, {selectedLocationState}, {selectedLocationCountry}";
            btnValidateErrorLocation.Enabled = true;
            lstSuggestions.Items.Clear();
            currentSuggestions.Clear();
            btnAddOverride.Enabled = false;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error selecting location: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void btnValidateErrorLocation_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(selectedLocationCity) || string.IsNullOrWhiteSpace(selectedLocationState))
        {
            MessageBox.Show("Please select a location error first", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            btnValidateErrorLocation.Enabled = false;
            lstSuggestions.Items.Clear();
            lstSuggestions.Items.Add("Loading suggestions...");
            Application.DoEvents();

            // Build URL with query parameters
            var country = selectedLocationCountry ?? "US";
            var url = $"{LocationApiUrl}?city={Uri.EscapeDataString(selectedLocationCity)}&state={Uri.EscapeDataString(selectedLocationState)}&country={Uri.EscapeDataString(country)}";

            // Send request
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("x-api-key", ApiKey);
            client.Timeout = TimeSpan.FromSeconds(30);

            var response = await client.GetAsync(url);
            var responseBody = await response.Content.ReadAsStringAsync();

            // Parse response
            var jsonDoc = JsonDocument.Parse(responseBody);
            var root = jsonDoc.RootElement;

            lstSuggestions.Items.Clear();
            currentSuggestions.Clear();

            if (root.TryGetProperty("valid", out var validElement) && validElement.GetBoolean())
            {
                // Exact match found
                lstSuggestions.Items.Add($"✓ {selectedLocationCity}, {selectedLocationState}, {country} (Exact Match)");
                currentSuggestions.Add(new LocationSuggestion
                {
                    City = selectedLocationCity,
                    State = selectedLocationState,
                    Country = country,
                    IsExactMatch = true
                });
            }

            if (root.TryGetProperty("suggestions", out var suggestionsElement) &&
                suggestionsElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var suggestion in suggestionsElement.EnumerateArray())
                {
                    // API returns suggestions as strings in format "City,State"
                    var suggestionStr = suggestion.GetString();
                    if (string.IsNullOrWhiteSpace(suggestionStr))
                        continue;

                    var parts = suggestionStr.Split(',');
                    if (parts.Length >= 2)
                    {
                        var city = parts[0].Trim();
                        var state = parts[1].Trim();

                        lstSuggestions.Items.Add($"{city}, {state}, US");
                        currentSuggestions.Add(new LocationSuggestion
                        {
                            City = city,
                            State = state,
                            Country = "US",
                            Similarity = 0.0
                        });
                    }
                }
            }

            if (lstSuggestions.Items.Count == 0)
            {
                lstSuggestions.Items.Add("No suggestions found");
            }
            else
            {
                lstSuggestions.SelectedIndex = 0;
                btnAddOverride.Enabled = true;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error validating location: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            lstSuggestions.Items.Clear();
            lstSuggestions.Items.Add("Error loading suggestions");
        }
        finally
        {
            btnValidateErrorLocation.Enabled = true;
        }
    }

    private async void btnAddOverride_Click(object sender, EventArgs e)
    {
        if (lstSuggestions.SelectedIndex < 0 || lstSuggestions.SelectedIndex >= currentSuggestions.Count)
        {
            MessageBox.Show("Please select a suggestion first", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(selectedLocationCity) || string.IsNullOrWhiteSpace(selectedLocationState))
        {
            MessageBox.Show("No location error selected", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            btnAddOverride.Enabled = false;

            var selectedSuggestion = currentSuggestions[lstSuggestions.SelectedIndex];

            // Confirm with user
            var confirmMessage = $"Add location override?\n\n" +
                                 $"When geocoding finds:\n" +
                                 $"  {selectedLocationCity}, {selectedLocationState}, {selectedLocationCountry ?? "US"}\n\n" +
                                 $"It will be corrected to:\n" +
                                 $"  {selectedSuggestion.City}, {selectedSuggestion.State}, {selectedSuggestion.Country}";

            var result = MessageBox.Show(confirmMessage, "Confirm Override", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result != DialogResult.Yes)
            {
                btnAddOverride.Enabled = true;
                return;
            }

            // Insert into database
            await using var connection = new NpgsqlConnection(DbConnectionString);
            await connection.OpenAsync();

            var sql = @"
                INSERT INTO post_process_location_lookups
                    (id, generated_city, generated_state, generated_country, city, state, country, created_at)
                VALUES
                    (@id, @genCity, @genState, @genCountry, @city, @state, @country, @createdAt)
                ON CONFLICT (generated_city, generated_state, generated_country)
                DO UPDATE SET
                    city = @city,
                    state = @state,
                    country = @country";

            await using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("id", Guid.NewGuid());
            cmd.Parameters.AddWithValue("genCity", selectedLocationCity);
            cmd.Parameters.AddWithValue("genState", selectedLocationState);
            cmd.Parameters.AddWithValue("genCountry", selectedLocationCountry ?? "US");
            cmd.Parameters.AddWithValue("city", selectedSuggestion.City);
            cmd.Parameters.AddWithValue("state", selectedSuggestion.State);
            cmd.Parameters.AddWithValue("country", selectedSuggestion.Country);
            cmd.Parameters.AddWithValue("createdAt", DateTime.UtcNow);

            await cmd.ExecuteNonQueryAsync();

            MessageBox.Show("Override added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Clear selection
            lstSuggestions.Items.Clear();
            currentSuggestions.Clear();
            btnAddOverride.Enabled = false;
            lblSelectedLocation.Text = "Select a location error to validate";
            selectedLocationCity = null;
            selectedLocationState = null;
            selectedLocationCountry = null;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error adding override: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            btnAddOverride.Enabled = currentSuggestions.Count > 0;
        }
    }

    // Helper class for location suggestions
    private class LocationSuggestion
    {
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Country { get; set; } = "US";
        public double Similarity { get; set; }
        public bool IsExactMatch { get; set; }
    }
}
