-- Drop old location and URL columns from jobs table
-- These have been migrated to job_locations and job_location_urls tables
-- KEEP job_description_hash for duplicate prevention

BEGIN;

-- Drop location-related columns
ALTER TABLE jobs
    DROP COLUMN IF EXISTS location,
    DROP COLUMN IF EXISTS country,
    DROP COLUMN IF EXISTS region,
    DROP COLUMN IF EXISTS locality,
    DROP COLUMN IF EXISTS postcode,
    DROP COLUMN IF EXISTS latitude,
    DROP COLUMN IF EXISTS longitude,
    DROP COLUMN IF EXISTS generated_city,
    DROP COLUMN IF EXISTS generated_state,
    DROP COLUMN IF EXISTS generated_country,
    DROP COLUMN IF EXISTS llm_location_retry_count,
    DROP COLUMN IF EXISTS job_url;

SELECT 'Columns dropped successfully' as status;

-- Verify remaining columns
SELECT column_name, data_type
FROM information_schema.columns
WHERE table_name = 'jobs'
ORDER BY ordinal_position;

COMMIT;
