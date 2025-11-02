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
  "limit": 10,
  "filters": [
    {
      "workplaceType": "remote|hybrid|onsite",
      "location": "City,State",
      "miles": 50
    }
  ]
}
```

### Parameters

- **prompt** (required): Natural language description of the job
  - Example: "senior software engineer with Python experience"

- **limit** (required): Number of results (1-100)

- **filters** (required): Array of workplace filters (1-5 filters)
  - **workplaceType**: One of:
    - `"remote"` - Remote positions (no location/miles needed)
    - `"hybrid"` - Hybrid positions (requires location and miles)
    - `"onsite"` - On-site positions (requires location and miles)
  - **location**: City and state in format "City,State" (required for hybrid/onsite)
    - Use the exact format returned by the location validation endpoint
  - **miles**: Search radius in miles (required for hybrid/onsite)

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
      "location": "New York City,NY",
      "salary": "$120k - $180k",
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
    "limit": 10,
    "filters": [
      {
        "workplaceType": "remote"
      }
    ]
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
    "limit": 20,
    "filters": [
      {
        "workplaceType": "onsite",
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
    "limit": 15,
    "filters": [
      {
        "workplaceType": "hybrid",
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
    "limit": 25,
    "filters": [
      {
        "workplaceType": "remote"
      },
      {
        "workplaceType": "hybrid",
        "location": "Austin,TX",
        "miles": 40
      },
      {
        "workplaceType": "hybrid",
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

**Invalid limit:**
```json
{
  "message": "Limit must be between 1 and 100"
}
```

**Invalid filters:**
```json
{
  "message": "Must provide between 1 and 5 filters"
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

3. **Remote Jobs**: For remote positions, you only need `workplaceType: "remote"`. Do not include location or miles.

4. **Multiple Filters**: You can combine up to 5 filters. The search uses OR logic - jobs matching ANY filter will be returned.

5. **Semantic Search**: The API uses AI-powered semantic search, so results are ranked by relevance to your natural language prompt.

6. **Country Parameter**: Currently only US locations are supported. The country parameter defaults to "US" and can be omitted.

---

## Testing Checklist

- [ ] Test location validation with exact match
- [ ] Test location validation with typo (verify suggestions)
- [ ] Test location validation with URL-encoded spaces
- [ ] Test remote job search (no location needed)
- [ ] Test on-site job search with validated location
- [ ] Test hybrid job search with validated location
- [ ] Test search with multiple filters (remote + multiple locations)
- [ ] Test error handling (missing API key)
- [ ] Test error handling (missing required parameters)
- [ ] Test error handling (invalid limit values)

---

## API Key

```
HHTnWCCgx2uCP7Ia3ZVB80SI6lviPPK0gR7eG8Ne
```

Include this in the `x-api-key` header for all requests.
