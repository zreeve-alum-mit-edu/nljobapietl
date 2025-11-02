-- Add is_valid column to jobs table
ALTER TABLE jobs ADD COLUMN IF NOT EXISTS is_valid BOOLEAN NOT NULL DEFAULT TRUE;

-- Create index for filtering by validity
CREATE INDEX IF NOT EXISTS idx_jobs_is_valid ON jobs(is_valid);
