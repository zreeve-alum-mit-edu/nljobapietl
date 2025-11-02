-- Add columns for workplace classification metadata
ALTER TABLE jobs
ADD COLUMN IF NOT EXISTS generated_workplace_inferred BOOLEAN,
ADD COLUMN IF NOT EXISTS generated_workplace_confidence VARCHAR(20);

-- Add index on generated_workplace for faster filtering
CREATE INDEX IF NOT EXISTS idx_jobs_generated_workplace ON jobs(generated_workplace);
CREATE INDEX IF NOT EXISTS idx_jobs_generated_workplace_confidence ON jobs(generated_workplace_confidence);
