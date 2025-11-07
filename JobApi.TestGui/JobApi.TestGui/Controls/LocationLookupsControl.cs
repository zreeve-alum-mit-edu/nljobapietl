using System.Data;
using Npgsql;

namespace JobApi.TestGui.Controls;

public partial class LocationLookupsControl : UserControl
{
    private const string DbConnectionString = "Host=nljobsearchapi.c74ioigi2bn4.us-east-2.rds.amazonaws.com;Database=nljobsearch;Username=JSadmin;Password=mxofoyLVkiV2aQACxIbJ;Port=5432";
    private DataTable? dataTable;

    public LocationLookupsControl()
    {
        InitializeComponent();
    }

    public void LoadData()
    {
        LoadLocationLookups();
    }

    private async void LoadLocationLookups()
    {
        try
        {
            btnRefreshLookups.Enabled = false;
            btnRefreshLookups.Text = "Loading...";
            Application.DoEvents();

            await using var connection = new NpgsqlConnection(DbConnectionString);
            await connection.OpenAsync();

            // Get all location lookups ordered by confidence descending
            var sql = @"
                SELECT
                    id,
                    location_text,
                    city,
                    state,
                    country,
                    confidence,
                    created_at
                FROM location_lookups
                ORDER BY confidence DESC, location_text ASC";

            await using var cmd = new NpgsqlCommand(sql, connection);
            dataTable = new DataTable();
            await using var reader = await cmd.ExecuteReaderAsync();
            dataTable.Load(reader);

            // Set all DataTable columns to NOT be read-only to avoid binding errors
            foreach (DataColumn column in dataTable.Columns)
            {
                column.ReadOnly = false;
            }

            // Bind to DataGridView
            dgvLocationLookups.DataSource = dataTable;

            // Format columns
            if (dgvLocationLookups.Columns.Count > 0)
            {
                // First, set ALL columns as read-only to match any DataTable read-only columns
                foreach (DataGridViewColumn column in dgvLocationLookups.Columns)
                {
                    column.ReadOnly = true;
                }

                // Hide the ID column
                dgvLocationLookups.Columns["id"]!.Visible = false;

                // Set header texts
                dgvLocationLookups.Columns["location_text"]!.HeaderText = "Location Text";
                dgvLocationLookups.Columns["city"]!.HeaderText = "City";
                dgvLocationLookups.Columns["state"]!.HeaderText = "State";
                dgvLocationLookups.Columns["country"]!.HeaderText = "Country";
                dgvLocationLookups.Columns["confidence"]!.HeaderText = "Confidence";
                dgvLocationLookups.Columns["created_at"]!.HeaderText = "Created At";

                // Now explicitly make ONLY confidence editable
                dgvLocationLookups.Columns["confidence"]!.ReadOnly = false;

                // Set specific column widths for better readability
                dgvLocationLookups.Columns["location_text"]!.Width = 300;
                dgvLocationLookups.Columns["city"]!.Width = 150;
                dgvLocationLookups.Columns["state"]!.Width = 80;
                dgvLocationLookups.Columns["country"]!.Width = 80;
                dgvLocationLookups.Columns["confidence"]!.Width = 100;
                dgvLocationLookups.Columns["created_at"]!.Width = 150;

                // Finally, set the DataGridView itself to NOT read-only to enable editing
                dgvLocationLookups.ReadOnly = false;
            }

            btnRefreshLookups.Text = "Refresh";
            btnRefreshLookups.Enabled = true;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading location lookups: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            btnRefreshLookups.Text = "Refresh";
            btnRefreshLookups.Enabled = true;
        }
    }

    private void btnRefreshLookups_Click(object sender, EventArgs e)
    {
        LoadLocationLookups();
    }

    private async void btnSaveLookups_Click(object sender, EventArgs e)
    {
        if (dataTable == null)
        {
            MessageBox.Show("No data to save", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        try
        {
            btnSaveLookups.Enabled = false;
            btnSaveLookups.Text = "Saving...";
            Application.DoEvents();

            await using var connection = new NpgsqlConnection(DbConnectionString);
            await connection.OpenAsync();

            var updatedCount = 0;

            // Get modified rows
            var modifiedRows = dataTable.AsEnumerable()
                .Where(row => row.RowState == DataRowState.Modified)
                .ToList();

            if (modifiedRows.Count == 0)
            {
                MessageBox.Show("No changes to save", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                btnSaveLookups.Text = "Save Changes";
                btnSaveLookups.Enabled = true;
                return;
            }

            foreach (var row in modifiedRows)
            {
                var id = (Guid)row["id"];
                var confidence = Convert.ToInt32(row["confidence"]);

                var sql = @"
                    UPDATE location_lookups
                    SET confidence = @confidence
                    WHERE id = @id";

                await using var cmd = new NpgsqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("id", id);
                cmd.Parameters.AddWithValue("confidence", confidence);

                await cmd.ExecuteNonQueryAsync();
                updatedCount++;
            }

            // Accept changes to clear modified state
            dataTable.AcceptChanges();

            MessageBox.Show($"Successfully saved {updatedCount} confidence score(s)", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

            btnSaveLookups.Text = "Save Changes";
            btnSaveLookups.Enabled = true;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error saving changes: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            btnSaveLookups.Text = "Save Changes";
            btnSaveLookups.Enabled = true;
        }
    }
}
