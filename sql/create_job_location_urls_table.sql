-- Create job_location_urls table
CREATE TABLE IF NOT EXISTS job_location_urls (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    job_location_id UUID NOT NULL,
    url VARCHAR(1000) NOT NULL,
    CONSTRAINT fk_job_location_urls_job_locations FOREIGN KEY (job_location_id) REFERENCES job_locations(id) ON DELETE CASCADE
);

-- Create index on job_location_id for fast lookups
CREATE INDEX IF NOT EXISTS idx_job_location_urls_job_location_id ON job_location_urls(job_location_id);

-- Create unique index on URL to prevent duplicate URLs (same as jobs.job_url constraint)
CREATE UNIQUE INDEX IF NOT EXISTS idx_job_location_urls_url ON job_location_urls(url) WHERE url IS NOT NULL;

-- Migrate existing job URLs from jobs table to job_location_urls
-- Match job_url to the corresponding job_location via job_id
INSERT INTO job_location_urls (job_location_id, url)
SELECT
    jl.id as job_location_id,
    j.job_url as url
FROM jobs j
INNER JOIN job_locations jl ON jl.job_id = j.id
WHERE j.job_url IS NOT NULL
ON CONFLICT DO NOTHING;

-- Add comment for documentation
COMMENT ON TABLE job_location_urls IS 'Stores URLs for job locations, allowing multiple URLs per location';
COMMENT ON COLUMN job_location_urls.url IS 'Job posting URL, must be unique across all job locations';
