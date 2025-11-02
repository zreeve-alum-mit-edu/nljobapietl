# NL Job Search API - Usage Instructions

## API Overview

The API has two endpoints:
1. **Location Validation** - Validate city/state and get suggestions if needed
2. **Job Search** - Search for jobs using natural language

## Authentication

All requests require an API key in the `x-api-key` header:
```
x-api-key: HHTnWCCgx2uCP7Ia3ZVB80SI6lviPPK0gR7eG8Ne
```

---

## Endpoint 1: Location Validation

**Purpose**: Validate that a city/state exists in our database before using it in a search. If the location is misspelled or doesn't exist, you'll get suggestions.

### Request Format
```
GET https://42r7s00kck.execute-api.us-east-2.amazonaws.com/locations/validate
```

### Query Parameters
- **city** (required): City name (e.g., "Austin", "New York City")
- **state** (required): Two-letter state code (e.g., "TX", "NY")
- **country** (optional): Country code (defaults to "US")

### Response Format

**Valid Location:**
```json
{
  "valid": true
}
```

**Invalid Location (with suggestions):**
```json
{
  "valid": false,
  "suggestions": [
    "Austin,TX",
    "Houston,TX",
    "Dallas,TX"
  ]
}
```

### Example 1: Validate Exact Match
```bash
curl -X GET "https://42r7s00kck.execute-api.us-east-2.amazonaws.com/locations/validate?city=Austin&state=TX" \
  -H "x-api-key: HHTnWCCgx2uCP7Ia3ZVB80SI6lviPPK0gR7eG8Ne"
```

**Response:**
```json
{
  "valid": true
}
```

### Example 2: Validate with Typo
```bash
curl -X GET "https://42r7s00kck.execute-api.us-east-2.amazonaws.com/locations/validate?city=Austinn&state=TX" \
  -H "x-api-key: HHTnWCCgx2uCP7Ia3ZVB80SI6lviPPK0gR7eG8Ne"
```

**Response:**
```json
{
  "valid": false,
  "suggestions": [
    "Austin,TX",
    "Justin,TX",
    "Gustine,TX"
  ]
}
```

### Example 3: Validate with Spaces
```bash
curl -X GET "https://42r7s00kck.execute-api.us-east-2.amazonaws.com/locations/validate?city=New%20York%20City&state=NY" \
  -H "x-api-key: HHTnWCCgx2uCP7Ia3ZVB80SI6lviPPK0gR7eG8Ne"
```

**Response:**
```json
{
  "valid": true
}
```

---

## Endpoint 2: Job Search

**Purpose**: Search for jobs using natural language and location filters. Use validated locations from the first endpoint to ensure accurate results.

### Request Format
```
POST https://42r7s00kck.execute-api.us-east-2.amazonaws.com/search
Content-Type: application/json
```

### Request Body
```json
{
  "prompt": "natural language job description",
  "numJobs": 10,
  "includeRemote": true,
  "daysSincePosting": 30,
  "filters": [
    {
      "includeOnsite": true,
      "includeHybrid": true,
      "location": "City,State",
      "miles": 50
    }
  ]
}
```

### Parameters

- **prompt** (required): Natural language description of the job
  - Example: "senior software engineer with Python experience"
  - Must be between 10 and 20,000 characters

- **numJobs** (required): Number of results (1-100)

- **includeRemote** (required): Boolean flag to include remote positions in results
  - Set to `true` to include remote jobs
  - Set to `false` to exclude remote jobs

- **daysSincePosting** (optional): Filter jobs posted within the last N days
  - Example: `30` for jobs posted in the last 30 days
  - Omit this parameter to search all jobs regardless of posting date

- **filters** (optional): Array of location-based workplace filters (0-10 filters)
  - **includeOnsite** (required): Boolean flag to include on-site positions at this location
  - **includeHybrid** (required): Boolean flag to include hybrid positions at this location
    - At least one of `includeOnsite` or `includeHybrid` must be `true`
  - **location** (required): City and state in format "City,State"
    - Use the exact format returned by the location validation endpoint
    - Example: "Austin,TX" or "New York City,NY"
  - **miles** (required): Search radius in miles (1-500)
    - Jobs within this distance from the location will be included

### Response Format
```json
{
  "jobs": [
    {
      "id": "uuid",
      "title": "Senior Software Engineer",
      "company": "Company Name",
      "description": "Job description...",
      "workplace": "REMOTE",
      "workplaceConfidence": "high",
      "location": "New York City,NY",
      "url": "https://...",
      "datePosted": "2025-01-15T10:30:00Z"
    }
  ],
  "totalCount": 10
}
```

---

## Complete Workflow Examples

### Workflow 1: Search for Remote Jobs (No Location Validation Needed)

**Step 1: Search directly for remote jobs**
```bash
curl -X POST https://42r7s00kck.execute-api.us-east-2.amazonaws.com/search \
  -H "Content-Type: application/json" \
  -H "x-api-key: HHTnWCCgx2uCP7Ia3ZVB80SI6lviPPK0gR7eG8Ne" \
  -d '{
    "prompt": "senior software engineer with Python and AWS experience",
    "numJobs": 10,
    "includeRemote": true,
    "filters": []
  }'
```

### Workflow 2: Search for On-site Jobs with Location Validation

**Step 1: Validate the location**
```bash
curl -X GET "https://42r7s00kck.execute-api.us-east-2.amazonaws.com/locations/validate?city=San%20Francisco&state=CA" \
  -H "x-api-key: HHTnWCCgx2uCP7Ia3ZVB80SI6lviPPK0gR7eG8Ne"
```

**Response:**
```json
{
  "valid": true
}
```

**Step 2: Use the validated location in search**
```bash
curl -X POST https://42r7s00kck.execute-api.us-east-2.amazonaws.com/search \
  -H "Content-Type: application/json" \
  -H "x-api-key: HHTnWCCgx2uCP7Ia3ZVB80SI6lviPPK0gR7eG8Ne" \
  -d '{
    "prompt": "data scientist machine learning",
    "numJobs": 20,
    "includeRemote": false,
    "filters": [
      {
        "includeOnsite": true,
        "includeHybrid": false,
        "location": "San Francisco,CA",
        "miles": 25
      }
    ]
  }'
```

### Workflow 3: Handle Invalid Location with Suggestions

**Step 1: Try to validate a misspelled location**
```bash
curl -X GET "https://42r7s00kck.execute-api.us-east-2.amazonaws.com/locations/validate?city=San%20Fran&state=CA" \
  -H "x-api-key: HHTnWCCgx2uCP7Ia3ZVB80SI6lviPPK0gR7eG8Ne"
```

**Response:**
```json
{
  "valid": false,
  "suggestions": [
    "San Francisco,CA",
    "San Juan,CA",
    "San Jose,CA"
  ]
}
```

**Step 2: Pick the correct suggestion and validate it**
```bash
curl -X GET "https://42r7s00kck.execute-api.us-east-2.amazonaws.com/locations/validate?city=San%20Francisco&state=CA" \
  -H "x-api-key: HHTnWCCgx2uCP7Ia3ZVB80SI6lviPPK0gR7eG8Ne"
```

**Response:**
```json
{
  "valid": true
}
```

**Step 3: Use the corrected location in search**
```bash
curl -X POST https://42r7s00kck.execute-api.us-east-2.amazonaws.com/search \
  -H "Content-Type: application/json" \
  -H "x-api-key: HHTnWCCgx2uCP7Ia3ZVB80SI6lviPPK0gR7eG8Ne" \
  -d '{
    "prompt": "frontend developer React TypeScript",
    "numJobs": 15,
    "includeRemote": false,
    "filters": [
      {
        "includeOnsite": false,
        "includeHybrid": true,
        "location": "San Francisco,CA",
        "miles": 30
      }
    ]
  }'
```

### Workflow 4: Search Multiple Locations (Remote OR Multiple Cities)

**Step 1: Validate all locations**
```bash
# Validate location 1
curl -X GET "https://42r7s00kck.execute-api.us-east-2.amazonaws.com/locations/validate?city=Austin&state=TX" \
  -H "x-api-key: HHTnWCCgx2uCP7Ia3ZVB80SI6lviPPK0gR7eG8Ne"

# Validate location 2
curl -X GET "https://42r7s00kck.execute-api.us-east-2.amazonaws.com/locations/validate?city=Seattle&state=WA" \
  -H "x-api-key: HHTnWCCgx2uCP7Ia3ZVB80SI6lviPPK0gR7eG8Ne"
```

**Step 2: Search with multiple filters (results will include jobs matching ANY filter)**
```bash
curl -X POST https://42r7s00kck.execute-api.us-east-2.amazonaws.com/search \
  -H "Content-Type: application/json" \
  -H "x-api-key: HHTnWCCgx2uCP7Ia3ZVB80SI6lviPPK0gR7eG8Ne" \
  -d '{
    "prompt": "product manager SaaS startup",
    "numJobs": 25,
    "includeRemote": true,
    "filters": [
      {
        "includeOnsite": false,
        "includeHybrid": true,
        "location": "Austin,TX",
        "miles": 40
      },
      {
        "includeOnsite": false,
        "includeHybrid": true,
        "location": "Seattle,WA",
        "miles": 35
      }
    ]
  }'
```

---

## Error Responses

### 400 Bad Request
**Missing required parameters:**
```json
{
  "message": "City and state parameters are required"
}
```

**Invalid numJobs:**
```json
{
  "message": "numJobs must be at least 1 (received: 0)"
}
```

**Invalid prompt length:**
```json
{
  "message": "Prompt must be at least 10 characters long (received: 5 characters)"
}
```

**Invalid filter configuration:**
```json
{
  "message": "Filter #1: At least one of 'includeOnsite' or 'includeHybrid' must be true. Current values: includeOnsite=false, includeHybrid=false"
}
```

**Invalid location:**
```json
{
  "message": "Location 'Austinn,TX' not found in database. Please use the /locations/validate endpoint to verify the location exists and get the correct format (e.g., GET /locations/validate?city=Austin&state=TX)"
}
```

### 403 Forbidden
**Missing or invalid API key:**
```json
{
  "message": "Forbidden: Invalid or missing API key"
}
```

### 404 Not Found
**Wrong endpoint:**
```json
{
  "message": "Not Found"
}
```

---

## Important Notes

1. **Location Format**: Always use the exact format returned by the location validation endpoint. Example: "New York City,NY" not "New York,NY"

2. **URL Encoding**: When using GET requests, remember to URL encode spaces and special characters:
   - Space = `%20`
   - Comma = `%2C`

3. **Remote Jobs**: To include remote positions, set `includeRemote: true`. You can combine this with location filters to get both remote jobs AND jobs near specific locations.

4. **Multiple Filters**: You can provide up to 10 location filters. The search uses OR logic - jobs matching ANY workplace condition will be returned (remote OR any location filter).

5. **Workplace Types**: Each location filter can include both on-site and hybrid positions. At least one of `includeOnsite` or `includeHybrid` must be true for each filter.

6. **Semantic Search**: The API uses AI-powered semantic search with vector embeddings, so results are ranked by semantic relevance to your natural language prompt.

7. **Date Filtering**: Use `daysSincePosting` to only get recent jobs. For example, `daysSincePosting: 7` will only return jobs posted in the last week.

8. **Country Parameter**: Currently only US locations are supported. The country parameter defaults to "US" and can be omitted.

---

## Testing Checklist

- [ ] Test location validation with exact match
- [ ] Test location validation with typo (verify suggestions)
- [ ] Test location validation with URL-encoded spaces
- [ ] Test remote-only job search (`includeRemote: true`, empty filters)
- [ ] Test on-site job search with validated location
- [ ] Test hybrid job search with validated location
- [ ] Test combined remote + location filters
- [ ] Test date filtering with `daysSincePosting`
- [ ] Test multiple location filters (2+ cities)
- [ ] Test error handling (missing API key)
- [ ] Test error handling (missing required parameters)
- [ ] Test error handling (invalid numJobs values)
- [ ] Test error handling (prompt too short or too long)
- [ ] Test error handling (filter with both includeOnsite and includeHybrid false)
- [ ] Test error handling (invalid location not in database)

---

## API Key

```
HHTnWCCgx2uCP7Ia3ZVB80SI6lviPPK0gR7eG8Ne
```

Include this in the `x-api-key` header for all requests.
