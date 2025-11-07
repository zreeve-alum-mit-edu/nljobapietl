# NL Job Search API - Usage Instructions

## API Overview

The API has three endpoints:
1. **Location Validation** - Validate city/state and get suggestions if needed
2. **Job Search** - Search for on-site/hybrid jobs in a specific location
3. **Remote Job Search** - Search for remote-only jobs

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

## Endpoint 2: Job Search (On-site/Hybrid)

**Purpose**: Search for on-site or hybrid jobs in a specific location using natural language. Use validated locations from the first endpoint to ensure accurate results.

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
  "city": "Austin",
  "state": "TX",
  "miles": 20,
  "includeOnsite": true,
  "includeHybrid": true,
  "daysSincePosting": 30
}
```

### Parameters

- **prompt** (required): Natural language description of the job
  - Example: "senior software engineer with Python experience"
  - Must be between 10 and 20,000 characters

- **numJobs** (required): Number of results (1-100)

- **city** (required): City name (e.g., "Austin", "New York City")
  - Use the exact city name returned by the location validation endpoint

- **state** (required): Two-letter state code (e.g., "TX", "NY")

- **miles** (required): Search radius in miles (1-20)
  - Jobs within this distance from the location will be included

- **includeOnsite** (required): Boolean flag to include on-site positions
  - Set to `true` to include on-site jobs
  - At least one of `includeOnsite` or `includeHybrid` must be `true`

- **includeHybrid** (required): Boolean flag to include hybrid positions
  - Set to `true` to include hybrid jobs
  - At least one of `includeOnsite` or `includeHybrid` must be `true`

- **daysSincePosting** (optional): Filter jobs posted within the last N days
  - Example: `30` for jobs posted in the last 30 days
  - Omit this parameter to search all jobs regardless of posting date

### Response Format
```json
{
  "jobs": [
    {
      "id": "uuid",
      "title": "Senior Software Engineer",
      "company": "Company Name",
      "description": "Job description...",
      "workplace": "ONSITE",
      "workplaceConfidence": "high",
      "location": "Austin,TX",
      "url": "https://...",
      "datePosted": "2025-01-15T10:30:00Z"
    }
  ],
  "totalCount": 10
}
```

---

## Endpoint 3: Remote Job Search

**Purpose**: Search for remote-only jobs using natural language. No location validation needed.

### Request Format
```
POST https://42r7s00kck.execute-api.us-east-2.amazonaws.com/search/remote
Content-Type: application/json
```

### Request Body
```json
{
  "prompt": "natural language job description",
  "numJobs": 10,
  "daysSincePosting": 30
}
```

### Parameters

- **prompt** (required): Natural language description of the job
  - Example: "senior software engineer with Python experience"
  - Must be between 10 and 20,000 characters

- **numJobs** (required): Number of results (1-100)

- **daysSincePosting** (optional): Filter jobs posted within the last N days
  - Example: `30` for jobs posted in the last 30 days
  - Omit this parameter to search all jobs regardless of posting date

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
      "location": null,
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

**Step 1: Search directly for remote jobs using /search/remote endpoint**
```bash
curl -X POST https://42r7s00kck.execute-api.us-east-2.amazonaws.com/search/remote \
  -H "Content-Type: application/json" \
  -H "x-api-key: HHTnWCCgx2uCP7Ia3ZVB80SI6lviPPK0gR7eG8Ne" \
  -d '{
    "prompt": "senior software engineer with Python and AWS experience",
    "numJobs": 10,
    "daysSincePosting": 30
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
    "city": "San Francisco",
    "state": "CA",
    "miles": 15,
    "includeOnsite": true,
    "includeHybrid": false,
    "daysSincePosting": 30
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
    "city": "San Francisco",
    "state": "CA",
    "miles": 20,
    "includeOnsite": false,
    "includeHybrid": true
  }'
```

### Workflow 4: Search for Hybrid Jobs in Specific Location

**Step 1: Validate the location**
```bash
curl -X GET "https://42r7s00kck.execute-api.us-east-2.amazonaws.com/locations/validate?city=Austin&state=TX" \
  -H "x-api-key: HHTnWCCgx2uCP7Ia3ZVB80SI6lviPPK0gR7eG8Ne"
```

**Step 2: Search for hybrid positions within radius**
```bash
curl -X POST https://42r7s00kck.execute-api.us-east-2.amazonaws.com/search \
  -H "Content-Type: application/json" \
  -H "x-api-key: HHTnWCCgx2uCP7Ia3ZVB80SI6lviPPK0gR7eG8Ne" \
  -d '{
    "prompt": "product manager SaaS startup",
    "numJobs": 25,
    "city": "Austin",
    "state": "TX",
    "miles": 15,
    "includeOnsite": true,
    "includeHybrid": true,
    "daysSincePosting": 14
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

**Invalid workplace configuration:**
```json
{
  "message": "At least one of 'includeOnsite' or 'includeHybrid' must be true. Current values: includeOnsite=false, includeHybrid=false"
}
```

**Missing required parameter (city):**
```json
{
  "message": "City is required (e.g., 'Austin')"
}
```

**Missing required parameter (state):**
```json
{
  "message": "State is required (e.g., 'TX')"
}
```

**Invalid location:**
```json
{
  "message": "Location 'Austinn, TX' not found in database. Please use the /locations/validate endpoint to verify the location exists and get the correct format (e.g., GET /locations/validate?city=Austin&state=TX)"
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

1. **Two Separate Endpoints**:
   - Use `/search` for location-based searches (on-site and/or hybrid jobs)
   - Use `/search/remote` for remote-only searches (no location parameters needed)

2. **Location Format**: Always use the exact city and state names returned by the location validation endpoint.
   - Example: City="San Francisco", State="CA" (not "San Fran" or "California")

3. **URL Encoding**: When using GET requests for location validation, remember to URL encode spaces:
   - Space = `%20`
   - Example: "San Francisco" becomes "San%20Francisco"

4. **Search Radius Limit**: The /search endpoint has a maximum radius of 20 miles. This is intentionally limited to ensure focused local searches.

5. **Workplace Types**: For the /search endpoint, at least one of `includeOnsite` or `includeHybrid` must be `true`. Set both to `true` to get all non-remote jobs in the area.

6. **Semantic Search**: The API uses AI-powered semantic search with vector embeddings, so results are ranked by semantic relevance to your natural language prompt.

7. **Date Filtering**: Use `daysSincePosting` to only get recent jobs. For example, `daysSincePosting: 7` will only return jobs posted in the last week.

8. **Country Parameter**: Currently only US locations are supported. The country parameter defaults to "US" and can be omitted from validation requests.

---

## Testing Checklist

### Location Validation Tests
- [ ] Test location validation with exact match
- [ ] Test location validation with typo (verify suggestions)
- [ ] Test location validation with URL-encoded spaces

### Remote Job Search Tests (/search/remote)
- [ ] Test remote-only job search with basic parameters
- [ ] Test remote search with date filtering (`daysSincePosting`)
- [ ] Test error handling (missing prompt)
- [ ] Test error handling (invalid numJobs values)

### Location-Based Job Search Tests (/search)
- [ ] Test on-site only job search with validated location
- [ ] Test hybrid only job search with validated location
- [ ] Test both on-site and hybrid job search
- [ ] Test date filtering with `daysSincePosting`
- [ ] Test various radius values (1-20 miles)
- [ ] Test error handling (missing city)
- [ ] Test error handling (missing state)
- [ ] Test error handling (missing miles)
- [ ] Test error handling (both includeOnsite and includeHybrid false)
- [ ] Test error handling (invalid location not in database)
- [ ] Test error handling (miles > 20 or miles < 1)

### General API Tests
- [ ] Test error handling (missing API key)
- [ ] Test error handling (invalid API key)
- [ ] Test error handling (prompt too short - less than 10 chars)
- [ ] Test error handling (prompt too long - more than 20,000 chars)
- [ ] Test error handling (numJobs < 1 or > 100)

---

## API Key

```
HHTnWCCgx2uCP7Ia3ZVB80SI6lviPPK0gR7eG8Ne
```

Include this in the `x-api-key` header for all requests.
