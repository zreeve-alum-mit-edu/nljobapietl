-- Drop old location and URL columns from jobs table (FAST version)
-- Disables triggers to avoid slow table rewrite

BEGIN;

-- Disable UPDATE triggers to speed up the operation
ALTER TABLE jobs DISABLE TRIGGER trigger_update_gistlocation;
ALTER TABLE jobs DISABLE TRIGGER trigger_sync_workplace_to_embedding;

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

-- Re-enable triggers
ALTER TABLE jobs ENABLE TRIGGER trigger_update_gistlocation;
ALTER TABLE jobs ENABLE TRIGGER trigger_sync_workplace_to_embedding;

SELECT 'Triggers re-enabled' as status;

-- Verify remaining columns
SELECT column_name, data_type
FROM information_schema.columns
WHERE table_name = 'jobs'
ORDER BY ordinal_position;

COMMIT;
