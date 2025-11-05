-- Drop old location and URL columns from jobs table
-- Also drops triggers that depend on these columns

BEGIN;

-- Drop triggers that depend on location columns
DROP TRIGGER IF EXISTS trigger_update_gistlocation ON jobs;
DROP TRIGGER IF EXISTS trigger_sync_workplace_to_embedding ON jobs;

-- Drop location-related columns
ALTER TABLE jobs
    DROP COLUMN IF EXISTS location CASCADE,
    DROP COLUMN IF EXISTS country CASCADE,
    DROP COLUMN IF EXISTS region CASCADE,
    DROP COLUMN IF EXISTS locality CASCADE,
    DROP COLUMN IF EXISTS postcode CASCADE,
    DROP COLUMN IF EXISTS latitude CASCADE,
    DROP COLUMN IF EXISTS longitude CASCADE,
    DROP COLUMN IF EXISTS generated_city CASCADE,
    DROP COLUMN IF EXISTS generated_state CASCADE,
    DROP COLUMN IF EXISTS generated_country CASCADE,
    DROP COLUMN IF EXISTS llm_location_retry_count CASCADE,
    DROP COLUMN IF EXISTS job_url CASCADE;

SELECT 'Columns dropped successfully' as status;

-- Verify remaining columns
SELECT column_name, data_type
FROM information_schema.columns
WHERE table_name = 'jobs'
ORDER BY ordinal_position;

COMMIT;
