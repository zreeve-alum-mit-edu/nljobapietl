-- Migrate remaining jobs that don't have location records yet
-- This handles all jobs that weren't part of the duplicate consolidation

BEGIN;

-- Step 1: Find all jobs without location records
DROP TABLE IF EXISTS temp_jobs_without_locations;
CREATE TEMP TABLE temp_jobs_without_locations AS
SELECT j.id as job_id
FROM jobs j
LEFT JOIN job_locations jl ON j.id = jl.job_id
WHERE jl.id IS NULL;

CREATE INDEX idx_temp_jobs_without_locations ON temp_jobs_without_locations(job_id);

SELECT 'Step 1 Complete: Found ' || COUNT(*) || ' jobs without locations' FROM temp_jobs_without_locations;

-- Step 2: Create job_location records for all remaining jobs
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
    j.id,
    j.location,
    j.country,
    j.region,
    j.locality,
    j.postcode,
    j.latitude,
    j.longitude,
    j.generated_city,
    j.generated_state,
    j.generated_country,
    j.llm_location_retry_count
FROM jobs j
INNER JOIN temp_jobs_without_locations t ON j.id = t.job_id;

SELECT 'Step 2 Complete: Created ' || COUNT(*) || ' location records'
FROM job_locations jl
INNER JOIN temp_jobs_without_locations t ON jl.job_id = t.job_id;

-- Step 3: Create job_location_url records for jobs with URLs
INSERT INTO job_location_urls (id, job_location_id, url)
SELECT DISTINCT
    gen_random_uuid(),
    jl.id,
    j.job_url
FROM jobs j
INNER JOIN temp_jobs_without_locations t ON j.id = t.job_id
INNER JOIN job_locations jl ON jl.job_id = j.id
WHERE j.job_url IS NOT NULL;

SELECT 'Step 3 Complete: Created ' || COUNT(*) || ' URL records'
FROM job_location_urls jlu
INNER JOIN job_locations jl ON jlu.job_location_id = jl.id
INNER JOIN temp_jobs_without_locations t ON jl.job_id = t.job_id;

-- Final summary
SELECT
    'FINAL SUMMARY' as status,
    (SELECT COUNT(*) FROM jobs) as total_jobs,
    (SELECT COUNT(*) FROM job_locations) as total_locations,
    (SELECT COUNT(*) FROM job_location_urls) as total_urls,
    (SELECT COUNT(*) FROM jobs j LEFT JOIN job_locations jl ON j.id = jl.job_id WHERE jl.id IS NULL) as jobs_without_locations;

COMMIT;
