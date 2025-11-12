using Npgsql;
using System.Text;
using System.Text.RegularExpressions;

namespace JobApi.TestGui.Controls;

public partial class QueryAnalyzerControl : UserControl
{
    private const string DbHost = "nljobsearchapi.c74ioigi2bn4.us-east-2.rds.amazonaws.com";
    private const string DbName = "nljobsearch";
    private const string DbUser = "JSadmin";
    private const string DbPassword = "mxofoyLVkiV2aQACxIbJ";

    private readonly Dictionary<string, string> _queryTemplates = new()
    {
        ["Custom Query"] = "",
        ["Remote Jobs - HNSW Vector Search"] = @"SELECT
    id, title, company, description,
    embedding <=> '[{embedding}]'::vector as distance
FROM jobs
WHERE status = 'embedded'
    AND generated_workplace = 'REMOTE'
ORDER BY embedding <=> '[{embedding}]'::vector
LIMIT {limit};",
        ["Location Filter ONLY (No Vector)"] = @"-- Test JUST the location filter (what API does first)
-- Uses actual API query structure without the ORDER BY embedding
SET LOCAL jit = off;

WITH base AS MATERIALIZED (
    SELECT DISTINCT j.id, j.job_title, j.company_name
    FROM jobs j
    INNER JOIN job_locations jl ON jl.job_id = j.id
    WHERE j.is_valid = true
        AND j.status = 'embedded'
        AND j.generated_workplace IN ('ONSITE', 'HYBRID')
        AND jl.gistlocation IS NOT NULL
        AND ST_DWithin(
            jl.gistlocation,
            ST_SetSRID(ST_MakePoint(-97.7431, 30.2672), 4326)::geography,  -- Austin, TX
            32186.9  -- 20 miles in meters
        )
)
SELECT
    COUNT(*) as jobs_matching_location
FROM base;",
        ["Location + Vector Search (ACTUAL API)"] = @"-- ACTUAL API QUERY - Full location-based search
-- Step 1: Filters by location THEN Step 2: ORDER BY embedding
SET LOCAL jit = off;

WITH base AS MATERIALIZED (
    SELECT DISTINCT j.id
    FROM jobs j
    INNER JOIN job_locations jl ON jl.job_id = j.id
    WHERE j.is_valid = true
        AND j.status = 'embedded'
        AND j.generated_workplace IN ('ONSITE', 'HYBRID')
        AND jl.gistlocation IS NOT NULL
        AND ST_DWithin(
            jl.gistlocation,
            ST_SetSRID(ST_MakePoint(-97.7431, 30.2672), 4326)::geography,  -- Austin, TX
            32186.9  -- 20 miles in meters
        )
),
base_count AS (
    SELECT COUNT(*) as total FROM base
)
SELECT
    j.id,
    j.job_title,
    j.company_name,
    j.job_description,
    (e.embedding <=> '[{embedding}]'::vector) AS similarity_score,
    (SELECT total FROM base_count) AS filtered_count
FROM base b
JOIN job_embeddings e ON e.job_id = b.id
JOIN jobs j ON j.id = b.id
WHERE e.embedding IS NOT NULL
ORDER BY similarity_score, j.id
LIMIT {limit};",
        ["Count Location-Filtered Jobs"] = @"-- How many jobs pass the location filter? (uses actual API logic)
WITH base AS MATERIALIZED (
    SELECT DISTINCT j.id
    FROM jobs j
    INNER JOIN job_locations jl ON jl.job_id = j.id
    WHERE j.is_valid = true
        AND j.status = 'embedded'
        AND j.generated_workplace IN ('ONSITE', 'HYBRID')
        AND jl.gistlocation IS NOT NULL
        AND ST_DWithin(
            jl.gistlocation,
            ST_SetSRID(ST_MakePoint(-97.7431, 30.2672), 4326)::geography,
            32186.9  -- 20 miles
        )
)
SELECT COUNT(*) as jobs_in_location FROM base;",
        ["Count Jobs by Status"] = @"SELECT
    status,
    COUNT(*) as count,
    COUNT(CASE WHEN generated_workplace = 'REMOTE' THEN 1 END) as remote_count,
    COUNT(CASE WHEN generated_workplace IN ('ONSITE', 'HYBRID') THEN 1 END) as location_count
FROM jobs
GROUP BY status
ORDER BY count DESC;",
        ["Check GiST Location Index"] = @"-- Check the critical GiST index for location filtering
SELECT
    schemaname,
    tablename,
    indexname,
    pg_size_pretty(pg_relation_size(indexrelid)) as index_size,
    idx_scan as times_used,
    idx_tup_read as tuples_read,
    idx_tup_fetch as tuples_fetched
FROM pg_stat_user_indexes
WHERE tablename = 'job_locations'
    AND indexname LIKE '%gist%'
ORDER BY pg_relation_size(indexrelid) DESC;",
        ["Check All Location Indexes"] = @"-- Check all indexes related to location search
SELECT
    schemaname,
    tablename,
    indexname,
    pg_size_pretty(pg_relation_size(indexrelid)) as index_size,
    idx_scan as times_used,
    idx_tup_read as tuples_read
FROM pg_stat_user_indexes
WHERE tablename IN ('jobs', 'job_locations', 'job_embeddings', 'centroid_assignments')
ORDER BY tablename, pg_relation_size(indexrelid) DESC;",
        ["Check HNSW Index Usage"] = @"SELECT
    schemaname,
    tablename,
    indexname,
    pg_size_pretty(pg_relation_size(indexrelid)) as index_size,
    idx_scan as times_used,
    idx_tup_read as tuples_read
FROM pg_stat_user_indexes
WHERE tablename = 'jobs'
    AND indexname LIKE '%hnsw%'
ORDER BY pg_relation_size(indexrelid) DESC;",
        ["Cache Hit Ratio"] = @"SELECT
    datname as database,
    blks_read as disk_blocks_read,
    blks_hit as cache_blocks_hit,
    ROUND(100.0 * blks_hit / NULLIF(blks_hit + blks_read, 0), 2) AS cache_hit_ratio_pct
FROM pg_stat_database
WHERE datname = 'nljobsearch';",
        ["Table and Index Sizes"] = @"SELECT
    schemaname,
    tablename,
    pg_size_pretty(pg_total_relation_size(schemaname||'.'||tablename)) AS total_size,
    pg_size_pretty(pg_relation_size(schemaname||'.'||tablename)) AS table_size,
    pg_size_pretty(pg_total_relation_size(schemaname||'.'||tablename) - pg_relation_size(schemaname||'.'||tablename)) AS indexes_size
FROM pg_tables
WHERE schemaname = 'public'
ORDER BY pg_total_relation_size(schemaname||'.'||tablename) DESC
LIMIT 10;",
        ["Location Centroid Distribution"] = @"-- Understand location centroid distribution
SELECT
    lc.city,
    lc.state,
    COUNT(j.id) as job_count,
    COUNT(CASE WHEN j.status = 'embedded' THEN 1 END) as embedded_count,
    lc.location as coordinates
FROM location_centroids lc
LEFT JOIN jobs j ON j.location_centroid_id = lc.id
WHERE j.generated_workplace IN ('ONSITE', 'HYBRID')
GROUP BY lc.id, lc.city, lc.state, lc.location
ORDER BY job_count DESC
LIMIT 50;"
    };

    public QueryAnalyzerControl()
    {
        InitializeComponent();
        LoadQueryTemplates();
    }

    private void LoadQueryTemplates()
    {
        cmbQueryTemplate.Items.Clear();
        foreach (var template in _queryTemplates.Keys)
        {
            cmbQueryTemplate.Items.Add(template);
        }
        cmbQueryTemplate.SelectedIndex = 0;
    }

    private void cmbQueryTemplate_SelectedIndexChanged(object sender, EventArgs e)
    {
        var selectedTemplate = cmbQueryTemplate.SelectedItem?.ToString();
        if (selectedTemplate != null && _queryTemplates.TryGetValue(selectedTemplate, out var query))
        {
            txtQuery.Text = query;
        }
    }

    private async void btnRunExplain_Click(object sender, EventArgs e)
    {
        await RunQuery(analyze: false);
    }

    private async void btnRunExplainAnalyze_Click(object sender, EventArgs e)
    {
        await RunQuery(analyze: true);
    }

    private async Task RunQuery(bool analyze)
    {
        try
        {
            btnRunExplain.Enabled = false;
            btnRunExplainAnalyze.Enabled = false;
            txtResults.Text = "Executing query...";
            txtStats.Text = "";

            var query = txtQuery.Text.Trim();
            if (string.IsNullOrWhiteSpace(query))
            {
                MessageBox.Show("Please enter a query", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Replace template variables
            query = ReplaceTemplateVariables(query);

            // Extract SET commands from the query
            var setCommands = new List<string>();
            var lines = query.Split('\n');
            var queryLines = new List<string>();

            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (trimmed.StartsWith("SET ", StringComparison.OrdinalIgnoreCase))
                {
                    setCommands.Add(trimmed);
                }
                else if (!string.IsNullOrWhiteSpace(trimmed) || queryLines.Count > 0)
                {
                    queryLines.Add(line);
                }
            }

            var cleanQuery = string.Join("\n", queryLines).Trim();

            // Build EXPLAIN command
            var explainOptions = new List<string>();
            if (analyze) explainOptions.Add("ANALYZE");
            if (chkBuffers.Checked) explainOptions.Add("BUFFERS");
            if (chkTiming.Checked) explainOptions.Add("TIMING");

            var explainQuery = $"EXPLAIN ({string.Join(", ", explainOptions)})\n{cleanQuery}";

            // Collect all commands to execute
            var commands = new List<string>();

            // Add ef_search setting if specified
            if (int.TryParse(txtEfSearch.Text, out int efSearch) && efSearch > 0)
            {
                commands.Add($"SET hnsw.ef_search = {efSearch};");
            }

            // Add any SET commands extracted from the query
            commands.AddRange(setCommands);

            // Add the EXPLAIN query
            commands.Add(explainQuery);

            var connectionString = $"Host={DbHost};Database={DbName};Username={DbUser};Password={DbPassword}";
            var results = new StringBuilder();

            await Task.Run(async () =>
            {
                using var connection = new NpgsqlConnection(connectionString);
                await connection.OpenAsync();

                foreach (var cmd in commands)
                {
                    using var command = new NpgsqlCommand(cmd, connection);
                    command.CommandTimeout = 60;

                    if (cmd.StartsWith("SET"))
                    {
                        await command.ExecuteNonQueryAsync();
                        continue;
                    }

                    using var reader = await command.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        results.AppendLine(reader.GetString(0));
                    }
                }
            });

            var output = results.ToString();
            txtResults.Text = output;

            // Parse and display stats
            if (analyze)
            {
                DisplayStats(output);
            }
            else
            {
                txtStats.Text = "Run EXPLAIN ANALYZE to see performance statistics";
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error executing query:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            txtResults.Text = $"Error: {ex.Message}\n\n{ex.StackTrace}";
        }
        finally
        {
            btnRunExplain.Enabled = true;
            btnRunExplainAnalyze.Enabled = true;
        }
    }

    private string ReplaceTemplateVariables(string query)
    {
        // Replace {limit}
        if (int.TryParse(txtLimit.Text, out int limit))
        {
            query = query.Replace("{limit}", limit.ToString());
        }

        // Replace {embedding} with a sample embedding (for demonstration)
        // In practice, you'd need to generate a real embedding
        var sampleEmbedding = string.Join(",", Enumerable.Range(0, 1536).Select(_ => "0.1"));
        query = query.Replace("{embedding}", sampleEmbedding);

        return query;
    }

    private void DisplayStats(string explainOutput)
    {
        var stats = new StringBuilder();

        // Extract execution time
        var executionTimeMatch = Regex.Match(explainOutput, @"Execution Time:\s*([\d.]+)\s*ms", RegexOptions.IgnoreCase);
        if (executionTimeMatch.Success)
        {
            var execTime = double.Parse(executionTimeMatch.Groups[1].Value);
            stats.AppendLine($"⏱️  Execution Time: {execTime:F2} ms ({execTime / 1000:F2} seconds)");
        }

        // Extract planning time
        var planningTimeMatch = Regex.Match(explainOutput, @"Planning Time:\s*([\d.]+)\s*ms", RegexOptions.IgnoreCase);
        if (planningTimeMatch.Success)
        {
            var planTime = double.Parse(planningTimeMatch.Groups[1].Value);
            stats.AppendLine($"📋 Planning Time: {planTime:F2} ms");
        }

        // Extract buffer stats
        var buffersMatch = Regex.Match(explainOutput, @"Buffers:\s*shared\s*hit=(\d+)\s*read=(\d+)", RegexOptions.IgnoreCase);
        if (buffersMatch.Success)
        {
            var hit = int.Parse(buffersMatch.Groups[1].Value);
            var read = int.Parse(buffersMatch.Groups[2].Value);
            var total = hit + read;
            var cacheRatio = total > 0 ? (double)hit / total * 100 : 0;

            stats.AppendLine($"💾 Cache Hits: {hit:N0} | Disk Reads: {read:N0} | Hit Ratio: {cacheRatio:F1}%");

            if (cacheRatio < 90)
            {
                stats.AppendLine($"⚠️  LOW CACHE HIT RATIO - Query is disk I/O bound!");
            }
            else if (cacheRatio > 99)
            {
                stats.AppendLine($"✅ EXCELLENT - Query is fully cached in RAM");
            }
        }

        // Extract I/O timing
        var ioTimingMatch = Regex.Match(explainOutput, @"I/O Timings:\s*read=([\d.]+)", RegexOptions.IgnoreCase);
        if (ioTimingMatch.Success)
        {
            var ioTime = double.Parse(ioTimingMatch.Groups[1].Value);
            stats.AppendLine($"💿 I/O Wait Time: {ioTime:F2} ms");

            if (executionTimeMatch.Success)
            {
                var execTime = double.Parse(executionTimeMatch.Groups[1].Value);
                var ioPercent = (ioTime / execTime) * 100;
                stats.AppendLine($"📊 I/O % of Total: {ioPercent:F1}%");

                if (ioPercent > 50)
                {
                    stats.AppendLine($"⚠️  BOTTLENECK: Disk I/O is {ioPercent:F0}% of execution time!");
                }
            }
        }

        // Check for index scans
        var usesHnswIndex = false;
        var usesLocationIndex = false;

        if (explainOutput.Contains("Index Scan using", StringComparison.OrdinalIgnoreCase))
        {
            stats.AppendLine($"✅ Using index scan (good!)");

            if (explainOutput.Contains("hnsw", StringComparison.OrdinalIgnoreCase))
            {
                stats.AppendLine($"🔍 HNSW vector index being used");
                usesHnswIndex = true;
            }

            if (explainOutput.Contains("gist", StringComparison.OrdinalIgnoreCase) ||
                explainOutput.Contains("location", StringComparison.OrdinalIgnoreCase))
            {
                stats.AppendLine($"📍 Location GiST index being used");
                usesLocationIndex = true;
            }
        }
        else if (explainOutput.Contains("Seq Scan", StringComparison.OrdinalIgnoreCase))
        {
            stats.AppendLine($"⚠️  Sequential scan detected - may be slow on large tables");
        }

        // Check for nested loop joins (common with location queries)
        if (explainOutput.Contains("Nested Loop", StringComparison.OrdinalIgnoreCase))
        {
            stats.AppendLine($"🔄 Nested Loop join detected");

            // If it's a location query with nested loop, this could be slow
            if (explainOutput.Contains("location_centroids", StringComparison.OrdinalIgnoreCase))
            {
                stats.AppendLine($"💡 TIP: Location JOIN may be bottleneck if many centroids matched");
            }
        }

        // Check for hash joins (better for location queries with many matches)
        if (explainOutput.Contains("Hash Join", StringComparison.OrdinalIgnoreCase))
        {
            stats.AppendLine($"✅ Hash Join being used (efficient for large result sets)");
        }

        // Check for temp files
        if (explainOutput.Contains("temp written:", StringComparison.OrdinalIgnoreCase))
        {
            stats.AppendLine($"⚠️  TEMP FILES WRITTEN - Increase work_mem!");
        }

        txtStats.Text = stats.ToString();
    }

    private void btnClear_Click(object sender, EventArgs e)
    {
        txtQuery.Clear();
        txtResults.Clear();
        txtStats.Clear();
    }
}
