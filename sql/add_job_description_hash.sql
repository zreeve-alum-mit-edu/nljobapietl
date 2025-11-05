-- Add job_description_hash column
ALTER TABLE jobs ADD COLUMN IF NOT EXISTS job_description_hash VARCHAR(64);

-- Create index on hash column for fast duplicate lookups
CREATE INDEX IF NOT EXISTS idx_job_description_hash
ON jobs (job_description_hash)
WHERE job_description_hash IS NOT NULL;

-- Backfill hashes for existing jobs (runs in batches to avoid long locks)
-- This might take a while depending on how many jobs you have
UPDATE jobs
SET job_description_hash = md5(COALESCE(job_description, ''))
WHERE job_description_hash IS NULL
  AND job_description IS NOT NULL;

-- Add comment for documentation
COMMENT ON COLUMN jobs.job_description_hash IS 'MD5 hash of job_description for duplicate detection';
