-- Add unique constraint on job_locations (job_id, location)
-- This prevents duplicate location entries for the same job

BEGIN;

-- Add unique constraint on (job_id, location)
ALTER TABLE job_locations
    ADD CONSTRAINT uq_job_locations_job_id_location
    UNIQUE (job_id, location);

SELECT 'Unique constraint added on (job_id, location)' as status;

-- Verify the constraint was created
SELECT conname, contype, pg_get_constraintdef(oid) as definition
FROM pg_constraint
WHERE conrelid = 'job_locations'::regclass
  AND conname = 'uq_job_locations_job_id_location';

COMMIT;
