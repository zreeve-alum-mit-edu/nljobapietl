# JobApi.Lambda.Api

API Gateway Lambda function for the Natural Language Job Search API.

## Structure

```
JobApi.Lambda.Api/
├── src/JobApi.Lambda.Api/
│   ├── Handlers/
│   │   ├── ApiGatewayHandler.cs    # Main entry point - routes requests
│   │   ├── SearchHandler.cs        # POST /search handler
│   │   └── LocationHandler.cs      # GET /locations/validate handler
│   ├── Models/
│   │   ├── SearchRequest.cs        # Request models for /search
│   │   ├── SearchResponse.cs       # Response models for /search
│   │   └── LocationResponse.cs     # Response models for /locations/validate
│   ├── JobApi.Lambda.Api.csproj
│   └── aws-lambda-tools-defaults.json
└── README.md
```

## Endpoints

### POST /search
Searches for jobs using natural language and location filters.

**Request:**
```json
{
  "prompt": "senior software engineer with Python",
  "limit": 10,
  "filters": [
    {
      "workplaceType": "remote"
    }
  ]
}
```

**Response:**
```json
{
  "jobs": [...],
  "totalCount": 10
}
```

### GET /locations/validate
Validates a city/state and provides suggestions if invalid.

**Query Parameters:**
- `city` (required)
- `state` (required)
- `country` (optional, defaults to "US")

**Response:**
```json
{
  "valid": true
}
```

or

```json
{
  "valid": false,
  "suggestions": ["Austin,TX", "Houston,TX"]
}
```

## Implementation Status

### ✅ Completed
- Project structure
- Model classes
- Handler stubs with TODO comments

### ⚠️ TODO - ApiGatewayHandler
- [ ] Deserialize and validate search requests
- [ ] Extract and validate location query parameters
- [ ] Error handling for 400 Bad Request
- [ ] Error handling for 500 Internal Server Error

### ⚠️ TODO - SearchHandler
- [ ] Create embedding from prompt using OpenAI API
- [ ] **CRITICAL**: Set `hnsw.ef_search = 500` before vector query
- [ ] Build WHERE clause from filters (OR logic)
- [ ] Parse location and get coordinates from geolocations table
- [ ] Execute vector similarity search
- [ ] Map database results to JobResult objects

### ⚠️ TODO - LocationHandler
- [ ] Check exact match in geolocations table
- [ ] Implement fuzzy matching for suggestions (Levenshtein distance)
- [ ] Return top 3 suggestions in "City,State" format

## Environment Variables

Set in `aws-lambda-tools-defaults.json`:
- `DB_HOST` - PostgreSQL host
- `DB_NAME` - Database name
- `DB_USER` - Database user
- `DB_PASSWORD` - Database password
- `HNSW_EF_SEARCH` - HNSW index search parameter (default: 500)
- `OPENAI_API_KEY` - OpenAI API key for embeddings

## Critical Notes

### HNSW ef_search Fix
The previous deployed version had a bug where `hnsw.ef_search` was not being set, causing vector searches to only return a few results. This MUST be set before executing vector queries:

```csharp
await using (var setCmd = new NpgsqlCommand($"SET hnsw.ef_search = {_hnswEfSearch}", connection))
{
    await setCmd.ExecuteNonQueryAsync();
}
```

See `JobApi.ETL/JobSearcher.cs:134-137` for the correct implementation pattern.

## Building and Deploying

```bash
cd JobApi.Lambda.Api/src/JobApi.Lambda.Api
dotnet publish -c Release
# Use AWS Lambda Tools to deploy
```

## Related Files

- API documentation: `/API-USAGE-INSTRUCTIONS.md`
- Reference implementation for HNSW: `/JobApi.ETL/JobSearcher.cs`
- Database context: `/JobApi.Common/JobContext.cs`
