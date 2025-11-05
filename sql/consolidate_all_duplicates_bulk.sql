-- Consolidate ALL duplicate jobs at once
-- This runs in a single transaction for maximum performance

BEGIN;

-- Step 1: Find ALL keeper jobs (oldest job per hash)
DROP TABLE IF EXISTS temp_all_keeper_jobs;
CREATE TEMP TABLE temp_all_keeper_jobs AS
SELECT DISTINCT ON (job_description_hash)
    job_description_hash,
    id as keeper_job_id
FROM jobs
WHERE job_description_hash IN (
    SELECT job_description_hash
    FROM jobs
    WHERE job_description_hash IS NOT NULL
    GROUP BY job_description_hash
    HAVING COUNT(*) > 1
)
ORDER BY job_description_hash, date_inserted ASC;

CREATE INDEX idx_keeper_hash ON temp_all_keeper_jobs(job_description_hash);
CREATE INDEX idx_keeper_id ON temp_all_keeper_jobs(keeper_job_id);

SELECT 'Step 1 Complete: Found ' || COUNT(*) || ' keeper jobs' FROM temp_all_keeper_jobs;

-- Step 2: Create all location records for all keeper jobs
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
SELECT DISTINCT
    gen_random_uuid(),
    k.keeper_job_id,
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
INNER JOIN temp_all_keeper_jobs k ON j.job_description_hash = k.job_description_hash;

SELECT 'Step 2 Complete: Created ' || COUNT(*) || ' location records' FROM job_locations WHERE job_id IN (SELECT keeper_job_id FROM temp_all_keeper_jobs);

-- Step 3: Create all URL records for all locations
INSERT INTO job_location_urls (id, job_location_id, url)
SELECT DISTINCT
    gen_random_uuid(),
    jl.id,
    j.job_url
FROM jobs j
INNER JOIN temp_all_keeper_jobs k ON j.job_description_hash = k.job_description_hash
INNER JOIN job_locations jl ON
    jl.job_id = k.keeper_job_id
    AND COALESCE(j.generated_city, '') = COALESCE(jl.generated_city, '')
    AND COALESCE(j.generated_state, '') = COALESCE(jl.generated_state, '')
    AND COALESCE(j.generated_country, '') = COALESCE(jl.generated_country, '')
WHERE j.job_url IS NOT NULL;

SELECT 'Step 3 Complete: Created ' || COUNT(*) || ' URL records' FROM job_location_urls WHERE job_location_id IN (SELECT id FROM job_locations WHERE job_id IN (SELECT keeper_job_id FROM temp_all_keeper_jobs));

-- Step 4: Delete ALL duplicate jobs (keeping only keepers)
DELETE FROM jobs
WHERE job_description_hash IN (SELECT job_description_hash FROM temp_all_keeper_jobs)
  AND id NOT IN (SELECT keeper_job_id FROM temp_all_keeper_jobs);

SELECT 'Step 4 Complete: Deleted duplicate jobs';

-- Final summary
SELECT
    'FINAL SUMMARY' as status,
    (SELECT COUNT(*) FROM temp_all_keeper_jobs) as keeper_jobs,
    (SELECT COUNT(*) FROM jobs WHERE job_description_hash IN (SELECT job_description_hash FROM temp_all_keeper_jobs)) as remaining_jobs,
    (SELECT COUNT(*) FROM job_locations WHERE job_id IN (SELECT keeper_job_id FROM temp_all_keeper_jobs)) as total_locations,
    (SELECT COUNT(*) FROM job_location_urls WHERE job_location_id IN (SELECT id FROM job_locations WHERE job_id IN (SELECT keeper_job_id FROM temp_all_keeper_jobs))) as total_urls;

COMMIT;
