-- Add unique constraint on job_description_hash to prevent duplicate jobs

BEGIN;

-- Add unique constraint on job_description_hash
-- This prevents inserting jobs with the same description hash
ALTER TABLE jobs
    ADD CONSTRAINT uq_jobs_job_description_hash
    UNIQUE (job_description_hash);

SELECT 'Unique constraint added on job_description_hash' as status;

-- Verify the constraint was created
SELECT conname, contype, pg_get_constraintdef(oid) as definition
FROM pg_constraint
WHERE conrelid = 'jobs'::regclass
  AND conname = 'uq_jobs_job_description_hash';

COMMIT;
