-- Consolidate duplicate jobs for a given job_description_hash
-- This script consolidates all duplicate jobs into a single job with multiple locations
--
-- Usage: psql ... -v job_hash='your_hash_here' -f consolidate_duplicate_jobs.sql

-- Transaction to ensure atomicity
BEGIN;

-- Set the job description hash to consolidate (override with -v job_hash='...')
\set job_hash '34ac0710882a469e0f3bd7261bfd6176'

-- Step 1: Find and store the keeper job (oldest job with this hash)
DROP TABLE IF EXISTS temp_keeper_job;
CREATE TEMP TABLE temp_keeper_job AS
SELECT id as keeper_job_id
FROM jobs
WHERE job_description_hash = :'job_hash'
ORDER BY date_inserted ASC
LIMIT 1;

-- Step 2: Store all duplicate jobs for this hash
DROP TABLE IF EXISTS temp_duplicate_jobs;
CREATE TEMP TABLE temp_duplicate_jobs AS
SELECT
    j.id as old_job_id,
    j.job_url,
    j.generated_city,
    j.generated_state,
    j.generated_country,
    j.location,
    j.country,
    j.region,
    j.locality,
    j.postcode,
    j.latitude,
    j.longitude,
    j.llm_location_retry_count
FROM jobs j
WHERE j.job_description_hash = :'job_hash';

-- Step 3: Store old job_location IDs for later URL updates
DROP TABLE IF EXISTS temp_old_job_locations;
CREATE TEMP TABLE temp_old_job_locations AS
SELECT
    jl.id as old_location_id,
    jl.job_id as old_job_id
FROM job_locations jl
WHERE jl.job_id IN (SELECT old_job_id FROM temp_duplicate_jobs);

-- Step 4: Create consolidated job_location records for the keeper job
WITH unique_locations AS (
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
    FROM temp_duplicate_jobs
)
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
FROM unique_locations;

-- Store the newly created consolidated locations
DROP TABLE IF EXISTS temp_consolidated_locations;
CREATE TEMP TABLE temp_consolidated_locations AS
SELECT
    id as new_location_id,
    generated_city,
    generated_state,
    generated_country
FROM job_locations
WHERE job_id = (SELECT keeper_job_id FROM temp_keeper_job);

-- Step 5: Update job_location_urls to point to consolidated locations
UPDATE job_location_urls jlu
SET job_location_id = cl.new_location_id
FROM temp_old_job_locations ojl
INNER JOIN temp_duplicate_jobs dj ON dj.old_job_id = ojl.old_job_id
INNER JOIN temp_consolidated_locations cl ON
    COALESCE(dj.generated_city, '') = COALESCE(cl.generated_city, '') AND
    COALESCE(dj.generated_state, '') = COALESCE(cl.generated_state, '') AND
    COALESCE(dj.generated_country, '') = COALESCE(cl.generated_country, '');

-- Step 6: Delete duplicate jobs (CASCADE will delete old locations, embeddings, centroid_assignments)
DELETE FROM jobs
WHERE job_description_hash = :'job_hash'
  AND id != (SELECT keeper_job_id FROM temp_keeper_job);

-- Final verification
SELECT
    'Consolidation Summary' as summary,
    (SELECT COUNT(*) FROM jobs WHERE job_description_hash = :'job_hash') as remaining_jobs,
    (SELECT COUNT(*) FROM job_locations WHERE job_id = (SELECT keeper_job_id FROM temp_keeper_job)) as consolidated_locations,
    (SELECT COUNT(*) FROM job_location_urls jlu
     INNER JOIN job_locations jl ON jl.id = jlu.job_location_id
     WHERE jl.job_id = (SELECT keeper_job_id FROM temp_keeper_job)) as total_urls;

-- Commit the transaction
COMMIT;
