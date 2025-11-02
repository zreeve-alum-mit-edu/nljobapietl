-- Add generated workplace columns to jobs table
ALTER TABLE jobs ADD COLUMN IF NOT EXISTS generated_workplace VARCHAR(20);
ALTER TABLE jobs ADD COLUMN IF NOT EXISTS generated_workplace_inferred BOOLEAN;
ALTER TABLE jobs ADD COLUMN IF NOT EXISTS generated_workplace_confidence VARCHAR(20);

-- Add indexes for filtering
CREATE INDEX IF NOT EXISTS idx_jobs_generated_workplace ON jobs(generated_workplace);
