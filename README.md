# Natural Language Job Search API

AI-powered semantic job search across 1M+ US job postings. Query using natural language prompts and get intelligent, relevant results in milliseconds.

## Quick Start

```python
import requests

url = "https://natural-language-job-search.p.rapidapi.com/search/remote"

payload = {
    "prompt": "senior software engineer with Python experience",
    "numJobs": 10
}

headers = {
    "x-rapidapi-key": "YOUR_RAPIDAPI_KEY",
    "x-rapidapi-host": "natural-language-job-search.p.rapidapi.com",
    "Content-Type": "application/json"
}

response = requests.post(url, json=payload, headers=headers)
print(response.json())
```

## Features

- 🤖 **AI-Powered Semantic Search** - Understands intent, not just keywords
- 📍 **Location Filtering** - Search within radius of any US city
- 🌎 **Remote-First** - Dedicated fast endpoint for remote jobs
- 📊 **1M+ Job Postings** - Continuously updated database
- 🎯 **High Relevance** - Vector similarity ranking

## Authentication

All endpoints (except `/health`) require authentication via RapidAPI headers:

```javascript
headers: {
  'x-rapidapi-key': 'YOUR_RAPIDAPI_KEY',
  'x-rapidapi-host': 'natural-language-job-search.p.rapidapi.com'
}
```

## Endpoints

### POST /search/remote

Search remote-only jobs using natural language.

**Request Body:**
```json
{
  "prompt": "looking for a job in retail fashion",
  "numJobs": 15,
  "daysSincePosting": 30
}
```

**Parameters:**
- `prompt` (string, required): Natural language job description (10-20000 chars)
- `numJobs` (integer, required): Number of results to return (1-100)
- `daysSincePosting` (integer, optional): Filter jobs posted within last N days

**Response:**
```json
{
  "jobs": [
   {
      "id": "413de972-8b86-45ba-854e-c3a0dc67faed",
      "title": "Remote Construction Project Manager-West Coast",
      "company": "Central States Manufacturing",
      "description": "Full job description...",
      "workplace": "REMOTE",
      "workplaceConfidence": "EXPLICIT",
      "location": "Villa Rica,GA",
      "url": "https://www.careerjet.com/jobad/us0e458b9f7246a52cf1cd8cc291400fdd",
      "datePosted": "2025-09-28T00:00:00Z"
    }
  ],
  "totalCount": 100,
}
```

**Example:**
```javascript
const axios = require('axios');

const options = {
  method: 'POST',
  url: 'https://natural-language-job-search.p.rapidapi.com/search/remote',
  headers: {
    'content-type': 'application/json',
    'x-rapidapi-key': 'YOUR_RAPIDAPI_KEY',
    'x-rapidapi-host': 'natural-language-job-search.p.rapidapi.com'
  },
  data: {
    prompt: 'backend engineer with Node.js and PostgreSQL',
    numJobs: 10
  }
};

axios.request(options).then(response => {
  console.log(response.data);
}).catch(error => {
  console.error(error);
});
```

---

### POST /search

Search jobs within a radius of a US city.

**Request Body:**
```json
{
  "prompt": "registered nurse wanting to work with kids",
  "numJobs": 20,
  "city": "Chicago",
  "state": "IL",
  "miles": 15,
  "includeOnsite": true,
  "includeHybrid": true,
  "daysSincePosting": 30
}
```

**Parameters:**
- `prompt` (string, required): Natural language job description (10-20000 chars)
- `numJobs` (integer, required): Number of results (1-100)
- `city` (string, required): City name (case-insensitive)
- `state` (string, required): Two-letter state code (e.g., "TX", "CA")
- `miles` (integer, required): Search radius (1-20 miles)
- `includeOnsite` (boolean, required): Include on-site positions
- `includeHybrid` (boolean, required): Include hybrid positions
- `daysSincePosting` (integer, optional): Filter jobs by recency



**Example:**
```python
import requests

url = "https://natural-language-job-search.p.rapidapi.com/search"

payload = {
    "prompt": "senior software engineer with AWS experience",
    "numJobs": 10,
    "city": "Austin",
    "state": "TX",
    "miles": 20,
    "includeOnsite": True,
    "includeHybrid": True,
    "daysSincePosting": 30
}

headers = {
    "x-rapidapi-key": "YOUR_RAPIDAPI_KEY",
    "x-rapidapi-host": "natural-language-job-search.p.rapidapi.com",
    "Content-Type": "application/json"
}

response = requests.post(url, json=payload, headers=headers)
jobs = response.json()
```

---

### GET /locations/validate

Validate that a city/state combination exists in the database.

**Query Parameters:**
- `city` (string, required): City name
- `state` (string, required): Two-letter state code
- `country` (string, optional): Country code (defaults to "US")

**Example Request:**
```
GET /locations/validate?city=Austin&state=TX
```

**Response (Valid):**
```json
{
  "valid": true,
  "suggestions": null
}
```

**Response (Invalid):**
```json
{
 "valid": false,
  "suggestions": [
    "Saint Paul,MN",
    "West Saint Paul,MN",
    "Saint Paul Park,MN",
    "South Saint Paul,MN",
    "North Saint Paul,MN"
  ]
}
```

**Example:**
```bash
curl "https://natural-language-job-search.p.rapidapi.com/locations/validate?city=Seattle&state=WA" \
  -H "x-rapidapi-key: YOUR_RAPIDAPI_KEY" \
  -H "x-rapidapi-host: natural-language-job-search.p.rapidapi.com"
```

---

### GET /health

Check API and database health status. **No authentication required.**

**Response:**
```json
{
  "status": "healthy",
  "timestamp": "2025-11-10T06:52:30.952Z",
  "service": "NLJobSearch API",
  "version": "2.0",
  "database": "connected",
  "responseTime": "781.5ms"
}
```

**Example:**
```javascript
fetch('https://natural-language-job-search.p.rapidapi.com/health')
  .then(response => response.json())
  .then(data => console.log(data));
```

---

## Error Handling

**400 Bad Request** - Invalid parameters
```json
{
  "message": "Prompt must be at least 10 characters long (received: 5 characters)"
}
```

**403 Forbidden** - Invalid or missing API key
```json
{
  "message": "Forbidden: Invalid or missing API key"
}
```

**503 Service Unavailable** - OpenAI or database error
```json
{
  "message": "OpenAI embedding service is currently unavailable. Please try again later."
}
```

---

## Best Practices

### Optimize Your Prompts

✅ **Good:**
- "I'd like a remote job as a software engineer senior level or higher.  I have a personal interest in applied AI and want to work for a company on the forefront of technology."
- "registered nurse with pediatric ICU experience looking to make a difference"
- "Looking for general laborer positions"

❌ **Avoid:**
- "job" (too vague)
- "engineer engineer engineer" (keyword stuffing)
- Single words without context

### Location Searches

1. **Validate locations first** using `/locations/validate`
2. **Use reasonable radius** (10-20 miles for cities, 5-10 for dense areas)
3. **Cache location coordinates** to avoid repeated validation calls

### Remote Searches

- **Much faster** than location-based searches
- Use `/search/remote` when location doesn't matter
- No location parameters needed

### Rate Limits

- Respect your RapidAPI plan's rate limits
- Implement retry logic with exponential backoff
- Cache results when appropriate

---

## Code Examples

### Python (requests)

```python
import requests

def search_jobs(prompt, num_jobs=10):
    url = "https://natural-language-job-search.p.rapidapi.com/search/remote"

    headers = {
        "x-rapidapi-key": "YOUR_RAPIDAPI_KEY",
        "x-rapidapi-host": "natural-language-job-search.p.rapidapi.com",
        "Content-Type": "application/json"
    }

    payload = {
        "prompt": prompt,
        "numJobs": num_jobs
    }

    response = requests.post(url, json=payload, headers=headers)
    response.raise_for_status()
    return response.json()

# Usage
results = search_jobs("data scientist with NLP experience", 15)
print(f"Found {len(results['jobs'])} jobs")
```

### JavaScript (Node.js)

```javascript
const axios = require('axios');

async function searchJobs(prompt, numJobs = 10) {
  const options = {
    method: 'POST',
    url: 'https://natural-language-job-search.p.rapidapi.com/search/remote',
    headers: {
      'content-type': 'application/json',
      'x-rapidapi-key': 'YOUR_RAPIDAPI_KEY',
      'x-rapidapi-host': 'natural-language-job-search.p.rapidapi.com'
    },
    data: { prompt, numJobs }
  };

  try {
    const response = await axios.request(options);
    return response.data;
  } catch (error) {
    console.error(error);
    throw error;
  }
}

// Usage
searchJobs('backend engineer with Go experience', 20)
  .then(data => console.log(`Found ${data.jobs.length} jobs`));
```

### cURL

```bash
# Remote search
curl -X POST "https://natural-language-job-search.p.rapidapi.com/search/remote" \
  -H "x-rapidapi-key: YOUR_RAPIDAPI_KEY" \
  -H "x-rapidapi-host: natural-language-job-search.p.rapidapi.com" \
  -H "Content-Type: application/json" \
  -d '{
    "prompt": "DevOps engineer with Kubernetes experience",
    "numJobs": 10
  }'

# Location search
curl -X POST "https://natural-language-job-search.p.rapidapi.com/search" \
  -H "x-rapidapi-key: YOUR_RAPIDAPI_KEY" \
  -H "x-rapidapi-host: natural-language-job-search.p.rapidapi.com" \
  -H "Content-Type: application/json" \
  -d '{
    "prompt": "mechanical engineer with CAD experience",
    "numJobs": 15,
    "city": "Detroit",
    "state": "MI",
    "miles": 20,
    "includeOnsite": true,
    "includeHybrid": true
  }'
```

### PHP

```php
<?php

$curl = curl_init();

curl_setopt_array($curl, [
  CURLOPT_URL => "https://natural-language-job-search.p.rapidapi.com/search/remote",
  CURLOPT_RETURNTRANSFER => true,
  CURLOPT_POST => true,
  CURLOPT_POSTFIELDS => json_encode([
    "prompt" => "frontend developer with React experience",
    "numJobs" => 10
  ]),
  CURLOPT_HTTPHEADER => [
    "content-type: application/json",
    "x-rapidapi-key: YOUR_RAPIDAPI_KEY",
    "x-rapidapi-host: natural-language-job-search.p.rapidapi.com"
  ],
]);

$response = curl_exec($curl);
$err = curl_error($curl);

curl_close($curl);

if ($err) {
  echo "cURL Error: " . $err;
} else {
  $data = json_decode($response);
  echo "Found " . count($data->jobs) . " jobs\n";
}
```
---

## Support

- **Documentation**: This README and OpenAPI specification
- **Issues**: Report via RapidAPI platform
- **Updates**: API versioning ensures backward compatibility