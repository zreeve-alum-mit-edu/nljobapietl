using System.Diagnostics;
using System.Text;
using System.Text.Json;
using JobApi.TestGui.Models;

namespace JobApi.TestGui.Controls;

public partial class LoadTestControl : UserControl
{
    private const string RemoteSearchApiUrl = "https://42r7s00kck.execute-api.us-east-2.amazonaws.com/search/remote";
    private const string LocationSearchApiUrl = "https://42r7s00kck.execute-api.us-east-2.amazonaws.com/search";
    private const string ApiKey = "HHTnWCCgx2uCP7Ia3ZVB80SI6lviPPK0gR7eG8Ne";

    private CancellationTokenSource? _cancellationTokenSource;
    private readonly List<LoadTestResult> _allResults = new();

    public LoadTestControl()
    {
        InitializeComponent();
        LoadDefaultPrompts();
        UpdateColumnVisibility();
    }

    private void cmbEndpointType_SelectedIndexChanged(object sender, EventArgs e)
    {
        UpdateColumnVisibility();
    }

    private void UpdateColumnVisibility()
    {
        bool isLocationBased = cmbEndpointType.SelectedIndex == 1;

        // Show/hide location columns
        colCity.Visible = isLocationBased;
        colState.Visible = isLocationBased;
        colMiles.Visible = isLocationBased;
        colOnsite.Visible = isLocationBased;
        colHybrid.Visible = isLocationBased;

        // Adjust prompt column width based on visible columns
        colPrompt.Width = isLocationBased ? 150 : 250;
    }

    private void LoadDefaultPrompts()
    {
        // Default Remote-only prompts
        var defaultRemotePrompts = new[]
        {
            new TestPrompt { Name = "SeniorDev", Prompt = "senior software engineer with Python and AWS", NumJobs = 10 },
            new TestPrompt { Name = "DataScience", Prompt = "data scientist specializing in machine learning", NumJobs = 10 },
            new TestPrompt { Name = "DevOps", Prompt = "DevOps engineer experienced with Kubernetes", NumJobs = 10 },
            new TestPrompt { Name = "Frontend", Prompt = "frontend developer React TypeScript", NumJobs = 10 },
            new TestPrompt { Name = "Backend", Prompt = "backend engineer Java microservices", NumJobs = 10 },
            new TestPrompt { Name = "FullStack", Prompt = "full stack developer Node.js PostgreSQL", NumJobs = 10 },
            new TestPrompt { Name = "CloudArch", Prompt = "cloud architect Azure certification", NumJobs = 10 },
            new TestPrompt { Name = "MobileiOS", Prompt = "mobile developer iOS Swift", NumJobs = 10 },
            new TestPrompt { Name = "Security", Prompt = "security engineer penetration testing", NumJobs = 10 },
            new TestPrompt { Name = "SRE", Prompt = "site reliability engineer monitoring", NumJobs = 10 }
        };

        foreach (var prompt in defaultRemotePrompts)
        {
            AddPromptToGrid(prompt);
        }
    }

    private void AddPromptToGrid(TestPrompt prompt)
    {
        dgvPrompts.Rows.Add(
            prompt.Name,
            prompt.Prompt,
            prompt.NumJobs,
            prompt.DaysSincePosting,
            prompt.City,
            prompt.State,
            prompt.Miles,
            prompt.IncludeOnsite ?? false,
            prompt.IncludeHybrid ?? false
        );
    }

    private void btnAdd_Click(object sender, EventArgs e)
    {
        bool isLocationBased = cmbEndpointType.SelectedIndex == 1;
        if (isLocationBased)
        {
            dgvPrompts.Rows.Add("NewTest", "Enter prompt here", 10, null, "Austin", "TX", 20, true, true);
        }
        else
        {
            dgvPrompts.Rows.Add("NewTest", "Enter prompt here", 10, null, null, null, null, false, false);
        }
    }

    private void btnRemove_Click(object sender, EventArgs e)
    {
        if (dgvPrompts.SelectedRows.Count > 0)
        {
            foreach (DataGridViewRow row in dgvPrompts.SelectedRows)
            {
                if (!row.IsNewRow)
                {
                    dgvPrompts.Rows.Remove(row);
                }
            }
        }
    }

    private void btnLoadCsv_Click(object sender, EventArgs e)
    {
        using var openFileDialog = new OpenFileDialog
        {
            Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
            Title = "Load Test Suite"
        };

        if (openFileDialog.ShowDialog() == DialogResult.OK)
        {
            try
            {
                dgvPrompts.Rows.Clear();
                var lines = File.ReadAllLines(openFileDialog.FileName);

                // Skip header if it exists
                var startIndex = lines.Length > 0 && lines[0].StartsWith("Name") ? 1 : 0;

                for (int i = startIndex; i < lines.Length; i++)
                {
                    var parts = lines[i].Split(',');
                    if (parts.Length >= 3)
                    {
                        var name = parts[0].Trim('"');
                        var prompt = parts[1].Trim('"');
                        var numJobs = int.TryParse(parts[2], out int jobs) ? jobs : 10;
                        var days = parts.Length > 3 && int.TryParse(parts[3], out int d) ? (int?)d : null;

                        // Location fields (optional)
                        var city = parts.Length > 4 ? parts[4].Trim('"') : null;
                        var state = parts.Length > 5 ? parts[5].Trim('"') : null;
                        var miles = parts.Length > 6 && int.TryParse(parts[6], out int m) ? (int?)m : null;
                        var onsite = parts.Length > 7 && bool.TryParse(parts[7], out bool o) ? o : false;
                        var hybrid = parts.Length > 8 && bool.TryParse(parts[8], out bool h) ? h : false;

                        dgvPrompts.Rows.Add(name, prompt, numJobs, days, city, state, miles, onsite, hybrid);
                    }
                }

                MessageBox.Show($"Loaded {dgvPrompts.Rows.Count} prompts", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading CSV: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private void btnSaveCsv_Click(object sender, EventArgs e)
    {
        using var saveFileDialog = new SaveFileDialog
        {
            Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
            Title = "Save Test Suite"
        };

        if (saveFileDialog.ShowDialog() == DialogResult.OK)
        {
            try
            {
                var csv = new StringBuilder();
                csv.AppendLine("Name,Prompt,NumJobs,Days,City,State,Miles,Onsite,Hybrid");

                foreach (DataGridViewRow row in dgvPrompts.Rows)
                {
                    if (!row.IsNewRow)
                    {
                        var name = row.Cells[0].Value?.ToString() ?? "";
                        var prompt = row.Cells[1].Value?.ToString() ?? "";
                        var numJobs = row.Cells[2].Value?.ToString() ?? "10";
                        var days = row.Cells[3].Value?.ToString() ?? "";
                        var city = row.Cells[4].Value?.ToString() ?? "";
                        var state = row.Cells[5].Value?.ToString() ?? "";
                        var miles = row.Cells[6].Value?.ToString() ?? "";
                        var onsite = row.Cells[7].Value?.ToString() ?? "False";
                        var hybrid = row.Cells[8].Value?.ToString() ?? "False";

                        csv.AppendLine($"\"{name}\",\"{prompt}\",{numJobs},{days},\"{city}\",\"{state}\",{miles},{onsite},{hybrid}");
                    }
                }

                File.WriteAllText(saveFileDialog.FileName, csv.ToString());
                MessageBox.Show("Test suite saved successfully", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving CSV: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private async void btnRunTest_Click(object sender, EventArgs e)
    {
        if (dgvPrompts.Rows.Count == 0 || (dgvPrompts.Rows.Count == 1 && dgvPrompts.Rows[0].IsNewRow))
        {
            MessageBox.Show("Please add at least one test prompt", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        // Parse settings
        var concurrency = int.Parse(cmbConcurrency.SelectedItem?.ToString() ?? "10");
        var iterations = int.Parse(cmbIterations.SelectedItem?.ToString() ?? "1");
        var apiVersion = cmbApiVersion.SelectedItem?.ToString() ?? "2";
        var endpointType = cmbEndpointType.SelectedIndex == 0 ? SearchEndpointType.Remote : SearchEndpointType.LocationBased;

        // Prepare test suite
        var testPrompts = new List<TestPrompt>();
        foreach (DataGridViewRow row in dgvPrompts.Rows)
        {
            if (!row.IsNewRow)
            {
                var prompt = new TestPrompt
                {
                    Name = row.Cells[0].Value?.ToString() ?? "Unnamed",
                    Prompt = row.Cells[1].Value?.ToString() ?? "",
                    NumJobs = int.TryParse(row.Cells[2].Value?.ToString(), out int jobs) ? jobs : 10,
                    DaysSincePosting = int.TryParse(row.Cells[3].Value?.ToString(), out int days) ? days : null,
                    City = row.Cells[4].Value?.ToString(),
                    State = row.Cells[5].Value?.ToString(),
                    Miles = int.TryParse(row.Cells[6].Value?.ToString(), out int miles) ? miles : null,
                    IncludeOnsite = row.Cells[7].Value is bool onsite ? onsite : null,
                    IncludeHybrid = row.Cells[8].Value is bool hybrid ? hybrid : null
                };

                if (!string.IsNullOrWhiteSpace(prompt.Prompt))
                {
                    testPrompts.Add(prompt);
                }
            }
        }

        if (testPrompts.Count == 0)
        {
            MessageBox.Show("No valid prompts to test", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        // UI state
        btnRunTest.Enabled = false;
        btnStop.Enabled = true;
        dgvResults.Rows.Clear();
        progressBar.Value = 0;
        progressBar.Maximum = testPrompts.Count * iterations;
        txtStats.Text = "Running tests...";

        _cancellationTokenSource = new CancellationTokenSource();

        try
        {
            await RunLoadTest(testPrompts, concurrency, iterations, apiVersion, endpointType, _cancellationTokenSource.Token);
        }
        catch (OperationCanceledException)
        {
            txtStats.Text = "Test cancelled by user";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error during load test: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            btnRunTest.Enabled = true;
            btnStop.Enabled = false;
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }
    }

    private async Task RunLoadTest(List<TestPrompt> prompts, int concurrency, int iterations, string apiVersion, SearchEndpointType endpointType, CancellationToken cancellationToken)
    {
        var semaphore = new SemaphoreSlim(concurrency, concurrency);
        var completedCount = 0;
        var totalTests = prompts.Count * iterations;

        for (int iteration = 0; iteration < iterations; iteration++)
        {
            var tasks = new List<Task<LoadTestResult>>();

            foreach (var prompt in prompts)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var iterationNumber = iteration + 1;
                var task = Task.Run(async () =>
                {
                    await semaphore.WaitAsync(cancellationToken);
                    try
                    {
                        return await ExecuteSingleTest(prompt, apiVersion, endpointType, iterationNumber, cancellationToken);
                    }
                    finally
                    {
                        semaphore.Release();

                        Interlocked.Increment(ref completedCount);

                        // Update progress on UI thread
                        if (InvokeRequired)
                        {
                            Invoke(() =>
                            {
                                progressBar.Value = completedCount;
                                lblProgress.Text = $"Progress: {completedCount}/{totalTests} completed";
                            });
                        }
                    }
                }, cancellationToken);

                tasks.Add(task);
            }

            var results = await Task.WhenAll(tasks);

            // Add results to grid and collection
            foreach (var result in results)
            {
                _allResults.Add(result);
                AddResultToGrid(result);
            }
        }

        // Calculate and display statistics
        DisplayStatistics();
    }

    private async Task<LoadTestResult> ExecuteSingleTest(TestPrompt testPrompt, string apiVersion, SearchEndpointType endpointType, int iteration, CancellationToken cancellationToken)
    {
        var result = new LoadTestResult
        {
            PromptName = iteration > 1 ? $"{testPrompt.Name} (#{iteration})" : testPrompt.Name,
            Prompt = testPrompt.Prompt,
            NumJobs = testPrompt.NumJobs,
            StartTime = DateTime.Now
        };

        try
        {
            object request;
            string url;

            if (endpointType == SearchEndpointType.Remote)
            {
                // Remote search request
                request = new
                {
                    prompt = testPrompt.Prompt,
                    numJobs = testPrompt.NumJobs,
                    daysSincePosting = testPrompt.DaysSincePosting
                };
                url = RemoteSearchApiUrl;
            }
            else
            {
                // Location-based search request
                request = new
                {
                    prompt = testPrompt.Prompt,
                    numJobs = testPrompt.NumJobs,
                    city = testPrompt.City,
                    state = testPrompt.State,
                    miles = testPrompt.Miles ?? 20,
                    includeOnsite = testPrompt.IncludeOnsite ?? true,
                    includeHybrid = testPrompt.IncludeHybrid ?? true,
                    daysSincePosting = testPrompt.DaysSincePosting
                };
                url = LocationSearchApiUrl;
            }

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("x-api-key", ApiKey);
            client.DefaultRequestHeaders.Add("X-API-Version", apiVersion);
            client.Timeout = TimeSpan.FromSeconds(60);

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var stopwatch = Stopwatch.StartNew();
            var response = await client.PostAsync(url, content, cancellationToken);
            stopwatch.Stop();

            result.EndTime = DateTime.Now;
            result.TotalTime = stopwatch.Elapsed;
            result.StatusCode = (int)response.StatusCode;
            result.Success = response.IsSuccessStatusCode;

            if (result.Success)
            {
                var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
                var jsonDoc = JsonDocument.Parse(responseBody);

                // Try to get result count from response
                if (jsonDoc.RootElement.TryGetProperty("jobs", out var jobs) && jobs.ValueKind == JsonValueKind.Array)
                {
                    result.ResultCount = jobs.GetArrayLength();
                }
                else if (jsonDoc.RootElement.TryGetProperty("count", out var count))
                {
                    result.ResultCount = count.GetInt32();
                }
            }
            else
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                result.Error = $"HTTP {result.StatusCode}: {errorBody.Substring(0, Math.Min(100, errorBody.Length))}";
            }
        }
        catch (Exception ex)
        {
            result.EndTime = DateTime.Now;
            result.TotalTime = result.EndTime - result.StartTime;
            result.Success = false;
            result.Error = ex.Message;
        }

        return result;
    }

    private void AddResultToGrid(LoadTestResult result)
    {
        if (InvokeRequired)
        {
            Invoke(() => AddResultToGrid(result));
            return;
        }

        var row = dgvResults.Rows.Add(
            result.PromptName,
            result.Prompt.Length > 50 ? result.Prompt.Substring(0, 47) + "..." : result.Prompt,
            result.StatusDisplay,
            result.TotalTimeMs,
            result.ResultCount,
            result.Error ?? ""
        );

        // Color code by status
        if (!result.Success)
        {
            dgvResults.Rows[row].DefaultCellStyle.BackColor = Color.LightPink;
        }
        else if (result.TotalTime.TotalMilliseconds > 3000)
        {
            dgvResults.Rows[row].DefaultCellStyle.BackColor = Color.LightYellow;
        }
        else if (result.TotalTime.TotalMilliseconds < 1000)
        {
            dgvResults.Rows[row].DefaultCellStyle.BackColor = Color.LightGreen;
        }
    }

    private void DisplayStatistics()
    {
        if (_allResults.Count == 0)
        {
            txtStats.Text = "No results to display";
            return;
        }

        var successfulResults = _allResults.Where(r => r.Success).ToList();
        var failedResults = _allResults.Where(r => !r.Success).ToList();

        var stats = new StringBuilder();
        stats.AppendLine($"Total Tests: {_allResults.Count}");
        stats.AppendLine($"Successful: {successfulResults.Count} ({(double)successfulResults.Count / _allResults.Count * 100:F1}%)");
        stats.AppendLine($"Failed: {failedResults.Count}");

        if (successfulResults.Any())
        {
            var times = successfulResults.Select(r => r.TotalTime.TotalMilliseconds).OrderBy(t => t).ToList();
            var avgTime = times.Average();
            var minTime = times.Min();
            var maxTime = times.Max();
            var p50 = times[times.Count / 2];
            var p95 = times[(int)(times.Count * 0.95)];
            var p99 = times[(int)(times.Count * 0.99)];

            stats.AppendLine();
            stats.AppendLine($"Avg Time: {avgTime:F0} ms ({avgTime / 1000:F2} s)");
            stats.AppendLine($"Min Time: {minTime:F0} ms | Max Time: {maxTime:F0} ms");
            stats.AppendLine($"P50: {p50:F0} ms | P95: {p95:F0} ms | P99: {p99:F0} ms");
        }

        txtStats.Text = stats.ToString();
    }

    private void btnClearResults_Click(object sender, EventArgs e)
    {
        dgvResults.Rows.Clear();
        _allResults.Clear();
        txtStats.Clear();
        progressBar.Value = 0;
        lblProgress.Text = "Progress:";
    }

    private void btnStop_Click(object sender, EventArgs e)
    {
        _cancellationTokenSource?.Cancel();
        btnStop.Enabled = false;
    }

    private void btnExportResults_Click(object sender, EventArgs e)
    {
        if (_allResults.Count == 0)
        {
            MessageBox.Show("No results to export", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using var saveFileDialog = new SaveFileDialog
        {
            Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
            Title = "Export Results",
            FileName = $"LoadTest_Results_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
        };

        if (saveFileDialog.ShowDialog() == DialogResult.OK)
        {
            try
            {
                var csv = new StringBuilder();
                csv.AppendLine("Name,Prompt,Status,Success,TotalTimeMs,ResultCount,StatusCode,Error,StartTime,EndTime");

                foreach (var result in _allResults)
                {
                    csv.AppendLine($"\"{result.PromptName}\",\"{result.Prompt}\",\"{result.StatusDisplay}\",{result.Success},{result.TotalTime.TotalMilliseconds:F0},{result.ResultCount},{result.StatusCode},\"{result.Error ?? ""}\",{result.StartTime:O},{result.EndTime:O}");
                }

                File.WriteAllText(saveFileDialog.FileName, csv.ToString());
                MessageBox.Show($"Results exported successfully to:\n{saveFileDialog.FileName}", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting results: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
