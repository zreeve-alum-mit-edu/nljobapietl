-- Create consolidated job_location and job_location_url records for a duplicate hash
-- This does NOT delete or modify existing jobs - only creates new normalized records
--
-- Usage: psql ... -v job_hash='your_hash_here' -f create_consolidated_records.sql

BEGIN;

-- Step 1: Find the keeper job (oldest job with this hash)
DROP TABLE IF EXISTS temp_keeper_job;
CREATE TEMP TABLE temp_keeper_job AS
SELECT id as keeper_job_id
FROM jobs
WHERE job_description_hash = :'job_hash'
ORDER BY date_inserted ASC
LIMIT 1;

-- Step 2: Get all unique location combinations for this hash
DROP TABLE IF EXISTS temp_unique_locations;
CREATE TEMP TABLE temp_unique_locations AS
SELECT DISTINCT
    generated_city,
    generated_state,
    generated_country,
    location,
    country,
    region,
    locality,
    postcode,
    latitude,
    longitude,
    llm_location_retry_count
FROM jobs
WHERE job_description_hash = :'job_hash';

-- Step 3: Create consolidated job_location records
INSERT INTO job_locations (
    id,
    job_id,
    location,
    country,
    region,
    locality,
    postcode,
    latitude,
    longitude,
    generated_city,
    generated_state,
    generated_country,
    llm_location_retry_count
)
SELECT
    gen_random_uuid(),
    (SELECT keeper_job_id FROM temp_keeper_job),
    location,
    country,
    region,
    locality,
    postcode,
    latitude,
    longitude,
    generated_city,
    generated_state,
    generated_country,
    llm_location_retry_count
FROM temp_unique_locations;

-- Step 4: Get all jobs with this hash and their URLs
DROP TABLE IF EXISTS temp_jobs_with_urls;
CREATE TEMP TABLE temp_jobs_with_urls AS
SELECT
    j.id as job_id,
    j.job_url,
    j.generated_city,
    j.generated_state,
    j.generated_country
FROM jobs j
WHERE j.job_description_hash = :'job_hash'
  AND j.job_url IS NOT NULL;

-- Step 5: Create job_location_url records, matching URLs to their locations
INSERT INTO job_location_urls (id, job_location_id, url)
SELECT DISTINCT
    gen_random_uuid(),
    jl.id as job_location_id,
    jwu.job_url as url
FROM temp_jobs_with_urls jwu
INNER JOIN job_locations jl ON
    jl.job_id = (SELECT keeper_job_id FROM temp_keeper_job)
    AND COALESCE(jwu.generated_city, '') = COALESCE(jl.generated_city, '')
    AND COALESCE(jwu.generated_state, '') = COALESCE(jl.generated_state, '')
    AND COALESCE(jwu.generated_country, '') = COALESCE(jl.generated_country, '');

-- Step 6: Delete all duplicate jobs EXCEPT the keeper
-- This cascades to delete old job_locations (if any existed), job_embeddings, centroid_assignments
DELETE FROM jobs
WHERE job_description_hash = :'job_hash'
  AND id != (SELECT keeper_job_id FROM temp_keeper_job);

-- Summary
SELECT
    'Consolidation Summary' as summary,
    (SELECT keeper_job_id FROM temp_keeper_job) as keeper_job_id,
    (SELECT COUNT(*) FROM jobs WHERE job_description_hash = :'job_hash') as remaining_jobs,
    (SELECT COUNT(*) FROM job_locations WHERE job_id = (SELECT keeper_job_id FROM temp_keeper_job)) as locations_created,
    (SELECT COUNT(*) FROM job_location_urls jlu
     INNER JOIN job_locations jl ON jl.id = jlu.job_location_id
     WHERE jl.job_id = (SELECT keeper_job_id FROM temp_keeper_job)) as urls_created;

COMMIT;
