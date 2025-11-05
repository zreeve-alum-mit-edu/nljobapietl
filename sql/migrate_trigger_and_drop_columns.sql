-- Migrate trigger to job_locations table and drop old columns from jobs table

BEGIN;

-- Step 1: Drop the trigger from jobs table
DROP TRIGGER IF EXISTS trigger_update_gistlocation ON jobs;

-- Step 2: Create the trigger on job_locations table
CREATE TRIGGER trigger_update_gistlocation
    BEFORE INSERT OR UPDATE OF latitude, longitude
    ON job_locations
    FOR EACH ROW
    EXECUTE FUNCTION update_gistlocation();

SELECT 'Trigger migrated to job_locations' as status;

-- Step 3: Drop all old location columns from jobs table
ALTER TABLE jobs
    DROP COLUMN IF EXISTS location CASCADE,
    DROP COLUMN IF EXISTS country CASCADE,
    DROP COLUMN IF EXISTS region CASCADE,
    DROP COLUMN IF EXISTS locality CASCADE,
    DROP COLUMN IF EXISTS postcode CASCADE,
    DROP COLUMN IF EXISTS latitude CASCADE,
    DROP COLUMN IF EXISTS longitude CASCADE,
    DROP COLUMN IF EXISTS gistlocation CASCADE,
    DROP COLUMN IF EXISTS generated_city CASCADE,
    DROP COLUMN IF EXISTS generated_state CASCADE,
    DROP COLUMN IF EXISTS generated_country CASCADE,
    DROP COLUMN IF EXISTS llm_location_retry_count CASCADE,
    DROP COLUMN IF EXISTS job_url CASCADE;

SELECT 'Columns dropped successfully from jobs table' as status;

-- Step 4: Verify remaining columns in jobs table
SELECT column_name, data_type
FROM information_schema.columns
WHERE table_name = 'jobs'
ORDER BY ordinal_position;

COMMIT;
