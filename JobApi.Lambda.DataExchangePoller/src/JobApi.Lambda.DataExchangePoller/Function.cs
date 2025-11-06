using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.Core;
using Amazon.S3;
using Amazon.S3.Model;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace JobApi.Lambda.DataExchangePoller;

public class Function
{
    private readonly IAmazonS3 _s3Client;
    private readonly IAmazonDynamoDB _dynamoDbClient;
    private readonly string _sourceAccessPointAlias;
    private readonly string _sourcePrefix;
    private readonly string _destinationBucket;
    private readonly string _destinationPrefix;
    private readonly string _trackingTable;

    // CUTOFF: Only download files modified after this date (Nov 6, 2025 02:30 UTC)
    private static readonly DateTime CUTOFF_DATE = new DateTime(2025, 11, 6, 2, 30, 0, DateTimeKind.Utc);

    public Function()
    {
        // Data Exchange Access Point from environment variables
        _sourceAccessPointAlias = Environment.GetEnvironmentVariable("SOURCE_ACCESS_POINT_ALIAS")
            ?? "48043042-44d9-4fcb-a-x8z39z7em3f4rhj3nwd9n1tytpktweuc1b-s3alias";
        _sourcePrefix = Environment.GetEnvironmentVariable("SOURCE_PREFIX") ?? "us/";

        // Destination bucket (your intake folder)
        _destinationBucket = Environment.GetEnvironmentVariable("DESTINATION_BUCKET")
            ?? "circuitdreams-nl-jobsearch-api";
        _destinationPrefix = Environment.GetEnvironmentVariable("DESTINATION_PREFIX")
            ?? "ingestable/intake/";

        // DynamoDB tracking table
        _trackingTable = Environment.GetEnvironmentVariable("TRACKING_TABLE")
            ?? "JobApi-DataExchangeFiles";

        // S3 client configured for Data Exchange Access Point (in eu-central-1)
        // Must use the Access Point's region for Requester Pays buckets
        var s3Config = new Amazon.S3.AmazonS3Config
        {
            RegionEndpoint = Amazon.RegionEndpoint.EUCentral1,  // Access Point region
            UseArnRegion = true  // Allows cross-region access via ARN/Access Point
        };
        _s3Client = new AmazonS3Client(s3Config);
        _dynamoDbClient = new AmazonDynamoDBClient();
    }

    public Function(IAmazonS3 s3Client, IAmazonDynamoDB dynamoDbClient)
    {
        _s3Client = s3Client;
        _dynamoDbClient = dynamoDbClient;
        _sourceAccessPointAlias = "48043042-44d9-4fcb-a-x8z39z7em3f4rhj3nwd9n1tytpktweuc1b-s3alias";
        _sourcePrefix = "us/";
        _destinationBucket = "circuitdreams-nl-jobsearch-api";
        _destinationPrefix = "ingestable/intake/";
        _trackingTable = "JobApi-DataExchangeFiles";
    }

    public async Task FunctionHandler(ILambdaContext context)
    {
        context.Logger.LogInformation("=== Data Exchange Poller Started ===");
        context.Logger.LogInformation($"Source: {_sourceAccessPointAlias}/{_sourcePrefix}");
        context.Logger.LogInformation($"Destination: s3://{_destinationBucket}/{_destinationPrefix}");

        var newFilesCount = 0;
        var updatedFilesCount = 0;
        var unchangedFilesCount = 0;

        try
        {
            // List files in the Data Exchange access point
            // IMPORTANT: Must set RequestPayer for Data Exchange Requester-Pays buckets
            var listRequest = new ListObjectsV2Request
            {
                BucketName = _sourceAccessPointAlias,
                Prefix = _sourcePrefix,
                RequestPayer = RequestPayer.Requester
            };

            ListObjectsV2Response listResponse;
            do
            {
                listResponse = await _s3Client.ListObjectsV2Async(listRequest);

                foreach (var s3Object in listResponse.S3Objects)
                {
                    // Skip directories
                    if (s3Object.Key.EndsWith("/"))
                        continue;

                    // Skip files older than the cutoff date
                    if (s3Object.LastModified < CUTOFF_DATE)
                    {
                        context.Logger.LogInformation($"SKIPPING old file: {s3Object.Key} (LastModified: {s3Object.LastModified} is before cutoff {CUTOFF_DATE})");
                        continue;
                    }

                    context.Logger.LogInformation($"Checking file: {s3Object.Key} (ETag: {s3Object.ETag}, LastModified: {s3Object.LastModified})");

                    // Check if file is new or updated
                    var trackedFile = await GetTrackedFile(s3Object.Key);

                    if (trackedFile == null)
                    {
                        // New file - download it
                        context.Logger.LogInformation($"NEW file detected: {s3Object.Key}");
                        await DownloadAndCopyFile(s3Object.Key, s3Object.ETag, s3Object.LastModified, context);
                        newFilesCount++;
                    }
                    else if (trackedFile.ETag != s3Object.ETag)
                    {
                        // File has been updated
                        context.Logger.LogInformation($"UPDATED file detected: {s3Object.Key} (old ETag: {trackedFile.ETag}, new ETag: {s3Object.ETag})");
                        await DownloadAndCopyFile(s3Object.Key, s3Object.ETag, s3Object.LastModified, context);
                        updatedFilesCount++;
                    }
                    else
                    {
                        context.Logger.LogInformation($"File unchanged: {s3Object.Key}");
                        unchangedFilesCount++;
                    }
                }

                listRequest.ContinuationToken = listResponse.NextContinuationToken;
            } while (listResponse.IsTruncated);

            context.Logger.LogInformation($"\n=== Polling Summary ===");
            context.Logger.LogInformation($"New files: {newFilesCount}");
            context.Logger.LogInformation($"Updated files: {updatedFilesCount}");
            context.Logger.LogInformation($"Unchanged files: {unchangedFilesCount}");
            context.Logger.LogInformation($"Total files checked: {newFilesCount + updatedFilesCount + unchangedFilesCount}");
        }
        catch (Exception ex)
        {
            context.Logger.LogError($"Error polling Data Exchange: {ex.Message}");
            context.Logger.LogError($"Full exception: {ex}");
            throw;
        }

        context.Logger.LogInformation("=== Data Exchange Poller Complete ===");
    }

    private async Task<TrackedFile?> GetTrackedFile(string s3Key)
    {
        try
        {
            var response = await _dynamoDbClient.GetItemAsync(new GetItemRequest
            {
                TableName = _trackingTable,
                Key = new Dictionary<string, AttributeValue>
                {
                    { "S3Key", new AttributeValue { S = s3Key } }
                }
            });

            if (response.Item == null || !response.Item.ContainsKey("S3Key"))
                return null;

            return new TrackedFile
            {
                S3Key = response.Item["S3Key"].S,
                ETag = response.Item.ContainsKey("ETag") ? response.Item["ETag"].S : "",
                LastModified = response.Item.ContainsKey("LastModified")
                    ? DateTime.Parse(response.Item["LastModified"].S)
                    : DateTime.MinValue
            };
        }
        catch (Exception)
        {
            return null;
        }
    }

    private async Task UpdateTrackedFile(string s3Key, string etag, DateTime lastModified, bool downloaded = false)
    {
        var item = new Dictionary<string, AttributeValue>
        {
            { "S3Key", new AttributeValue { S = s3Key } },
            { "ETag", new AttributeValue { S = etag } },
            { "LastModified", new AttributeValue { S = lastModified.ToString("O") } },
            { "LastChecked", new AttributeValue { S = DateTime.UtcNow.ToString("O") } }
        };

        if (downloaded)
        {
            item.Add("DownloadedAt", new AttributeValue { S = DateTime.UtcNow.ToString("O") });
        }

        await _dynamoDbClient.PutItemAsync(new PutItemRequest
        {
            TableName = _trackingTable,
            Item = item
        });
    }

    private async Task DownloadAndCopyFile(string sourceKey, string etag, DateTime lastModified, ILambdaContext context)
    {
        try
        {
            // Extract filename from key and add timestamp for uniqueness
            var fileName = Path.GetFileName(sourceKey);
            var fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
            var extension = Path.GetExtension(fileName);
            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
            var uniqueFileName = $"{fileNameWithoutExt}_{timestamp}{extension}";
            var destinationKey = $"{_destinationPrefix}{uniqueFileName}";

            context.Logger.LogInformation($"Downloading {sourceKey} → s3://{_destinationBucket}/{destinationKey}");

            // Download from Data Exchange to /tmp, then re-upload to your bucket
            // CopyObject doesn't work across regions/access points, so we download and re-upload
            // IMPORTANT: Must set RequestPayer for Data Exchange Requester-Pays buckets
            var tempFile = Path.Combine("/tmp", Path.GetFileName(sourceKey));

            var getRequest = new GetObjectRequest
            {
                BucketName = _sourceAccessPointAlias,
                Key = sourceKey,
                RequestPayer = RequestPayer.Requester
            };

            // Download to /tmp
            using (var getResponse = await _s3Client.GetObjectAsync(getRequest))
            await getResponse.WriteResponseStreamToFileAsync(tempFile, false, CancellationToken.None);

            context.Logger.LogInformation($"Downloaded file to /tmp, size: {new FileInfo(tempFile).Length} bytes");

            // Upload to destination bucket
            var destS3Config = new Amazon.S3.AmazonS3Config
            {
                RegionEndpoint = Amazon.RegionEndpoint.USEast2
            };
            using (var destS3Client = new AmazonS3Client(destS3Config))
            {
                var putRequest = new PutObjectRequest
                {
                    BucketName = _destinationBucket,
                    Key = destinationKey,
                    FilePath = tempFile
                };

                await destS3Client.PutObjectAsync(putRequest);
            }

            // Clean up temp file
            File.Delete(tempFile);

            context.Logger.LogInformation($"Successfully uploaded file to intake folder");

            // Update tracking record with download timestamp
            await UpdateTrackedFile(sourceKey, etag, lastModified, downloaded: true);
            context.Logger.LogInformation($"Updated tracking record for {sourceKey} (downloaded at {DateTime.UtcNow:O})");
        }
        catch (Exception ex)
        {
            context.Logger.LogError($"Failed to copy file {sourceKey}: {ex.Message}");
            throw;
        }
    }

    private class TrackedFile
    {
        public string S3Key { get; set; } = string.Empty;
        public string ETag { get; set; } = string.Empty;
        public DateTime LastModified { get; set; }
    }
}
