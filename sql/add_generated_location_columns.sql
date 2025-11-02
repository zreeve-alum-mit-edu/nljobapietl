-- Add generated location columns to jobs table
ALTER TABLE jobs ADD COLUMN IF NOT EXISTS generated_city VARCHAR(100);
ALTER TABLE jobs ADD COLUMN IF NOT EXISTS generated_state VARCHAR(2);
ALTER TABLE jobs ADD COLUMN IF NOT EXISTS generated_country VARCHAR(2);

-- Add indexes for filtering
CREATE INDEX IF NOT EXISTS idx_jobs_generated_city ON jobs(generated_city);
CREATE INDEX IF NOT EXISTS idx_jobs_generated_state ON jobs(generated_state);
CREATE INDEX IF NOT EXISTS idx_jobs_generated_country ON jobs(generated_country);
