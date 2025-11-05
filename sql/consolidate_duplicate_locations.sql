-- Consolidate duplicate job_locations (same job_id and location)
-- Merge URLs and keep only one location record per (job_id, location)

BEGIN;

-- Step 1: Find all duplicate (job_id, location) combinations and pick a keeper
-- Use COALESCE to handle NULL locations as empty strings for grouping
DROP TABLE IF EXISTS temp_location_keepers;
CREATE TEMP TABLE temp_location_keepers AS
SELECT DISTINCT ON (job_id, COALESCE(location, ''))
    job_id,
    location,
    id as keeper_id
FROM job_locations
WHERE (job_id, COALESCE(location, '')) IN (
    SELECT job_id, COALESCE(location, '')
    FROM job_locations
    GROUP BY job_id, COALESCE(location, '')
    HAVING COUNT(*) > 1
)
ORDER BY job_id, COALESCE(location, ''), id ASC;

CREATE INDEX idx_location_keepers ON temp_location_keepers(job_id, location);

SELECT 'Step 1 Complete: Found ' || COUNT(*) || ' keeper locations' FROM temp_location_keepers;

-- Step 2: Delete URLs from duplicate locations that already exist on keeper location
-- This prevents duplicate key violations
DELETE FROM job_location_urls jlu
WHERE EXISTS (
    SELECT 1
    FROM job_locations jl
    INNER JOIN temp_location_keepers k
      ON jl.job_id = k.job_id
      AND COALESCE(jl.location, '') = COALESCE(k.location, '')
    INNER JOIN job_location_urls keeper_url
      ON keeper_url.job_location_id = k.keeper_id
      AND keeper_url.url = jlu.url
    WHERE jlu.job_location_id = jl.id
      AND jl.id != k.keeper_id
);

SELECT 'Step 2 Complete: Deleted duplicate URLs from non-keeper locations';

-- Step 3: Copy remaining unique URLs from duplicate locations to keeper location
-- Use INSERT with ON CONFLICT to handle any remaining duplicates
INSERT INTO job_location_urls (job_location_id, url)
SELECT DISTINCT k.keeper_id, jlu.url
FROM job_location_urls jlu
INNER JOIN job_locations jl ON jlu.job_location_id = jl.id
INNER JOIN temp_location_keepers k
  ON jl.job_id = k.job_id
  AND COALESCE(jl.location, '') = COALESCE(k.location, '')
WHERE jl.id != k.keeper_id
ON CONFLICT (job_location_id, url) DO NOTHING;

SELECT 'Step 3 Complete: Copied remaining URLs to keeper locations';

-- Step 3b: Delete URLs from non-keeper locations
DELETE FROM job_location_urls jlu
WHERE EXISTS (
    SELECT 1
    FROM job_locations jl
    INNER JOIN temp_location_keepers k
      ON jl.job_id = k.job_id
      AND COALESCE(jl.location, '') = COALESCE(k.location, '')
    WHERE jlu.job_location_id = jl.id
      AND jl.id != k.keeper_id
);

SELECT 'Step 3b Complete: Deleted URLs from non-keeper locations';

-- Step 4: Delete duplicate location records (keep only keepers)
DELETE FROM job_locations jl
WHERE EXISTS (
    SELECT 1
    FROM temp_location_keepers k
    WHERE jl.job_id = k.job_id
      AND COALESCE(jl.location, '') = COALESCE(k.location, '')
      AND jl.id != k.keeper_id
);

SELECT 'Step 4 Complete: Deleted duplicate location records';

-- Step 5: Verify no duplicates remain
SELECT
    CASE
        WHEN COUNT(*) = 0 THEN 'SUCCESS: No duplicates remain'
        ELSE 'ERROR: ' || COUNT(*) || ' duplicates still exist'
    END as verification
FROM (
    SELECT job_id, COALESCE(location, '') as location, COUNT(*) as cnt
    FROM job_locations
    GROUP BY job_id, COALESCE(location, '')
    HAVING COUNT(*) > 1
) sub;

COMMIT;
